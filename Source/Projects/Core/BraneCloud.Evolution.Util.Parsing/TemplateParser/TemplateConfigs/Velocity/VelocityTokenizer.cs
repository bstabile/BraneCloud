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

namespace BraneCloud.Evolution.Util.Parsing.Parser.Config
{
    public class VelocityTokenizer : TemplateTokenizer
    {
        public VelocityTokenizer()
        {
            AddTokenMatcher(TemplateTokenType.IncludeFile, new VelocityTagMatcher("include"), true);
            AddTokenMatcher(TemplateTokenType.IncludeFile, new VelocityTagMatcher("parse"), true);
            AddTokenMatcher(TemplateTokenType.ForEach, new VelocityForEachMatcher(), true);
            AddTokenMatcher(TemplateTokenType.EndBlock, new AnyOfStringMatcher("#end","#{end}"), true);
            AddTokenMatcher(TemplateTokenType.If, new VelocityTagMatcher("if"), true);
            AddTokenMatcher(TemplateTokenType.ElseIf, new VelocityTagMatcher("elseif"), true);
            AddTokenMatcher(TemplateTokenType.Else, new AnyOfStringMatcher("#else","#{else}"), true);
            AddTokenMatcher(TemplateTokenType.Statement, new WrappedExpressionMatcher("${#", "}"), true);
            AddTokenMatcher(TemplateTokenType.Expression, new WrappedExpressionMatcher("${","}"));
            AddTokenMatcher(TemplateTokenType.Expression, new DollarExpressionMatcher());
        }

        private class VelocityForEachMatcher : CompositeMatcher
        {
            public VelocityForEachMatcher()
                : base(
                    new AnyOfStringMatcher("#foreach","#{foreach}"),
                    new WhiteSpacePaddedMatcher(new CharMatcher('(')),
                    new WhiteSpacePaddedMatcher(new VariableMatcher()),
                    new WhiteSpaceMatcher(),
                    new StringMatcher("in"),
                    new WhiteSpaceMatcher(),
                    new SmartExpressionMatcher(")"),
                    new CharMatcher(')'))
            {
            }

            protected override string TranslateToken(string originalToken, CompositeTokenProcessor tokenProcessor)
            {
                string iterator = originalToken.Substring(tokenProcessor.StartIndexes[2], tokenProcessor.StartIndexes[3] - tokenProcessor.StartIndexes[2]).Trim();
                string expr = originalToken.Substring(tokenProcessor.StartIndexes[6], tokenProcessor.StartIndexes[7] - tokenProcessor.StartIndexes[6]).Trim();

                return iterator + '\0' + expr;
            }

        }

        private class VelocityTagMatcher : CompositeMatcher
        {
            public VelocityTagMatcher(string keyword)
                : base(
                    new AnyOfStringMatcher("#" + keyword, "#{" + keyword + "}"),
                    new WhiteSpacePaddedMatcher(new CharMatcher('(')),
                    new WhiteSpacePaddedMatcher(new SmartExpressionMatcher(")")),
                    new WhiteSpacePaddedMatcher(new CharMatcher(')'))
                )
            {
            }

            protected override string TranslateToken(string originalToken, CompositeTokenProcessor tokenProcessor)
            {
                return originalToken.Substring(tokenProcessor.StartIndexes[2], tokenProcessor.StartIndexes[3] - tokenProcessor.StartIndexes[2]).Trim();
            }
        }


    }
}