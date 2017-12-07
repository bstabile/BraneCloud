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
 * The extending class must explicity call {@link #_setA(CMatrixRMaj)}
 * inside of its {@link #setA} function.
 * </p>
 * 
 * @author Peter Abeles
 */
    public abstract class LinearSolverAbstract_CDRM : LinearSolverDense<CMatrixRMaj>
    {

        protected CMatrixRMaj A;
        protected int numRows;
        protected int numCols;
        protected int stride;

        public CMatrixRMaj getA()
        {
            return A;
        }

        protected void _setA(CMatrixRMaj A)
        {
            this.A = A;
            this.numRows = A.numRows;
            this.numCols = A.numCols;
            this.stride = numCols * 2;
        }

        //@Override
        public virtual void invert(CMatrixRMaj A_inv)
        {
            InvertUsingSolve_CDRM.invert(this, A, A_inv);
        }

        public abstract bool setA(CMatrixRMaj A);
        public abstract double quality();
        public abstract void solve(CMatrixRMaj B, CMatrixRMaj X);
        public abstract bool modifiesA();
        public abstract bool modifiesB();
        public abstract DecompositionInterface<CMatrixRMaj> getDecomposition();
    }
}