using System;

namespace SharpMatrix.Equation
{
    //package org.ejml.equation;

/**
 * Types of low level operators which can be applied in the code
 *
 * @author Peter Abeles
 */
    public enum Symbol
    {
        PLUS,
        MINUS,
        TIMES,
        LDIVIDE,
        RDIVIDE,
        POWER,
        PERIOD,
        ELEMENT_TIMES,
        ELEMENT_DIVIDE,
        ELEMENT_POWER,
        ASSIGN,
        PAREN_LEFT,
        PAREN_RIGHT,
        BRACKET_LEFT,
        BRACKET_RIGHT,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_EQ,
        LESS_THAN_EQ,
        COMMA,
        TRANSPOSE,
        COLON,
        SEMICOLON
    }

    public static class SymbolLookup
    {
        public static Symbol lookup(char c)
        {
            switch (c)
            {
                case '.': return Symbol.PERIOD;
                case ',': return Symbol.COMMA;
                case '\'': return Symbol.TRANSPOSE;
                case '+': return Symbol.PLUS;
                case '-': return Symbol.MINUS;
                case '*': return Symbol.TIMES;
                case '\\': return Symbol.LDIVIDE;
                case '/': return Symbol.RDIVIDE;
                case '^': return Symbol.POWER;
                case '=': return Symbol.ASSIGN;
                case '(': return Symbol.PAREN_LEFT;
                case ')': return Symbol.PAREN_RIGHT;
                case '[': return Symbol.BRACKET_LEFT;
                case ']': return Symbol.BRACKET_RIGHT;
                case '>': return Symbol.GREATER_THAN;
                case '<': return Symbol.LESS_THAN;
                case ':': return Symbol.COLON;
                case ';': return Symbol.SEMICOLON;
            }
            throw new InvalidOperationException("Unknown type " + c);
        }

        public static Symbol lookupElementWise(char c)
        {
            switch (c)
            {
                case '*': return Symbol.ELEMENT_TIMES;
                case '/': return Symbol.ELEMENT_DIVIDE;
                case '^': return Symbol.ELEMENT_POWER;
            }
            throw new InvalidOperationException("Unknown element-wise type " + c);
        }
    }
}
