using System;
using System.IO;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Simple.Ops
{
    //package org.ejml.simple.ops;

/**
 * @author Peter Abeles
 */
    public class SimpleOperations_CDRM : SimpleOperations<CMatrixRMaj>
    {
        //@Override
        public void transpose(CMatrixRMaj input, CMatrixRMaj output)
        {
            CommonOps_CDRM.transpose(input, output);
        }

        //@Override
        public void mult(CMatrixRMaj A, CMatrixRMaj B, CMatrixRMaj output)
        {
            CommonOps_CDRM.mult(A, B, output);
        }

        //@Override
        public void kron(CMatrixRMaj A, CMatrixRMaj B, CMatrixRMaj output)
        {
//        CommonOps_CDRM.kron(A,B,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void plus(CMatrixRMaj A, CMatrixRMaj B, CMatrixRMaj output)
        {
            CommonOps_CDRM.add(A, B, output);
        }

        //@Override
        public void minus(CMatrixRMaj A, CMatrixRMaj B, CMatrixRMaj output)
        {
            CommonOps_CDRM.subtract(A, B, output);
        }

        //@Override
        public void minus(CMatrixRMaj A, /**/double b, CMatrixRMaj output)
        {
//        CommonOps_CDRM.subtract(A, (float)b, output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void plus(CMatrixRMaj A, /**/double b, CMatrixRMaj output)
        {
//        CommonOps_CDRM.add(A, (float)b, output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void plus(CMatrixRMaj A, /**/double beta, CMatrixRMaj b, CMatrixRMaj output)
        {
//        CommonOps_CDRM.add(A, (float)beta, b, output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public /**/ double dot(CMatrixRMaj A, CMatrixRMaj v)
        {
//        return VectorVectorMult_FDRM.innerProd(A, v);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void scale(CMatrixRMaj A, /**/double val, CMatrixRMaj output)
        {
//        CommonOps_CDRM.scale( (float)val, 0,A,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void divide(CMatrixRMaj A, /**/double val, CMatrixRMaj output)
        {
//        CommonOps_CDRM.divide( A, (float)val,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public bool invert(CMatrixRMaj A, CMatrixRMaj output)
        {
            return CommonOps_CDRM.invert(A, output);
        }

        //@Override
        public void pseudoInverse(CMatrixRMaj A, CMatrixRMaj output)
        {
//        CommonOps_CDRM.pinv(A,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public bool solve(CMatrixRMaj A, CMatrixRMaj X, CMatrixRMaj B)
        {
            return CommonOps_CDRM.solve(A, B, X);
        }

        //@Override
        public void set(CMatrixRMaj A, /**/double val)
        {
            CommonOps_CDRM.fill(A, (float) val, 0);
        }

        //@Override
        public void zero(CMatrixRMaj A)
        {
            A.zero();
        }

        //@Override
        public /**/ double normF(CMatrixRMaj A)
        {
            return NormOps_CDRM.normF(A);
        }

        //@Override
        public /**/ double conditionP2(CMatrixRMaj A)
        {
//        return NormOps_CDRM.conditionP2(A);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public /**/ double determinant(CMatrixRMaj A)
        {
            return CommonOps_CDRM.det(A).real;
        }

        //@Override
        public /**/ double trace(CMatrixRMaj A)
        {
//        return CommonOps_CDRM.trace(A);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void setRow(CMatrixRMaj A, int row, int startColumn, /**/double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                A.set(row, startColumn + i, (float) values[i], 0);
            }
        }

        //@Override
        public void setColumn(CMatrixRMaj A, int column, int startRow, /**/double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                A.set(startRow + i, column, (float) values[i], 0);
            }
        }

        //@Override
        public void extract(CMatrixRMaj src, int srcY0, int srcY1, int srcX0, int srcX1, CMatrixRMaj dst, int dstY0,
            int dstX0)
        {
            CommonOps_CDRM.extract(src, srcY0, srcY1, srcX0, srcX1, dst, dstY0, dstX0);
        }

        //@Override
        public bool hasUncountable(CMatrixRMaj M)
        {
            return MatrixFeatures_CDRM.hasUncountable(M);
        }

        //@Override
        public void changeSign(CMatrixRMaj a)
        {
//        CommonOps_CDRM.changeSign(a);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public /**/ double elementMaxAbs(CMatrixRMaj A)
        {
            return CommonOps_CDRM.elementMaxAbs(A);
        }

        //@Override
        public /**/ double elementSum(CMatrixRMaj A)
        {
//        return CommonOps_CDRM.elementSum(A);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementMult(CMatrixRMaj A, CMatrixRMaj B, CMatrixRMaj output)
        {
//        CommonOps_CDRM.elementMult(A,B,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementDiv(CMatrixRMaj A, CMatrixRMaj B, CMatrixRMaj output)
        {
//        CommonOps_CDRM.elementDiv(A,B,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementPower(CMatrixRMaj A, CMatrixRMaj B, CMatrixRMaj output)
        {
//        CommonOps_CDRM.elementPower(A,B,output);
            throw new InvalidOperationException("Unsupported");

        }

        //@Override
        public void elementPower(CMatrixRMaj A, /**/double b, CMatrixRMaj output)
        {
//        CommonOps_CDRM.elementPower(A, (float)b, output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementExp(CMatrixRMaj A, CMatrixRMaj output)
        {
//        CommonOps_CDRM.elementExp(A,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void elementLog(CMatrixRMaj A, CMatrixRMaj output)
        {
//        CommonOps_CDRM.elementLog(A,output);
            throw new InvalidOperationException("Unsupported");
        }

        //@Override
        public void print(Stream output, Matrix mat)
        {
            MatrixIO.print(output, (CMatrixRMaj) mat);
        }
    }
}