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
 * The extending class must explicity call {@link #_setA(FMatrixRMaj)}
 * inside of its {@link #setA} function.
 * </p>
 * 
 * @author Peter Abeles
 */
    public abstract class LinearSolverAbstract_FDRM : LinearSolverDense<FMatrixRMaj>
    {

        protected FMatrixRMaj A;
        protected int numRows;
        protected int numCols;

        public virtual FMatrixRMaj getA()
        {
            return A;
        }

        protected void _setA(FMatrixRMaj A)
        {
            this.A = A;
            this.numRows = A.numRows;
            this.numCols = A.numCols;
        }

        public virtual void invert(FMatrixRMaj A_inv)
        {
            InvertUsingSolve_FDRM.invert(this, A, A_inv);
        }

        public abstract bool setA(FMatrixRMaj A);
        public abstract double quality();
        public abstract void solve(FMatrixRMaj B, FMatrixRMaj X);
        public abstract bool modifiesA();
        public abstract bool modifiesB();
        public abstract DecompositionInterface<FMatrixRMaj> getDecomposition();
    }
}