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
using BraneCloud.Evolution.EC.CoEvolve;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.GP.GE
{
    /// <summary>
    /// GEProblem is a special replacement for Problem which performs GE mapping.  You do not subclass
    /// from GEProblem.  Rather, create a GPProblem subclass and set it to be the 'problem' parameter of the GEProblem.
    /// The GEProblem will convert the GEIndividual into a GPIndividual, then pass this GPIndividual to the GPProblem
    /// to be evaluated.
    /// 
    /// <p/>The procedure is as follows.  Let's say your GPProblem is the Artificial Ant problem.  Instead of saying...
    /// 
    /// <p/><tt>eval.problem = ec.app.ant.Ant<br/>
    /// eval.problem = ec.app.ant.Ant<br/>
    /// eval.problem.data = ec.app.ant.AntData<br/>
    /// eval.problem.moves = 400<br/>
    /// eval.problem.file = santafe.trl
    /// </tt>
    /// 
    /// <p/>... you instead make your problem a GEProblem like this:
    /// 
    /// <p/><tt>eval.problem = ec.gp.ge.GEProblem</tt>
    /// 
    /// <p/>... and then you hang the Ant problem, and all its subsidiary data, as the 'problem' parameter from the GEProblem like so:
    /// 
    /// <p/><tt>eval.problem.problem = ec.app.ant.Ant<br/>
    /// eval.problem.problem.data = ec.app.ant.AntData<br/>
    /// eval.problem.problem.moves = 400<br/>
    /// eval.problem.problem.file = santafe.trl
    /// </tt>
    /// 
    /// <p/>Everything else should be handled for you.  GEProblem is also compatible with the MasterProblem procedure
    /// for distributed evaluation, and is also both a SimpleProblemForm and a IGroupedProblem.  We've got you covered.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>problem</tt><br/>
    /// <font size="-1">classname, inherits from GPProblem</font></td>
    /// <td valign="top">(The GPProblem which actually performs the evaluation of the mapped GPIndividual)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ge.GEProblem")]
    public class GEProblem : Problem, ISimpleProblem, IGroupedProblem
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public new const string P_PROBLEM = "problem";

        #endregion // Constants

        #region Properties

        public IGPProblem Problem { get; set; }

        #endregion // Properties

        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            Problem = (GPProblem) state.Parameters.GetInstanceForParameter(paramBase.Push(P_PROBLEM), null, typeof(GPProblem));
            Problem.Setup(state, paramBase.Push(P_PROBLEM));
        }

        #endregion // Setup

        #region Cloning

        public override object Clone()
        {
            var other = (GEProblem) base.Clone();
            other.Problem = (GPProblem) Problem.Clone();
            return other;
        }

        #endregion // Cloning

        #region Operations

        public override void PrepareToEvaluate(IEvolutionState state, int threadnum)
        {
            Problem.PrepareToEvaluate(state, threadnum);
        }

        public override void FinishEvaluating(IEvolutionState state, int threadnum)
        {
            Problem.FinishEvaluating(state, threadnum);
        }

        public override void InitializeContacts(IEvolutionState state)
        {
            Problem.InitializeContacts(state);
        }

        public void ReinitializeContacts(EvolutionState state)
        {
            Problem.ReinitializeContacts(state);
        }

        public void CloseContacts(EvolutionState state, int result)
        {
            Problem.CloseContacts(state, result);
        }

        public override bool CanEvaluate => Problem.CanEvaluate;

        public void PreprocessPopulation(IEvolutionState state, Population pop, bool[] prepareForFitnessAssessment,
            bool countVictoriesOnly)
        {
            if (!(Problem is IGroupedProblem))
                state.Output.Fatal("GEProblem's underlying Problem is not a IGroupedProblem");
            ((IGroupedProblem) Problem).PreprocessPopulation(state, pop, prepareForFitnessAssessment,
                countVictoriesOnly);
        }

        public int PostprocessPopulation(IEvolutionState state, Population pop, bool[] assessFitness, bool countVictoriesOnly)
        {
            return ((IGroupedProblem) Problem).PostprocessPopulation(state, pop, assessFitness, countVictoriesOnly);
        }

        /// <summary>
        /// Default version assumes that every individual is a GEIndividual.
        /// The underlying problem.evaluate() must be prepared for the possibility that some
        /// GPIndividuals handed it are in fact null, meaning that they couldn't be extracted
        /// from the GEIndividual string.  You should assign them bad fitness in some appropriate way.
        /// </summary>
        /// <param name="state">The current EvolutionState.</param>
        /// <param name="ind">The individuals to evaluate together</param>
        /// <param name="updateFitness">Should this individuals' fitness be updated?</param>
        /// <param name="countVictoriesOnly">Don't bother updating Fitness with socres, just victories</param>
        /// <param name="subpops">Subpopulations</param>
        /// <param name="threadnum">Thread number</param>
        public void Evaluate(IEvolutionState state,
            Individual[] ind,
            bool[] updateFitness,
            bool countVictoriesOnly,
            int[] subpops,
            int threadnum)
        {
            // the default version assumes that every subpopulation is a GE Individual
            var gpi = new GPIndividual[ind.Length];
            for (var i = 0; i < gpi.Length; i++)
            {
                if (ind[i] is GEIndividual)
                {
                    var indiv = (GEIndividual) ind[i];
                    var species = (GESpecies) ind[i].Species;

                    // warning: gpi[i] may be null
                    gpi[i] = species.Map(state, indiv, threadnum, null);
                }
                else if (ind[i] is GPIndividual)
                {
                    state.Output.WarnOnce("GPIndividual provided to GEProblem.  Hope that's correct.");
                    gpi[i] = (GPIndividual) ind[i];
                }
                else
                {
                    state.Output.Fatal("Individual " + i +
                                       " passed to Grouped evaluate(...) was neither a GP nor GE Individual: " +
                                       ind[i]);
                }
            }

            ((IGroupedProblem) Problem).Evaluate(state, gpi, updateFitness, countVictoriesOnly, subpops, threadnum);

            for (var i = 0; i < gpi.Length; i++)
            {
                // Now we need to move the evaluated flag from the GPIndividual
                // to the GEIndividual, and also for good measure, let's copy over
                // the GPIndividual's fitness because even though the mapping function
                // set the two Individuals to share the same fitness, it's possible
                // that the evaluation function may have replaced the fitness.
                ind[i].Fitness = gpi[i].Fitness; // if it's a GPIndividual anyway it'll just copy onto itself
                ind[i].Evaluated = gpi[i].Evaluated; // if it's a GPIndividual anyway it'll just copy onto itself
            }
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            // this shouldn't ever happen because GEProblem's Problems are ALWAYS
            // ISimpleProblem, but we include it here to be future-proof
            if (!(Problem is ISimpleProblem))
                state.Output.Fatal("GEProblem's underlying Problem is not a ISimpleProblem");

            if (ind is GEIndividual)
            {
                var indiv = (GEIndividual) ind;
                var species = (GESpecies) (ind.Species);
                var gpi = species.Map(state, indiv, threadnum, null);
                if (gpi == null)
                {
                    var fitness = (KozaFitness) (ind.Fitness);
                    fitness.SetStandardizedFitness(state, Single.MaxValue);
                }
                else
                {
                    ((ISimpleProblem) Problem).Evaluate(state, gpi, subpop, threadnum);
                    // Now we need to move the evaluated flag from the GPIndividual
                    // to the GEIndividual, and also for good measure, let's copy over
                    // the GPIndividual's fitness because even though the mapping function
                    // set the two Individuals to share the same fitness, it's possible
                    // that the evaluation function may have replaced the fitness.
                    ind.Fitness = gpi.Fitness;
                    ind.Evaluated = gpi.Evaluated;
                }
            }
            else if (ind is GPIndividual)
            {
                state.Output.WarnOnce("GPIndividual provided to GEProblem.  Hope that's correct.");
                ((ISimpleProblem) Problem).Evaluate(state, ind, subpop, threadnum); // just evaluate directly
            }
            else
            {
                state.Output.Fatal("Individual passed to Evaluate(...) was neither a GP nor GE Individual: " + ind);
            }
        }

        public override void Describe(IEvolutionState state, Individual ind, int subpop, int threadnum, int log)
        {
            if (ind is GEIndividual)
            {
                var indiv = (GEIndividual) ind;
                var species = (GESpecies) (ind.Species);
                var gpi = species.Map(state, indiv, threadnum, null);
                if (gpi != null)
                {
                    Problem.Describe(state, gpi, subpop, threadnum, log);

                    // though this is probably not necessary for describe(...),
                    // for good measure we're doing the same rigamarole that we
                    // did for evaluate(...) above.
                    ind.Fitness = gpi.Fitness;
                    ind.Evaluated = gpi.Evaluated;
                }
            }
            else if (ind is GPIndividual)
            {
                state.Output.WarnOnce("GPIndividual provided to GEProblem.  Hope that's correct.");
                ((ISimpleProblem) Problem).Describe(state, ind, subpop, threadnum, log); // just describe directly
            }
            else
            {
                state.Output.Fatal("Individual passed to Describe(...) was neither a GP nor GE Individual: " + ind);
            }
        }

        #endregion // Operations
    }
}