using System;
using System.Collections.Generic;

namespace SharpMatrix.Equation
{
    //package org.ejml.equation;

/**
 * Contains a sequence of operations.  This is the result of compiling the equation.  Once created it can
 * be invoked an arbitrary number of times by invoking {@link #perform()}.
 *
 * @author Peter Abeles
 */
    public class Sequence
    {
        // List of in sequence operations which the equation string described
        public List<Operation> operations = new List<Operation>();

        public void addOperation(Operation operation)
        {
            operations.Add(operation);
        }

        /**
         * Executes the sequence of operations
         */
        public void perform()
        {
            for (int i = 0; i < operations.Count; i++)
            {
                operations[i].process();
            }
        }
    }
}