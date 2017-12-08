using SharpMatrix.Data;

namespace SharpMatrix.Interfaces.LinSol
{
    //package org.ejml.interfaces.linsol;

/**
 * <p>Implementation of {@link ReducedRowEchelonForm} for 64-bit floats </p>
 *
 * @author Peter Abeles
 */
    public interface ReducedRowEchelonForm_F64<T> : ReducedRowEchelonForm<T>
        where T : Matrix
    {

        /**
         * Specifies tolerance for determining if the system is singular and it should stop processing.
         * A reasonable value is: tol = EPS/max(||tol||).
         *
         * @param tol Tolerance for singular matrix. A reasonable value is: tol = EPS/max(||tol||). Or just set to zero.
         */
        void setTolerance(double tol);
    }
}
