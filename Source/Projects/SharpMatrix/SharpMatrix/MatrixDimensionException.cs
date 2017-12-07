using System;

namespace BraneCloud.Evolution.EC.MatrixLib
{
    /**
     * If two matrices did not have compatible dimensions for the operation this exception
     * is thrown.
     *
     * @author Peter Abeles
     */
    public class MatrixDimensionException

        : InvalidOperationException
    {
        public MatrixDimensionException()
        {
        }

        public MatrixDimensionException(string message)
            : base(message)
        {
        }
    }
}