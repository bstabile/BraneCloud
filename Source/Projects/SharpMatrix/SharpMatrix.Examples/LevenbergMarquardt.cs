using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;

namespace BraneCloud.Evolution.EC.MatrixLib.Examples
{
    //package org.ejml.example;

/**
 * <p>
 * This is a straight forward implementation of the Levenberg-Marquardt (LM) algorithm. LM is used to minimize
 * non-linear cost functions:<br>
 * <br>
 * S(P) = Sum{ i=1:m , [y<sub>i</sub> - f(x<sub>i</sub>,P)]<sup>2</sup>}<br>
 * <br>
 * where P is the set of parameters being optimized.
 * </p>
 *
 * <p>
 * In each iteration the parameters are updated using the following equations:<br>
 * <br>
 * P<sub>i+1</sub> = (H + &lambda; I)<sup>-1</sup> d <br>
 * d =  (1/N) Sum{ i=1..N , (f(x<sub>i</sub>;P<sub>i</sub>) - y<sub>i</sub>) * jacobian(:,i) } <br>
 * H =  (1/N) Sum{ i=1..N , jacobian(:,i) * jacobian(:,i)<sup>T</sup> }
 * </p>
 * <p>
 * Whenever possible the allocation of new memory is avoided.  This is accomplished by reshaping matrices.
 * A matrix that is reshaped won't grow unless the new shape requires more memory than it has available.
 * </p>
 * @author Peter Abeles
 */
    public class LevenbergMarquardt
    {
        // how much the numerical jacobian calculation perturbs the parameters by.
        // In better implementation there are better ways to compute this delta.  See Numerical Recipes.
        private static double DELTA = 1e-8;

        private double initialLambda;

        // the function that is optimized
        private Function func;

        // the optimized parameters and associated costs
        private DMatrixRMaj param;

        private double initialCost;
        private double finalCost;

        // used by matrix operations
        private DMatrixRMaj d;

        private DMatrixRMaj H;
        private DMatrixRMaj negDelta;
        private DMatrixRMaj tempParam;
        private DMatrixRMaj A;

        // variables used by the numerical jacobian algorithm
        private DMatrixRMaj temp0;

        private DMatrixRMaj temp1;

        // used when computing d and H variables
        private DMatrixRMaj tempDH;

        // Where the numerical Jacobian is stored.
        private DMatrixRMaj jacobian;

        /**
         * Creates a new instance that uses the provided cost function.
         *
         * @param funcCost Cost function that is being optimized.
         */
        public LevenbergMarquardt(Function funcCost)
        {
            this.initialLambda = 1;

            // declare data to some initial small size. It will grow later on as needed.
            int maxElements = 1;
            int numParam = 1;

            this.temp0 = new DMatrixRMaj(maxElements, 1);
            this.temp1 = new DMatrixRMaj(maxElements, 1);
            this.tempDH = new DMatrixRMaj(maxElements, 1);
            this.jacobian = new DMatrixRMaj(numParam, maxElements);

            this.func = funcCost;

            this.param = new DMatrixRMaj(numParam, 1);
            this.d = new DMatrixRMaj(numParam, 1);
            this.H = new DMatrixRMaj(numParam, numParam);
            this.negDelta = new DMatrixRMaj(numParam, 1);
            this.tempParam = new DMatrixRMaj(numParam, 1);
            this.A = new DMatrixRMaj(numParam, numParam);
        }


        public double getInitialCost()
        {
            return initialCost;
        }

        public double getFinalCost()
        {
            return finalCost;
        }

        public DMatrixRMaj getParameters()
        {
            return param;
        }

        /**
         * Finds the best fit parameters.
         *
         * @param initParam The initial set of parameters for the function.
         * @param X The inputs to the function.
         * @param Y The "observed" output of the function
         * @return true if it succeeded and false if it did not.
         */
        public bool optimize(DMatrixRMaj initParam,
            DMatrixRMaj X,
            DMatrixRMaj Y)
        {
            configure(initParam, X, Y);

            // save the cost of the initial parameters so that it knows if it improves or not
            initialCost = cost(param, X, Y);

            // iterate until the difference between the costs is insignificant
            // or it iterates too many times
            if (!adjustParam(X, Y, initialCost))
            {
                finalCost = double.NaN;
                return false;
            }

            return true;
        }

        /**
         * Iterate until the difference between the costs is insignificant
         * or it iterates too many times
         */
        private bool adjustParam(DMatrixRMaj X, DMatrixRMaj Y,
            double prevCost)
        {
            // lambda adjusts how big of a step it takes
            double lambda = initialLambda;
            // the difference between the current and previous cost
            double difference = 1000;

            for (int iter = 0; iter < 20 || difference < 1e-6; iter++)
            {
                // compute some variables based on the gradient
                computeDandH(param, X, Y);

                // try various step sizes and see if any of them improve the
                // results over what has already been done
                bool foundBetter = false;
                for (int i = 0; i < 5; i++)
                {
                    computeA(A, H, lambda);

                    if (!CommonOps_DDRM.solve(A, d, negDelta))
                    {
                        return false;
                    }
                    // compute the candidate parameters
                    CommonOps_DDRM.subtract(param, negDelta, tempParam);

                    double cost = this.cost(tempParam, X, Y);
                    if (cost < prevCost)
                    {
                        // the candidate parameters produced better results so use it
                        foundBetter = true;
                        param.set(tempParam);
                        difference = prevCost - cost;
                        prevCost = cost;
                        lambda /= 10.0;
                    }
                    else
                    {
                        lambda *= 10.0;
                    }
                }

                // it reached a point where it can't improve so exit
                if (!foundBetter)
                    break;
            }
            finalCost = prevCost;
            return true;
        }

