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

namespace BraneCloud.Evolution.EC.Eval
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
    [ECConfiguration("ec.eval.IJob")]
    public interface IJob
    {
        /// <summary>
        /// Either SlaveEvaluationType.Simple or SlaveEvaluationType.Grouped
        /// </summary>
        SlaveEvaluationType Type { get; set; }

        bool Sent { get; set; }

        /// <summary>
        /// Original individuals.
        /// </summary>
        Individual[] Inds { get; set; }

        /// <summary>
        /// Individuals that were returned -- may be different individuals!
        /// </summary>
        Individual[] NewInds { get; set; }

        int[] Subpops { get; set; }
        bool CountVictoriesOnly { get; set; }
        bool[] UpdateFitness { get; set; }

        void CopyIndividualsForward();

        void CopyIndividualsBack(IEvolutionState state);
    }
}