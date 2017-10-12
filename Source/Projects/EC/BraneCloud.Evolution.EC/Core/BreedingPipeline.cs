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
using BraneCloud.Evolution.EC.SteadyState;

namespace BraneCloud.Evolution.EC
{
    /// <summary> 
    /// A BreedingPipeline is a BreedingSource which provides "fresh" individuals which
    /// can be used to fill a new population.  BreedingPipelines might include
    /// Crossover pipelines, various Mutation pipelines, etc.  This abstract class
    /// provides some default versions of various methods to simplify matters for you.
    /// It also contains an array of breeding sources for your convenience.  You don't
    /// have to use them of course, but this means you have to customize the
    /// default methods below to make sure they get distributed to your special
    /// sources.  Note that these sources may contain references to the same
    /// object -- they're not necessarily distinct.  This is to provide both
    /// some simple DAG features and also to conserve space.
    /// 
    /// 
    /// <p/>A BreedingPipeline implements ISteadyStateBSource, meaning that
    /// it receives the individualReplaced(...) and SourcesAreProperForm(...) messages.
    /// however by default it doesn't do anything with these except distribute them
    /// to its sources.  You might override these to do something more interesting.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>num-sources</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(User-specified number of sources to the pipeline.  
    /// Some pipelines have hard-coded numbers of sources; others indicate 
    /// (with the java constant DYNAMIC_SOURCES) that the number of sources is determined by this
    /// user parameter instead.)</td></tr>
    /// <tr>
    /// <td valign="top"/><i>base</i>.<tt>source.</tt><i>n</i><br/>
    /// <font size="-1"/>classname, inherits and != BreedingSource, or the value <tt>same</tt><br/>
    /// <td valign="top">(Source <i>n</i> for this BreedingPipeline.
    /// If the value is set to <tt>same</tt>, then this source is the
    /// exact same source object as <i>base</i>.<tt>source.</tt><i>n-1</i>, and
    /// further parameters for this object will be ignored and treated as the same 
    /// as those for <i>n-1</i>.  <tt>same</tt> is not valid for 
    /// <i>base</i>.<tt>source.0</tt>)</td>
    /// </tr>
    /// </table>
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>source.</tt><i>n</i><br/>
    /// <td>Source <i>n</i></td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.BreedingPipeline")]
    public abstract class BreedingPipeline : BreedingSource, ISteadyStateBSource
    {
        #region Constants

        /// <summary>
        /// Indicates that a source is the exact same source as the previous source. 
        /// </summary>
        public const string V_SAME = "same";

        /// <summary>
        /// Indicates the probability that the Breeding Pipeline will perform 
        /// its mutative action instead of just doing reproduction.
        /// </summary>
        public const String P_LIKELIHOOD = "likelihood";

        /// <summary>
        /// Indicates that the number of sources is variable and determined by the
        /// user in the parameter file. 
        /// </summary>       
        public const int DYNAMIC_SOURCES = 0;

        /// <summary>
        /// Standard parameter for number of sources (only used if numSources returns DYNAMIC_SOURCES. 
        /// </summary>        
        public const string P_NUMSOURCES = "num-sources";

        /// <summary>
        /// Standard parameter for individual-selectors associated with a BreedingPipeline. 
        /// </summary>
        public const string P_SOURCE = "source";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// My parameter base -- I keep it around so I can print some messages that
        /// are useful with it (not deep cloned) 
        /// </summary>        
        public IParameter MyBase { get; set; }

        /// <summary>
        /// Array of sources feeding the pipeline. 
        /// </summary>
        public IBreedingSource[] Sources { get; set; }

        /// <summary>
        /// Returns the number of sources to this pipeline.  Called during
        /// BreedingPipeline's Setup.  Be sure to return a value > 0, or
        /// DYNAMIC_SOURCES which indicates that Setup should check the parameter
        /// file for the parameter "num-sources" to make its determination. 
        /// </summary>        
        public abstract int NumSources { get; }

