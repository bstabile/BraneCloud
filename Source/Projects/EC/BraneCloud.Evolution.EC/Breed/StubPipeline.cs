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
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Breed
{
    /**
     * StubPipeline is a BreedingPipeline subclass which, during fillStubs(), fills all the stubs
     * with its own stub pipeline.  The stub pipeline's stubs are first filled by parent
     * stub sources.
     
     <p><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br>
     ...as many as the child produces


     <p><b>Number of Sources</b><br>
     1

     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base.</i><tt>stub-source</tt><br>
     <font size=-1>classname, inherits and != ec.BreedingSource</font></td>
     <td valign=top>(the prototypical "stub pipeline" Breeding Source)</td></tr>
     </table>
     
     <p><b>Parameter bases</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>stub-source</tt></td>
     <td>i_prototype (the stub pipeline)</td></tr>
     </table>
     
     <p><b>Default Base</b><br>
     breed.stub

     * @author Sean Luke
     * @version 1.0 
     */

    public class StubPipeline : ReproductionPipeline
    {
        public const string P_STUB = "stub";
        public const string P_STUB_PIPELINE = "stub-source";

        public BreedingSource StubSource { get; set; }

        public override IParameter DefaultBase => BreedDefaults.ParamBase.Push(P_STUB);

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IParameter def = DefaultBase;

            // load the breeding pipeline
            StubSource = (BreedingSource) state.Parameters.GetInstanceForParameter(
                paramBase.Push(P_STUB_PIPELINE), def.Push(P_STUB_PIPELINE), typeof(BreedingSource));
            StubSource.Setup(state, paramBase.Push(P_STUB_PIPELINE));
        }

        public override object Clone()
        {
            StubPipeline other = (StubPipeline) base.Clone();
            other.StubSource = (BreedingSource) other.StubSource.Clone();
            return other;
        }

        public override void FillStubs(IEvolutionState state, IBreedingSource source)
        {
            // fill the stubs in my stub-pipeline first with my parent source
            StubSource.FillStubs(state, source);

            // fill subsidiary stubs with my stubpipeline, including my immediate source
            base.FillStubs(state, StubSource);
        }
    }
}