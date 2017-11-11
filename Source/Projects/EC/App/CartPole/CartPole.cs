using System;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.NEAT;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.App.CartPole
{
    [ECConfiguration("ec.app.cartpole.CartPole")]
    public class CartPole : Problem, ISimpleProblem
    {
        int MAX_STEPS = 100000;

        double _x, /* cart position, meters */
            _xDot, /* cart velocity */
            _theta, /* pole angle, radians */
            _thetaDot; /* pole angular velocity */

        int _steps, _y;


        public double[] GetNetOutput(NEATNetwork net, double[][] input, IEvolutionState state)
        {
            double[] output;
            int netDepth = net.MaxDepth();

            net.LoadSensors(input[0]);
            for (int relax = 0; relax < netDepth; relax++)
            {
                net.Activate(state);
            }

            output = net.GetOutputResults();

            net.Flush();


            return output;
        }

        public int RunCartPole(NEATNetwork net, IEvolutionState state)
        {



            // double in[] = new double[5];  //Input loading array

            double out1;
            double out2;
            double twelve_degrees = 0.2094384;
            _x = _xDot = _theta = _thetaDot = 0.0;
            _steps = 0;


            double[][] input = TensorFactory.Create<double>(1, 5);
            while (_steps++ < MAX_STEPS)
            {

                /*-- setup the input layer based on the four inputs and bias --*/
                //setup_input(net,x,x_dot,theta,theta_dot);
                input[0][0] = 1.0; //Bias
                input[0][1] = (_x + 2.4) / 4.8;

                input[0][2] = (_xDot + .75) / 1.5;
                input[0][3] = (_theta + twelve_degrees) / .41;
                input[0][4] = (_thetaDot + 1.0) / 2.0;

                double[] output = GetNetOutput(net, input, state);

                /*-- decide which way to push via which output unit is greater --*/
                if (output[0] > output[1])
                    _y = 0;
                else
                    _y = 1;

                /*--- Apply action to the simulated cart-pole ---*/
                cart_pole(_y);

                /*--- Check for failure.  If so, return steps ---*/
                if (_x < -2.4 || _x > 2.4 || _theta < -twelve_degrees || _theta > twelve_degrees)
                    return _steps;
            }

            return _steps;
        }

        void cart_pole(int action)
        {
            double xacc, thetaacc, force, costheta, sintheta, temp;

            double GRAVITY = 9.8;
            double MASSCART = 1.0;
            double MASSPOLE = 0.1;
            double TOTAL_MASS = MASSPOLE + MASSCART;
            double LENGTH = 0.5; /* actually half the pole's length */
            double POLEMASS_LENGTH = MASSPOLE * LENGTH;
            double FORCE_MAG = 10.0;
            double TAU = 0.02; /* seconds between state updates */
            double FOURTHIRDS = 1.3333333333333;

            force = action > 0 ? FORCE_MAG : -FORCE_MAG;
            costheta = Math.Cos(_theta);
            sintheta = Math.Sin(_theta);

            temp = (force + POLEMASS_LENGTH * _thetaDot * _thetaDot * sintheta) / TOTAL_MASS;

            thetaacc = (GRAVITY * sintheta - costheta * temp)
                       / (LENGTH * (FOURTHIRDS - MASSPOLE * costheta * costheta
                                    / TOTAL_MASS));

            xacc = temp - POLEMASS_LENGTH * thetaacc * costheta / TOTAL_MASS;

            /*** Update the four state variables, using Euler's method. ***/

            _x += TAU * _xDot;
            _xDot += TAU * xacc;
            _theta += TAU * _thetaDot;
            _thetaDot += TAU * thetaacc;
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (ind.Evaluated) return;

            if (!(ind is NEATIndividual))
                state.Output.Fatal("Whoa! It's not a NEATIndividual!!!", null);

            var neatInd = (NEATIndividual) ind;

            if (!(neatInd.Fitness is SimpleFitness))
                state.Output.Fatal("Whoa! It's not a SimpleFitness!!!", null);

            NEATNetwork net = neatInd.CreateNetwork();

            double fitness = RunCartPole(net, state);

            ((SimpleFitness) neatInd.Fitness).SetFitness(state, fitness, fitness >= MAX_STEPS);
            neatInd.Evaluated = true;
        }

    }
}