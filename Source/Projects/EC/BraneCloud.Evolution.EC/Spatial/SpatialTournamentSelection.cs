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

using BraneCloud.Evolution.EC.Select;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Spatial
{
    /// <summary> 
    /// A slight modification of the tournament selection procedure for use with spatially-embedded EAs.
    /// 
    /// When selecting an individual, the SpatialTournamentSelection method selects one from the neighbors
    /// of a specific individual (as indicated by its index in the subpop).
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1 <b>or</b> 1.0 &lt; float &lt; 2.0</font></td>
    /// <td valign="top">(the tournament size)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>pick-worst</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should we pick the <i>worst</i> individual in the tournament instead of the <i>best</i>?)</td></tr>
    /// </table>
    /// Further parameters may be found in ec.select.TournamentSelection.
    /// <p/><b>Default Base</b><br/>
    /// spatial.tournament
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.spatial.SpatialTournamentSelection")]
    public class SpatialTournamentSelection : TournamentSelection
    {
        #region Constants

        /// <summary>
        /// The size of the neighborhood from where parents are selected.  Small neighborhood sizes
        /// enforce a local selection pressure, while larger values for this parameters allow further-away
        /// individuals to compete for breeding as well.
        /// </summary>
        public const string P_N_SIZE = "neighborhood-size";

        /// <summary>
        /// Some models assume an individual is always selected to compete for breeding a child that would
        /// take its location in space.  Other models don't make this assumption.  This parameter allows one
        /// to specify whether an individual will be selected to compete with others for breeding a child that
        /// will take its location in space.  If the parameter value is not specified, it is assumed to be false
        /// by default.
        /// </summary>
        public const string P_IND_COMPETES = "ind-competes";

        // Selection procedure.
        public const string P_TYPE = "type";
        public const string V_UNIFORM = "uniform";
        public const string V_RANDOM_WALK = "random-walk";
        public const int TYPE_UNIFORM = 0;
        public const int TYPE_RANDOM_WALK = 1;

        #endregion // Constants
        #region Fields

        int type;

        #endregion // Fields
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SpatialDefaults.ParamBase.Push(P_TOURNAMENT); }
        }

        public int NeighborhoodSize { get; set; }

        public bool IndCompetes { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            NeighborhoodSize = state.Parameters.GetInt(paramBase.Push(P_N_SIZE), def.Push(P_N_SIZE), 1);

            if (NeighborhoodSize < 1)
                state.Output.Fatal("Parameter not found, or its value is < 1.", paramBase.Push(P_N_SIZE),
                                   def.Push(P_N_SIZE));

            if (!state.Parameters.ParameterExists(paramBase.Push(P_TYPE), def.Push(P_TYPE)) ||
                state.Parameters.GetString(paramBase.Push(P_TYPE), def.Push(P_TYPE)).Equals(V_UNIFORM))
                type = TYPE_UNIFORM;
            else if (state.Parameters.GetString(paramBase.Push(P_TYPE), def.Push(P_TYPE)).Equals(V_RANDOM_WALK))
                type = TYPE_RANDOM_WALK;
            else
                state.Output.Fatal("Invalid parameter, must be either " + V_RANDOM_WALK + " or " + V_UNIFORM + ".",
                                   paramBase.Push(P_TYPE), def.Push(P_TYPE));

            IndCompetes = state.Parameters.GetBoolean(paramBase.Push(P_IND_COMPETES), def.Push(P_IND_COMPETES), false);
        }

        #endregion // Setup
        #region Selection

        public int GetRandomIndividual(int number, int subpop, EvolutionState state, int thread)
        {
            var sp = state.Population.Subpops[subpop];
            if (!(sp is ISpace))
                state.Output.Fatal("Subpopulation " + subpop + " is not a spatially-embedded subpopulation.\n");

            var space = (ISpace)(state.Population.Subpops[subpop]);
            var index = space.GetIndex(thread);

            if (number == 0 && IndCompetes)           // Should we just return the individual?
                return index;

            // Should we pick randomly in the space up to the given distance?
            if (type == TYPE_UNIFORM)
                return space.GetIndexRandomNeighbor(state, thread, NeighborhoodSize);

            // if (type == TYPE_RANDOM_WALK)  // Should we do a random walk?
            var oldIndex = index;

            for (var x = 0; x < NeighborhoodSize; x++)
                space.SetIndex(thread, space.GetIndexRandomNeighbor(state, thread, 1));

            var val = space.GetIndex(thread);
            space.SetIndex(thread, oldIndex);  // just in case we weren't supposed to mess around with that
            return val;
        }

        #endregion // Selection
    }
}