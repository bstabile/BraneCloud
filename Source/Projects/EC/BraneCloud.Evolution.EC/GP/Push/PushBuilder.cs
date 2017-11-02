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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.GP.Push
{
    /// <summary>
    ///
    ///
    /// PushBuilder implements the Push-style tree building algorithm, which permits nonterminals of arbitrary arity.
    /// This algorithm is as follows:
    /// 
    /// <p/><tt><pre>
    /// BUILD-TREE(size)
    /// If size == 1, return a terminal
    /// Else
    /// .... Make a parent nonterminal p
    /// .... while (size > 0)
    /// .... .... a &lt;- random number from 1 to size
    /// .... .... size &lt;- size - a
    /// .... .... c &lt;- BUILD-TREE(a)
    /// .... .... Add c as a child of p
    /// shuffle order of children of p
    /// return p
    /// </pre></tt>
    ///
    /// <p/>You must specify a size distribution for PushBuilder.
    ///  
    /// <p/><b>Default Base</b><br/>
    /// gp.push.builder
    ///
    /// </summary>
    [ECConfiguration("ec.gp.push.PushBuilder")]
    public class PushBuilder : GPNodeBuilder
    {
        public static string P_PUSHBUILDER = "builder";

        public override IParameter DefaultBase => PushDefaults.ParamBase.Push(P_PUSHBUILDER);


        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            IParameter def = DefaultBase;

            // we use size distributions -- did the user specify any?
            if (!CanPick())
                state.Output.Fatal(
                    "PushBuilder needs a distribution of tree sizes to pick from.  You can do this by either setting a distribution (with " +
                    P_NUMSIZES + ") or with "
                    + P_MINSIZE + " and " + P_MAXSIZE + ".", paramBase, def);
        }


        // shuffles the children of the node, if any, and returns the node
        public GPNode[] Shuffle(GPNode[] objs, IEvolutionState state, int thread)
        {
            int numObjs = objs.Length;
            IMersenneTwister random = state.Random[thread];

            for (int x = numObjs - 1; x >= 1; x--)
            {
                var rand = random.NextInt(x + 1);
                var obj = objs[x];
                objs[x] = objs[rand];
                objs[rand] = obj;
            }
            return objs;
        }

        public override GPNode NewRootedTree(IEvolutionState state,
            GPType type,
            int thread,
            IGPNodeParent parent,
            GPFunctionSet set,
            int argPosition,
            int requestedSize)
        {
            int t = type.Type;
            GPNode[] terminals = set.Terminals[t];
            GPNode[] nonterminals = set.Nonterminals[t];

            if (requestedSize == NOSIZEGIVEN)
                requestedSize = PickSize(state, thread);

            GPNode n;
            if (requestedSize == 1)
            {
                // pick a random terminal
                n = terminals[state.Random[thread].NextInt(terminals.Length)].LightClone();
            }
            else
            {
                n = nonterminals[state.Random[thread].NextInt(nonterminals.Length)]
                    .LightClone(); // it's always going to be the Dummy

                // do decomposition
                byte pos = 0; // THIS WILL HAVE TO BE MODIFIED TO AN INT LATER ON AND THIS WILL AFFECT ARGPOSITIONS!!!
                IList<GPNode> list = new List<GPNode>(); // dunno if this is too expensive

                while (requestedSize >= 1)
                {
                    int amount = state.Random[thread].NextInt(requestedSize) + 1;
                    requestedSize -= amount;
                    GPNode f = NewRootedTree(state, type, thread, parent, set, pos, amount);
                    list.Add(f);
                }

                // shuffle and reassign argument position
                n.Children = list.ToArray();
                n.Children = Shuffle(n.Children, state, thread);

                for (int i = 0; i < n.Children.Length; i++)
                    n.Children[i].ArgPosition = (byte) i;
            }

            n.ResetNode(state, thread); // give ERCs a chance to randomize
            n.ArgPosition = (byte) argPosition;
            n.Parent = parent;

            return n;
        }
    }

}
