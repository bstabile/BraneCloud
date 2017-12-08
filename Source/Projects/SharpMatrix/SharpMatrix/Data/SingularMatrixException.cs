using System;

namespace SharpMatrix.Data
{
    //package org.ejml.data;


/**
 * This exception is thrown if an operation can not be finished because the matrix is singular.
 * It is a InvalidOperationException to allow the code to be written cleaner and also because singular
 * matrices are not always detected.  Forcing an exception to be caught provides a false sense
 * of security.
 *
 * @author Peter Abeles
 */
    public class SingularMatrixException : InvalidOperationException
    {

        public SingularMatrixException()
        {
        }

        public SingularMatrixException(string message)
            : base(message)
        {
        }
    }
}
