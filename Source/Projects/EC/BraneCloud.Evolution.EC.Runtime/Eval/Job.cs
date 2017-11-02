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
using BraneCloud.Evolution.EC.Eval;

namespace BraneCloud.Evolution.EC.Runtime.Eval
{	
    /// <summary> 
    /// Job
    /// This class stores information regarding a job submitted to a Slave: the individuals,
    /// the subpops in which they are stored, a scratch array for the individuals used
    /// internally, and various coevolutionary information (whether we should only count victories
    /// single-elimination-tournament style; which individuals should have their fitnesses updated).
    /// 
    /// <p/>Jobs are of two types: traditional evaluations (SlaveEvaluationType.Simple), and coevolutionary
    /// evaluations (SlaveEvaluationType.Grouped).  <i>type</i> indicates the type of job.
    /// For traditional evaluations, we may submit a group of individuals all at one time.  
    /// Only the individuals and their subpop numbers are needed. 
    /// Coevolutionary evaluations require the number of individuals, the subpops they come from, the
    /// pointers to the individuals, boolean flags indicating whether their fitness is to be updated or
    /// not, and another boolean flag indicating whether to count only victories in competitive tournament.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.eval.Job")]
    public class Job : IJob
    {
        /// <summary>
        /// Either SlaveEvaluationType.Simple or SlaveEvaluationType.Grouped
        /// </summary>
        public SlaveEvaluationType Type { get; set; }

        public bool Sent { get; set; }

        /// <inheritdoc />
        public Individual[] Inds { get; set; }

        /// <inheritdoc />
        public Individual[] NewInds { get; set; }

        public int[] Subpops { get; set; }
        public bool CountVictoriesOnly { get; set; }
        public bool[] UpdateFitness { get; set; }
        
        public virtual void CopyIndividualsForward()
        {
            if (NewInds == null || NewInds.Length != Inds.Length)
                NewInds = new Individual[Inds.Length];
            for (var i = 0; i < Inds.Length; i++)
            {
                NewInds[i] = (Individual) Inds[i].Clone();
                // delete the trials since they'll get remerged
                NewInds[i].Fitness.Trials = null;
                // delete the context, since it'll get remerged
                NewInds[i].Fitness.SetContext(null);
            }
        }
        
        public virtual void CopyIndividualsBack(IEvolutionState state)
        {
            for (var i = 0; i < Inds.Length; i++)
                Inds[i].Merge(state, NewInds[i]);
            NewInds = null;
        }
    }
}