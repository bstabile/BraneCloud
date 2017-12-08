using SharpMatrix.Data;

namespace SharpMatrix.Dense.Row.Decomposition.Eig
{
    //package org.ejml.dense.row.decomposition.eig;

/**
 * @author Peter Abeles
 */
    public interface EigenvalueExtractor_FDRM
    {

        bool process(FMatrixRMaj A);

        int getNumberOfEigenvalues();

        Complex_F32[] getEigenvalues();
    }
}