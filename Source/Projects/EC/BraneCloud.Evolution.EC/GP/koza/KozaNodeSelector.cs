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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP.Koza
{
    /// <summary> 
    /// KozaNodeSelector is a GPNodeSelector which picks nodes in trees a-la Koza I,
    /// with the addition of having a probability of always picking the root.
    /// The method divides the range 0.0...1.0 into four probability areas: 
    /// <ul>
    /// <li/>One area specifies that the selector must pick a terminal.
    /// <li/>Another area specifies that the selector must pick a nonterminal (if there is one, else a terminal).
    /// <li/>The third area specifies that the selector pick the root node.
    /// <li/>The fourth area specifies that the selector pick any random node.
    /// </ul>
    /// <p/>The KozaNodeSelector chooses by probability between these four situations.
    /// Then, based on the situation it has picked, it selects either a random 
    /// terminal, nonterminal, root, or arbitrary node from the tree and returns it.
    /// 
    /// <p/>As the selector picks a node, it builds up some statistics information
    /// which makes it able to pick a little faster in subsequent passes.  Thus
    /// if you want to reuse this selector on another tree, you need to call
    /// Reset() first.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>terminals</tt><br/>
    /// <font size="-1">0.0 &lt;= float &lt;= 1.0,<br/>
    /// nonterminals + terminals + root &lt;= 1.0</font></td>
    /// <td valign="top">(the probability we must pick a terminal)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>nonterminals</tt><br/>
    /// <font size="-1">0.0 &lt;= float &lt;= 1.0,<br/>
    /// nonterminals + terminals + root &lt;= 1.0</font></td>
    /// <td valign="top">(the probability we must pick a nonterminal if possible)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>root</tt><br/>
    /// <font size="-1">0.0 &lt;= float &lt;= 1.0,<br/>
    /// nonterminals + terminals + root &lt;= 1.0</font></td>
    /// <td valign="top">(the probability we must pick the root)</td></tr>
    /// </table>
    /// <p/><b>DefaultBase</b><br/>
    /// gp.koza.ns
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.koza.KozaNodeSelector")]
    public class KozaNodeSelector : IGPNodeSelector
    {
        #region Constants

        public const string P_NODESELECTOR = "ns";
        public const string P_TERMINAL_PROBABILITY = "terminals";
        public const string P_NONTERMINAL_PROBABILITY = "nonterminals";
        public const string P_ROOT_PROBABILITY = "root";

        #endregion // Constants
        #region Properties

        public virtual IParameter DefaultBase
        {
            get { return GPKozaDefaults.ParamBase.Push(P_NODESELECTOR); }
        }

        /// <summary>
        /// The probability the root must be chosen 
        /// </summary>
        public float RootProbability { get; set; }

        /// <summary>
        /// The probability a terminal must be chosen 
        /// </summary>
        public float TerminalProbability { get; set; }

        /// <summary>
        /// The probability a nonterminal must be chosen. 
        /// </summary>
        public float NonterminalProbability { get; set; }

        /// <summary>
        /// The number of nonterminals in the tree, -1 if unknown. 
        /// </summary>
        public int Nonterminals { get; set; }

        /// <summary>
        /// The number of terminals in the tree, -1 if unknown. 
        /// </summary>
        public int Terminals { get; set; }

        /// <summary>
        /// The number of nodes in the tree, -1 if unknown. 
        /// </summary>
        public int Nodes { get; set; }

        /// <summary>
        /// Used internally to look for a node.  This is threadsafe as long as
        /// an instance of KozaNodeSelector is used by only one thread. 
        /// </summary>
        public GPNodeGatherer Gatherer { get; set; }

        #endregion // Properties
        #region Setup

        public KozaNodeSelector()
        {
            Gatherer = new GPNodeGatherer();
            Reset();
        }

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            TerminalProbability = state.Parameters.GetFloatWithMax(
                paramBase.Push(P_TERMINAL_PROBABILITY), def.Push(P_TERMINAL_PROBABILITY), 0.0, 1.0);

            if (TerminalProbability == -1.0)
                state.Output.Fatal("Invalid terminal probability for KozaNodeSelector ",
                    paramBase.Push(P_TERMINAL_PROBABILITY), def.Push(P_TERMINAL_PROBABILITY));

            NonterminalProbability = state.Parameters.GetFloatWithMax(
                paramBase.Push(P_NONTERMINAL_PROBABILITY), def.Push(P_NONTERMINAL_PROBABILITY), 0.0, 1.0);

            if (NonterminalProbability == -1.0)
                state.Output.Fatal("Invalid nonterminal probability for KozaNodeSelector ",
                    paramBase.Push(P_NONTERMINAL_PROBABILITY), def.Push(P_NONTERMINAL_PROBABILITY));

            RootProbability = state.Parameters.GetFloatWithMax(
                paramBase.Push(P_ROOT_PROBABILITY), def.Push(P_ROOT_PROBABILITY), 0.0, 1.0);

            if (RootProbability == -1.0)
                state.Output.Fatal("Invalid root probability for KozaNodeSelector ",
                    paramBase.Push(P_ROOT_PROBABILITY), def.Push(P_ROOT_PROBABILITY));

            if (RootProbability + TerminalProbability + NonterminalProbability > 1.0f)
            {
                state.Output.Fatal("The terminal, nonterminal, and root for KozaNodeSelector"
                    + paramBase + " may not sum to more than 1.0. (" + TerminalProbability + " "
                    + NonterminalProbability + " " + RootProbability + ")", paramBase);
            }

            Reset();
        }

        public virtual void Reset()
        {
            Nonterminals = Terminals = Nodes = -1;
        }

        #endregion // Setup
        #region Operations

        public virtual GPNode PickNode(IEvolutionState s, int subpop, int thread, GPIndividual ind, GPTree tree)
        {
            var rnd = s.Random[thread].NextFloat();

            if (rnd > NonterminalProbability + TerminalProbability + RootProbability)
            // pick anyone
            {
                if (Nodes == -1)
                    Nodes = tree.Child.NumNodes(GPNode.NODESEARCH_ALL);
                {
                    tree.Child.NodeInPosition(s.Random[thread].NextInt(Nodes), Gatherer, GPNode.NODESEARCH_ALL);
                    return Gatherer.Node;
                }
            }
            if (rnd > NonterminalProbability + TerminalProbability)
            // pick the root
            {
                return tree.Child;
            }
            if (rnd > NonterminalProbability)
            // pick terminals
            {
                if (Terminals == -1)
                    Terminals = tree.Child.NumNodes(GPNode.NODESEARCH_TERMINALS);

                tree.Child.NodeInPosition(s.Random[thread].NextInt(Terminals), Gatherer, GPNode.NODESEARCH_TERMINALS);
                return Gatherer.Node;
            }
            // pick nonterminals if you can

            if (Nonterminals == -1)
                Nonterminals = tree.Child.NumNodes(GPNode.NODESEARCH_NONTERMINALS);
            if (Nonterminals > 0)
            // there are some nonterminals
            {
                tree.Child.NodeInPosition(s.Random[thread].NextInt(Nonterminals), Gatherer, GPNode.NODESEARCH_NONTERMINALS);
                return Gatherer.Node;
            }
            // there ARE no nonterminals!  It must be the root node

            return tree.Child;
        }

        #endregion // Operations
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                var s = (KozaNodeSelector)MemberwiseClone();
                // allocate a new gatherer, so we're always threadsafe
                s.Gatherer = new GPNodeGatherer();
                s.Reset();
                return s;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        #endregion // Cloning
    }
}