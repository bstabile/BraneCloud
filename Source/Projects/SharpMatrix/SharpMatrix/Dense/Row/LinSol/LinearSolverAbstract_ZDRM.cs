using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Interfaces.LinSol;

namespace SharpMatrix.Dense.Row.LinSol
{
    //package org.ejml.dense.row.linsol;

/**
 * <p>
 * An abstract class that provides some common functionality and a default implementation
 * of invert that uses the solve function of the child class.
 * </p>
 *
 * <p>
 * The extending class must explicity call {@link #_setA(ZMatrixRMaj)}
 * inside of its {@link #setA} function.
 * </p>
 * 
 * @author Peter Abeles
 */
    public abstract class LinearSolverAbstract_ZDRM : LinearSolverDense<ZMatrixRMaj>
    {

        protected ZMatrixRMaj A;
        protected int numRows;
        protected int numCols;
        protected int stride;

        public ZMatrixRMaj getA()
        {
            return A;
        }

        protected void _setA(ZMatrixRMaj A)
        {
            this.A = A;
            this.numRows = A.numRows;
            this.numCols = A.numCols;
            this.stride = numCols * 2;
        }

        //@Override
        public virtual void invert(ZMatrixRMaj A_inv)
        {
            InvertUsingSolve_ZDRM.invert(this, A, A_inv);
        }

        public abstract bool setA(ZMatrixRMaj A);
        public abstract double quality();
        public abstract void solve(ZMatrixRMaj B, ZMatrixRMaj X);
        public abstract bool modifiesA();
        public abstract bool modifiesB();
        public abstract DecompositionInterface<ZMatrixRMaj> getDecomposition();
    }
}