        public float Likelihood { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            MyBase = paramBase;

            var def = DefaultBase;

            Likelihood = state.Parameters.GetFloatWithDefault(paramBase.Push(P_LIKELIHOOD), def.Push(P_LIKELIHOOD), 1.0f);
            if (Likelihood < 0.0f || Likelihood > 1.0f)
                state.Output.Fatal("Breeding Pipeline likelihood must be a value between 0.0 and 1.0 inclusive",
                    paramBase.Push(P_LIKELIHOOD),
                    def.Push(P_LIKELIHOOD));

            var numsources = NumSources;
            if (numsources <= DYNAMIC_SOURCES)
            {
                // figure it from the file
                numsources = state.Parameters.GetInt(paramBase.Push(P_NUMSOURCES), def.Push(P_NUMSOURCES), 1);
                if (numsources == 0)
                    state.Output.Fatal("Breeding pipeline num-sources value must be > 0", paramBase.Push(P_NUMSOURCES), def.Push(P_NUMSOURCES));
            }

            Sources = new BreedingSource[numsources];

            for (var x = 0; x < Sources.Length; x++)
            {
                var p = paramBase.Push(P_SOURCE).Push("" + x);
                var d = def.Push(P_SOURCE).Push("" + x);

                var s = state.Parameters.GetString(p, d);
                if (s != null && s.Equals(V_SAME))
                {
                    if (x == 0)
                        // oops
                        state.Output.Fatal("Source #0 cannot be declared with the value \"same\".", p, d);

                    // else the source is the same source as before
                    Sources[x] = Sources[x - 1];
                }
                else
                {
                    Sources[x] = (IBreedingSource)(state.Parameters.GetInstanceForParameter(p, d, typeof(IBreedingSource)));
                    Sources[x].Setup(state, p);
                }
            }
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Operations

        public virtual void SourcesAreProperForm(SteadyStateEvolutionState state)
        {
            for (var x = 0; x < Sources.Length; x++)
                if (!(Sources[x] is ISteadyStateBSource))
                {
                    state.Output.Error("The following breeding source is not of ISteadyStateBSource.",
                        MyBase.Push(P_SOURCE).Push("" + x), DefaultBase.Push(P_SOURCE).Push("" + x));
                }
                else
                    ((ISteadyStateBSource)(Sources[x])).SourcesAreProperForm(state);
        }

        /// <summary>
        /// Returns the minimum among the TypicalIndsProduced for any children --
        /// a function that's useful internally, not very useful for you to call externally. 
        /// </summary>
        public virtual int MinChildProduction
        {
            get
            {
                if (Sources.Length == 0)
                    return 0;
                var min = Sources[0].TypicalIndsProduced;
                for (var x = 1; x < Sources.Length; x++)
                {
                    var cur = Sources[x].TypicalIndsProduced;
                    if (min > cur)
                        min = cur;
                }
                return min;
            }
        }

        /// <summary>
        /// Returns the maximum among the TypicalIndsProduced for any children --
        /// a function that's useful internally, not very useful for you to call externally. 
        /// </summary>
        public virtual int MaxChildProduction
        {
            get
            {
                if (Sources.Length == 0)
                    return 0;
                var max = Sources[0].TypicalIndsProduced;
                for (var x = 1; x < Sources.Length; x++)
                {
                    var cur = Sources[x].TypicalIndsProduced;
                    if (max < cur)
                        max = cur;
                }
                return max;
            }
        }

        /// <summary>
        /// Returns the "typical" number of individuals produced -- by default
        /// this is the minimum typical number of individuals produced by any
        /// children sources of the pipeline.  If you'd prefer something different,
        /// override this method. 
        /// </summary>
        public override int TypicalIndsProduced
        {
            get { return MinChildProduction; }
        }

        /// <summary>
        /// Performs direct cloning of n individuals.  if produceChildrenFromSource is true, then...
        /// </summary>
        public int Reproduce(int n, int start, int subpopulation, Individual[] inds, IEvolutionState state,
                             int thread, bool produceChildrenFromSource)
        {
            if (produceChildrenFromSource)
                Sources[0].Produce(n, n, start, subpopulation, inds, state, thread);
            if (Sources[0] is SelectionMethod)
                for (var q = start; q < n + start; q++)
                    inds[q] = (Individual)(inds[q].Clone());
            return n;
        }

        public override bool Produces(IEvolutionState state, Population newpop, int subpop, int thread)
        {
            for (var x = 0; x < Sources.Length; x++)
                if (x == 0 || Sources[x] != Sources[x - 1])
                    if (!Sources[x].Produces(state, newpop, subpop, thread))
                        return false;
            return true;
        }

        public override void PrepareToProduce(IEvolutionState state, int subpop, int thread)
        {
            for (var x = 0; x < Sources.Length; x++)
                if (x == 0 || Sources[x] != Sources[x - 1])
                    Sources[x].PrepareToProduce(state, subpop, thread);
        }

        public override void FinishProducing(IEvolutionState state, int subpop, int thread)
        {
            for (var x = 0; x < Sources.Length; x++)
                if (x == 0 || Sources[x] != Sources[x - 1])
                    Sources[x].FinishProducing(state, subpop, thread);
        }

        public override void PreparePipeline(object hook)
        {
            // the default form calls this on all the sources.
            // note that it follows all the source paths even if they're duplicates
            foreach (var t in Sources)
                t.PreparePipeline(hook);
        }

        public virtual void IndividualReplaced(SteadyStateEvolutionState state, int subpop, int thread, int individual)
        {
            foreach (var t in Sources)
                ((ISteadyStateBSource)(t)).IndividualReplaced(state, subpop, thread, individual);
        }

        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (BreedingPipeline)(base.Clone());

            // make a new array
            c.Sources = new IBreedingSource[Sources.Length];

            // clone the sources -- we won't go through the hassle of
            // determining if we have a DAG or not -- we'll just clone
            // it out to a tree.  I doubt it's worth it.

            for (var x = 0; x < Sources.Length; x++)
            {
                if (x == 0 || Sources[x] != Sources[x - 1])
                    c.Sources[x] = (IBreedingSource)(Sources[x].Clone());
                else
                    c.Sources[x] = c.Sources[x - 1];
            }

            return c;
        }

        #endregion // Cloning
    }
}