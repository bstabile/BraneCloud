using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.Decomposition;
using BraneCloud.Evolution.EC.MatrixLib.Interfaces.LinSol;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row.LinSol
{
    //package org.ejml.dense.row.linsol;

/**
 * <p>
 * An abstract class that provides some common functionality and a default implementation
 * of invert that uses the solve function of the child class.
 * </p>
 *
 * <p>
 * The extending class must explicity call {@link #_setA(DMatrixRMaj)}
 * inside of its {@link #setA} function.
 * </p>
 * 
 * @author Peter Abeles
 */
    public abstract class LinearSolverAbstract_DDRM : LinearSolverDense<DMatrixRMaj>
    {

        protected DMatrixRMaj A;
        protected int numRows;
        protected int numCols;

        public virtual DMatrixRMaj getA()
        {
            return A;
        }

        protected void _setA(DMatrixRMaj A)
        {
            this.A = A;
            this.numRows = A.numRows;
            this.numCols = A.numCols;
        }

        public virtual void invert(DMatrixRMaj A_inv)
        {
            InvertUsingSolve_DDRM.invert(this, A, A_inv);
        }

        public abstract bool setA(DMatrixRMaj A);
        public abstract double quality();
        public abstract void solve(DMatrixRMaj B, DMatrixRMaj X);
        public abstract bool modifiesA();
        public abstract bool modifiesB();
        public abstract DecompositionInterface<DMatrixRMaj> getDecomposition();
    }
}
