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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Breed
{
    /// <summary>
    /// If empty, a BufferedBreedingPipeline makes a request of exactly <i>num-inds</i> 
    /// individuals from a single child source; it then uses these
    /// individuals to fill requests (returning min each time),
    /// until the Buffer is emptied, at
    /// which time it grabs exactly <i>num-inds</i> more individuals, and so on.
    /// 
    /// <p/>What is this useful for?  Well, let's say for example that 
    /// you want to cross over two individuals, then cross
    /// them over again.  You'd like to hook up two CrossoverPipelines
    /// in series.  Unfortunately, CrossoverPipeline takes
    /// two sources; even if you set them to the same source, it requests
    /// <i>one</i> individual from the first source and then <i>one</i>
    /// from the second, where what you really want is for it to request
    /// <i>two</i> individuals from a single source (the other CrossoverPipeline).
    /// 
    /// <p/>The solution to this is to hook a CrossoverPipeline as the
    /// source to a BufferedBreedingPipeline of Buffer-size 2 (or some
    /// multiple of 2 actually).  Then the BufferedBreedingPipeline is
    /// set as both sources to another CrossoverPipeline.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// 1
    /// <p/><b>Number of Sources</b><br/>
    /// 1
    /// </summary>
    /// <summary><p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>num-inds</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(the Buffer size)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// breed.buffered
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.breed.BufferedBreedingPipeline")]
    public class BufferedBreedingPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_BUFSIZE = "num-inds";
        public const string P_BUFFERED = "buffered";
        public const int INDS_PRODUCED = 1;
        public const int NUM_SOURCES = 1;

        #endregion // Constants
        #region Properties

        public IList<Individual> Buffer { get; set; }
        public int BufSize { get; set; }

        public override IParameter DefaultBase => BreedDefaults.ParamBase.Push(P_BUFFERED);

        public override int NumSources => NUM_SOURCES; 

        public override int TypicalIndsProduced => INDS_PRODUCED; 


        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var def = DefaultBase;

            BufSize = state.Parameters.GetInt(paramBase.Push(P_BUFSIZE), def.Push(P_BUFSIZE), 1);
            if (BufSize == 0)
                state.Output.Fatal("BufferedBreedingPipeline's number of individuals must be >= 1.", paramBase.Push(P_BUFSIZE), def.Push(P_BUFSIZE));

            Buffer = new List<Individual>(BufSize);

            // declare that likelihood isn't used
            if (Likelihood < 1.0)
                state.Output.Warning("BufferedBreedingPipeline does not respond to the 'likelihood' parameter.",
                    paramBase.Push(P_LIKELIHOOD), def.Push(P_LIKELIHOOD));
        }

        #endregion // Setup
        #region Operations

        public override void PrepareToProduce(IEvolutionState state, int subpop, int thread)
        {
            base.PrepareToProduce(state, subpop, thread);
            // reset my number of individuals to 0
            Buffer.Clear();
        }

        public override int Produce(
            int min, 
            int max, 
            int subpop, 
            IList<Individual> inds, 
            IEvolutionState state, 
            int thread, 
            IDictionary<string, object> misc)
        {
            for (int q = 0; q < min; q++)
            {
                if (Buffer.Count == 0)       // reload
                {
                    Sources[0].Produce(BufSize, BufSize, subpop, Buffer, state, thread, misc);
                }
                var top = Buffer.RemoveFromTopDesctructively();
                inds.Add(top);
            }
            return min;
        }

        #endregion // Operations
    }
}