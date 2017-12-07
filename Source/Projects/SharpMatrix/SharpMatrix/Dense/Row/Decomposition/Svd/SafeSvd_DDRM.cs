using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.Decomposition.Svd
{
    //package org.ejml.dense.row.decomposition.svd;

/**
 * Wraps around a {@link SingularValueDecomposition} and ensures that the input is not modified.
 *
 * @author Peter Abeles
 */
    public class SafeSvd_DDRM : SingularValueDecomposition_F64<DMatrixRMaj>
    {
        // the decomposition algorithm
        SingularValueDecomposition_F64<DMatrixRMaj> alg;

        // storage for the input if it would be modified
        DMatrixRMaj work = new DMatrixRMaj(1, 1);

        public SafeSvd_DDRM(SingularValueDecomposition_F64<DMatrixRMaj> alg)
        {
            this.alg = alg;
        }

        public virtual double[] getSingularValues()
        {
            return alg.getSingularValues();
        }

        public virtual int numberOfSingularValues()
        {
            return alg.numberOfSingularValues();
        }

        public virtual bool isCompact()
        {
            return alg.isCompact();
        }

        public virtual DMatrixRMaj getU(DMatrixRMaj U, bool transposed)
        {
            return alg.getU(U, transposed);
        }

        public virtual DMatrixRMaj getV(DMatrixRMaj V, bool transposed)
        {
            return alg.getV(V, transposed);
        }

        public virtual DMatrixRMaj getW(DMatrixRMaj W)
        {
            return alg.getW(W);
        }

        public virtual int numRows()
        {
            return alg.numRows();
        }

        public virtual int numCols()
        {
            return alg.numCols();
        }

        public virtual bool decompose(DMatrixRMaj orig)
        {
            if (alg.inputModified())
            {
                work.reshape(orig.numRows, orig.numCols);
                work.set(orig);
                return alg.decompose(work);
            }
            else
            {
                return alg.decompose(orig);
            }
        }

        public virtual bool inputModified()
        {
            return false;
        }
    }
}