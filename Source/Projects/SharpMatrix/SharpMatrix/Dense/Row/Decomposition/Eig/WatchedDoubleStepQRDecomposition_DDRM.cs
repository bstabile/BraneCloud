using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.Eig.Watched;
using SharpMatrix.Dense.Row.Decomposition.Hessenberg;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Dense.Row.Decomposition.Eig
{
    //package org.ejml.dense.row.decomposition.eig;

/**
 * <p>
 * Finds the eigenvalue decomposition of an arbitrary square matrix using the implicit double-step QR algorithm.
 * Watched is included in its name because it is designed to print out internal debugging information.  This
 * class is still underdevelopment and has yet to be optimized.
 * </p>
 *
 * <p>
 * Based off the description found in:<br>
 * David S. Watkins, "Fundamentals of Matrix Computations." Second Edition.
 * </p>
 *
 * @author Peter Abeles
 */
//TODO looks like there might be some pointless copying of arrays going on
    public class WatchedDoubleStepQRDecomposition_DDRM
        : EigenDecomposition_F64<DMatrixRMaj>
    {

        HessenbergSimilarDecomposition_DDRM hessenberg;
        WatchedDoubleStepQREigenvalue_DDRM algValue;
        WatchedDoubleStepQREigenvector_DDRM algVector;

        DMatrixRMaj H;

        // should it compute eigenvectors or just eigenvalues
        bool computeVectors;

        public WatchedDoubleStepQRDecomposition_DDRM(bool computeVectors)
        {
            hessenberg = new HessenbergSimilarDecomposition_DDRM(10);
            algValue = new WatchedDoubleStepQREigenvalue_DDRM();
            algVector = new WatchedDoubleStepQREigenvector_DDRM();

            this.computeVectors = computeVectors;
        }

        public virtual bool decompose(DMatrixRMaj A)
        {

            if (!hessenberg.decompose(A))
                return false;

            H = hessenberg.getH(null);

            algValue.getImplicitQR().createR = false;
//        algValue.getImplicitQR().setChecks(true,true,true);

            if (!algValue.process(H))
                return false;

//        for( int i = 0; i < A.numRows; i++ ) {
//            Console.WriteLine(algValue.getEigenvalues()[i]);
//        }

            algValue.getImplicitQR().createR = true;

            if (computeVectors)
                return algVector.process(algValue.getImplicitQR(), H, hessenberg.getQ(null));
            else
                return true;
        }

        public virtual bool inputModified()
        {
            return hessenberg.inputModified();
        }

        public virtual int getNumberOfEigenvalues()
        {
            return algValue.getEigenvalues().Length;
        }

        public virtual Complex_F64 getEigenvalue(int index)
        {
            return algValue.getEigenvalues()[index];
        }

        public virtual DMatrixRMaj getEigenVector(int index)
        {
            return algVector.getEigenvectors()[index];
        }
    }
}