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

namespace BraneCloud.Evolution.EC.Select
{
    /// <summary>
    /// Picks individuals in a population using the Stochastic Universal Selection (SUS) process, using
    /// fitnesses as returned by their fitness() methods.  This is expensive to
    /// set up and bring down, so it's not appropriate for steady-state evolution.
    /// If you're not familiar with the relative advantages of 
    /// selection methods and just want a good one,
    /// use TournamentSelection instead.   Not appropriate for
    /// multiobjective fitnesses.
    /// 
    /// <p/>By default this implementation of SUS shuffles the order of the individuals
    /// in the distribution before performing selection.  This isn't always present in classic
    /// implementations of the algorithm but it can't hurt anything and certainly can avoid
    /// certain pathological situations.  If you'd prefer not to preshuffle, set Shuffled=false
    /// Note that we don't actually change the order of the individuals in the population -- instead
    /// we maintain our own internal array of indices and shuffle that.
    /// 
    /// <p/>Like truncation selection, 
    /// SUS samples N individuals (with replacement) up front from the population,
    /// Then returns those individuals one by one.
    /// ECJ's implementation assumes that N is the size of the population -- that is, you're
    /// going to ultimately request a whole population out of this one selection method.
    /// This could be a false assumption: for example, if you only sometimes call this
    /// selection method, and sometimes TournamentSelection; or if you've got multiple
    /// pipelines.  In these cases, SUS is probably a bad choice anyway.
    /// 
    /// <p/>If you ask for <i>more</i> than a population's worth of individuals, SUS tries
    /// to handle this gracefully by reshuffling its array and starting to select over
    /// again.  But again that might suggest you are doing something wrong.
    /// 
    /// <p/><b><font color="red">
    /// Note: Fitnesses must be non-negative.  0 is assumed to be the worst fitness.
    /// </font></b>
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>shuffled</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> (default) or <tt>false</tt></font></td>
    /// <td valign="top">(should we preshuffle the array before doing selection?)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.sus
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.select.SUSSelection")]
    public class SUSSelection : SelectionMethod
    {
        #region Constants

        /// <summary>
        /// Default base.
        /// </summary>
        public const string P_SUS = "sus";
        public const string P_SHUFFLED = "shuffled";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => SelectDefaults.ParamBase.Push(P_SUS); 
        

        /// <summary>
        /// An array of pointers to individuals in the population, shuffled along with the fitnesses array.
        /// </summary>
        public int[] Indices { get; set; }

        /// <summary>
        /// The distribution of fitnesses.
        /// </summary>
        public double[] Fitnesses { get; set; }

        /// <summary>
        /// Should we shuffle first?
        /// </summary>
        public bool Shuffle { get; protected set; } = true;

        /// <summary>
        /// The floating point value to consider for the next selected individual.
        /// </summary>
        public double Offset { get; protected set; }

        /// <summary>
        /// The index in the array of the last individual selected.
        /// </summary>
        public int LastIndex { get; set; }

        /// <summary>
        /// How many samples have been done?
        /// </summary>
        public int Steps { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;
            Shuffle = state.Parameters.GetBoolean(paramBase.Push(P_SHUFFLED), def.Push(P_SHUFFLED), true);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Largely stolen from sim.util.Bag.  Shuffles both the indices and the floats.
        /// </summary>
        /// <remarks>
        /// BRS: I'm not really sure what the point is of passing in these arrays as arguments???
        /// They are simply references to the instance member arrays.
        /// </remarks>
        void ShuffleFitnessesAndIndices(IMersenneTwister random, double[] fitnesses, int[] indices)
        {
            // TODO : Figure out why Sean passes these into this method.
            //        The member arrays are basically overwritten in PrepareToProduce
            //        and the arguments here are just references to those arrays.
            //        Why not just operate on the member arrays directly?
            //        I suppose this allows one to pass in other arrays
            //        but that is currently not done anywhere.

            var numObjs = Fitnesses.Length;
            //var fitnesses = Fitnesses;
            //var indices = Indices;

            for (var x = numObjs - 1; x >= 1; x--)
            {
                var rand = random.NextInt(x + 1);
                var f = fitnesses[x];
                fitnesses[x] = fitnesses[rand];
                fitnesses[rand] = f;

                var i = indices[x];
                indices[x] = indices[rand];
                indices[rand] = i;
            }
        }

        // don't need clone etc.

        public override void PrepareToProduce(IEvolutionState s, int subpop, int thread)
        {
            base.PrepareToProduce(s, subpop, thread);

            LastIndex = 0;
            Steps = 0;

            Fitnesses = new double[s.Population.Subpops[subpop].Individuals.Count];

            // compute offset
            Offset = (double)(s.Random[thread].NextDouble() / Fitnesses.Length);

            // load fitnesses but don't build distribution yet
            for (var x = 0; x < Fitnesses.Length; x++)
            {
                Fitnesses[x] = s.Population.Subpops[subpop].Individuals[x].Fitness.Value;
                if (Fitnesses[x] < 0) // uh oh
                    s.Output.Fatal("Discovered a negative fitness value.  SUSSelection requires that all fitness values be non-negative(offending subpopulation #" + subpop + ")");
            }

            // construct and optionally shuffle fitness distribution and indices
            Indices = new int[s.Population.Subpops[subpop].Individuals.Count];
            for (var i = 0; i < Indices.Length; i++) Indices[i] = i;
            if (Shuffle) ShuffleFitnessesAndIndices(s.Random[thread], Fitnesses, Indices);

            // organize the distribution.  All zeros in fitness is fine
            RandomChoice.OrganizeDistribution(Fitnesses, true);
        }

        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            if (Steps >= Fitnesses.Length)  // we've gone too far, clearly an error
            {
                state.Output.Warning("SUSSelection was asked for too many individuals, so we're re-shuffling.  This will give you proper results, but it might suggest an error in your code.");
                var s = Shuffle;
                Shuffle = true;
                PrepareToProduce(state, subpop, thread);  // rebuild
                Shuffle = s; // just in case
            }

            // find the next index
            for ( /* empty */ ; LastIndex < Fitnesses.Length - 1; LastIndex++)
                if ((LastIndex == 0 || Offset >= Fitnesses[LastIndex - 1]) && Offset < Fitnesses[LastIndex])
                    break;

            Offset += (double)(1.0 / Fitnesses.Length);  // update for next time
            Steps++;
            return Indices[LastIndex];
        }

        #endregion // Operations
    }
}