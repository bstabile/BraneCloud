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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.GP.GE
{
    /// <summary>
    /// GEIndividual is a simple subclass of ByteVectorIndividual which not only prints out (for humans)
    /// the Individual as a byte vector but also prints out the Individual's tree representation.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ge.GEIndividual")]
    public class GEIndividual : ByteVectorIndividual
    {
        #region Constants

        public const string GP_PREAMBLE = "Equivalent GP Individual:";
        public const string ERC_PREAMBLE = "ERCs: ";
        public const string BAD_TREE = "[BAD]";

        #endregion // Constants
        #region Operations

        public override void PrintIndividualForHumans(IEvolutionState state, int log)
        {
            base.PrintIndividualForHumans(state, log);

            IDictionary<int, GPNode> ERCmap = new Dictionary<int, GPNode>();

            // print out Trees
            state.Output.PrintLn(GP_PREAMBLE, log);
            GPIndividual ind = (((GESpecies) Species).Map(state, this, 0, ERCmap));
            if (ind == null) state.Output.PrintLn(BAD_TREE, log);
            else ind.PrintTrees(state, log);

            // print out ERC mapping
            state.Output.Print(ERC_PREAMBLE, log);
            foreach (var key in ERCmap.Keys)
            {
                var val = ERCmap[key];
                state.Output.Print("    " + key + " -> " + val.ToStringForHumans(), log);
            }
            state.Output.PrintLn("", log);
        }

        #endregion // Operations
    }
}