using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

/**
 * Exception generated for parse errors in {@link Equation}
 *
 * @author Peter Abeles
 */
    public class ParseError : InvalidOperationException
    {
        public ParseError(string message)
            : base(message)
        {
        }
    }
}