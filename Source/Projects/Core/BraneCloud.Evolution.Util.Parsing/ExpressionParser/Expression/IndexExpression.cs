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
using System.Reflection;

namespace BraneCloud.Evolution.Util.Parsing.Parser
{
    public class IndexExpression : Expression
    {
        private readonly Expression _target;
        private readonly Expression[] _parameters;

        public IndexExpression(TokenPosition position, Expression target, Expression[] parameters) : base(position)
        {
            _target = target;
            _parameters = parameters;
        }

        public override ValueExpression Evaluate(IParserContext context)
        {
            ValueExpression targetValue = _target.Evaluate(context);

            Type targetType = targetValue.Type;
            object targetObject = targetValue.Value;

            ValueExpression[] parameters = EvaluateExpressionArray(_parameters, context);
            Type[] parameterTypes = Array.ConvertAll(parameters, expr => expr.Type);
            object[] parameterValues = Array.ConvertAll(parameters, expr => expr.Value);

            if (targetType.IsArray)
            {
                if (targetType.GetArrayRank() != parameters.Length)
                    throw new Exception("Array has a different rank. Number of arguments is incorrect");

                Type returnType = targetType.GetElementType();

                bool useLong = false;

                foreach (Type t in parameterTypes)
                {
                    if (t == typeof(long) || t == typeof(long?))
                        useLong = true;
                    else if (t != typeof(int) & t != typeof(int?) && t != typeof(short) && t != typeof(short?) && t != typeof(ushort) && t != typeof(ushort?))
                        throw new BadArgumentException(t.GetType().Name + " is not a valid type for array indexers", this);
                }

                if (useLong)
                {
                    long[] indexes = new long[parameters.Length];

                    for (int i=0;i<parameters.Length;i++)
                        indexes[i] = Convert.ToInt64(parameterValues[i]);

                    return Exp.Value(TokenPosition, ((Array)targetObject).GetValue(indexes), returnType);
                }
                else
                {
                    int[] indexes = new int[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                        indexes[i] = Convert.ToInt32(parameterValues[i]);

                    return Exp.Value(TokenPosition, ((Array)targetObject).GetValue(indexes), returnType);
                }
            }
            else
            {
                DefaultMemberAttribute[] att = (DefaultMemberAttribute[]) targetType.GetCustomAttributes(typeof(DefaultMemberAttribute), true);

                MethodInfo methodInfo = targetType.GetMethod("get_" + att[0].MemberName, parameterTypes);

                object value = methodInfo.Invoke(targetObject, parameterValues);

                return new ValueExpression(TokenPosition, value, methodInfo.ReturnType);
            }
        }

        public override string ToString()
        {
            string[] parameters = Array.ConvertAll(_parameters, expr => expr.ToString());

            return "(" + _target + "[" + String.Join(",", parameters) + "])";
        }
    }


}