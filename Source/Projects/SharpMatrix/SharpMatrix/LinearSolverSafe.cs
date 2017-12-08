using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Interfaces.LinSol;

namespace SharpMatrix
{
    //package org.ejml;

/**
 * Ensures that any linear solver it is wrapped around will never modify
 * the input matrices.
 *
 * @author Peter Abeles
 */
//@SuppressWarnings({"unchecked"})
    public class LinearSolverSafe<T> : LinearSolverDense<T> where T : ReshapeMatrix
    {

        // the solver it is wrapped around
        private LinearSolverDense<T> alg;

        // local copies of input matrices that can be modified.
        private T A;

        private T B;

        /**
         *
         * @param alg The solver it is wrapped around.
         */
        public LinearSolverSafe(LinearSolverDense<T> alg)
        {
            this.alg = alg;
        }

        public virtual bool setA(T A)
        {

            if (alg.modifiesA())
            {
                if (this.A == null)
                {
                    this.A = (T) A.copy();
                }
                else
                {
                    if (this.A.getNumRows() != A.getNumRows() || this.A.getNumCols() != A.getNumCols())
                    {
                        this.A.reshape(A.getNumRows(), A.getNumCols());
                    }
                    this.A.set(A);
                }
                return alg.setA(this.A);
            }

            return alg.setA(A);
        }

        public virtual /**/ double quality()
        {
            return alg.quality();
        }

        public virtual void solve(T B, T X)
        {
            if (alg.modifiesB())
            {
                if (this.B == null)
                {
                    this.B = (T) B.copy();
                }
                else
                {
                    if (this.B.getNumRows() != B.getNumRows() || this.B.getNumCols() != B.getNumCols())
                    {
                        this.B.reshape(A.getNumRows(), B.getNumCols());
                    }
                    this.B.set(B);
                }
                B = this.B;
            }

            alg.solve(B, X);
        }

        public virtual void invert(T A_inv)
        {
            alg.invert(A_inv);
        }

        public virtual bool modifiesA()
        {
            return false;
        }

        public virtual bool modifiesB()
        {
            return false;
        }

        public virtual DecompositionInterface<T> getDecomposition()
        {
            return alg.getDecomposition();
        }
    }
}