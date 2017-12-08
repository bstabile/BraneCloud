using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Factory;
using SharpMatrix.Interfaces.LinSol;

namespace SharpMatrix.Dense.Row.Decomposition.Eig.Watched
{
    //package org.ejml.dense.row.decomposition.eig.watched;

/**
 * @author Peter Abeles
 */
    public class WatchedDoubleStepQREigenvector_FDRM
    {

        WatchedDoubleStepQREigen_FDRM _implicit;

        // Q matrix from float step QR
        FMatrixRMaj Q;


        FMatrixRMaj[] eigenvectors;

        FMatrixRMaj eigenvectorTemp;

        LinearSolverDense<FMatrixRMaj> solver;

        Complex_F32[] origEigenvalues;
        int N;

        int[] splits;
        int numSplits;

        int x1, x2;

        int indexVal;
        bool onscript;

        public bool process(WatchedDoubleStepQREigen_FDRM imp, FMatrixRMaj A, FMatrixRMaj Q_h)
        {
            this._implicit = imp;

            if (N != A.numRows)
            {
                N = A.numRows;
                Q = new FMatrixRMaj(N, N);
                splits = new int[N];
                origEigenvalues = new Complex_F32[N];
                eigenvectors = new FMatrixRMaj[N];
                eigenvectorTemp = new FMatrixRMaj(N, 1);

                solver = LinearSolverFactory_FDRM.linear(0);
            }
            else
            {
//            UtilEjml.setnull(eigenvectors);
                eigenvectors = new FMatrixRMaj[N];
            }
            Array.Copy(_implicit.eigenvalues, 0, origEigenvalues, 0, N);

            _implicit.setup(A);
            _implicit.setQ(Q);
            numSplits = 0;
            onscript = true;

//        Console.WriteLine("Orig A");
//        A.print("%12.10ff");

            if (!findQandR())
                return false;

            return extractVectors(Q_h);
        }

        public bool extractVectors(FMatrixRMaj Q_h)
        {
            //Arrays.fill(eigenvectorTemp.data, 0);
            Array.Clear(eigenvectorTemp.data, 0, eigenvectorTemp.data.Length);

            // extract eigenvectors from the shur matrix
            // start at the top left corner of the matrix
            bool triangular = true;
            for (int i = 0; i < N; i++)
            {

                Complex_F32 c = _implicit.eigenvalues[N - i - 1];

                if (triangular && !c.isReal())
                    triangular = false;

                if (c.isReal() && eigenvectors[N - i - 1] == null)
                {
                    solveEigenvectorDuplicateEigenvalue(c.real, i, triangular);
                }
            }

            // translate the eigenvectors into the frame of the original matrix
            if (Q_h != null)
            {
                FMatrixRMaj temp = new FMatrixRMaj(N, 1);
                for (int i = 0; i < N; i++)
                {
                    FMatrixRMaj v = eigenvectors[i];

                    if (v != null)
                    {
                        CommonOps_FDRM.mult(Q_h, v, temp);
                        eigenvectors[i] = temp;
                        temp = v;
                    }
                }
            }

            return true;
        }

        private void solveEigenvectorDuplicateEigenvalue(float real, int first, bool isTriangle)
        {

            float scale = Math.Abs(real);
            if (scale == 0) scale = 1;

            eigenvectorTemp.reshape(N, 1, false);
            eigenvectorTemp.zero();

            if (first > 0)
            {
                if (isTriangle)
                {
                    solveUsingTriangle(real, first, eigenvectorTemp);
                }
                else
                {
                    solveWithLU(real, first, eigenvectorTemp);
                }
            }

            eigenvectorTemp.reshape(N, 1, false);

            for (int i = first; i < N; i++)
            {
                Complex_F32 c = _implicit.eigenvalues[N - i - 1];

                if (c.isReal() && Math.Abs(c.real - real) / scale < 100.0f * UtilEjml.F_EPS)
                {
                    eigenvectorTemp.data[i] = 1;

                    FMatrixRMaj v = new FMatrixRMaj(N, 1);
                    CommonOps_FDRM.multTransA(Q, eigenvectorTemp, v);
                    eigenvectors[N - i - 1] = v;
                    NormOps_FDRM.normalizeF(v);

                    eigenvectorTemp.data[i] = 0;
                }
            }
        }

        private void solveUsingTriangle(float real, int index, FMatrixRMaj r)
        {
            for (int i = 0; i < index; i++)
            {
                _implicit.A.add(i, i, -real);
            }

            SpecializedOps_FDRM.subvector(_implicit.A, 0, index, index, false, 0, r);
            CommonOps_FDRM.changeSign(r);

            TriangularSolver_FDRM.solveU(_implicit.A.data, r.data, _implicit.A.numRows, 0, index);

            for (int i = 0; i < index; i++)
            {
                _implicit.A.add(i, i, real);
            }
        }

