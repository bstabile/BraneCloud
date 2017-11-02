using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.Regression.Func
{
    /// <summary>
    /// Copyright 2012 by Sean Luke
    /// Licensed under the Academic Free License version 3.0
    /// See the file "LICENSE" for more information
    ///
    /// Neg.java
    /// 
    /// Created: Mon Apr 23 17:15:35 EDT 2012
    /// By: Sean Luke
    ///
    /// <p/>This function appears in the Keijzer function set, and is (0 - x)
    /// <p/>M. Keijzer. Improving Symbolic Regression with Interval Arithmetic and Linear Scaling. In <i>Proc. EuroGP.</i> 2003.
    /// </summary>
    public class Neg : GPNode
    {
        public override string ToString() { return "0-"; }

        public override int ExpectedChildren => 1;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var rd = ((RegressionData)(input));

            Children[0].Eval(state, thread, input, stack, individual, problem);
            rd.x = 0.0 - rd.x;
        }
    }
}


