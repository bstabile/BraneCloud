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

using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// A BreedingSource is a IPrototype which 
    /// provides Individuals to populate new populations based on
    /// old ones.  The BreedingSource/BreedingPipeline/SelectionMethod mechanism
    /// is inherently designed to work within single subpops, which is
    /// by far the most common case.  If for some
    /// reason you need to breed among different subpops to produce new ones
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
    /// <font size="-1">0.0 &lt;= doubke &lt;= 1.0, or undefined</font></td>
    /// <td valign="top">(probability this BreedingSource gets chosen.  
    /// Undefined is only valid if the caller of this BreedingSource doesn't need a Probability)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.IBreedingSource")]
    public interface IBreedingSource : IPrototype, IRandomChoiceChooserD
    {
        double Probability { get; set; }
        int PickRandom(IBreedingSource[] sources, double prob);
        void SetupProbabilities(IBreedingSource[] sources);

        int TypicalIndsProduced { get; }

        int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread);
        bool Produces(IEvolutionState state, Population newpop, int subpop, int thread);
        void PrepareToProduce(IEvolutionState state, int subpop, int thread);
        void FinishProducing(IEvolutionState state, int subpop, int thread);

        void PreparePipeline(object hook);
    }
}