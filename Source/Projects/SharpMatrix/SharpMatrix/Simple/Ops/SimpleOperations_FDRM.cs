using System;
using System.IO;
using SharpMatrix.Data;
using SharpMatrix.Dense;
using SharpMatrix.Dense.Row;
using SharpMatrix.Ops;

namespace SharpMatrix.Simple.Ops
{
    //package org.ejml.simple.ops;

/**
 * @author Peter Abeles
 */
    public class SimpleOperations_FDRM : SimpleOperations<float, FMatrixRMaj>
    {
        public void transpose(FMatrixRMaj input, FMatrixRMaj output)
        {
            CommonOps_FDRM.transpose(input, output);
        }

        public void mult(FMatrixRMaj A, FMatrixRMaj B, FMatrixRMaj output)
        {
            CommonOps_FDRM.mult(A, B, output);
        }

        public void kron(FMatrixRMaj A, FMatrixRMaj B, FMatrixRMaj output)
        {
            CommonOps_FDRM.kron(A, B, output);
        }

        public void plus(FMatrixRMaj A, FMatrixRMaj B, FMatrixRMaj output)
        {
            CommonOps_FDRM.add(A, B, output);
        }

        public void minus(FMatrixRMaj A, FMatrixRMaj B, FMatrixRMaj output)
        {
            CommonOps_FDRM.subtract(A, B, output);
        }

        public void minus(FMatrixRMaj A, /**/float b, FMatrixRMaj output)
        {
            CommonOps_FDRM.subtract(A, b, output);
        }

        public void plus(FMatrixRMaj A, /**/float b, FMatrixRMaj output)
        {
            CommonOps_FDRM.add(A, b, output);
        }

        public void plus(FMatrixRMaj A, /**/float beta, FMatrixRMaj b, FMatrixRMaj output)
        {
            CommonOps_FDRM.add(A, beta, b, output);
        }

        public /**/ float dot(FMatrixRMaj A, FMatrixRMaj v)
        {
            return VectorVectorMult_FDRM.innerProd(A, v);
        }

        public void scale(FMatrixRMaj A, /**/float val, FMatrixRMaj output)
        {
            CommonOps_FDRM.scale(val, A, output);
        }

        public void divide(FMatrixRMaj A, /**/float val, FMatrixRMaj output)
        {
            CommonOps_FDRM.divide(A, val, output);
        }

        public bool invert(FMatrixRMaj A, FMatrixRMaj output)
        {
            return CommonOps_FDRM.invert(A, output);
        }

        public void pseudoInverse(FMatrixRMaj A, FMatrixRMaj output)
        {
            CommonOps_FDRM.pinv(A, output);
        }

        public bool solve(FMatrixRMaj A, FMatrixRMaj X, FMatrixRMaj B)
        {
            return CommonOps_FDRM.solve(A, B, X);
        }

        public void set(FMatrixRMaj A, /**/float val)
        {
            CommonOps_FDRM.fill(A, val);
        }

        public void zero(FMatrixRMaj A)
        {
            A.zero();
        }

        public /**/ float normF(FMatrixRMaj A)
        {
            return NormOps_FDRM.normF(A);
        }

        public /**/ float conditionP2(FMatrixRMaj A)
        {
            return NormOps_FDRM.conditionP2(A);
        }

        public /**/ float determinant(FMatrixRMaj A)
        {
            return CommonOps_FDRM.det(A);
        }

        public /**/ float trace(FMatrixRMaj A)
        {
            return CommonOps_FDRM.trace(A);
        }

        public void setRow(FMatrixRMaj A, int row, int startColumn, /**/float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                A.set(row, startColumn + i, values[i]);
            }
        }

        public void setColumn(FMatrixRMaj A, int column, int startRow, /**/float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                A.set(startRow + i, column, values[i]);
            }
        }

        public void extract(FMatrixRMaj src, int srcY0, int srcY1, int srcX0, int srcX1, FMatrixRMaj dst, int dstY0,
            int dstX0)
        {
            CommonOps_FDRM.extract(src, srcY0, srcY1, srcX0, srcX1, dst, dstY0, dstX0);
        }

        public bool hasUncountable(FMatrixRMaj M)
        {
            return MatrixFeatures_FDRM.hasUncountable(M);
        }

        public void changeSign(FMatrixRMaj a)
        {
            CommonOps_FDRM.changeSign(a);
        }

        public /**/ float elementMaxAbs(FMatrixRMaj A)
        {
            return CommonOps_FDRM.elementMaxAbs(A);
        }

        public /**/ float elementSum(FMatrixRMaj A)
        {
            return CommonOps_FDRM.elementSum(A);
        }

        public void elementMult(FMatrixRMaj A, FMatrixRMaj B, FMatrixRMaj output)
        {
            CommonOps_FDRM.elementMult(A, B, output);
        }

        public void elementDiv(FMatrixRMaj A, FMatrixRMaj B, FMatrixRMaj output)
        {
            CommonOps_FDRM.elementDiv(A, B, output);
        }

        public void elementPower(FMatrixRMaj A, FMatrixRMaj B, FMatrixRMaj output)
        {
            CommonOps_FDRM.elementPower(A, B, output);

        }

        public void elementPower(FMatrixRMaj A, /**/float b, FMatrixRMaj output)
        {
            CommonOps_FDRM.elementPower(A, b, output);
        }

        public void elementExp(FMatrixRMaj A, FMatrixRMaj output)
        {
            CommonOps_FDRM.elementExp(A, output);
        }

        public void elementLog(FMatrixRMaj A, FMatrixRMaj output)
        {
            CommonOps_FDRM.elementLog(A, output);
        }

        public void print(Stream output, Matrix mat)
        {
            MatrixIO.print(output, (FMatrixRMaj) mat);
        }

    }
}