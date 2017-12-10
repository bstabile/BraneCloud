using System;
using System.IO;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;
using SharpMatrix.Ops;

namespace SharpMatrix.Simple.Ops
{
    //package org.ejml.simple.ops;

/**
 * @author Peter Abeles
 */
    public class SimpleOperations_ZDRM : SimpleOperations<double, ZMatrixRMaj>
    {
        //@Override
        public void transpose(ZMatrixRMaj input, ZMatrixRMaj output)
        {
            CommonOps_ZDRM.transpose(input, output);
        }

        //@Override
        public void mult(ZMatrixRMaj A, ZMatrixRMaj B, ZMatrixRMaj output)
        {
            CommonOps_ZDRM.mult(A, B, output);
        }

        //@Override
        public void kron(ZMatrixRMaj A, ZMatrixRMaj B, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.kron(A,B,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void plus(ZMatrixRMaj A, ZMatrixRMaj B, ZMatrixRMaj output)
        {
            CommonOps_ZDRM.add(A, B, output);
        }

        //@Override
        public void minus(ZMatrixRMaj A, ZMatrixRMaj B, ZMatrixRMaj output)
        {
            CommonOps_ZDRM.subtract(A, B, output);
        }

        //@Override
        public void minus(ZMatrixRMaj A, /**/double b, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.subtract(A, (double)b, output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void plus(ZMatrixRMaj A, /**/double b, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.add(A, (double)b, output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void plus(ZMatrixRMaj A, /**/double beta, ZMatrixRMaj b, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.add(A, (double)beta, b, output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public /**/ double dot(ZMatrixRMaj A, ZMatrixRMaj v)
        {
//        return VectorVectorMult_DDRM.innerProd(A, v);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void scale(ZMatrixRMaj A, /**/double val, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.scale( (double)val, 0,A,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void divide(ZMatrixRMaj A, /**/double val, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.divide( A, (double)val,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public bool invert(ZMatrixRMaj A, ZMatrixRMaj output)
        {
            return CommonOps_ZDRM.invert(A, output);
        }

        //@Override
        public void pseudoInverse(ZMatrixRMaj A, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.pinv(A,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public bool solve(ZMatrixRMaj A, ZMatrixRMaj X, ZMatrixRMaj B)
        {
            return CommonOps_ZDRM.solve(A, B, X);
        }

        //@Override
        public void set(ZMatrixRMaj A, /**/double val)
        {
            CommonOps_ZDRM.fill(A, (double) val, 0);
        }

        //@Override
        public void zero(ZMatrixRMaj A)
        {
            A.zero();
        }

        //@Override
        public /**/ double normF(ZMatrixRMaj A)
        {
            return NormOps_ZDRM.normF(A);
        }

        //@Override
        public /**/ double conditionP2(ZMatrixRMaj A)
        {
//        return NormOps_ZDRM.conditionP2(A);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public /**/ double determinant(ZMatrixRMaj A)
        {
            return CommonOps_ZDRM.det(A).real;
        }

        //@Override
        public /**/ double trace(ZMatrixRMaj A)
        {
//        return CommonOps_ZDRM.trace(A);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void setRow(ZMatrixRMaj A, int row, int startColumn, /**/double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                A.set(row, startColumn + i, (double) values[i], 0);
            }
        }

        //@Override
        public void setColumn(ZMatrixRMaj A, int column, int startRow, /**/double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                A.set(startRow + i, column, (double) values[i], 0);
            }
        }

        //@Override
        public void extract(ZMatrixRMaj src, int srcY0, int srcY1, int srcX0, int srcX1, ZMatrixRMaj dst, int dstY0,
            int dstX0)
        {
            CommonOps_ZDRM.extract(src, srcY0, srcY1, srcX0, srcX1, dst, dstY0, dstX0);
        }

        //@Override
        public bool hasUncountable(ZMatrixRMaj M)
        {
            return MatrixFeatures_ZDRM.hasUncountable(M);
        }

        //@Override
        public void changeSign(ZMatrixRMaj a)
        {
//        CommonOps_ZDRM.changeSign(a);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public /**/ double elementMaxAbs(ZMatrixRMaj A)
        {
            return CommonOps_ZDRM.elementMaxAbs(A);
        }

        //@Override
        public /**/ double elementSum(ZMatrixRMaj A)
        {
//        return CommonOps_ZDRM.elementSum(A);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementMult(ZMatrixRMaj A, ZMatrixRMaj B, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.elementMult(A,B,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementDiv(ZMatrixRMaj A, ZMatrixRMaj B, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.elementDiv(A,B,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementPower(ZMatrixRMaj A, ZMatrixRMaj B, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.elementPower(A,B,output);
            throw new InvalidOperationException("Unsupported");

        }

        //@Override
        public void elementPower(ZMatrixRMaj A, /**/double b, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.elementPower(A, (double)b, output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementExp(ZMatrixRMaj A, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.elementExp(A,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementLog(ZMatrixRMaj A, ZMatrixRMaj output)
        {
//        CommonOps_ZDRM.elementLog(A,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void print(Stream output, Matrix mat)
        {
            MatrixIO.print(output, (ZMatrixRMaj) mat);
        }


    }
}