        /**
         * Performs sanity checks on the input data and reshapes internal matrices.  By reshaping
         * a matrix it will only declare new memory when needed.
         */
        public void configure(DMatrixRMaj initParam, DMatrixRMaj X, DMatrixRMaj Y)
        {
            if (Y.getNumRows() != X.getNumRows())
            {
                throw new ArgumentException("Different vector lengths");
            }
            else if (Y.getNumCols() != 1 || X.getNumCols() != 1)
            {
                throw new ArgumentException("Inputs must be a column vector");
            }

            int numParam = initParam.getNumElements();
            int numPoints = Y.getNumRows();

            if (param.getNumElements() != initParam.getNumElements())
            {
                // reshaping a matrix means that new memory is only declared when needed
                this.param.reshape(numParam, 1, false);
                this.d.reshape(numParam, 1, false);
                this.H.reshape(numParam, numParam, false);
                this.negDelta.reshape(numParam, 1, false);
                this.tempParam.reshape(numParam, 1, false);
                this.A.reshape(numParam, numParam, false);
            }

            param.set(initParam);

            // reshaping a matrix means that new memory is only declared when needed
            temp0.reshape(numPoints, 1, false);
            temp1.reshape(numPoints, 1, false);
            tempDH.reshape(numPoints, 1, false);
            jacobian.reshape(numParam, numPoints, false);


        }

        /**
         * Computes the d and H parameters.  Where d is the average error gradient and
         * H is an approximation of the hessian.
         */
        private void computeDandH(DMatrixRMaj param, DMatrixRMaj x, DMatrixRMaj y)
        {
            func.compute(param, x, tempDH);
            CommonOps_DDRM.subtractEquals(tempDH, y);

            computeNumericalJacobian(param, x, jacobian);

            int numParam = param.getNumElements();
            int length = x.getNumElements();

            // d = average{ (f(x_i;p) - y_i) * jacobian(:,i) }
            for (int i = 0; i < numParam; i++)
            {
                double total = 0;
                for (int j = 0; j < length; j++)
                {
                    total += tempDH.get(j, 0) * jacobian.get(i, j);
                }
                d.set(i, 0, total / length);
            }

            // compute the approximation of the hessian
            CommonOps_DDRM.multTransB(jacobian, jacobian, H);
            CommonOps_DDRM.scale(1.0 / length, H);
        }

        /**
         * A = H + lambda*I <br>
         * <br>
         * where I is an identity matrix.
         */
        private void computeA(DMatrixRMaj A, DMatrixRMaj H, double lambda)
        {
            int numParam = param.getNumElements();

            A.set(H);
            for (int i = 0; i < numParam; i++)
            {
                A.set(i, i, A.get(i, i) + lambda);
            }
        }

        /**
         * Computes the "cost" for the parameters given.
         *
         * cost = (1/N) Sum (f(x;p) - y)^2
         */
        private double cost(DMatrixRMaj param, DMatrixRMaj X, DMatrixRMaj Y)
        {
            func.compute(param, X, temp0);

            double error = SpecializedOps_DDRM.diffNormF(temp0, Y);

            return error * error / (double) X.numRows;
        }

        /**
         * Computes a simple numerical Jacobian.
         *
         * @param param The set of parameters that the Jacobian is to be computed at.
         * @param pt The point around which the Jacobian is to be computed.
         * @param deriv Where the jacobian will be stored
         */
        public void computeNumericalJacobian(DMatrixRMaj param,
            DMatrixRMaj pt,
            DMatrixRMaj deriv)
        {
            double invDelta = 1.0 / DELTA;

            func.compute(param, pt, temp0);

            // compute the jacobian by perturbing the parameters slightly
            // then seeing how it effects the results.
            for (int i = 0; i < param.numRows; i++)
            {
                param.data[i] += DELTA;
                func.compute(param, pt, temp1);
                // compute the difference between the two parameters and divide by the delta
                CommonOps_DDRM.add(invDelta, temp1, -invDelta, temp0, temp1);
                // copy the results into the jacobian matrix
                Array.Copy(temp1.data, 0, deriv.data, i * pt.numRows, pt.numRows);

                param.data[i] -= DELTA;
            }
        }

        /**
         * The function that is being optimized.
         */
        public interface Function
        {
            /**
             * Computes the output for each value in matrix x given the set of parameters.
             *
             * @param param The parameter for the function.
             * @param x the input points.
             * @param y the resulting output.
             */
            void compute(DMatrixRMaj param, DMatrixRMaj x, DMatrixRMaj y);
        }
    }
}