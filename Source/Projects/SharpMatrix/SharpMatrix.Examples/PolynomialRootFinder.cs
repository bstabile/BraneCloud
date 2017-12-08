using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Factory;
using SharpMatrix.Interfaces.Decomposition;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * <p>
 * Eigenvalue decomposition can be used to find the roots in a polynomial by constructing the
 * so called companion matrix.  While faster techniques do exist for root finding, this is
 * one of the most stable and probably the easiest to implement.
 * </p>
 *
 * <p>
 * Because the companion matrix is not symmetric a generalized eigenvalue decomposition is needed.
 * The roots of the polynomial may also be complex.  Complex eigenvalues is the only instance in
 * which EJML supports complex arithmetic.  Depending on the application one might need to check
 * to see if the eigenvalues are real or complex.
 * </p>
 *
 * <p>
 * For more algorithms and robust solution for finding polynomial roots check out http://ddogleg.org
 * </p>
 *
 * @author Peter Abeles
 */
    public class PolynomialRootFinder
    {

        /**
         * <p>
         * Given a set of polynomial coefficients, compute the roots of the polynomial.  Depending on
         * the polynomial being considered the roots may contain complex number.  When complex numbers are
         * present they will come in pairs of complex conjugates.
         * </p>
         *
         * <p>
         * Coefficients are ordered from least to most significant, e.g: y = c[0] + x*c[1] + x*x*c[2].
         * </p>
         *
         * @param coefficients Coefficients of the polynomial.
         * @return The roots of the polynomial
         */
        public static Complex_F64[] findRoots(params double[] coefficients)
        {
            int N = coefficients.Length - 1;

            // Construct the companion matrix
            DMatrixRMaj c = new DMatrixRMaj(N, N);

            double a = coefficients[N];
            for (int i = 0; i < N; i++)
            {
                c.set(i, N - 1, -coefficients[i] / a);
            }
            for (int i = 1; i < N; i++)
            {
                c.set(i, i - 1, 1);
            }

            // use generalized eigenvalue decomposition to find the roots
            EigenDecomposition_F64<DMatrixRMaj> evd = DecompositionFactory_DDRM.eig(N, false);

            evd.decompose(c);

            Complex_F64[] roots = new Complex_F64[N];

            for (int i = 0; i < N; i++)
            {
                roots[i] = evd.getEigenvalue(i);
            }

            return roots;
        }
    }
}