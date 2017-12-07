using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Factory;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

/**
 * Performs math operations.
 *
 * @author Peter Abeles
 */
    public class Operation
    {
        public class Extents
        {
            public int row0, row1;
            public int col0, col1;
        }

        public class Info
        {
            public Operation op;
            public Variable output;
        }

        public class ArrayExtent
        {
            public int[] array;
            public int length;

            public ArrayExtent()
            {
                array = new int[1];
            }

            public void setLength(int length)
            {
                if (length > array.Length)
                {
                    array = new int[length];
                }
                this.length = length;
            }
        }

        string _name;

        public Operation(string name)
        {
            this._name = name;
        }

        public Action process { get; set; }

        public string name()
        {
            return _name;
        }

        /**
         * If the variable is a local temporary variable it will be resized so that the operation can complete.  If not
         * temporary then it will not be reshaped
         * @param mat Variable containing the matrix
         * @param numRows Desired number of rows
         * @param numCols Desired number of columns
         */
        protected void resize(VariableMatrix mat, int numRows, int numCols)
        {
            if (mat.isTemp())
            {
                mat.matrix.reshape(numRows, numCols);
            }
        }

        public static Info multiply(Variable A, Variable B, ManagerTempVariables manager)
        {

            Info ret = new Info();

            if (A is VariableMatrix && B is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("multiply-mm")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        VariableMatrix mB = (VariableMatrix)B;
                        ret.op.resize(output, mA.matrix.numRows, mB.matrix.numCols);
                        CommonOps_DDRM.mult(mA.matrix, mB.matrix, output.matrix);
                    }
            };
            }
            else if (A is VariableInteger && B is VariableInteger)
            {
                VariableInteger output = manager.createInteger();
                ret.output = output;
                ret.op = new Operation("multiply-ii")
                {
                    process = () =>
                    {
                        VariableInteger mA = (VariableInteger)A;
                        VariableInteger mB = (VariableInteger)B;
                        output.value = mA.value * mB.value;
                    }
                };
            }
            else if (A is VariableScalar && B is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("multiply-ss")
                {
                    process = () =>
                    {
                        VariableScalar mA = (VariableScalar)A;
                        VariableScalar mB = (VariableScalar)B;
                        output.value = mA.getDouble() * mB.getDouble();
                    }
                };
            }
            else
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                VariableMatrix m;
                VariableScalar s;

                if (A is VariableMatrix)
                {
                    m = (VariableMatrix) A;
                    s = (VariableScalar) B;
                }
                else
                {
                    m = (VariableMatrix) B;
                    s = (VariableScalar) A;
                }

                ret.op = new Operation("multiply-ms")
                {
                    process = () =>
                    {
                        output.matrix.reshape(m.matrix.numRows, m.matrix.numCols);
                        CommonOps_DDRM.scale(s.getDouble(), m.matrix, output.matrix);
                    }
                };
            }

            return ret;
        }

        public static Info divide(Variable A, Variable B, ManagerTempVariables manager)
        {

            Info ret = new Info();

            if (A is VariableMatrix && B is VariableMatrix)
            {
                return solve(B, A, manager);
            }
            else if (A is VariableMatrix && B is VariableScalar)
            {
                VariableMatrix output = manager.createMatrix();
                VariableMatrix m = (VariableMatrix) A;
                VariableScalar s = (VariableScalar) B;
                ret.output = output;
                ret.op = new Operation("divide-ma")
                {
                    process = () =>
                    {
                        output.matrix.reshape(m.matrix.numRows, m.matrix.numCols);
                        CommonOps_DDRM.divide(m.matrix, s.getDouble(), output.matrix);
                    }
                };
            }
            else if (A is VariableScalar && B is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                VariableMatrix m = (VariableMatrix) B;
                VariableScalar s = (VariableScalar) A;
                ret.output = output;
                ret.op = new Operation("divide-ma")
                {
                    process = () =>
                    {
                        output.matrix.reshape(m.matrix.numRows, m.matrix.numCols);
                        CommonOps_DDRM.divide(s.getDouble(), m.matrix, output.matrix);
                    }
                };
            }
            else if (A is VariableInteger && B is VariableInteger)
            {
                VariableInteger output = manager.createInteger();
                ret.output = output;
                ret.op = new Operation("divide-ii")
                {
                    process = () =>
                    {
                        VariableInteger mA = (VariableInteger)A;
                        VariableInteger mB = (VariableInteger)B;
                        output.value = mA.value / mB.value;
                    }
                };
            }
            else
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("divide-ss")
                {
                    process = () =>
                    {
                        VariableScalar mA = (VariableScalar)A;
                        VariableScalar mB = (VariableScalar)B;
                        output.value = mA.getDouble() / mB.getDouble();
                    }
                };
            }

            return ret;
        }

        /**
         * Returns the negative of the input variable
         */
        public static Info neg(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableInteger)
            {
                VariableInteger output = manager.createInteger();
                ret.output = output;
                ret.op = new Operation("neg-i")
                {
                    process = () =>
                    {
                        output.value = -((VariableInteger)A).value;
                    }
                };
            }
            else if (A is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("neg-s")
                {
                    process = () =>
                    {
                        output.value = -((VariableScalar)A).getDouble();
                    }
                };
            }
            else if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("neg-m")
                {
                    process = () =>
                    {
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        output.matrix.reshape(a.numRows, a.numCols);
                        CommonOps_DDRM.changeSign(a, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Unsupported variable " + A);
            }

            return ret;
        }

        public static Info pow(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableScalar && B is VariableScalar)
            {

                ret.op = new Operation("pow-ss")
                {
                    process = () =>
                    {
                        double a = ((VariableScalar)A).getDouble();
                        double b = ((VariableScalar)B).getDouble();
                        output.value = Math.Pow(a, b);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Only scalar to scalar power supported");
            }

            return ret;
        }

        public static Info atan2(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableScalar && B is VariableScalar)
            {

                ret.op = new Operation("atan2-ss")
                {
                    process = () =>
                    {
                        double a = ((VariableScalar)A).getDouble();
                        double b = ((VariableScalar)B).getDouble();
                        output.value = Math.Atan2(a, b);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Only scalar to scalar atan2 supported");
            }

            return ret;
        }

        public static Info sqrt(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableScalar)
            {

                ret.op = new Operation("sqrt-s")
                {
                    process = () =>
                    {
                        double a = ((VariableScalar)A).getDouble();
                        output.value = Math.Sqrt(a);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Only scalars are supported");
            }

            return ret;
        }

        public static Info sin(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableScalar)
            {
                ret.op = new Operation("sin-s")
                {
                    process = () =>
                    {
                        output.value = Math.Sin(((VariableScalar)A).getDouble());
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Only scalars are supported");
            }

            return ret;
        }

        public static Info cos(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableScalar)
            {
                ret.op = new Operation("cos-s")
                {
                    process = () =>
                    {
                        output.value = Math.Cos(((VariableScalar)A).getDouble());
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Only scalars are supported");
            }

            return ret;
        }

        public static Info atan(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableScalar)
            {
                ret.op = new Operation("atan-s")
                {
                    process = () =>
                    {
                        output.value = Math.Atan(((VariableScalar)A).getDouble());
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Only scalars are supported");
            }

            return ret;
        }

        public static Info exp(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();


            if (A is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("exp-s")
                {
                    process = () =>
                    {
                        output.value = Math.Exp(((VariableScalar)A).getDouble());
                    }
                };
            }
            else if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("exp-m")
                {
                    process = () =>
                    {
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        DMatrixRMaj outm = ((VariableMatrix)ret.output).matrix;
                        outm.reshape(a.numRows, a.numCols);
                        CommonOps_DDRM.elementExp(a, outm);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Only scalars are supported");
            }

            return ret;
        }

        public static Info log(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("log-s")
                {
                    process = () =>
                    {
                        output.value = Math.Log(((VariableScalar)A).getDouble());
                    }
                };
            }
            else if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("log-m")
                {
                    process = () =>
                    {
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        DMatrixRMaj outm = ((VariableMatrix)ret.output).matrix;
                        outm.reshape(a.numRows, a.numCols);
                        CommonOps_DDRM.elementLog(a, outm);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Only scalars are supported");
            }
            return ret;
        }

        public static Info add(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix && B is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("add-mm")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        VariableMatrix mB = (VariableMatrix)B;
                        ret.op.resize(output, mA.matrix.numRows, mA.matrix.numCols);
                        CommonOps_DDRM.add(mA.matrix, mB.matrix, output.matrix);
                    }
                };
            }
            else if (A is VariableInteger && B is VariableInteger)
            {
                VariableInteger output = manager.createInteger(0);
                ret.output = output;
                ret.op = new Operation("add-ii")
                {
                    process = () =>
                    {
                        VariableInteger mA = (VariableInteger) A;
                        VariableInteger mB = (VariableInteger) B;
                        output.value = mA.value + mB.value;
                    }
                };
            }
            else if (A is VariableScalar && B is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("add-ss")
                {
                    process = () =>
                    {
                        VariableScalar mA = (VariableScalar)A;
                        VariableScalar mB = (VariableScalar)B;
                        output.value = mA.getDouble() + mB.getDouble();
                    }
                };
            }
            else
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                VariableMatrix m;
                VariableScalar s;

                if (A is VariableMatrix)
                {
                    m = (VariableMatrix) A;
                    s = (VariableScalar) B;
                }
                else
                {
                    m = (VariableMatrix) B;
                    s = (VariableScalar) A;
                }

                ret.op = new Operation("add-ms")
                {
                    process = () =>
                    {
                        output.matrix.reshape(m.matrix.numRows, m.matrix.numCols);
                        CommonOps_DDRM.add(m.matrix, s.getDouble(), output.matrix);
                    }
                };
            }

            return ret;
        }

        public static Info subtract(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix && B is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("subtract-mm")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        VariableMatrix mB = (VariableMatrix)B;
                        ret.op.resize(output, mA.matrix.numRows, mA.matrix.numCols);
                        CommonOps_DDRM.subtract(mA.matrix, mB.matrix, output.matrix);
                    }
                };
            }
            else if (A is VariableInteger && B is VariableInteger)
            {
                VariableInteger output = manager.createInteger(0);
                ret.output = output;
                ret.op = new Operation("subtract-ii")
                {
                    process = () =>
                    {
                        VariableInteger mA = (VariableInteger)A;
                        VariableInteger mB = (VariableInteger)B;
                        output.value = mA.value - mB.value;
                    }
                };
            }
            else if (A is VariableScalar && B is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("subtract-ss")
                {
                    process = () =>
                    {
                        VariableScalar mA = (VariableScalar)A;
                        VariableScalar mB = (VariableScalar)B;
                        output.value = mA.getDouble() - mB.getDouble();
                    }
                };
            }
            else
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;

                if (A is VariableMatrix)
                {
                    ret.op = new Operation("subtract-ms")
                    {
                        process = () =>
                        {
                            DMatrixRMaj m = ((VariableMatrix)A).matrix;
                            double v = ((VariableScalar)B).getDouble();
                            output.matrix.reshape(m.numRows,
                            m.numCols);
                            CommonOps_DDRM.subtract(m,
                            v,
                            output.matrix);
                        }
                    };
                }
                else
                {
                    ret.op = new Operation("subtract-sm")
                    {
                        process = () =>
                        {
                            DMatrixRMaj m = ((VariableMatrix)B).matrix;
                            double v = ((VariableScalar)A).getDouble();
                            output.matrix.reshape(m.numRows,
                            m.numCols);
                            CommonOps_DDRM.subtract(v,
                            m,
                            output.matrix);
                        }
                    };
                }
            }

            return ret;
        }

        public static Info elementMult(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix && B is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("elementMult-mm")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        VariableMatrix mB = (VariableMatrix)B;
                        ret.op.resize(output, mA.matrix.numRows, mA.matrix.numCols);
                        CommonOps_DDRM.elementMult(mA.matrix, mB.matrix, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Both inputs must be matrices for element wise multiplication");
            }

            return ret;
        }

        public static Info elementDivision(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix && B is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("elementDivision-mm")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        VariableMatrix mB = (VariableMatrix)B;
                        ret.op.resize(output, mA.matrix.numRows, mA.matrix.numCols);
                        CommonOps_DDRM.elementDiv(mA.matrix, mB.matrix, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Both inputs must be matrices for element wise multiplication");
            }

            return ret;
        }

        public static Info elementPow(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();


            if (A is VariableScalar && B is VariableScalar)
            {

                VariableDouble output = manager.createDouble();
                ret.output = output;

                ret.op = new Operation("elementPow-ss")
                {
                    process = () =>
                    {
                        double a = ((VariableScalar)A).getDouble();
                        double b = ((VariableScalar)B).getDouble();
                        output.value = Math.Pow(a, b);
                    }
                };
            }
            else if (A is VariableMatrix && B is VariableMatrix)
            {

                VariableMatrix output = manager.createMatrix();
                ret.output = output;

                ret.op = new Operation("elementPow-mm")
                {
                    process = () =>
                    {
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        DMatrixRMaj b = ((VariableMatrix)B).matrix;
                        ret.op.resize(output, a.numRows, a.numCols);
                        CommonOps_DDRM.elementPower(a, b, output.matrix);
                    }
                };
            }
            else if (A is VariableMatrix && B is VariableScalar)
            {

                VariableMatrix output = manager.createMatrix();
                ret.output = output;

                ret.op = new Operation("elementPow-ms")
                {
                    process = () =>
                    {
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        double b = ((VariableScalar)B).getDouble();
                        ret.op.resize(output, a.numRows, a.numCols);
                        CommonOps_DDRM.elementPower(a, b, output.matrix);
                    }
                };
            }
            else if (A is VariableScalar && B is VariableMatrix)
            {

                VariableMatrix output = manager.createMatrix();
                ret.output = output;

                ret.op = new Operation("elementPow-sm")
                {
                    process = () =>
                    {
                        double a = ((VariableScalar)A).getDouble();
                        DMatrixRMaj b = ((VariableMatrix)B).matrix;
                        ret.op.resize(output, b.numRows, b.numCols);
                        CommonOps_DDRM.elementPower(a, b, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Unsupport element-wise power input types");
            }

            return ret;
        }

        public static Operation copy(Variable src, Variable dst)
        {

            if (src is VariableMatrix)
            {
                if (dst is VariableMatrix)
                {
                    return new Operation("copy-mm")
                    {
                        process = () =>
                        {
                            DMatrixRMaj d = ((VariableMatrix)dst).matrix;
                            DMatrixRMaj s = ((VariableMatrix)src).matrix;
                            d.reshape(s.numRows, s.numCols);
                            d.set(((VariableMatrix)src).matrix);
                        }
                    };
                }
                else if (dst is VariableDouble)
                {
                    return new Operation("copy-sm1")
                    {
                        process = () =>
                        {
                            DMatrixRMaj s = ((VariableMatrix)src).matrix;
                            if (s.numRows != 1 || s.numCols != 1)
                                throw new InvalidOperationException("Attempting to assign a non 1x1 matrix to a double");
                            ((VariableDouble)dst).value = s.unsafe_get(0, 0);
                        }
                    };
                }
            }
            if (src is VariableInteger && dst is VariableInteger)
            {
                return new Operation("copy-ii")
                {
                    process = () =>
                    {
                        ((VariableInteger)dst).value = ((VariableInteger)src).value;
                    }
                };
            }
            if (src is VariableScalar && dst is VariableDouble)
            {
                return new Operation("copy-ss")
                {
                    process = () =>
                    {
                        ((VariableDouble)dst).value = ((VariableScalar)src).getDouble();
                    }
                };
            }

            if (src is VariableIntegerSequence)
            {
                if (dst is VariableIntegerSequence)
                {
                    return new Operation("copy-is-is")
                    {
                        process = () =>
                        {
                            ((VariableIntegerSequence)dst).sequence = ((VariableIntegerSequence)src).sequence;
                        }
                    };
                }
            }

            throw new InvalidOperationException("Unsupported copy types; src = " + src.GetType().Name +
                                                " dst = " + dst.GetType().Name);
        }

        public static Operation copy(Variable src, Variable dst, List<Variable> range)
        {
            if (src is VariableMatrix && dst is VariableMatrix)
            {
                return new Operation("copyR-mm")
                {
                    process = () =>
                    {
                        Extents extents = new Extents();
                        ArrayExtent rowExtent = new ArrayExtent();
                        ArrayExtent colExtent = new ArrayExtent();

                        DMatrixRMaj msrc = ((VariableMatrix)src).matrix;
                        DMatrixRMaj mdst = ((VariableMatrix)dst).matrix;
                        if (range.Count == 1)
                        {
                            if (!MatrixFeatures_DDRM.isVector(msrc))
                            {
                                throw new ParseError("Source must be a vector for copy into elements");
                            }
                            if (extractSimpleExtents(range[0], extents, false, mdst.getNumElements()))
                            {
                                int length = extents.col1 - extents.col0 + 1;
                                if (msrc.getNumElements() != length)
                                    throw new ArgumentException("Source vector not the right length.");
                                if (extents.col1 + 1 > mdst.getNumElements())
                                    throw new ArgumentException("Requested range is outside of dst length");
                                Array.Copy(msrc.data, 0, mdst.data, extents.col0, length);
                            }
                            else
                            {
                                extractArrayExtent(range[0], mdst.getNumElements(), colExtent);
                                if (colExtent.length > msrc.getNumElements())
                                    throw new ArgumentException("src doesn't have enough elements");
                                for (int i = 0; i < colExtent.length; i++)
                                {
                                    mdst.data[colExtent.array[i]] = msrc.data[i];
                                }
                            }
                        }
                        else if (range.Count == 2)
                        {
                            if (extractSimpleExtents(range[0], extents, true, mdst.getNumRows()) &&
                                extractSimpleExtents(range[1], extents, false, mdst.getNumCols()))
                            {

                                int numRows = extents.row1 - extents.row0 + 1;
                                int numCols = extents.col1 - extents.col0 + 1;

                                CommonOps_DDRM.extract(msrc, 0, numRows, 0, numCols, mdst, extents.row0, extents.col0);
                            }
                            else
                            {
                                extractArrayExtent(range[0], mdst.numRows, rowExtent);
                                extractArrayExtent(range[1], mdst.numCols, colExtent);

                                CommonOps_DDRM.insert(msrc, mdst, rowExtent.array, rowExtent.length,
                                    colExtent.array, colExtent.length);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Unexpected number of ranges.  Should have been caught earlier");
                        }
                    }
                };
            }
            else if (src is VariableScalar && dst is VariableMatrix)
            {
                return new Operation("copyR-sm")
                {

                    process = () =>
                        {
                            Extents extents = new Extents();
                            ArrayExtent rowExtent = new ArrayExtent();
                            ArrayExtent colExtent = new ArrayExtent();

                            double msrc = ((VariableScalar)src).getDouble();
                            DMatrixRMaj mdst = ((VariableMatrix)dst).matrix;
                            if (range.Count == 1)
                            {
                                if (extractSimpleExtents(range[0], extents, false, mdst.getNumElements()))
                                {
                                    //Arrays.fill(mdst.data, extents.col0, extents.col1 + 1, msrc);
                                    for (var i = 0; i < extents.col1 + 1; i++)
                                        mdst.data[i] = msrc;
                                }
                                else
                                {
                                    extractArrayExtent(range[0], mdst.getNumElements(), colExtent);
                                    for (int i = 0; i < colExtent.length; i++)
                                    {
                                        mdst.data[colExtent.array[i]] = msrc;
                                    }
                                }
                            }
                            else if (range.Count == 2)
                            {
                                if (extractSimpleExtents(range[0], extents, true, mdst.getNumRows()) &&
                                    extractSimpleExtents(range[1], extents, false, mdst.getNumCols()))
                                {
                                    extents.row1 += 1;
                                    extents.col1 += 1;
                                    for (int i = extents.row0; i < extents.row1; i++)
                                    {
                                        int index = i * mdst.numCols + extents.col0;
                                        for (int j = extents.col0; j < extents.col1; j++)
                                        {
                                            mdst.data[index++] = msrc;
                                        }
                                    }
                                }
                                else
                                {
                                    extractArrayExtent(range[0], mdst.numRows, rowExtent);
                                    extractArrayExtent(range[1], mdst.numCols, colExtent);
                                    for (int i = 0; i < rowExtent.length; i++)
                                    {
                                        for (int j = 0; j < colExtent.length; j++)
                                        {
                                            mdst.unsafe_set(rowExtent.array[i], colExtent.array[j], msrc);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    "Unexpected number of ranges.  Should have been caught earlier");
                            }
                        }
                };
            }
            else
            {
                throw new InvalidOperationException("Both variables must be of type VariableMatrix");
            }
        }

        public static Info transpose(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("transpose-m")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        output.matrix.reshape(mA.matrix.numCols, mA.matrix.numRows);
                        CommonOps_DDRM.transpose(mA.matrix, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Transpose only makes sense for a matrix");
            }
            return ret;
        }

        /**
         * Matrix inverse
         */
        public static Info inv(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("inv-m")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        output.matrix.reshape(mA.matrix.numRows, mA.matrix.numCols);
                        if (!CommonOps_DDRM.invert(mA.matrix, output.matrix))
                            throw new InvalidOperationException("Inverse failed!");
                    }
                };
            }
            else
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("inv-s")
                {
                    process = () =>
                    {
                        VariableScalar mA = (VariableScalar)A;
                        output.value = 1.0 / mA.getDouble();
                    }
                };
            }

            return ret;
        }

        /**
         * Matrix pseudo-inverse
         */
        public static Info pinv(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("pinv-m")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        output.matrix.reshape(mA.matrix.numCols, mA.matrix.numRows);
                        CommonOps_DDRM.pinv(mA.matrix, output.matrix);
                    }
                };
            }
            else
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("pinv-s")
                {
                    process = () =>
                    {
                        VariableScalar mA = (VariableScalar)A;
                        output.value = 1.0 / mA.getDouble();
                    }
                };
            }

            return ret;
        }

        public static Info rref(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("rref-m")
                {
                    process = () =>
                    {
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        output.matrix.reshape(a.numRows, a.numCols);
                        CommonOps_DDRM.rref(a, -1, output.matrix);
                    }
                };
            }
            else
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("rref-s")
                {
                    process = () =>
                    {
                        double a = ((VariableScalar)A).getDouble();
                        output.value = a == 0 ? 0 : 1;
                    }
                };
            }

            return ret;
        }

        /**
         * Matrix determinant
         */
        public static Info det(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableMatrix)
            {
                ret.op = new Operation("det-m")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        output.value = CommonOps_DDRM.det(mA.matrix);
                    }
                };
            }
            else
            {
                ret.op = new Operation("det-s")
                {
                    process = () =>
                    {
                        VariableScalar mA = (VariableScalar)A;
                        output.value = mA.getDouble();
                    }
                };
            }

            return ret;
        }

        public static Info trace(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableMatrix)
            {
                ret.op = new Operation("trace-m")
                {
                    process = () =>
                    {
                        VariableMatrix mA = (VariableMatrix)A;
                        output.value = CommonOps_DDRM.trace(mA.matrix);
                    }
                };
            }
            else
            {
                ret.op = new Operation("trace-s")
                {
                    process = () =>
                    {
                        VariableScalar mA = (VariableScalar)A;
                        output.value = mA.getDouble();
                    }
                };
            }

            return ret;
        }

        public static Info normF(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableMatrix)
            {
                ret.op = new Operation("normF-m")
                {
                    process = () =>
                    {
                        output.value = NormOps_DDRM.normF(((VariableMatrix)A).matrix);
                    }
                };
            }
            else
            {
                ret.op = new Operation("normF-s")
                {
                    process = () =>
                    {
                        output.value = Math.Abs(((VariableScalar)A).getDouble());
                    }
                };
            }

            return ret;
        }

        public static Info normP(Variable A, Variable P, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (!(A is VariableMatrix) || !(P is VariableScalar))
                throw new InvalidOperationException("normP(A,p) A should be a matrix and p a scalar");

            double valueP = ((VariableScalar) P).getDouble();
            VariableMatrix varA = (VariableMatrix) A;

            ret.op = new Operation("normP")
            {
                process = () =>
                {
                    output.value = NormOps_DDRM.normP(varA.matrix, valueP);
                }
            };

            return ret;
        }

        public static Info max(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("max-m")
                {
                    process = () =>
                    {
                        output.value = CommonOps_DDRM.elementMax(((VariableMatrix)A).matrix);
                    }
                };
            }
            else if (A is VariableInteger)
            {
                VariableInteger output = manager.createInteger();
                ret.output = output;
                ret.op = new Operation("max-i")
                {
                    process = () =>
                    {
                        output.value = ((VariableInteger)A).value;
                    }
                };
            }
            else if (A is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("max-s")
                {
                    process = () =>
                    {
                        output.value = ((VariableDouble)A).getDouble();
                    }
                };
            }

            return ret;
        }

        public static Info max_two(Variable A, Variable P, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (!(A is VariableMatrix) || !(P is VariableScalar))
                throw new InvalidOperationException("max(A,d) A = matrix and d = scalar");

            double valueP = ((VariableScalar) P).getDouble();
            VariableMatrix varA = (VariableMatrix) A;

            if (valueP == 0)
            {
                ret.op = new Operation("max_rows")
                {
                    process = () =>
                    {
                        output.matrix.reshape(varA.matrix.numRows, 1);
                        CommonOps_DDRM.maxRows(varA.matrix, output.matrix);
                    }
                };
            }
            else if (valueP == 1)
            {
                ret.op = new Operation("max_cols")
                {
                    process = () =>
                    {
                        output.matrix.reshape(1, varA.matrix.numCols);
                        CommonOps_DDRM.maxCols(varA.matrix, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("max(A,d) expected d to be 0 for rows or 1 for columns");
            }

            return ret;
        }

        public static Info min(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("min-m")
                {
                    process = () =>
                    {
                        output.value = CommonOps_DDRM.elementMin(((VariableMatrix)A).matrix);
                    }
                };
            }
            else if (A is VariableInteger)
            {
                VariableInteger output = manager.createInteger();
                ret.output = output;
                ret.op = new Operation("min-i")
                {
                    process = () =>
                    {
                        output.value = ((VariableInteger)A).value;
                    }
                };
            }
            else if (A is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("min-s")
                {
                    process = () =>
                    {
                        output.value = ((VariableDouble)A).getDouble();
                    }
                };
            }

            return ret;
        }

        public static Info min_two(Variable A, Variable P, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (!(A is VariableMatrix) || !(P is VariableScalar))
                throw new InvalidOperationException("min(A,d) A = matrix and d = scalar");

            double valueP = ((VariableScalar) P).getDouble();
            VariableMatrix varA = (VariableMatrix) A;

            if (valueP == 0)
            {
                ret.op = new Operation("min_rows")
                {
                    process = () =>
                    {
                        output.matrix.reshape(varA.matrix.numRows, 1);
                        CommonOps_DDRM.minRows(varA.matrix, output.matrix);
                    }
                };
            }
            else if (valueP == 1)
            {
                ret.op = new Operation("min_cols")
                {
                    process = () =>
                    {
                        output.matrix.reshape(1, varA.matrix.numCols);
                        CommonOps_DDRM.minCols(varA.matrix, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("min(A,d) expected d to be 0 for rows or 1 for columns");
            }

            return ret;
        }

        public static Info abs(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("abs-m")
                {
                    process = () =>
                    {
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        output.matrix.reshape(a.numRows, a.numCols);
                        int N = a.getNumElements();
                        for (int i = 0; i < N; i++)
                        {
                            output.matrix.data[i] = Math.Abs(a.data[i]);
                        }
                    }
                };
            }
            else if (A is VariableInteger)
            {
                VariableInteger output = manager.createInteger();
                ret.output = output;
                ret.op = new Operation("abs-i")
                {
                    process = () =>
                    {
                        output.value = Math.Abs(((VariableInteger)A).value);
                    }
                };
            }
            else if (A is VariableScalar)
            {
                VariableDouble output = manager.createDouble();
                ret.output = output;
                ret.op = new Operation("abs-s")
                {
                    process = () =>
                    {
                        output.value = Math.Abs((((VariableDouble)A).getDouble()));
                    }
                };
            }

            return ret;
        }

        /**
         * Returns an identity matrix
         */
        public static Info eye(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (A is VariableMatrix)
            {
                ret.op = new Operation("eye-m")
                {
                    process = () =>
                    {
                        DMatrixRMaj mA = ((VariableMatrix)A).matrix;
                        output.matrix.reshape(mA.numRows,
                        mA.numCols);
                        CommonOps_DDRM.setIdentity(output.matrix);
                    }
                };
            }
            else if (A is VariableInteger)
            {
                ret.op = new Operation("eye-i")
                {
                    process = () =>
                    {
                        int N = ((VariableInteger)A).value;
                        output.matrix.reshape(N, N);
                        CommonOps_DDRM.setIdentity(output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Unsupported variable type " + A);
            }
            return ret;
        }

        public static Info diag(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();

            if (A is VariableMatrix)
            {
                VariableMatrix output = manager.createMatrix();
                ret.output = output;
                ret.op = new Operation("diag-m")
                {
                    process = () =>
                    {
                        DMatrixRMaj mA = ((VariableMatrix)A).matrix;
                        if (MatrixFeatures_DDRM.isVector(mA))
                        {
                            int N = mA.getNumElements();
                            output.matrix.reshape(N,
                            N);
                            CommonOps_DDRM.diag(output.matrix,
                            N,
                            mA.data);
                        }
                        else
                        {
                            int N = Math.Min(mA.numCols, mA.numRows);
                            output.matrix.reshape(N, 1);
                            for (int i = 0; i < N; i++)
                            {
                                output.matrix.data[i] = mA.unsafe_get(i, i);
                            }
                        }
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("diag requires a matrix as input");
            }
            return ret;
        }

        /**
         * Returns a matrix full of zeros
         */
        public static Info zeros(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (A is VariableInteger && B is VariableInteger)
            {
                ret.op = new Operation("zeros-ii")
                {
                    process = () =>
                    {
                        int numRows = ((VariableInteger)A).value;
                        int numCols = ((VariableInteger)B).value;
                        output.matrix.reshape(numRows, numCols);
                        CommonOps_DDRM.fill(output.matrix, 0);
                        //not sure if this is necessary.  Can its value every be modified?
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Expected two integers got " + A + " " + B);
            }

            return ret;
        }

        /**
         * Returns a matrix full of ones
         */
        public static Info ones(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (A is VariableInteger && B is VariableInteger)
            {
                ret.op = new Operation("ones-ii")
                {
                    process = () =>
                    {
                        int numRows = ((VariableInteger)A).value;
                        int numCols = ((VariableInteger)B).value;
                        output.matrix.reshape(numRows, numCols);
                        CommonOps_DDRM.fill(output.matrix, 1);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Expected two integers got " + A + " " + B);
            }

            return ret;
        }

        /**
         * Kronecker product
         */
        public static Info kron(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (A is VariableMatrix && B is VariableMatrix)
            {
                ret.op = new Operation("kron-mm")
                {
                    process = () =>
                    {
                        DMatrixRMaj mA = ((VariableMatrix)A).matrix;
                        DMatrixRMaj mB = ((VariableMatrix)B).matrix;
                        output.matrix.reshape(mA.numRows * mB.numRows, mA.numCols * mB.numCols);
                        CommonOps_DDRM.kron(mA, mB, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Both inputs must be matrices ");
            }

            return ret;
        }

        /**
         * If input is two vectors then it returns the dot product as a double.
         */
        public static Info dot(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (A is VariableMatrix && B is VariableMatrix)
            {
                ret.op = new Operation("dot-mm")
                {
                    process = () =>
                    {
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        DMatrixRMaj b = ((VariableMatrix)B).matrix;
                        if (!MatrixFeatures_DDRM.isVector(a) || !MatrixFeatures_DDRM.isVector(b))
                            throw new InvalidOperationException("Both inputs to dot() must be vectors");
                        output.value = VectorVectorMult_DDRM.innerProd(a, b);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Expected two matrices got " + A + " " + B);
            }

            return ret;
        }

        /**
         * If input is two vectors then it returns the dot product as a double.
         */
        public static Info solve(Variable A, Variable B, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (A is VariableMatrix && B is VariableMatrix)
            {
                ret.op = new Operation("solve-mm")
                {
                    process = () =>
                    {
                        //LinearSolverDense<DMatrixRMaj> solver;
                        DMatrixRMaj a = ((VariableMatrix)A).matrix;
                        DMatrixRMaj b = ((VariableMatrix)B).matrix;

                        //if (solver == null)
                        //{
                            var solver = LinearSolverFactory_DDRM.leastSquares(a.numRows, a.numCols);
                        //}

                        if (!solver.setA(a))
                            throw new InvalidOperationException("Solver failed!");

                        output.matrix.reshape(a.numCols, b.numCols);
                        solver.solve(b, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Expected two matrices got " + A + " " + B);
            }

            return ret;
        }

        public static Info extract(List<Variable> inputs, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (!(inputs[0] is VariableMatrix))
                throw new InvalidOperationException("First parameter must be a matrix.");


            for (int i = 1; i < inputs.Count; i++)
            {
                if (!(inputs[i] is VariableInteger) &&
                    (inputs[i].getType() != VariableType.INTEGER_SEQUENCE))
                    throw new InvalidOperationException("Parameters must be integers, integer list, or array range");
            }

            ret.op = new Operation("extract")
            {
                process = () =>
                {
                    Extents extents = new Extents();

                    ArrayExtent rowExtent = new ArrayExtent();
                    ArrayExtent colExtent = new ArrayExtent();

                    DMatrixRMaj A = ((VariableMatrix)inputs[0]).matrix;

                    if (inputs.Count == 2)
                    {
                        if (extractSimpleExtents(inputs[1], extents, false, A.getNumElements()))
                        {
                            extents.col1 += 1;
                            output.matrix.reshape(1, extents.col1 - extents.col0);
                            Array.Copy(A.data, extents.col0, output.matrix.data, 0, extents.col1 - extents.col0);
                        }
                        else
                        {
                            extractArrayExtent(inputs[1], A.getNumElements(), colExtent);
                            output.matrix.reshape(1, colExtent.length);
                            CommonOps_DDRM.extract(A,
                                colExtent.array, colExtent.length, output.matrix);
                        }
                    }
                    else if (extractSimpleExtents(inputs[1], extents, true, A.numRows) &&
                             extractSimpleExtents(inputs[2], extents, false, A.numCols))
                    {
                        extents.row1 += 1;
                        extents.col1 += 1;
                        output.matrix.reshape(extents.row1 - extents.row0, extents.col1 - extents.col0);
                        CommonOps_DDRM.extract(A, extents.row0, extents.row1, extents.col0, extents.col1, output.matrix, 0, 0);
                    }
                    else
                    {
                        extractArrayExtent(inputs[1], A.numRows, rowExtent);
                        extractArrayExtent(inputs[2], A.numCols, colExtent);

                        output.matrix.reshape(rowExtent.length, colExtent.length);
                        CommonOps_DDRM.extract(A,
                            rowExtent.array, rowExtent.length,
                            colExtent.array, colExtent.length, output.matrix);
                    }
                }
            };

            return ret;
        }

        public static Info sum_one(Variable A, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (!(A is VariableMatrix))
                throw new InvalidOperationException("sum(A) A = matrix");

            VariableMatrix varA = (VariableMatrix) A;

            ret.op = new Operation("sum_all")
            {
                process = () =>
                {
                    output.value = CommonOps_DDRM.elementSum(varA.matrix);
                }
            };

            return ret;
        }

        public static Info sum_two(Variable A, Variable P, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableMatrix output = manager.createMatrix();
            ret.output = output;

            if (!(A is VariableMatrix) || !(P is VariableScalar))
                throw new InvalidOperationException("sum(A,p) A = matrix and p = scalar");

            double valueP = ((VariableScalar) P).getDouble();
            VariableMatrix varA = (VariableMatrix) A;

            if (valueP == 0)
            {
                ret.op = new Operation("sum_rows")
                {
                    process = () =>
                    {
                        output.matrix.reshape(varA.matrix.numRows, 1);
                        CommonOps_DDRM.sumRows(varA.matrix, output.matrix);
                    }
                };
            }
            else if (valueP == 1)
            {
                ret.op = new Operation("sum_cols")
                {
                    process = () =>
                    {
                        output.matrix.reshape(1, varA.matrix.numCols);
                        CommonOps_DDRM.sumCols(varA.matrix, output.matrix);
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("sum(A,d) expected d to be 0 for rows or 1 for columns");
            }

            return ret;
        }

        public static Info extractScalar(List<Variable> inputs, ManagerTempVariables manager)
        {
            Info ret = new Info();
            VariableDouble output = manager.createDouble();
            ret.output = output;

            if (!(inputs[0] is VariableMatrix))
                throw new InvalidOperationException("First parameter must be a matrix.");

            for (int i = 1; i < inputs.Count; i++)
            {
                if (!(inputs[i] is VariableInteger))
                    throw new InvalidOperationException("Parameters must be integers for extract scalar");
            }

            ret.op = new Operation("extractScalar")
            {
                process = () =>
                {
                    DMatrixRMaj A = ((VariableMatrix)inputs[0]).matrix;
                    if (inputs.Count == 2)
                    {
                        int index = ((VariableInteger)inputs[1]).value;
                        output.value = A.get(index);
                    }
                    else
                    {
                        int row = ((VariableInteger)inputs[1]).value;
                        int col = ((VariableInteger)inputs[2]).value;
                        output.value = A.get(row, col);
                    }
                }
            };

            return ret;
        }

        /**
         * See if a simple sequence can be used to extract the array.  A simple extent is a continuous block from
         * a min to max index
         *
         * @return true if it is a simple range or false if not
         */
        private static bool extractSimpleExtents(Variable var, Extents e, bool row, int length)
        {
            int lower;
            int upper;
            if (var.getType() == VariableType.INTEGER_SEQUENCE)
            {
                IntegerSequence sequence = ((VariableIntegerSequence) var).sequence;
                if (sequence.getType() == IntegerSequenceType.FOR)
                {
                    For seqFor = (For) sequence;
                    seqFor.initialize(length);
                    if (seqFor.getStep() == 1)
                    {
                        lower = seqFor.getStart();
                        upper = seqFor.getEnd();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (var.getType() == VariableType.SCALAR)
            {
                lower = upper = ((VariableInteger) var).value;
            }
            else
            {
                throw new InvalidOperationException("How did a bad variable get put here?!?!");
            }
            if (row)
            {
                e.row0 = lower;
                e.row1 = upper;
            }
            else
            {
                e.col0 = lower;
                e.col1 = upper;
            }
            return true;
        }

        private static void extractArrayExtent(Variable var, int length, ArrayExtent extent)
        {
            if (var.getType() == VariableType.INTEGER_SEQUENCE)
            {
                IntegerSequence sequence = ((VariableIntegerSequence) var).sequence;
                sequence.initialize(length - 1);
                extent.setLength(sequence.length());
                int index = 0;
                while (sequence.hasNext())
                {
                    extent.array[index++] = sequence.next();
                }
            }
            else if (var.getType() == VariableType.SCALAR)
            {
                extent.setLength(1);
                extent.array[0] = ((VariableInteger) var).value;
            }
            else
            {
                throw new InvalidOperationException("How did a bad variable get put here?!?!");
            }
        }

        public static Info matrixConstructor(MatrixConstructor m)
        {
            Info ret = new Info
            {
                output = m.getOutput(),
                op = new Operation("matrixConstructor")
                {
                    process = m.construct
                }
            };

            return ret;
        }
    }
}