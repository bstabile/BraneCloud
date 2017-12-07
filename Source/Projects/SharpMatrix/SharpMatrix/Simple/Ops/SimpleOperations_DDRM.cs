using System;
using System.IO;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Dense;
using BraneCloud.Evolution.EC.MatrixLib.Dense.Row;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Simple.Ops
{
    /**
     * @author Peter Abeles
     */
    public class SimpleOperations_DDRM : SimpleOperations<DMatrixRMaj>
    {

        public virtual void transpose(DMatrixRMaj input, DMatrixRMaj output)
        {
            CommonOps_DDRM.transpose(input, output);
        }

        public virtual void mult(DMatrixRMaj A, DMatrixRMaj B, DMatrixRMaj output)
        {
            CommonOps_DDRM.mult(A, B, output);
        }

        public virtual void kron(DMatrixRMaj A, DMatrixRMaj B, DMatrixRMaj output)
        {
            CommonOps_DDRM.kron(A, B, output);
        }

        public virtual void plus(DMatrixRMaj A, DMatrixRMaj B, DMatrixRMaj output)
        {
            CommonOps_DDRM.add(A, B, output);
        }

        public virtual void minus(DMatrixRMaj A, DMatrixRMaj B, DMatrixRMaj output)
        {
            CommonOps_DDRM.subtract(A, B, output);
        }

        public virtual void minus(DMatrixRMaj A, /**/double b, DMatrixRMaj output)
        {
            CommonOps_DDRM.subtract(A, (double) b, output);
        }

        public virtual void plus(DMatrixRMaj A, /**/double b, DMatrixRMaj output)
        {
            CommonOps_DDRM.add(A, (double) b, output);
        }

        public virtual void plus(DMatrixRMaj A, /**/double beta, DMatrixRMaj b, DMatrixRMaj output)
        {
            CommonOps_DDRM.add(A, (double) beta, b, output);
        }

        public virtual double dot(DMatrixRMaj A, DMatrixRMaj v)
        {
            return VectorVectorMult_DDRM.innerProd(A, v);
        }

        public virtual void scale(DMatrixRMaj A, /**/double val, DMatrixRMaj output)
        {
            CommonOps_DDRM.scale((double) val, A, output);
        }

        public virtual void divide(DMatrixRMaj A, /**/double val, DMatrixRMaj output)
        {
            CommonOps_DDRM.divide(A, (double) val, output);
        }

        public virtual bool invert(DMatrixRMaj A, DMatrixRMaj output)
        {
            return CommonOps_DDRM.invert(A, output);
        }

        public virtual void pseudoInverse(DMatrixRMaj A, DMatrixRMaj output)
        {
            CommonOps_DDRM.pinv(A, output);
        }

        public virtual bool solve(DMatrixRMaj A, DMatrixRMaj X, DMatrixRMaj B)
        {
            return CommonOps_DDRM.solve(A, B, X);
        }

        public virtual void set(DMatrixRMaj A, /**/double val)
        {
            CommonOps_DDRM.fill(A, (double) val);
        }

        public virtual void zero(DMatrixRMaj A)
        {
            A.zero();
        }

        public virtual double normF(DMatrixRMaj A)
        {
            return NormOps_DDRM.normF(A);
        }

        public virtual double conditionP2(DMatrixRMaj A)
        {
            return NormOps_DDRM.conditionP2(A);
        }

        public virtual double determinant(DMatrixRMaj A)
        {
            return CommonOps_DDRM.det(A);
        }

        public virtual double trace(DMatrixRMaj A)
        {
            return CommonOps_DDRM.trace(A);
        }

        public virtual void setRow(DMatrixRMaj A, int row, int startColumn, double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                A.set(row, startColumn + i, (double) values[i]);
            }
        }

        public virtual void setColumn(DMatrixRMaj A, int column, int startRow, /**/double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                A.set(startRow + i, column, (double) values[i]);
            }
        }

        public virtual void extract(DMatrixRMaj src, int srcY0, int srcY1, int srcX0, int srcX1, DMatrixRMaj dst,
            int dstY0, int dstX0)
        {
            CommonOps_DDRM.extract(src, srcY0, srcY1, srcX0, srcX1, dst, dstY0, dstX0);
        }

        public virtual bool hasUncountable(DMatrixRMaj M)
        {
            return MatrixFeatures_DDRM.hasUncountable(M);
        }

        public virtual void changeSign(DMatrixRMaj a)
        {
            CommonOps_DDRM.changeSign(a);
        }

        public virtual double elementMaxAbs(DMatrixRMaj A)
        {
            return CommonOps_DDRM.elementMaxAbs(A);
        }

        public virtual double elementSum(DMatrixRMaj A)
        {
            return CommonOps_DDRM.elementSum(A);
        }

        public virtual void elementMult(DMatrixRMaj A, DMatrixRMaj B, DMatrixRMaj output)
        {
            CommonOps_DDRM.elementMult(A, B, output);
        }

        public virtual void elementDiv(DMatrixRMaj A, DMatrixRMaj B, DMatrixRMaj output)
        {
            CommonOps_DDRM.elementDiv(A, B, output);
        }

        public virtual void elementPower(DMatrixRMaj A, DMatrixRMaj B, DMatrixRMaj output)
        {
            CommonOps_DDRM.elementPower(A, B, output);

        }

        public virtual void elementPower(DMatrixRMaj A, /**/double b, DMatrixRMaj output)
        {
            CommonOps_DDRM.elementPower(A, (double) b, output);
        }

        public virtual void elementExp(DMatrixRMaj A, DMatrixRMaj output)
        {
            CommonOps_DDRM.elementExp(A, output);
        }

        public virtual void elementLog(DMatrixRMaj A, DMatrixRMaj output)
        {
            CommonOps_DDRM.elementLog(A, output);
        }

        public virtual void print(Stream output, Matrix mat)
        {
            MatrixIO.print(output, (DMatrixRMaj) mat);
        }
    }
}