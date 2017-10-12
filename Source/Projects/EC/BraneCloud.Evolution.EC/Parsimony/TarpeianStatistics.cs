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

using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Parsimony
{    
    /// <summary>
    /// This Statistics subclass implements Poli's "Tarpeian" method of parsimony control, whereby some
    /// <i>kill-proportion</i> of above-average-sized individuals in each subpop have their fitnesses
    /// set to a very bad value, and marks them as already evaluated (so the Evaluator can skip them).  
    /// The specific individuals in this proportion is determined at random.
    /// 
    /// <p/>Different Fitnesses have different meanings of the word "bad".  At present, we set the fitness
    /// to -Float.MAX_VALUE if it's a SimpleFitness, and set it to Float.MAX_VALUE if it's a KozaFitnesss.
    /// If it's any other kind of Fitness, an error is reported.  You can override the "bad-setter" function
    /// setMinimumFitness(...) to make other kinds of fitness bad in different ways.  In the future we may
    /// revisit how to set Fitnesses to "bad" in a more general way if this becomes an issue.
    /// 
    /// <p/>Tarpeian is implemented as a Statistics.  Why?  Because we need to mark individuals as evaluated
    /// prior to the Evaluator getting to them, and also need to keep track of the total proportion marked
    /// as such.  We considered doing this as a SelectionMethod, as a BreedingPipeline, as a Breeder, and
    /// as an Evaluator.  None are good options really -- Evaluator is the best approach but it means we
    /// have special Tarpeian Evaluators, so it's no longer orthogonal with other Evaluators.  Eventually
    /// we settled on the one object which has the right hooks and can be easily stuck onto the system without
    /// modifying anything in a special-purpose way: a Statistics object.
    /// 
    /// <p/>All you need to do is add TarpeianStatistics as a child to your existing Statistics chain.  If you
    /// have one existing Statistics, then you just add the parameters <tt>stat.num-children=1</tt> and
    /// <tt>stat.Child.0=ec.parsimony.TarpeianStatistics</tt>  You'll also need to specify the kill proportion
    /// (for example, <tt>stat.Child.0.kill-proportion=0.2</tt> )
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>kill-proportion</tt><br/>
    /// <font size="-1">0 &lt; int &lt; 1</font></td>
    /// <td valign="top">(proportion of above-average-sized individuals killed)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.parsimony.TarpeianStatistics")]
    public class TarpeianStatistics : Statistics
    {
        #region Constants

        /// <summary>
        /// One in n individuals are killed.
        /// </summary>
        public const string P_KILL_PROPORTION = "kill-proportion";

        #endregion // Constants
        #region Properties

        public float KillProportion { get; set; }

        #endregion // Properties
        #region Setup

        public override void  Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            KillProportion = state.Parameters.GetFloat(paramBase.Push(P_KILL_PROPORTION), null, 0.0);
            if (KillProportion < 0 || KillProportion > 1)
                state.Output.Fatal("Parameter not found, or it has an invalid value (<0 or >1).", paramBase.Push(P_KILL_PROPORTION));
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Marks a proportion (killProportion) of individuals with 
        /// above-average size (within their own subpop) to a minimum value.
        /// </summary>
        public override void  PreEvaluationStatistics(IEvolutionState state)
        {
            for (var subpop = 0; subpop < state.Population.Subpops.Length; subpop++)
            {
                double averageSize = 0;
                
                for (var i = 0; i < state.Population.Subpops[subpop].Individuals.Length; i++)
                    averageSize += state.Population.Subpops[subpop].Individuals[i].Size;
                
                averageSize /= state.Population.Subpops[subpop].Individuals.Length;
                
                for (var i = 0; i < state.Population.Subpops[subpop].Individuals.Length; i++)
                {
                    if ((state.Population.Subpops[subpop].Individuals[i].Size > averageSize) && (state.Random[0].NextFloat() < KillProportion))
                    {
                        var ind = state.Population.Subpops[subpop].Individuals[i];
                        SetMinimumFitness(state, subpop, ind);
                        ind.Evaluated = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// Sets the fitness of an individual to the minimum fitness possible.
        /// If the fitness is of type ec.simple.SimpleFitness, that minimum value is -Float.MAX_VALUE;
        /// If the fitness is of type KozaFitness, that minimum value is Float.MAX_VALUE;
        /// Else, a fatal error is reported.
        /// You need to override this method if you're using any other type of fitness.
        /// </summary>
        public virtual void SetMinimumFitness(IEvolutionState state, int subpop, Individual ind)
        {
            var fitness = ind.Fitness;
            if (fitness is KozaFitness)
                ((KozaFitness) fitness).SetStandardizedFitness(state, Single.MaxValue);
            else if (fitness is SimpleFitness)
                ((SimpleFitness) fitness).SetFitness(state, - Single.MaxValue, false);
            else
                state.Output.Fatal("TarpeianStatistics only accepts individuals with fitness of type ec.simple.SimpleFitness or KozaFitness.");
        }

        #endregion // Operations
    }
}