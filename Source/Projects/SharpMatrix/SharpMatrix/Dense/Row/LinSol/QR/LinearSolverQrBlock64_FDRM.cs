using SharpMatrix.Dense.Block.LinSol.QR;

namespace SharpMatrix.Dense.Row.LinSol.QR
{
    //package org.ejml.dense.row.linsol.qr;

/**
 * Wrapper around {@link QrHouseHolderSolver_FDRB} that allows it to process
 * {@link FMatrixRMaj}.
 *
 * @author Peter Abeles
 */
    public class LinearSolverQrBlock64_FDRM : LinearSolver_FDRB_to_FDRM
    {

        public LinearSolverQrBlock64_FDRM()
            : base(new QrHouseHolderSolver_FDRB())
        {
        }
    }
}