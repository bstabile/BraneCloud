/*
 * BraneCloud.Evolution.EC (Evolutionary Computation)
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 *
 * This is an independent conversion from Java to .NET of ...
 *
 * Sean Luke's ECJ project at GMU 
 * (Academic Free License v3.0): 
 * http://www.cs.gmu.edu/~eclab/projects/ecj
 *
 * Radical alteration was required throughout (including structural).
 * The author of ECJ cannot and MUST not be expected to support this fork.
 *
 * If you wish to create yet another fork, please use a different root namespace.
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using System;
using System.Linq;

namespace BraneCloud.Evolution.Util.Parsing.Parser
{
    public class DefaultValueExpression : Expression
    {
        private readonly Expression _value;
        private readonly Expression _defaultValue;

        public DefaultValueExpression(TokenPosition position, Expression value, Expression defaultValue)
            : base(position)
        {
            _value = value;
            _defaultValue = defaultValue;
        }

        public override ValueExpression Evaluate(IParserContext context)
        {
            ValueExpression result = _value.Evaluate(context);


            if (context.ToBoolean(result.Value))
                return result;
            else
                return _defaultValue.Evaluate(context);
        }

        public override string ToString()
        {
            return "(" + _value + " ?: " + _defaultValue + ")";
        }
    }
}