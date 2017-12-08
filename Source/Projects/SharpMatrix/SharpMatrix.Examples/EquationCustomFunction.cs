using System;
using System.Collections.Generic;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;
using SharpMatrix.Equation;
using SharpMatrix.Simple;
using Randomization;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * Demonstration on how to create and use a custom function in Equation.  A custom function must implement
 * ManagerFunctions.Input1 or ManagerFunctions.InputN, depending on the number of inputs it takes.
 *
 * @author Peter Abeles
 */
    public class EquationCustomFunction
    {

        public static void main(string[] args)
        {
            IMersenneTwister rand = new MersenneTwisterFast(234);

            Equation.Equation eq = new Equation.Equation();
            eq.getFunctions().add("multTransA", createMultTransA());

            SimpleMatrix<DMatrixRMaj> A = new SimpleMatrix<DMatrixRMaj>(1, 1); // will be resized
            SimpleMatrix<DMatrixRMaj> B = SimpleMatrix<DMatrixRMaj>.random64(3, 4, -1, 1, rand);
            SimpleMatrix<DMatrixRMaj> C = SimpleMatrix<DMatrixRMaj>.random64(3, 4, -1, 1, rand);

            eq.alias(A, "A", B, "B", C, "C");

            eq.process("A=multTransA(B,C)");

            Console.WriteLine("Found");
            Console.WriteLine(A);
            Console.WriteLine("Expected");
            B.transpose().mult(C).print();
        }

        /**
         * Create the function.  Be sure to handle all possible input types and combinations correctly and provide
         * meaningful error messages.  The output matrix should be resized to fit the inputs.
         */
        public static ManagerFunctions.InputN createMultTransA()
        {
            return new ManagerFunctions.InputN((l, m) =>
            {
                var inputs = l;
                var manager = m;

                if (inputs.Count != 2)
                    throw new InvalidOperationException("Two inputs required");

                Variable varA = inputs[0];
                Variable varB = inputs[1];

                Operation.Info ret = new Operation.Info();

                if (varA is VariableMatrix && varB is VariableMatrix)
                {

                    // The output matrix or scalar variable must be created with the provided manager
                    VariableMatrix output = manager.createMatrix();
                    ret.output = output;
                    ret.op = new Operation("multTransA-mm")
                    {
                        process = () =>
                        {
                            DMatrixRMaj mA = ((VariableMatrix) varA).matrix;
                            DMatrixRMaj mB = ((VariableMatrix) varB).matrix;
                            output.matrix.reshape(mA.numCols, mB.numCols);

                            CommonOps_DDRM.multTransA(mA, mB, output.matrix);
                        }
                    };
                }
                else
                {
                    throw new ArgumentException("Expected both inputs to be a matrix");
                }

                return ret;
            });
        }
    }
}
