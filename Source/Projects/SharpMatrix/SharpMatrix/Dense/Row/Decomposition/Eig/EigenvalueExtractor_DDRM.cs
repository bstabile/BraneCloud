using SharpMatrix.Data;

namespace SharpMatrix.Dense.Row.Decomposition.Eig
{
    //package org.ejml.dense.row.decomposition.eig;

/**
 * @author Peter Abeles
 */
    public interface EigenvalueExtractor_DDRM
    {

        bool process(DMatrixRMaj A);

        int getNumberOfEigenvalues();

        Complex_F64[] getEigenvalues();
    }
}