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

namespace BraneCloud.Evolution.EC.Spatial
{
    /// <summary>      
    /// A Spatial1DSubpopulation is an EC subpop that is additionally embedded into
    /// a one-dimmensional space.
    /// In a spatially-embedded EA, the subpops of individuals are assumed to be
    /// spatially distributed in some sort of space, be it one-dimmensional, two-
    /// dimmensional, or whatever else.  The space may or may not be toroidal (although
    /// it usually is).  Each location in the space has a set of neighboring locations.
    /// Thus, each individual has an index in the subpop, and also a location in
    /// the space.
    /// 
    /// <p/>This public interface provides a method to obtain the indexes of the neighbors
    /// of a location.
    /// 
    /// <p/>This Subpopulation does not include toroidalness in writing out to streams.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>Toroidal</tt><br/>
    /// <font size="-1">true (default) or false</font></td>
    /// <td valign="top">(Is this space toroidal?)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.spatial.Spatial1DSubpopulation")]
    public class Spatial1DSubpopulation : Subpopulation, ISpace
    {
        #region Constants

        /// <summary>
        /// This parameter stipulates whether the world is toroidal or not.
        /// If missing, its default value is true.
        /// </summary>
        public const string P_TOROIDAL = "toroidal";

        #endregion // Constants
        #region Properties

        public bool Toroidal { get; set; }

        /// <summary>
        /// Indexed by threadnum.
        /// </summary>
        public int[] Indexes { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Read additional parameters for the spatially-embedded subpop.
        /// </summary>
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // by default, the space is toroidal
            Toroidal = state.Parameters.GetBoolean(paramBase.Push(P_TOROIDAL), null, true);
        }

        #endregion // Setup
        #region Operations

        public virtual void SetIndex(int threadnum, int index)
        {
            if (Indexes == null)
                Indexes = new int[threadnum + 1];
            if (threadnum >= Indexes.Length)
            {
                var currentSize = Indexes.Length;
                var temp = new int[threadnum * 2 + 1];
                Array.Copy(Indexes, 0, temp, 0, currentSize);
                Indexes = temp;
            }
            Indexes[threadnum] = index;
        }

        public virtual int GetIndex(int threadnum)
        {
            if (Indexes == null || threadnum > Indexes.Length)
                return -1;

            return Indexes[threadnum];
        }

        /// <summary>
        /// Returns a the index of a random neighbor.
        /// </summary>
        public virtual int GetIndexRandomNeighbor(IEvolutionState state, int threadnum, int distance)
        {
            var index = Indexes[threadnum];
            var size = Individuals.Count;
            if (size == 0)
                return index;
            if (Toroidal)
            {
                var max = (2 * distance + 1 > size) ? size : (2 * distance + 1);
                var rand = state.Random[threadnum].NextInt(max);
                var val = (index + rand - distance);
                if (val >= 0 && val < size) return val;
                val = val % size;
                if (val >= 0) return val;
                return val + size;
            }
            else
            {
                var min = (index - distance < 0) ? 0 : (index - distance);
                var max = (index + distance >= size) ? size : (index + distance);
                return min + state.Random[threadnum].NextInt(max - min + 1);
            }
        }

        #endregion // Operations
    }
}