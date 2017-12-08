using SharpMatrix.Data;
using SharpMatrix.Dense.Row.Decomposition.QR;

namespace SharpMatrix.Dense.Row.LinSol.QR
{
    //package org.ejml.dense.row.linsol.qr;

/**
 * A solver for QR decomposition that can efficiently modify the previous decomposition when
 * data is added or removed.
 *
 * @author Peter Abeles
 */
    public class AdjLinearSolverQr_DDRM : LinearSolverQr_DDRM, AdjustableLinearSolver_DDRM
    {

        private QrUpdate_DDRM update;

        // NOTE: This hides the protected field from the base class.
        //       I'm assuming this was intended.
        private new DMatrixRMaj A;

        public AdjLinearSolverQr_DDRM()
            : base(new QRDecompositionHouseholderColumn_DDRM())
        {
        }

        public override void setMaxSize(int maxRows, int maxCols)
        {
            // allow it some room to grow
            maxRows += 5;

            base.setMaxSize(maxRows, maxCols);

            update = new QrUpdate_DDRM(maxRows, maxCols, true);
            A = new DMatrixRMaj(maxRows, maxCols);
        }

        /**
         * Compute the A matrix from the Q and R matrices.
         *
         * @return The A matrix.
         */
        public override DMatrixRMaj getA()
        {
            if (A.data.Length < numRows * numCols)
            {
                A = new DMatrixRMaj(numRows, numCols);
            }
            A.reshape(numRows, numCols, false);
            CommonOps_DDRM.mult(Q, R, A);

            return A;
        }

        public virtual bool addRowToA(double[] A_row, int rowIndex)
        {
            // see if it needs to grow the data structures
            if (numRows + 1 > maxRows)
            {
                // grow by 10%
                int grow = maxRows / 10;
                if (grow < 1) grow = 1;
                maxRows = numRows + grow;
                Q.reshape(maxRows, maxRows, true);
                R.reshape(maxRows, maxCols, true);
            }

            update.addRow(Q, R, A_row, rowIndex, true);
            numRows++;

            return true;
        }

        public virtual bool removeRowFromA(int index)
        {
            update.deleteRow(Q, R, index, true);
            numRows--;
            return true;
        }

    }
}