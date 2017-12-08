using System;
using System.Collections.Generic;

namespace SharpMatrix.Equation
{
    //package org.ejml.equation;

/**
 * Definition of a macro.  Each input will replace the word of its name
 *
 * @author Peter Abeles
 */
    public class Macro
    {
        public string name;
        public List<string> inputs = new List<string>();
        public TokenList tokens;

        public TokenList execute(List<TokenList.Token> replacements)
        {
            TokenList output = new TokenList();

            TokenList.Token t = tokens.getFirst();
            while (t != null)
            {
                if (t.word != null)
                {
                    bool matched = false;
                    for (int i = 0; i < inputs.Count; i++)
                    {
                        if (inputs[i].Equals(t.word))
                        {
                            output.insert(output.last, replacements[i].copy());
                            matched = true;
                            break;
                        }
                    }
                    if (!matched)
                    {
                        output.insert(output.last, t.copy());
                    }
                }
                else
                {
                    output.insert(output.last, t.copy());
                }
                t = t.next;
            }
            return output;
        }

        public class Assign : Operation
        {
            protected internal Assign(IDictionary<string, Macro> macros, Macro macro)
                : base("Macro:" + macro.name)
            {
                process = () => macros.Add(macro.name, macro);
            }
        }

        public Operation createOperation(Dictionary<string, Macro> macros)
        {
            return new Assign(macros, this);
        }
    }
}