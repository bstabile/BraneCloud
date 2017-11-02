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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP.Push
{
    /// <summary>
    /// Nonterminal is one of two GPNodes which are used to encode Push programs in ECJ GP trees.
    ///  
    /// <p/>ECJ implements Push's s-expressions as trees of nonterminals
    /// and terminals.The nonterminals are all dummy instances of the Nonterminal class.
    /// Terminals are all instances of the Terminal class.
    ///     
    /// <p/>The nonterminals and terminals aren't actually evaluated.  Instead, the
    /// tree is printed out as a lisp s-expression and sent to the Push interpreter.
    /// 
    /// <p/>The Nonterminal class can have any number of children.When writing itself out
    /// via printNodeForHumans, it displays itself as "."  But when writing itself out via
    /// printNode, it doesn't display anything at all (which produces a proper-looking Push program).
    ///        
    /// <p/>You must specify a size distribution for PushBuilder.
    /// </summary>
    /// <remarks>
    /// ECJ implements Push's s-expressions as trees of nonterminals
    /// and terminals.The nonterminals are all dummies -- this is the
    /// class in question.Notably the nonterminals also have an arbitrary
    /// arity, requiring a custom tree builder(see PushBuilder).  The terminals
    /// are instances of Terminal.java.
    /// </remarks>
    [ECConfiguration("ec.gp.push.Nonterminal")]
    public class Nonterminal : GPNode
    {
        // Note that ExpectedChildren is not overridden, so the default is CHILDREN_UKNOWN
        // which results in arbitrary arity.

        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack,
            GPIndividual individual, IProblem problem)
        {
            // do nothing at all
        }

        /// <summary>
        /// Display a "." when being printed in computer-readable fashion.
        /// </summary>
        public override string ToString()
        {
            return ".";
        }

        /// <summary>
        /// Don't print it when being displayed.
        /// </summary>
        public override string ToStringForHumans()
        {
            return "";
        }

    }

}

