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

namespace BraneCloud.Evolution.Util.Parsing.Parser
{
    public class DecimalLiteralMatcher : ITokenMatcher, ITokenProcessor
    {
        private enum Stage
        {
            Num1a,
            Num1b,
            Num2a,
            Num2b,
            Done
        }

        private const string SUFFIXES = "FfDdMm";

        private Stage _stage;

        public ITokenProcessor CreateTokenProcessor()
        {
            return new DecimalLiteralMatcher();
        }

        public void ResetState()
        {
            _stage = Stage.Num1a;
        }

        public TokenizerState ProcessChar(char c, string fullExpression, int currentIndex)
        {
            bool isDigit = (c >= '0' && c <= '9');

            switch (_stage)
            {
                case Stage.Num1a:
                    {
                        if (!isDigit)
                            return TokenizerState.Fail;

                        _stage = Stage.Num1b;
                    }
                    break;

                case Stage.Num1b:
                    {
                        if (c == '.')
                        {
                            _stage = Stage.Num2a;
                        }
                        else if (!isDigit)
                        {
                            return TokenizerState.Fail;
                        }
                    }
                    break;

                case Stage.Num2a:
                    {
                        if (!isDigit)
                            return TokenizerState.Fail;

                        _stage = Stage.Num2b;
                    }
                    break;

                case Stage.Num2b:
                    {
                        if (SUFFIXES.IndexOf(c) >= 0)
                            _stage = Stage.Done;
                        else if (!isDigit)
                            return TokenizerState.Success;
                    }
                    break;

                case Stage.Done:
                    return TokenizerState.Success;
            }

            return TokenizerState.Valid;
        }

        public string TranslateToken(string originalToken, ITokenProcessor tokenProcessor)
        {
            return originalToken;
        }
    }
}