        private void solveWithLU(float real, int index, FMatrixRMaj r)
        {
            FMatrixRMaj A = new FMatrixRMaj(index, index);

            CommonOps_FDRM.extract(_implicit.A, 0, index, 0, index, A, 0, 0);

            for (int i = 0; i < index; i++)
            {
                A.add(i, i, -real);
            }

            r.reshape(index, 1, false);

            SpecializedOps_FDRM.subvector(_implicit.A, 0, index, index, false, 0, r);
            CommonOps_FDRM.changeSign(r);

            // TODO this must be very inefficient
            if (!solver.setA(A))
                throw new InvalidOperationException("Solve failed");
            solver.solve(r, r);
        }

        public bool findQandR()
        {
            CommonOps_FDRM.setIdentity(Q);

            x1 = 0;
            x2 = N - 1;

            // use the already computed eigenvalues to recompute the Q and R matrices
            indexVal = 0;
            while (indexVal < N)
            {
                if (!findNextEigenvalue())
                {
                    return false;
                }
            }

//        Q.print("%1.10ff");
//
//        _implicit.A.print("%1.10ff");

            return true;
        }

        private bool findNextEigenvalue()
        {
            bool foundEigen = false;
            while (!foundEigen && _implicit.steps < _implicit.maxIterations)
            {
//            _implicit.A.print();
                _implicit.incrementSteps();

                if (x2 < x1)
                {
                    moveToNextSplit();
                }
                else if (x2 - x1 == 0)
                {
                    _implicit.addEigenAt(x1);
                    x2--;
                    indexVal++;
                    foundEigen = true;
                }
                else if (x2 - x1 == 1 && !_implicit.isReal2x2(x1, x2))
                {
                    _implicit.addComputedEigen2x2(x1, x2);
                    x2 -= 2;
                    indexVal += 2;
                    foundEigen = true;
                }
                else if (_implicit.steps - _implicit.lastExceptional > _implicit.exceptionalThreshold)
                {
//                _implicit.A.print("%e");
                    //System.err.println("If it needs to do an exceptional shift then something went very bad.");
//                return false;
                    _implicit.exceptionalShift(x1, x2);
                    _implicit.lastExceptional = _implicit.steps;
                }
                else if (_implicit.isZero(x2, x2 - 1))
                {
                    // check for convergence
                    _implicit.addEigenAt(x2);
                    foundEigen = true;
                    x2--;
                    indexVal++;
                }
                else
                {
                    checkSplitPerformImplicit();
                }
            }
            return foundEigen;
        }


        private void checkSplitPerformImplicit()
        {
            // check for splits
            for (int i = x2; i > x1; i--)
            {
                if (_implicit.isZero(i, i - 1))
                {
                    x1 = i;
                    splits[numSplits++] = i - 1;
                    // reduce the scope of what it is looking at
                    return;
                }
            }
            // first try using known eigenvalues in the same order they were originally found
            if (onscript)
            {
                if (_implicit.steps > _implicit.exceptionalThreshold / 2)
                {
                    onscript = false;
                }
                else
                {
                    Complex_F32 a = origEigenvalues[indexVal];

                    // if no splits are found perform an _implicit step
                    if (a.isReal())
                    {
                        _implicit.performImplicitSingleStep(x1, x2, a.getReal());
                    }
                    else if (x2 - x1 >= 1 && x1 + 2 < N)
                    {
                        _implicit.performImplicitDoubleStep(x1, x2, a.real, a.imaginary);
                    }
                    else
                    {
                        onscript = false;
                    }
                }
            }
            else
            {
                // that didn't work so try a modified order
                if (x2 - x1 >= 1 && x1 + 2 < N)
                    _implicit.implicitDoubleStep(x1, x2);
                else
                    _implicit.performImplicitSingleStep(x1, x2, _implicit.A.get(x2, x2));
            }
        }


        private void moveToNextSplit()
        {
            if (numSplits <= 0)
                throw new InvalidOperationException("bad");

            x2 = splits[--numSplits];

            if (numSplits > 0)
            {
                x1 = splits[numSplits - 1] + 1;
            }
            else
            {
                x1 = 0;
            }
        }

        public FMatrixRMaj getQ()
        {
            return Q;
        }

        public WatchedDoubleStepQREigen_FDRM getImplicit()
        {
            return _implicit;
        }

        public FMatrixRMaj[] getEigenvectors()
        {
            return eigenvectors;
        }

        public Complex_F32[] getEigenvalues()
        {
            return _implicit.eigenvalues;
        }
    }
}