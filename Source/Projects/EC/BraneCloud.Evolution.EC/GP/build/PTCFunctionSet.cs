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
    /// PTCFunctionSet is a GPFunctionSet which adheres to IPTCFunctionSet, and thus
    /// can be used with the PTC1 and PTC2 methods.  Terminal and nonterminal probabilities
    /// for nodes used in this function set are determined by the <tt>prob</tt> parameter
    /// for the nodes' GPNodeConstraints object.  That's not the greatest solution,
    /// because it could require making a lot of different GPNodeConstraints, customized for each
    /// node, but it's the best I can do for now.
    /// 
    /// The nonterminalSelectionProbabilities() method computes nonterminal selection
    /// probability using the probabilities above, per type, for the size requested.
    /// If the size is small enough (smaller than CACHE_SIZE), then the result is
    /// memoized so it doesn't need to be computed again next time.
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.gp.build.PTCFunctionSet")]
    public class PTCFunctionSet : GPFunctionSet, IPTCFunctionSet
    {
        #region Constants

        public const int CACHE_SIZE = 1024;

        #endregion // Constants
        #region Properties

        /// <summary>
        /// Terminal probabilities[type][thenodes], in organized form 
        /// </summary>
        public double[][] q_ty { get; set; }

        /// <summary>
        /// Nonterminal probabilities[type][thenodes], in organized form 
        /// </summary>
        public double[][] q_ny { get; set; }

        /// <summary>
        /// Cache of nonterminal selection probabilities -- dense array 
        /// [size-1][type].  If any items are null, they're not in the dense cache. 
        /// </summary>
        public double[][] p_y { get; set; }

        #endregion // Properties

        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // load our probabilities here.

            q_ny = new double[Nonterminals.Length][];
            q_ty = new double[Terminals.Length][];

            var allOnes = true;
            var noOnes = true;
            var allZeros = true;
            var initializer = ((GPInitializer)state.Initializer);

            for (var type = 0; type < Nonterminals.Length; type++)
            {
                q_ny[type] = new double[Nonterminals[type].Length];
                for (var x = 0; x < Nonterminals[type].Length; x++)
                {
                    q_ny[type][x] = Nonterminals[type][x].Constraints(initializer).ProbabilityOfSelection;
                    if (q_ny[type][x] != 0.0)
                        allZeros = false;
                    if (q_ny[type][x] == 1.0)
                        noOnes = false;
                    else
                        allOnes = false;
                }
            }

            if (allZeros)
                state.Output.Warning("In this function set, the probabilities of all nonterminal functions have a 0.0 selection probability"
                                            + " -- this will cause them all to be selected uniformly.  That could be an error.", paramBase);

            // BRS : TODO : Investigate the "allZeroes" logic as described below...
            // In ECJ v20 the following is reinitialized to false, 
            // but I think that is a BUG because it is about to check again 
            // and set it to false if any of the probabilities do NOT equal zero.
            // allZeros = false;
            // I'm setting this to true for the reason described above!
            allZeros = true;

            for (var type = 0; type < Terminals.Length; type++)
            {
                q_ty[type] = new double[Terminals[type].Length];
                for (var x = 0; x < Terminals[type].Length; x++)
                {
                    q_ty[type][x] = Terminals[type][x].Constraints(initializer).ProbabilityOfSelection;
                    if (q_ty[type][x] != 0.0)
                        allZeros = false;
                    if (q_ty[type][x] == 1.0)
                        noOnes = false;
                    else
                        allOnes = false;
                }
            }

            if (allZeros)
                state.Output.Warning("In this function set, the probabilities of all terminal functions have a 0.0 selection probability"
                                            + " -- this will cause them all to be selected uniformly.  That could be an error.", paramBase);

            if (!allOnes && !noOnes)
                state.Output.Warning("In this function set, there are some functions with a selection probability of 1.0,"
                                                            + " but not all of them.  That could be an error.", paramBase);

            // set up our node probabilities.  Allow all zeros.
            for (var x = 0; x < q_ty.Length; x++)
            {
                if (q_ty[x].Length == 0)
                    state.Output.Warning("Function Set " + Name + " has no terminals for type number " + x + ".  This may cause problems for you.");
                else
                    RandomChoice.OrganizeDistribution(q_ty[x], true);
                if (q_ny[x].Length == 0)
                    state.Output.Warning("Function Set " + Name + " has no nonterminals for type number " + x + ".  This may cause problems for you.");
                else
                    RandomChoice.OrganizeDistribution(q_ny[x], true);
            }

            // set up cache
            p_y = new double[CACHE_SIZE][];
        }

        #endregion // Setup
        #region Operations

        public virtual double[] TerminalProbabilities(int type)
        {
            return q_ty[type];
        }

        public virtual double[] NonterminalProbabilities(int type)
        {
            return q_ny[type];
        }

        public virtual double[] NonterminalSelectionProbabilities(int expectedTreeSize)
        {
            // check cache first
            if (expectedTreeSize < CACHE_SIZE)
            {
                if (p_y[expectedTreeSize - 1] != null)
                    return p_y[expectedTreeSize - 1];

                return p_y[expectedTreeSize - 1] = ComputeNonterminalSelectionProbabilities(expectedTreeSize);
            }
            // we'll have to compute it
            return ComputeNonterminalSelectionProbabilities(expectedTreeSize);
        }

        public virtual double[] ComputeNonterminalSelectionProbabilities(int expectedTreeSize)
        {
            var p = new double[q_ny.Length];

            // for each type...
            for (var x = 0; x < q_ny.Length; x++)
            {
                double count = 0;
                // gather branching factor * prob for each nonterminal
                for (var y = 0; y < q_ny[x].Length; y++)
                    count += (y == 0 ? q_ny[x][y] : q_ny[x][y] - q_ny[x][y - 1]) // it's organized
                        * Nonterminals[x][y].Children.Length;

                p[x] = (1.0 - (1.0 / expectedTreeSize)) / count;
            }
            return p;
        }

        #endregion // Operations

        // BRS : TODO : If implementing ISerializable
        //protected PTCFunctionSet(SerializationInfo info, StreamingContext context) : base(info, context)
        //{
        //}
        //public PTCFunctionSet()
        //{
        //}
    }
}