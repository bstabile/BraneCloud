using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

/**
 * A function is an operator with the following syntax "&lt;Name&gt;( Input )"
 *
 * @author Peter Abeles
 */
    public class Function
    {
        /**
         * Name of operator and the string it looks for when parsing
         */
        public string name;

        public Function(string name)
        {
            this.name = name;
        }

        public string getName()
        {
            return name;
        }

        //@Override
        public string toString()
        {
            return "Function{" +
                   "name='" + name + '\'' +
                   '}';
        }
    }
}