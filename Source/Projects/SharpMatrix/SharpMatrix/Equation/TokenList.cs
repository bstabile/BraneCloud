using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Equation
{
    //package org.ejml.equation;

    /**
     * Specifies the type of data stored in a Token.
     */
    public enum TokenType
    {
        FUNCTION,
        VARIABLE,
        SYMBOL,
        WORD
    }

/**
     * Linked-list of tokens parsed from the equations string.
     *
     * @author Peter Abeles
     */
    public class TokenList
    {

        public Token first;
        public Token last;
        /**
         * Number of tokens in the list
         */
        public int size = 0;

        public TokenList()
        {
        }

        /**
         * Creates a list from the two given tokens.  These tokens are assumed to form a linked list starting at 'first'
         * and ending at 'last'
         * @param first First element in the new list
         * @param last Last element in the new list
         */
        public TokenList(Token first, Token last)
        {
            this.first = first;
            this.last = last;

            Token t = first;
            while (t != null)
            {
                size++;
                t = t.next;
            }
        }

        /**
         * Adds a function to the end of the token list
         * @param function Function which is to be added
         * @return The new Token created around function
         */
        public Token add(Function function)
        {
            Token t = new Token(function);
            push(t);
            return t;
        }

        /**
         * Adds a variable to the end of the token list
         * @param variable Variable which is to be added
         * @return The new Token created around variable
         */
        public Token add(Variable variable)
        {
            Token t = new Token(variable);
            push(t);
            return t;
        }

        /**
         * Adds a symbol to the end of the token list
         * @param symbol Symbol which is to be added
         * @return The new Token created around symbol
         */
        public Token add(Symbol symbol)
        {
            Token t = new Token(symbol);
            push(t);
            return t;
        }

        /**
         * Adds a word to the end of the token list
         * @param word word which is to be added
         * @return The new Token created around symbol
         */
        public Token add(string word)
        {
            Token t = new Token(word);
            push(t);
            return t;
        }

        /**
         * Adds a new Token to the end of the linked list
         */
        public void push(Token token)
        {
            size++;
            if (first == null)
            {
                first = token;
                last = token;
                token.previous = null;
                token.next = null;
            }
            else
            {
                last.next = token;
                token.previous = last;
                token.next = null;
                last = token;
            }
        }

        /**
         * Inserts 'token' after 'where'.  if where is null then it is inserted to the beginning of the list.
         * @param where Where 'token' should be inserted after.  if null the put at it at the beginning
         * @param token The token that is to be inserted
         */
        public void insert(Token where, Token token)
        {
            if (where == null)
            {
                // put at the front of the list
                if (size == 0)
                    push(token);
                else
                {
                    first.previous = token;
                    token.previous = null;
                    token.next = first;
                    first = token;
                    size++;
                }
            }
            else if (where == last || null == last)
            {
                push(token);
            }
            else
            {
                token.next = where.next;
                token.previous = where;
                where.next.previous = token;
                where.next = token;
                size++;
            }
        }

        /**
         * Removes the token from the list
         * @param token Token which is to be removed
         */
        public void remove(Token token)
        {
            if (token == first)
            {
                first = first.next;
            }
            if (token == last)
            {
                last = last.previous;
            }
            if (token.next != null)
            {
                token.next.previous = token.previous;
            }
            if (token.previous != null)
            {
                token.previous.next = token.next;
            }

            token.next = token.previous = null;
            size--;
        }

        /**
         * Removes 'original' and places 'target' at the same location
         */
        public void replace(Token original, Token target)
        {
            if (first == original)
                first = target;
            if (last == original)
                last = target;

            target.next = original.next;
            target.previous = original.previous;

            if (original.next != null)
                original.next.previous = target;
            if (original.previous != null)
                original.previous.next = target;

            original.next = original.previous = null;
        }

        /**
         * Removes elements from begin to end from the list, inclusive.  Returns a new list which
         * is composed of the removed elements
         */
        public TokenList extractSubList(Token begin, Token end)
        {
            if (begin == end)
            {
                remove(begin);
                return new TokenList(begin, begin);
            }
            else
            {
                if (first == begin)
                {
                    first = end.next;
                }
                if (last == end)
                {
                    last = begin.previous;
                }
                if (begin.previous != null)
                {
                    begin.previous.next = end.next;
                }
                if (end.next != null)
                {
                    end.next.previous = begin.previous;
                }
                begin.previous = null;
                end.next = null;

                TokenList ret = new TokenList(begin, end);
                size -= ret.size;
                return ret;
            }
        }

        /**
         * Inserts the LokenList immediately following the 'before' token
         */
        public void insertAfter(Token before, TokenList list)
        {
            Token after = before.next;

            before.next = list.first;
            list.first.previous = before;
            if (after == null)
            {
                last = list.last;
            }
            else
            {
                after.previous = list.last;
                list.last.next = after;
            }
            size += list.size;
        }

        /**
         * Prints the list of tokens
         */
        public string toString()
        {
            string ret = "";
            Token t = first;
            while (t != null)
            {
                ret += t + " ";
                t = t.next;
            }
            return ret;
        }

        /**
         * First token in the list
         */
        public Token getFirst()
        {
            return first;
        }

        /**
         * Last token in the list
         */
        public Token getLast()
        {
            return last;
        }


        /**
         * The token class contains a reference to parsed data (e.g. function, variable, or symbol) and reference
         * to list elements before and after it.
         */
        public class Token
        {
            /**
             * Next element in the list.  If null then it's at the end of the list
             */
            public Token next;

            /**
             * Previous element in the list.  If null then it's the first element in the list
             */
            public Token previous;

            public Function function;
            public Variable variable;
            public Symbol symbol;
            public string word;

            public Token(Function function)
            {
                this.function = function;
            }

            public Token(Variable variable)
            {
                this.variable = variable;
            }

            public Token(Symbol symbol)
            {
                this.symbol = symbol;
            }

            public Token(string word)
            {
                this.word = word;
            }

            public Token()
            {
            }

            public TokenType getType()
            {
                if (function != null)
                    return TokenType.FUNCTION;
                else if (variable != null)
                    return TokenType.VARIABLE;
                else if (word != null)
                    return TokenType.WORD;
                else
                    return TokenType.SYMBOL;
            }

            public Variable getVariable()
            {
                return variable;
            }

            public Function getFunction()
            {
                return function;
            }

            public Symbol getSymbol()
            {
                return symbol;
            }

            public string getWord()
            {
                return word;
            }

            /**
             * If a scalar variable it returns its type, otherwise null
             */
            public VariableScalar.ScalarType getScalarType()
            {
                if (variable != null)
                    if (variable.getType() == VariableType.SCALAR)
                    {
                        return ((VariableScalar) variable).getScalarType();
                    }
                return VariableScalar.ScalarType.UNKOWN;
            }

            public string toString()
            {
                switch (getType())
                {
                    case TokenType.FUNCTION:
                        return "Func:" + function.getName();
                    case TokenType.SYMBOL:
                        return "" + symbol;
                    case TokenType.VARIABLE:
                        return variable.toString();
                    case TokenType.WORD:
                        return "Word:" + word;
                }
                throw new InvalidOperationException("Unknown type");
            }

            public Token copy()
            {
                Token t = new Token();
                t.word = word;
                t.function = function;
                t.symbol = symbol;
                t.variable = variable;

                return t;
            }
        }

        public void print()
        {
            Token t = first;
            while (t != null)
            {
                Console.WriteLine(t);
                t = t.next;
            }
        }

    }
}