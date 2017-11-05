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

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// A BreedingSource is an IPrototype which 
    /// provides Individuals to populate new populations based on old ones.  
    /// The BreedingSource/BreedingPipeline/SelectionMethod mechanism
    /// is inherently designed to work within single subpops, which is by far the most common case.  
    /// If for some reason you need to breed among different subpops to produce new ones
    /// in a manner that can't be handled with exchanges, you will probably have to
    /// write your own custom Breeder; you'd have to write your own custom breeding
    /// pipelines anyway of course, though you can probably get away with reusing
    /// the SelectionMethods.
    /// 
    /// <p/>A BreedingSource may have parent sources which feed it as well.
    /// Some BreedingSources, <i>SelectionMethods</i>,
    /// are meant solely to plug into other BreedingSources, <i>BreedingPipelines</i>.
    /// BreedingPipelines can plug into other BreedingPipelines, and can also be
    /// used to provide the final Individual meant to populate a new generation.
    /// 
    /// <p/>Think of BreedingSources as Streams of Individuals; at one end of the
    /// stream is the provider, a SelectionMethod, which picks individuals from
    /// the old population.  At the other end of the stream is a BreedingPipeline
    /// which hands you the finished product, a small set of new Individuals
    /// for you to use in populating your new population.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i><tt>.prob</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt;= 1.0, or undefined</font></td>
    /// <td valign="top">(probability this BreedingSource gets chosen.  
    /// Undefined is only valid if the caller of this BreedingSource doesn't need a Probability)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.BreedingSource")]
    public abstract class BreedingSource : IBreedingSource
    {
        #region Constants

        public const string P_PROB = "prob";
        public const double NO_PROBABILITY = -1.0;

        #endregion // Constants
        #region Properties

        public abstract IParameter DefaultBase { get; }

        /// <summary>
        /// The probability that this BreedingSource will be chosen 
        /// to breed over other BreedingSources.  This may or may
        /// not be used, depending on what the caller to this BreedingSource is.
        /// It also might be modified by external sources owning this object,
        /// for their own purposes.  A BreedingSource should not use it for
        /// any purpose of its own, nor modify it except when setting it up.
        /// <p/>The most common modification is to normalize it with some other
        /// set of probabilities, then set all of them up in increasing summation;
        /// this allows the use of the fast static BreedingSource-picking utility
        /// method, BreedingSource.pickRandom(...).  In order to use this method,
        /// for example, if four
        /// breeding source probabilities are {0.3, 0.2, 0.1, 0.4}, then
        /// they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Returns the "typical" number of individuals generated with one call of Produce(...). 
        /// </summary>
        public abstract int TypicalIndsProduced { get; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Sets up the BreedingPipeline.  You can use state.Output.error here
        /// because the top-level caller promises to call exitIfErrors() after calling
        /// Setup.  Note that probability might get modified again by
        /// an external source if it doesn't normalize right. 
        /// <p/>The most common modification is to normalize it with some other
        /// set of probabilities, then set all of them up in increasing summation;
        /// this allows the use of the fast static BreedingSource-picking utility
        /// method, BreedingSource.pickRandom(...).  In order to use this method,
        /// for example, if four
        /// breeding source probabilities are {0.3, 0.2, 0.1, 0.4}, then
        /// they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.
        /// </summary>
        /// <seealso cref="IPrototype.Setup(IEvolutionState,IParameter)"/>
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            if (!state.Parameters.ParameterExists(paramBase.Push(P_PROB), def.Push(P_PROB)))
                Probability = NO_PROBABILITY;
            else
            {
                Probability = state.Parameters.GetDouble(paramBase.Push(P_PROB), def.Push(P_PROB), 0.0);
                if (Probability < 0.0)
                    state.Output.Error("Breeding Source's Probability must be a double floating point value >= 0.0, or empty, which represents NO_PROBABILITY.",
                        paramBase.Push(P_PROB), def.Push(P_PROB));
            }
        }

        #endregion // Setup
        #region Operations

        #region Probability

        public double GetProbability(object obj)
        {
            return ((IBreedingSource)obj).Probability;
        }

        public void SetProbability(object obj, double prob)
        {
            ((IBreedingSource)obj).Probability = prob;
        }

        /// <summary>
        /// Normalizes and arranges the probabilities in sources so that they
        /// are usable by pickRandom(...).  If the sources have all zero probabilities,
        /// then a uniform selection is used.  Negative probabilities will
        /// generate an ArithmeticException, as will an empty source array. 
        /// </summary>
        public static void SetupProbabilities(IBreedingSource[] sources)
        {
            RandomChoice.OrganizeDistribution(sources, sources[0], true);
        }

        void IBreedingSource.SetupProbabilities(IBreedingSource[] sources)
        {
            SetupProbabilities(sources);
        }

        /// <summary>
        /// Picks a random source from an array of sources, with their
        /// probabilities normalized and summed as follows:  For example,
        /// if four
        /// breeding source probabilities are {0.3, 0.2, 0.1, 0.4}, then
        /// they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}. 
        /// </summary>		
        public static int PickRandom(IBreedingSource[] sources, double prob)
        {
            return RandomChoice.PickFromDistribution(sources, sources[0], prob);
        }

        int IBreedingSource.PickRandom(IBreedingSource[] sources, double prob)
        {
            return PickRandom(sources, prob);
        }

        #endregion // Probability

        /// <summary>
        /// A hook which should be passed to all your subsidiary breeding
        /// sources.  The default does this for you already, so ordinarily
        /// you don't need to change anything.  If you are a BreedingPipeline and you
        /// implement your sources in a way different
        /// than using the sources[] array, be sure to override this method
        /// so that it calls preparePipeline(hook) on all of your sources.
        /// 
        /// <p/>ECJ at present does not custom-implement or call this method: 
        /// it's available for you. Becuase it has custom functionality, 
        /// this method might get called more than once, and by various objects
        /// as needed.  If you use it, you should determine somehow how to use
        /// it to send information under the assumption that it might be sent
        /// by nested items in the pipeline; you don't want to scribble over
        /// each other's calls! Note that this method should travel *all*
        /// breeding source paths regardless of whether or not it's redundant to
        /// do so. 
        /// </summary>
        public abstract void PreparePipeline(object hook);

        /// <summary>
        /// Returns true if this BreedingSource, when attached to the given
        /// subpop, will produce individuals of the subpop species.
        /// SelectionMethods should additionally make sure that their Fitnesses are
        /// of a valid type, if necessary. newpop *may* be the same as state.Population
        /// </summary>	
        public abstract bool Produces(IEvolutionState state, Population newpop, int subpop, int thread);

        /// <summary>
        /// Called before Produce(...), usually once a generation, or maybe only
        /// once if you're doing steady-state evolution, to let the breeding source
        /// "warm up" prior to producing.  Individuals should be produced from
        /// old individuals in positions [start...start+length] in the subpop 
        /// only.  May be called again to reset the BreedingSource for a whole
        /// 'nuther subpop. 
        /// </summary>		
        public abstract void PrepareToProduce(IEvolutionState state, int subpop, int thread);

        /// <summary>
        /// Called after Produce(...), usually once a generation, or maybe only
        /// once if you're doing steady-state evolution (at the end of the run). 
        /// </summary>		
        public abstract void FinishProducing(IEvolutionState s, int subpop, int thread);

        /// <summary>
        /// Produces <i>n</i> individuals from the given subpop
        /// and puts them into inds[start...start+n-1],
        /// where n = Min(Max(q,min),max), where <i>q</i> is the "typical" number of 
        /// individuals the BreedingSource produces in one shot, and returns
        /// <i>n</i>.  max must be >= min, and min must be >= 1. For example, crossover
        /// might typically produce two individuals, tournament selection might typically
        /// produce a single individual, etc. 
        /// </summary>
        public abstract int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread);

        #endregion // Operations
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("A cloning error has occurred.", ex);
            } // never happens
        }

        #endregion // Cloning
    }
}