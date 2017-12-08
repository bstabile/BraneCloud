using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.Eig.Watched;

namespace SharpMatrix.Dense.Row.Decomposition.Eig
{
    //package org.ejml.dense.row.decomposition.eig.watched;

/**
 * @author Peter Abeles
 */
    public class WatchedDoubleStepQREigenvalue_DDRM : EigenvalueExtractor_DDRM
    {

        WatchedDoubleStepQREigen_DDRM implicitQR;

        int[] splits;
        int numSplits;

        int x1;
        int x2;

        public WatchedDoubleStepQREigenvalue_DDRM()
        {
            implicitQR = new WatchedDoubleStepQREigen_DDRM();
        }

        public void setup(DMatrixRMaj A)
        {
            implicitQR.setup(A);
            implicitQR.setQ(null);

            splits = new int[A.numRows];
            numSplits = 0;
        }

        public virtual bool process(DMatrixRMaj origA)
        {
            setup(origA);

            x1 = 0;
            x2 = origA.numRows - 1;

            while (implicitQR.numEigen < origA.numRows)
            {
                if (implicitQR.steps > implicitQR.maxIterations)
                    return false;

                implicitQR.incrementSteps();

                if (x2 < x1)
                {
                    moveToNextSplit();
                }
                else if (x2 - x1 == 0)
                {
//                implicitQR.A.print();
                    implicitQR.addEigenAt(x1);
                    x2--;
                }
                else if (x2 - x1 == 1)
                {
//                implicitQR.A.print();
                    implicitQR.addComputedEigen2x2(x1, x2);
                    x2 -= 2;
                }
                else if (implicitQR.steps - implicitQR.lastExceptional > implicitQR.exceptionalThreshold)
                {
                    // see if the matrix blew up
                    if (double.IsNaN(implicitQR.A.get(x2, x2)))
                    {
                        return false;
                    }

                    implicitQR.exceptionalShift(x1, x2);
                }
                else if (implicitQR.isZero(x2, x2 - 1))
                {
//                implicitQR.A.print();
                    implicitQR.addEigenAt(x2);
                    x2--;
                }
                else
                {
                    performIteration();
                }
            }

            return true;
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

        private void performIteration()
        {
            bool changed = false;

            // see if it can perform a split
            for (int i = x2; i > x1; i--)
            {
                if (implicitQR.isZero(i, i - 1))
                {
                    x1 = i;
                    splits[numSplits++] = i - 1;
                    changed = true;
                    // reduce the scope of what it is looking at
                    break;
                }
            }

            if (!changed)
                implicitQR.implicitDoubleStep(x1, x2);
        }

        public virtual int getNumberOfEigenvalues()
        {
            return implicitQR.getNumberOfEigenvalues();
        }

        public virtual Complex_F64[] getEigenvalues()
        {
            return implicitQR.getEigenvalues();
        }

        public WatchedDoubleStepQREigen_DDRM getImplicitQR()
        {
            return implicitQR;
        }
    }
}