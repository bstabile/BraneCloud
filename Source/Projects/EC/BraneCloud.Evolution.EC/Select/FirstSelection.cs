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
using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Select
{
    /// <summary> 
    /// Always picks the first individual in the subpop.  This is mostly for testing purposes.
    /// 
    /// <p/><b>Default Base</b><br/>
    /// select.first
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.select.FirstSelection")]
    public class FirstSelection : SelectionMethod, ISteadyStateBSource
    {
        #region Constants

        /// <summary>
        /// Default base 
        /// </summary>
        public const string P_FIRST = "first";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => SelectDefaults.ParamBase.Push(P_FIRST); 

        #endregion // Properties
        #region Operations

        /// <summary>
        /// I hard-code both Produce(...) methods for efficiency's sake.
        /// </summary>
        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            return 0;
        }
                
        /// <summary>
        /// I hard-code both Produce(...) methods for efficiency's sake.
        /// </summary>
        public int Produce(
            int min, 
            int max, 
            int start,
            int subpop, 
            IList<Individual> inds, 
            IEvolutionState state, 
            int thread, 
            IDictionary<string, object> misc)
        {
            var n = 1;
            if (n > max) n = max;
            if (n < min) n = min;
            
            for (var q = 0; q < n; q++)
            {
                // pick size random individuals, then pick the best.
                var oldinds = state.Population.Subpops[subpop].Individuals;
                inds[start + q] = oldinds[0]; // note it's a pointer transfer, not a copy!
                if (misc != null && misc.ContainsKey(KEY_PARENTS))
                {
                    IntBag parent = new IntBag(1);
                    parent.Add(0);
                    ((IntBag[])misc[KEY_PARENTS])[start + q] = parent;
                }
            }
            return n;
        }
        
        public virtual void  IndividualReplaced(SteadyStateEvolutionState state, int subpop, int thread, int individual)
        {
            return ;
        }
        
        public virtual void  SourcesAreProperForm(SteadyStateEvolutionState state)
        {
            return ;
        }

        #endregion // Operations
    }
}