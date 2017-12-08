using SharpMatrix.Dense.Block.LinSol.QR;

namespace SharpMatrix.Dense.Row.LinSol.QR
{
    //package org.ejml.dense.row.linsol.qr;

/**
 * Wrapper around {@link QrHouseHolderSolver_DDRB} that allows it to process
 * {@link DMatrixRMaj}.
 *
 * @author Peter Abeles
 */
    public class LinearSolverQrBlock64_DDRM : LinearSolver_DDRB_to_DDRM
    {

        public LinearSolverQrBlock64_DDRM()
            : base(new QrHouseHolderSolver_DDRB())
        {
        }
    }
}