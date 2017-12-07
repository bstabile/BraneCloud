using System;
using System.Collections.Generic;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

/**
 * Centralized place to create new instances of operations and functions.  Must call
 * {@link #setManagerTemp} before any other functions.
 *
 * @author Peter Abeles
 */
    public class ManagerFunctions
    {

        // List of functions which take in N inputs
        IDictionary<string, Input1> input1 = new Dictionary<string, Input1>();

        IDictionary<string, InputN> inputN = new Dictionary<string, InputN>();

        // Reference to temporary variable manager
        protected ManagerTempVariables managerTemp;

        public ManagerFunctions()
        {
            addBuiltIn();
        }

        /**
         * Returns true if the string matches the name of a function
         */
        public bool isFunctionName(string s)
        {
            if (input1.ContainsKey(s))
                return true;
            if (inputN.ContainsKey(s))
                return true;

            return false;
        }

        /**
         * Create a new instance of single input functions
         * @param name function name
         * @param var0 Input variable
         * @return Resulting operation
         */
        public Operation.Info create(string name, Variable var0)
        {
            Input1 func = input1[name];
            if (func == null)
                return null;
            return func.create(var0, managerTemp);
        }

        /**
         * Create a new instance of single input functions
         * @param name function name
         * @param vars Input variables
         * @return Resulting operation
         */
        public Operation.Info create(string name, List<Variable> vars)
        {
            InputN func = inputN[name];
            if (func == null)
                return null;
            return func.create(vars, managerTemp);
        }

        /**
         * Create a new instance of a single input function from an operator character
         * @param op Which operation
         * @param input Input variable
         * @return Resulting operation
         */
        public Operation.Info create(char op, Variable input)
        {
            switch (op)
            {
                case '\'':
                    return Operation.transpose(input, managerTemp);

                default:
                    throw new InvalidOperationException("Unknown operation " + op);
            }
        }

        /**
         * Create a new instance of a two input function from an operator character
         * @param op Which operation
         * @param left Input variable on left
         * @param right Input variable on right
         * @return Resulting operation
         */
        public Operation.Info create(Symbol op, Variable left, Variable right)
        {
            switch (op)
            {
                case Symbol.PLUS:
                    return Operation.add(left, right, managerTemp);

                case Symbol.MINUS:
                    return Operation.subtract(left, right, managerTemp);

                case Symbol.TIMES:
                    return Operation.multiply(left, right, managerTemp);

                case Symbol.RDIVIDE:
                    return Operation.divide(left, right, managerTemp);

                case Symbol.LDIVIDE:
                    return Operation.divide(right, left, managerTemp);

                case Symbol.POWER:
                    return Operation.pow(left, right, managerTemp);

                case Symbol.ELEMENT_DIVIDE:
                    return Operation.elementDivision(left, right, managerTemp);

                case Symbol.ELEMENT_TIMES:
                    return Operation.elementMult(left, right, managerTemp);

                case Symbol.ELEMENT_POWER:
                    return Operation.elementPow(left, right, managerTemp);

                default:
                    throw new InvalidOperationException("Unknown operation " + op);
            }
        }

        /**
         *
         * @param managerTemp
         */

        public void setManagerTemp(ManagerTempVariables managerTemp)
        {
            this.managerTemp = managerTemp;
        }

        /**
         * Adds a function, with a single input, to the list
         * @param name Name of function
         * @param function Function factory
         */
        public void add(string name, Input1 function)
        {
            input1.Add(name, function);
        }

        /**
         * Adds a function, with a two inputs, to the list
         * @param name Name of function
         * @param function Function factory
         */
        public void add(string name, InputN function)
        {
            inputN.Add(name, function);
        }

        /**
         * Adds built in functions
         */
        private void addBuiltIn()
        {
            // 1 input
            input1.Add("inv", new Input1((v, m) => Operation.inv(v, m)));
            input1.Add("pinv", new Input1((v, m) => Operation.pinv(v, m)));
            input1.Add("rref", new Input1((v, m) => Operation.rref(v, m)));
            input1.Add("eye", new Input1((v, m) => Operation.eye(v, m)));
            input1.Add("det", new Input1((v, m) => Operation.det(v, m)));
            input1.Add("normF", new Input1((v, m) => Operation.normF(v, m)));
            input1.Add("sum", new Input1((v, m) => Operation.sum_one(v, m)));
            input1.Add("trace", new Input1((v, m) => Operation.trace(v, m)));
            input1.Add("diag", new Input1((v, m) => Operation.diag(v, m)));
            input1.Add("min", new Input1((v, m) => Operation.min(v, m)));
            input1.Add("max", new Input1((v, m) => Operation.max(v, m)));
            input1.Add("abs", new Input1((v, m) => Operation.abs(v, m)));
            input1.Add("sin", new Input1((v, m) => Operation.sin(v, m)));
            input1.Add("cos", new Input1((v, m) => Operation.cos(v, m)));
            input1.Add("atan", new Input1((v, m) => Operation.atan(v, m)));
            input1.Add("exp", new Input1((v, m) => Operation.exp(v, m)));
            input1.Add("log", new Input1((v, m) => Operation.log(v, m)));
            input1.Add("sqrt", new Input1((v, m) => Operation.sqrt(v, m)));

            // 2 inputs
            inputN.Add("normP", new InputN((l, m) => Operation.normP(l[0], l[1], m)));
            inputN.Add("max", new InputN((l, m) => Operation.max_two(l[0], l[1], m)));
            inputN.Add("min", new InputN((l, m) => Operation.min_two(l[0], l[1], m)));
            inputN.Add("sum", new InputN((l, m) => Operation.sum_two(l[0], l[1], m)));
            inputN.Add("zeros", new InputN((l, m) => Operation.zeros(l[0], l[1], m)));
            inputN.Add("ones", new InputN((l, m) => Operation.ones(l[0], l[1], m)));
            inputN.Add("kron", new InputN((l, m) => Operation.kron(l[0], l[1], m)));
            inputN.Add("dot", new InputN((l, m) => Operation.dot(l[0], l[1], m)));
            inputN.Add("pow", new InputN((l, m) => Operation.pow(l[0], l[1], m)));
            inputN.Add("atan2", new InputN((l, m) => Operation.atan2(l[0], l[1], m)));
            inputN.Add("solve", new InputN((l, m) => Operation.solve(l[0], l[1], m)));

            // N inputs
            inputN.Add("extract", new InputN((l, m) => Operation.extract(l, m)));

            // 2 or 3 inputs
            inputN.Add("extractScalar", new InputN((l, m) => Operation.extractScalar(l, m)));
                //public Operation.Info create(List<Variable> inputs, ManagerTempVariables manager)
                //{
                //    if( inputs.size() != 2 && inputs.size() != 3 ) throw new InvalidOperationException(
                //    "Two or three inputs expected");
                //    return Operation.extractScalar(inputs,
                //    manager);
                //}
        }

        public ManagerTempVariables getManagerTemp()
        {
            return managerTemp;
        }

        /**
         * Creates new instances of functions from a single variable
         */
        public class Input1
        {
            //public Input1() { }
            public Input1(Func<Variable, ManagerTempVariables, Operation.Info> create)
            {
                this.create = create;
            }
            public Func<Variable, ManagerTempVariables, Operation.Info> create { get; set; }
        }

        /**
         * Creates a new instance of functions from two variables
         */
        public class InputN
        {
            //public InputN() { }
            public InputN(Func<List<Variable>, ManagerTempVariables, Operation.Info> create)
            {
                this.create = create;
            }
            public Func<List<Variable>, ManagerTempVariables, Operation.Info> create { get; set; }
        }
    }
}