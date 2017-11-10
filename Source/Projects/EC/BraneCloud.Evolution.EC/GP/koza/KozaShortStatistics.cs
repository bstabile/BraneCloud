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
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.GP.Koza
{
    /// <summary> 
    /// A Koza-style statistics generator, intended to be easily parseable with awk or other Unix tools.  
    /// Prints fitness information, one generation (or pseudo-generation) per line.
    /// If gather-full is true, then timing information, number of nodes
    /// and depths of trees, etc. are also given.  No statistics information is given.
    /// 
    /// <p/> Each line represents a single generation.  
    /// The first items on a line are always:
    /// <ul>
    /// <li/> The generation number
    /// <li/> (if do-time) how long initialization took in milliseconds, or how long the previous generation took to breed to form this generation
    /// <li/> (if do-time) How long evaluation took in milliseconds this generation
    /// </ul>
    /// <p/>Then, (if do-subpops) the following items appear, once per each subpopulation:
    /// <ul>
    /// <li/> (if do-depth) [a|b|c...], representing the average depth of tree <i>a</i>, <i>b</i>, etc. of individuals this generation
    /// <li/> (if do-size) [a|b|c...], representing the average number of nodes used in tree <i>a</i>, <i>b</i>, etc. of individuals this generation
    /// <li/> (if do-size) The average size of an individual this generation
    /// <li/> (if do-size) The average size of an individual so far in the run
    /// <li/> (if do-size) The size of the best individual this generation
    /// <li/> (if do-size) The size of the best individual so far in the run
    /// <li/> The mean standardized fitness of the subpopulation this generation
    /// <li/> The best standardized fitness of the subpopulation this generation
    /// <li/> The best standardized fitness of the subpopulation so far in the run
    /// </ul>
    /// 
    /// <p/>Then the following items appear, for the whole population:
    /// <ul>
    /// <li/> (if do-depth) [a|b|c...], representing the average depth of tree <i>a</i>, <i>b</i>, etc. of individuals this generation
    /// <li/> (if do-size) [a|b|c...], representing the average number of nodes used in tree <i>a</i>, <i>b</i>, etc. of individuals this generation
    /// <li/> (if do-size) The average size of an individual this generation
    /// <li/> (if do-size) The average size of an individual so far in the run
    /// <li/> (if do-size) The size of the best individual this generation
    /// <li/> (if do-size) The size of the best individual so far in the run    /// <li/> The mean raw fitness of the subpop this generation
    /// <li/> The mean standardized fitness of the subpopulation this generation
    /// <li/> The best standardized fitness of the subpopulation this generation
    /// <li/> The best standardized fitness of the subpopulation so far in the run
    /// </ul>
    /// KozaStatistics assumes that every one of the Individuals in your population (and all subpopualtions) are GPIndividuals, 
    /// and further that they all have the same number of trees.
    /// 
    /// Besides the parameter below, KozaShortStatistics obeys all the SimpleShortStatistics parameters.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>do-depth</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(print depth information?)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.koza.KozaShortStatistics")]
    public class KozaShortStatistics : SimpleShortStatistics
    {
        #region Constants

        public const string P_DO_DEPTH = "do-depth";

        #endregion // Constants
        #region Fields

        long[][] _totalDepthSoFarTree;
        long[][] _totalSizeSoFarTree;
        long[][] _totalSizeThisGenTree;			// per-subpop total size of individuals this generation per tree
        long[][] _totalDepthThisGenTree;			// per-subpop total size of individuals this generation per tree

        #endregion // Fields
        #region Properties

        public bool DoDepth { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            DoDepth = state.Parameters.GetBoolean(paramBase.Push(P_DO_DEPTH), null, false);
        }

        #endregion // Setup
        #region Operations

        public override void PostInitializationStatistics(IEvolutionState state)
        {
            base.PostInitializationStatistics(state);

            _totalDepthSoFarTree = new long[state.Population.Subpops.Count][];
            _totalSizeSoFarTree = new long[state.Population.Subpops.Count][];

            for (var x = 0; x < state.Population.Subpops.Count; x++)
            {
                // check to make sure they're the right class
                if (!(state.Population.Subpops[x].Species is GPSpecies))
                    state.Output.Fatal("Subpopulation " + x +
                        " is not of the species form GPSpecies." +
                        "  Cannot do timing statistics with KozaShortStatistics.");

                var i = (GPIndividual)state.Population.Subpops[x].Individuals[0];
                _totalDepthSoFarTree[x] = new long[i.Trees.Length];
                _totalSizeSoFarTree[x] = new long[i.Trees.Length];
            }
        }

        protected void PrepareStatistics(EvolutionState state)
        {
            _totalDepthThisGenTree = new long[state.Population.Subpops.Count][];
            _totalSizeThisGenTree = new long[state.Population.Subpops.Count][];

            for (var x = 0; x < state.Population.Subpops.Count; x++)
            {
                var i = (GPIndividual)(state.Population.Subpops[x].Individuals[0]);
                _totalDepthThisGenTree[x] = new long[i.Trees.Length];
                _totalSizeThisGenTree[x] = new long[i.Trees.Length];
            }
        }

        protected void GatherExtraSubpopStatistics(EvolutionState state, int subpop, int individual)
        {
            var i = (GPIndividual)(state.Population.Subpops[subpop].Individuals[individual]);
            for (var z = 0; z < i.Trees.Length; z++)
            {
                _totalDepthThisGenTree[subpop][z] += i.Trees[z].Child.Depth;
                _totalDepthSoFarTree[subpop][z] += _totalDepthThisGenTree[subpop][z];
                _totalSizeThisGenTree[subpop][z] += i.Trees[z].Child.NumNodes(GPNode.NODESEARCH_ALL);
                _totalSizeSoFarTree[subpop][z] += _totalSizeThisGenTree[subpop][z];
            }
        }

        #endregion // Operations
        #region IO

        protected void PrintExtraSubpopStatisticsBefore(EvolutionState state, int subpop)
        {
            if (DoDepth)
            {
                state.Output.Print("[ ", StatisticsLog);
                for (var z = 0; z < _totalDepthThisGenTree[subpop].Length; z++)
                    state.Output.Print("" + (TotalIndsThisGen[subpop] > 0 ? ((double)_totalDepthThisGenTree[subpop][z]) / TotalIndsThisGen[subpop] : 0) + " ", StatisticsLog);
                state.Output.Print("] ", StatisticsLog);
            }
            if (DoSize)
            {
                state.Output.Print("[ ", StatisticsLog);
                for (var z = 0; z < _totalSizeThisGenTree[subpop].Length; z++)
                    state.Output.Print("" + (TotalIndsThisGen[subpop] > 0 ? ((double)_totalSizeThisGenTree[subpop][z]) / TotalIndsThisGen[subpop] : 0) + " ", StatisticsLog);
                state.Output.Print("] ", StatisticsLog);
            }
        }

        protected void PrintExtraPopStatisticsBefore(EvolutionState state)
        {
            var totalDepthThisGenTreePop = new long[_totalDepthSoFarTree[0].Length];
            var totalSizeThisGenTreePop = new long[_totalSizeSoFarTree[0].Length];		// will assume each subpop has the same tree size
            long totalIndsThisGenPop = 0;
            //long totalDepthThisGenPop = 0;
            //long totalDepthSoFarPop = 0;

            int subpops = state.Population.Subpops.Count;

            for (var y = 0; y < subpops; y++)
            {
                totalIndsThisGenPop += TotalIndsThisGen[y];
                for (var z = 0; z < totalSizeThisGenTreePop.Length; z++)
                    totalSizeThisGenTreePop[z] += _totalSizeThisGenTree[y][z];
                for (var z = 0; z < totalDepthThisGenTreePop.Length; z++)
                    totalDepthThisGenTreePop[z] += _totalDepthThisGenTree[y][z];
            }

            if (DoDepth)
            {
                state.Output.Print("[ ", StatisticsLog);
                foreach (var t in totalDepthThisGenTreePop)
                    state.Output.Print("" + (totalIndsThisGenPop > 0 ? ((double)t) / totalIndsThisGenPop : 0) + " ", StatisticsLog);
                state.Output.Print("] ", StatisticsLog);
            }
            if (DoSize)
            {
                state.Output.Print("[ ", StatisticsLog);
                foreach (var t in totalSizeThisGenTreePop)
                    state.Output.Print("" + (totalIndsThisGenPop > 0 ? ((double)t) / totalIndsThisGenPop : 0) + " ", StatisticsLog);
                state.Output.Print("] ", StatisticsLog);
            }
        }

        #endregion // IO
    }
}