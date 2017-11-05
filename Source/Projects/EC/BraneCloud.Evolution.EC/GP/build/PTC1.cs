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
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.GP.Build
{
    /// <summary> 
    /// PTC1 implements the "Strongly-typed Probabilistic Tree Creation 1 (PTC1)" algorithm described in 
    /// 
    /// <p/>Luke, Sean. 2000. <i>Issues in Scaling Genetic Programming: Breeding Strategies, Tree Generation, and Code Bloat.</i> Ph.D. Dissertation, Department of Computer Science, University of Maryland, College Park, Maryland. 
    /// 
    /// <p/> ...and also in
    /// 
    /// <p/>Luke, Sean. 2000. Two fast tree-creation algorithms for genetic programming. In <i>IEEE Transactions on Evolutionary Computation</i> 4:3 (September 2000), 274-283. IEEE. 
    /// 
    /// <p/> Both can be found at <a href="http://www.cs.gmu.edu/~sean/papers/">http://www.cs.gmu.edu/~sean/papers/</a>
    /// 
    /// <p/> PTC1 requires that your function set to implement PTCFunctionSetForm.  The
    /// provided function set, PTCFunctionSet, does exactly this.
    /// 
    /// <p/>The Strongly-typed PTC1 algorithm is a derivative of the GROW algorithm
    /// used in ec.gp.koza.GrowBuilder.  The primary differences are:
    /// 
    /// <ul>
    /// <li/> PTC1 guarantees that trees generated will have an <i>expected</i> (mean) tree size, provided by the user.  There is no guarantee on variance.  This is different from GROW, which doesn't give any user control at all.
    /// <li/> PTC1 does not have a min-depth value.  In essence, PTC1's min-depth value is always set to 1.
    /// <li/> PTC1's max-depth value should really only be used to enforce a large memory restriction.  Unlike GROW, where it's used to keep GROW from going nuts.
    /// <li/> PTC1 has provisions for picking nonterminals with various probabilities over other nonterminals (and likewise for terminals).  To use this, tweak the IPTCFunctionSet object.
    /// </ul>
    /// 
    /// PTC1 assumes that the requested size passed to NewRootedTree(...) is the <i>expected</i> size.   If the value is NOSIZEGIVEN, then PTC1 will use the expected size defined by the <tt>expected-size</tt> parameter.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>expected-size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">default expected tree size</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>max-depth</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">maximum allowable tree depth (usually a big value)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.build.PTC1")]
    public class PTC1 : GPNodeBuilder
    {
        #region Constants

        public const string P_PTC1 = "ptc1";
        public const string P_EXPECTED = "expected-size";
        public const string P_MAXDEPTH = "max-depth";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase=> GPBuildDefaults.ParamBase.Push(P_PTC1);
       

        /// <summary>
        /// The largest maximum tree depth PTC1 can specify -- should be big. 
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// The default expected tree size for PTC1 
        /// </summary>
        public int ExpectedSize { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            ExpectedSize = state.Parameters.GetInt(paramBase.Push(P_EXPECTED), def.Push(P_EXPECTED), 1);
            if (ExpectedSize < 1)
                state.Output.Fatal("Default expected size must be >= 1", paramBase.Push(P_EXPECTED), def.Push(P_EXPECTED));

            MaxDepth = state.Parameters.GetInt(paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH), 1);
            if (MaxDepth < 1)
                state.Output.Fatal("Maximum depth must be >= 1", paramBase.Push(P_MAXDEPTH), def.Push(P_MAXDEPTH));
        }

        #endregion // Setup
        #region Operations

        public override GPNode NewRootedTree(IEvolutionState state, GPType type, int thread, IGPNodeParent parent, GPFunctionSet funcs, int ArgPosition, int requestedSize)
        {
            if (!(funcs is IPTCFunctionSet))
                state.Output.Fatal("Set " + funcs.Name + " is not of the form ec.gp.build.IPTCFunctionSet, and so cannot be used with PTC Nodebuilders.");
            
            // build the tree
            if (requestedSize == NOSIZEGIVEN) // use the default
            {
                return Ptc1(state, 0, type, thread, parent, ArgPosition, funcs, (IPTCFunctionSet) funcs, ((IPTCFunctionSet) funcs).NonterminalSelectionProbabilities(ExpectedSize));
            }
            if (requestedSize < 1)
                state.Output.Fatal("ec.gp.build.PTC1 was requested to build a tree, but a requested size was given that is < 1.");
            return Ptc1(state, 0, type, thread, parent, ArgPosition, funcs, (IPTCFunctionSet) funcs, 
                ((IPTCFunctionSet) funcs).NonterminalSelectionProbabilities(requestedSize));
        }
        
        /// <summary>
        /// A private function which recursively returns a GROW tree to NewRootedTree(...) 
        /// </summary>
        private GPNode Ptc1(IEvolutionState state, int current, GPType type, int thread, IGPNodeParent parent, 
            int argPosition, GPFunctionSet funcs, IPTCFunctionSet pfuncs, double[] nonterminalSelectProbs)
        {
            // ptc1 can mess up if there are no available terminals for a given type.  If this occurs,
            // and we find ourselves unable to pick a terminal when we want to do so, we will issue a warning,
            // and pick a nonterminal, violating the PTC1 size and depth contracts.  This can lead to pathological situations
            // where the system will continue to go on and on unable to stop because it can't pick a terminal,
            // resulting in running out of memory or some such.  But there are cases where we'd want to let
            // this work itself out.
            var triedTerminals = false;
            
            var t = type.Type;
            var terminals = funcs.Terminals[t];
            var nonterminals = funcs.Nonterminals[t];
            var nodes = funcs.Nodes[t];
            
            if (nodes.Length == 0)
                ErrorAboutNoNodeWithType(type, state); // total failure
            
            // Now pick if we're at max depth
            // OR if we're below p_y
            // OR if there are NO nonterminals!
            // [first set triedTerminals]
            // AND if there are available terminals
            if (((current + 1 >= MaxDepth) 
                    || !(state.Random[thread].NextBoolean(nonterminalSelectProbs[t])) 
                    || WarnAboutNonterminal(nonterminals.Length == 0, type, false, state)) 
                    && (triedTerminals = true) && terminals.Length != 0)
            {
                var n = terminals[RandomChoice.PickFromDistribution(pfuncs.TerminalProbabilities(t), 
                                                state.Random[thread].NextDouble())].LightClone();

                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte) argPosition;
                n.Parent = parent;
                return n;
            }
            // above p_y, pick a nonterminal by q_ny probabilities
            else
            {
                if (triedTerminals)
                    WarnAboutNoTerminalWithType(type, false, state); // we tried terminals and we're here because there were none!
                
                var n = nonterminals[RandomChoice.PickFromDistribution(pfuncs.NonterminalProbabilities(t), 
                                                        state.Random[thread].NextDouble())].LightClone();

                n.ResetNode(state, thread); // give ERCs a chance to randomize
                n.ArgPosition = (sbyte) argPosition;
                n.Parent = parent;
                
                // Populate the node...
                var childtypes = n.Constraints((GPInitializer) state.Initializer).ChildTypes;
                for (var x = 0; x < childtypes.Length; x++)
                    n.Children[x] = Ptc1(state, current + 1, childtypes[x], thread, n, x, funcs, pfuncs, nonterminalSelectProbs);

                return n;
            }
        }

        #endregion // Operations
    }
}