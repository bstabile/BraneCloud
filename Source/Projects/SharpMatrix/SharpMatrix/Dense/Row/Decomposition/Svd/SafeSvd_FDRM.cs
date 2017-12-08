using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.Svd
{
    //package org.ejml.dense.row.decomposition.svd;

/**
 * Wraps around a {@link SingularValueDecomposition} and ensures that the input is not modified.
 *
 * @author Peter Abeles
 */
    public class SafeSvd_FDRM : SingularValueDecomposition_F32<FMatrixRMaj>
    {
        // the decomposition algorithm
        SingularValueDecomposition_F32<FMatrixRMaj> alg;

        // storage for the input if it would be modified
        FMatrixRMaj work = new FMatrixRMaj(1, 1);

        public SafeSvd_FDRM(SingularValueDecomposition_F32<FMatrixRMaj> alg)
        {
            this.alg = alg;
        }

        //@Override
        public float[] getSingularValues()
        {
            return alg.getSingularValues();
        }

        //@Override
        public int numberOfSingularValues()
        {
            return alg.numberOfSingularValues();
        }

        //@Override
        public bool isCompact()
        {
            return alg.isCompact();
        }

        //@Override
        public FMatrixRMaj getU(FMatrixRMaj U, bool transposed)
        {
            return alg.getU(U, transposed);
        }

        //@Override
        public FMatrixRMaj getV(FMatrixRMaj V, bool transposed)
        {
            return alg.getV(V, transposed);
        }

        //@Override
        public FMatrixRMaj getW(FMatrixRMaj W)
        {
            return alg.getW(W);
        }

        //@Override
        public int numRows()
        {
            return alg.numRows();
        }

        //@Override
        public int numCols()
        {
            return alg.numCols();
        }

        //@Override
        public bool decompose(FMatrixRMaj orig)
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

        //@Override
        public bool inputModified()
        {
            return false;
        }
    }
}