using System;
using SharpMatrix.Data;
using SharpMatrix.Interfaces.Decomposition;
using SharpMatrix.Sparse;
using SharpMatrix.Sparse.Csc;
using SharpMatrix.Sparse.Csc.Factory;
using SharpMatrix.Sparse.Csc.Misc;
using Randomization;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * In this example the results from a sparse LU decomposition is found and then used to solve. Code for dense
 * matrices is directly analogous.
 *
 * This is intended as an example for how to use a decomposition's high level interface. When solving a system
 * 99% if the time you want to use a built in solver instead of decomposing the matrix yourself and solving.
 * The built in solver will take full advantage of internal data structures and is likely to have a lower
 * memory foot print and be faster.
 *
 * @author Peter Abeles
 */
    public class ExampleDecompositionSolve
    {
        public static void main(string[] args)
        {
            // create a random matrix that can be solved
            int N = 5;
            IMersenneTwister rand = new MersenneTwisterFast(234);

            DMatrixSparseCSC A = RandomMatrices_DSCC.rectangle(N, N, N * N / 4, rand);
            RandomMatrices_DSCC.ensureNotSingular(A, rand);

            // Create the LU decomposition
            LUSparseDecomposition_F64<DMatrixSparseCSC> decompose =
                DecompositionFactory_DSCC.lu(FillReducing.NONE);

            // Decompose the matrix.
            // If you care about the A matrix being modified call decompose.inputModified()
            if (!decompose.decompose(A))
                throw new InvalidOperationException("The matrix is singular");

            // Extract new copies of the L and U matrices
            DMatrixSparseCSC L = decompose.getLower(null);
            DMatrixSparseCSC U = decompose.getUpper(null);
            DMatrixSparseCSC P = decompose.getRowPivot(null);

            // Storage for an intermediate step
            DMatrixSparseCSC tmp = (DMatrixSparseCSC) A.createLike();

            // Storage for the inverse matrix
            DMatrixSparseCSC Ainv = (DMatrixSparseCSC) A.createLike();

            // Solve for the inverse: P*I = L*U*inv(A)
            TriangularSolver_DSCC.solve(L, true, P, tmp, null, null, null);
            TriangularSolver_DSCC.solve(U, false, tmp, Ainv, null, null, null);

            // Make sure the inverse has been found. A*inv(A) = identity should be an identity matrix
            DMatrixSparseCSC found = (DMatrixSparseCSC) A.createLike();

            CommonOps_DSCC.mult(A, Ainv, found);
            found.print();

        }
    }
}