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

namespace BraneCloud.Evolution.EC.Select
{
    /// <summary> 
    /// MultiSelection is a SelectionMethod which stores some <i>n</i> subordinate SelectionMethods.  
    /// Each time it must produce an individual, it picks one of these SelectionMethods 
    /// at random and has it do the production instead.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>Produce(...)</tt> call</b><br/>
    /// Always 1.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>num-selects</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(The number of subordinate SelectionMethods)</td></tr>
    /// <tr><td valign="top"/><i>base</i>.<tt>select.</tt><i>n</i><br/>
    /// <font size="-1"/>classname, inherits and != <tt>SelectionMethod</tt><br/>
    /// <td valign="top">(Subordinate SelectionMethod <i>n</i>)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.multiselect
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>select.</tt><i>n</i><br/>
    /// <td>Subordinate SelectionMethod <i>n</i></td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.select.MultiSelection")]
    public class MultiSelection : SelectionMethod
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_NUMSELECTS = "num-selects";
        public const string P_SELECT = "select";
        public const string P_MULTISELECT = "multiselect";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_MULTISELECT); }
        }

        /// <summary>
        /// The MultiSelection's individuals 
        /// </summary>
        public SelectionMethod[] Selects { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            var numSelects = state.Parameters.GetInt(paramBase.Push(P_NUMSELECTS), def.Push(P_NUMSELECTS), 1);
            if (numSelects == 0)
                state.Output.Fatal("The number of MultiSelection sub-selection methods must be >= 1).",
                                                    paramBase.Push(P_NUMSELECTS), def.Push(P_NUMSELECTS));

            // make our arrays
            Selects = new SelectionMethod[numSelects];

            var total = 0.0;

            for (var x = 0; x < numSelects; x++)
            {
                var p = paramBase.Push(P_SELECT).Push("" + x);
                var d = def.Push(P_SELECT).Push("" + x);

                Selects[x] = (SelectionMethod)state.Parameters.GetInstanceForParameter(p, d, typeof(SelectionMethod));
                Selects[x].Setup(state, p);

                // now check probability
                if (Selects[x].Probability < 0.0)
                    state.Output.Error("MultiSelection select #" + x + " must have a probability >= 0.0", p.Push(P_PROB), d.Push(P_PROB));
                else
                    total += Selects[x].Probability;
            }

            state.Output.ExitIfErrors();

            // Now check for valid probability
            if (total <= 0.0)
                state.Output.Fatal("MultiSelection selects do not sum to a positive probability", paramBase);

            if (!total.Equals(1.0))
            {
                state.Output.Message("Must normalize probabilities for " + paramBase);
                for (var x = 0; x < numSelects; x++)
                    Selects[x].Probability /= total;
            }

            // totalize
            var tmp = 0.0;
            for (var x = 0; x < numSelects - 1; x++)
            // yes, it's off by one
            {
                tmp += Selects[x].Probability;
                Selects[x].Probability = tmp;
            }
            Selects[numSelects - 1].Probability = 1.0;
        }

        #endregion // Setup
        #region Operations

        public override bool Produces(IEvolutionState state, Population newpop, int subpop, int thread)
        {
            if (!base.Produces(state, newpop, subpop, thread))
                return false;

            for (var x = 0; x < Selects.Length; x++)
                if (!Selects[x].Produces(state, newpop, subpop, thread))
                    return false;
            return true;
        }

        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            foreach (SelectionMethod sm in Selects)
                sm.PrepareToProduce(s, subpop, thread);
        }

        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            return Selects[PickRandom(Selects, state.Random[thread].NextDouble())].Produce(subpop, state, thread);
        }

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            return Selects[PickRandom(Selects, state.Random[thread].NextDouble())].Produce(min, max, start, subpop, inds, state, thread);
        }

        public override void PreparePipeline(object hook)
        {
            // the default form calls this on all the selects.
            // note that it follows all the source paths even if they're
            // duplicates
            foreach (SelectionMethod sm in Selects)
                sm.PreparePipeline(hook);
        }

        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (MultiSelection)base.Clone();

            // make a new array
            c.Selects = new SelectionMethod[Selects.Length];

            // clone the selects -- we won't go through the hassle of
            // determining if we have a DAG or not -- we'll just clone
            // it out to a tree.  I doubt it's worth it.

            for (var x = 0; x < Selects.Length; x++)
                c.Selects[x] = (SelectionMethod)Selects[x].Clone();

            return c;
        }

        #endregion // Cloning
    }
}