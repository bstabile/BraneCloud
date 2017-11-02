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
using System.IO;
using System.Linq;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Simple
{
    /// <summary> 
    /// A simple default fitness, consisting of a single floating-point value
    /// where fitness A is superior to fitness B if and only if A > B.  
    /// Fitness values may range from (-infinity,infinity) exclusive -- that is,
    /// you may not have infinite fitnesses.  
    /// 
    /// <p/>Some kinds of selection methods require a more stringent definition of
    /// fitness.  For example, FitProportionateSelection requires that fitnesses
    /// be non-negative (since it must place them into a proportionate distribution).
    /// You may wish to restrict yourself to values in [0,1] or [0,infinity) in
    /// such cases.
    /// 
    /// <p/><b>Default Base</b><br/>
    /// simple.Fitness
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.simple.SimpleFitness")]
    public class SimpleFitness : Fitness
    {
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SimpleDefaults.ParamBase.Push(P_FITNESS); }
        }

        public override bool IsIdeal
        {
            get { return _isIdeal; }
        }
        protected internal bool _isIdeal;

        public override float Value
        {
            get { return _value; }
            //protected set { _value = value; }
        }
        protected internal float _value;

        #endregion // Properties
        #region Operations

        public virtual void SetFitness(IEvolutionState state, float f, bool isIdeal)
        {
            // we now allow f to be *any* value, positive or negative
            if (f == Single.PositiveInfinity || f == Single.NegativeInfinity || Single.IsNaN(f))
            {
                state.Output.Warning("Bad fitness: " + f + ", setting to 0.");
                _value = 0;
            }
            else
                _value = f;
            _isIdeal = isIdeal;
        }

        public override void SetToMeanOf(IEvolutionState state, IFitness[] fitnesses)
        {
            // this is not numerically stable.  Perhaps we should have a numerically stable algorithm for sums
            // we're presuming it's not a very large number of elements, so it's probably not a big deal,
            // since this function is meant to be used mostly for gathering trials together.
            var f = 0.0;
            var ideal = true;
            foreach (var fit in fitnesses.Select(t => (SimpleFitness)(t)))
            {
                f += fit.Value;
                ideal = ideal && fit.IsIdeal;
            }
            f /= fitnesses.Length;
            _value = (float)f;
            _isIdeal = ideal;
        }

        #endregion // Operations
        #region Comparison

        public override bool EquivalentTo(IFitness fitness)
        {
            return ((SimpleFitness)fitness).Value == Value;
        }

        public override bool BetterThan(IFitness fitness)
        {
            return Value > fitness.Value;
        }

        #endregion // Comparison
        #region IO

        public override string FitnessToString()
        {
            return FITNESS_PREAMBLE + Code.Encode(Value);
        }

        public override string FitnessToStringForHumans()
        {
            return FITNESS_PREAMBLE + Value;
        }

        /// <summary>
        /// Presently does not decode the fact that the fitness is ideal or not. 
        /// </summary>
        public override void ReadFitness(IEvolutionState state, StreamReader reader)
        {
            SetFitness(state, Code.ReadFloatWithPreamble(FITNESS_PREAMBLE, state, reader), false);
        }

        public override void WriteFitness(IEvolutionState state, BinaryWriter dataOutput)
        {
            dataOutput.Write(_value);
            dataOutput.Write(_isIdeal);
            WriteTrials(state, dataOutput);
        }

        public override void ReadFitness(IEvolutionState state, BinaryReader dataInput)
        {
            _value = dataInput.ReadSingle();
            _isIdeal = dataInput.ReadBoolean();
            ReadTrials(state, dataInput);
        }

        #endregion // IO
    }
}