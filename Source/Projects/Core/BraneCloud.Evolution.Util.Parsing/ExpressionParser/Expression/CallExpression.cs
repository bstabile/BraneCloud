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

#region License
//=============================================================================
// Vici Core - Productivity Library for .NET 3.5 
//
// Copyright (c) 2008-2010 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace BraneCloud.Evolution.Util.Parsing.Parser
{
    public class CallExpression : Expression
    {
        private readonly Expression _methodExpression;
        private readonly Expression[] _parameters;

        public CallExpression(TokenPosition position, Expression methodExpression, Expression[] parameters) : base(position)
        {
            _methodExpression = methodExpression;
            _parameters = parameters;
        }

        public override ValueExpression Evaluate(IParserContext context)
        {
            object methodObject = _methodExpression.Evaluate(context).Value;

            ValueExpression[] parameters = EvaluateExpressionArray(_parameters, context);
            Type[] parameterTypes = Array.ConvertAll(parameters, expr => expr.Type);
            object[] parameterValues = Array.ConvertAll(parameters, expr => expr.Value);

			if (methodObject is MethodDefinition)
			{
				Type returnType;

                return Exp.Value(TokenPosition, ((MethodDefinition)methodObject).Invoke(parameterTypes, parameterValues, out returnType, new LazyBinder()), returnType);
			}

			if (methodObject is ConstructorInfo[])
			{
				ConstructorInfo[] constructors = (ConstructorInfo[]) methodObject;

                MethodBase method = new LazyBinder().SelectMethod(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, constructors, parameterTypes, null);

				if (method == null)
					throw new ExpressionEvaluationException("No match found for constructor " + constructors[0].Name, this);

				object value = ((ConstructorInfo)method).Invoke(parameterValues);

                return Exp.Value(TokenPosition, value, method.ReflectedType);
			}

			if (methodObject is Delegate[])
			{
				Delegate[] delegates = (Delegate[]) methodObject;
				MethodBase[] methods = Array.ConvertAll<Delegate, MethodBase>(delegates, d => d.Method);

                MethodBase method = new LazyBinder().SelectMethod(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, methods, parameterTypes, null);

				if (method == null)
					throw new ExpressionEvaluationException("No match found for delegate " + _methodExpression, this);

				object value = method.Invoke(delegates[Array.IndexOf(methods, method)].Target, parameterValues);

                return Exp.Value(TokenPosition, value, ((MethodInfo)method).ReturnType);
			}

            if (methodObject is Delegate)
            {
                Delegate method = (Delegate) methodObject;

                object value = method.Method.Invoke(method.Target, parameterValues);

                return new ValueExpression(TokenPosition, value, method.Method.ReturnType);
            }

            throw new ExpressionEvaluationException(_methodExpression + " is not a function", this);
        }

        public override string ToString()
        {
            string[] parameters = Array.ConvertAll(_parameters, expr => expr.ToString());

            return "(" + _methodExpression + "(" + String.Join(",", parameters) + "))";
        }
    }
}