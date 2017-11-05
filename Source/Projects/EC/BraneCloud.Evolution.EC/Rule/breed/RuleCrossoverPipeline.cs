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

namespace BraneCloud.Evolution.EC.Rule.Breed
{
    /// <summary> 
    /// RuleCrossoverPipeline is a BreedingPipeline which implements a simple default crossover
    /// for RuleIndividuals.  Normally it takes two individuals and returns two crossed-over 
    /// child individuals.  Optionally, it can take two individuals, cross them over, but throw
    /// away the second child (a one-child crossover).  RuleCrossoverPipeline works by iteratively taking rulesets
    /// from each individual, and migrating rules from either to the other with a certain
    /// per-rule probability.  Rule crossover preserves the min and max rule restrictions.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// 1 or 2
    /// <p/><b>Number of Sources</b><br/>
    /// 2
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>toss</tt><br/>
    /// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font>/td>
    /// <td valign="top">(after crossing over with the first new individual, should its second sibling individual be thrown away instead of adding it to the population?)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>prob</tt><br/>
    /// <font size="-1">0.0 &lt;= double &lt; 1.0, or 0.5 (default)</font>/td>
    /// <td valign="top">(probability that a rule will cross over from one individual to the other)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// rule.xover
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.rule.breed.RuleCrossoverPipeline")]
    public class RuleCrossoverPipeline : BreedingPipeline
    {
        #region Constants

        public const string P_TOSS = "toss";
        public const string P_CROSSOVER = "xover";
        public const string P_CROSSOVERPROB = "crossover-prob";
        public const int INDS_PRODUCED = 2;
        public const int NUM_SOURCES = 2;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return RuleDefaults.ParamBase.Push(P_CROSSOVER); }
        }

        /// <summary>
        /// Returns 2 
        /// </summary>
        public override int NumSources => NUM_SOURCES; 

        /// <summary>
        /// Should the pipeline discard the second parent after crossing over? 
        /// </summary>
        public bool TossSecondParent { get; set; }

        /// <summary>
        /// What is the probability of a rule migrating? 
        /// </summary>
        public double RuleCrossProbability { get; set; }

        /// <summary>
        /// Temporary holding place for Parents 
        /// </summary>
        public RuleIndividual[] Parents { get; set; }

        /// <summary>
        /// Returns 2 (unless tossing the second sibling, in which case it returns 1) 
        /// </summary>
        public override int TypicalIndsProduced => TossSecondParent ? 1 : INDS_PRODUCED;
        

        #endregion // Properties
        #region Setup

        public RuleCrossoverPipeline()
        {
            Parents = new RuleIndividual[2];
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            var def = DefaultBase;

            TossSecondParent = state.Parameters.GetBoolean(paramBase.Push(P_TOSS), def.Push(P_TOSS), false);
            RuleCrossProbability = state.Parameters.GetDoubleWithDefault(paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB), 0.5);

            if (RuleCrossProbability > 1.0 || RuleCrossProbability < 0.0)
                state.Output.Fatal("Rule cross probability must be between 0 and 1", paramBase.Push(P_CROSSOVERPROB), def.Push(P_CROSSOVERPROB));
        }

        #endregion // Setup
        #region Operations

        public override int Produce(int min, int max, int start, int subpop, Individual[] inds, IEvolutionState state, int thread)
        {
            // how many individuals should we make?
            var n = (TossSecondParent ? 1 : INDS_PRODUCED);
            if (n < min)
                n = min;
            if (n > max)
                n = max;

            // should we bother?
            if (!state.Random[thread].NextBoolean(Likelihood))
                return Reproduce(n, start, subpop, inds, state, thread, true);  // DO produce children from source -- we've not done so already

            var initializer = ((RuleInitializer)state.Initializer);

            for (var q = start; q < n + start; )
            // keep on going until we're filled up
            {
                // grab two individuals from our Sources
                if (Sources[0] == Sources[1])
                // grab from the same source
                {
                    Sources[0].Produce(2, 2, 0, subpop, Parents, state, thread);
                    if (!(Sources[0] is BreedingPipeline))
                    // it's a selection method probably
                    {
                        Parents[0] = (RuleIndividual)Parents[0].Clone();
                        Parents[1] = (RuleIndividual)Parents[1].Clone();
                    }
                }
                // grab from different Sources
                else
                {
                    Sources[0].Produce(1, 1, 0, subpop, Parents, state, thread);
                    Sources[1].Produce(1, 1, 1, subpop, Parents, state, thread);

                    if (!(Sources[0] is BreedingPipeline)) // it's a selection method probably
                        Parents[0] = (RuleIndividual)Parents[0].Clone();

                    if (!(Sources[1] is BreedingPipeline)) // it's a selection method probably
                        Parents[1] = (RuleIndividual)Parents[1].Clone();
                }

                // at this point, Parents[] contains our two selected individuals,
                // AND they're copied so we own them and can make whatever modifications
                // we like on them.

                // so we'll cross them over now.

                Parents[0].PreprocessIndividual(state, thread);
                Parents[1].PreprocessIndividual(state, thread);

                if (Parents[0].Rulesets.Length != Parents[1].Rulesets.Length)
                {
                    state.Output.Fatal("The number of rule sets should be identical in both Parents ( "
                        + Parents[0].Rulesets.Length + " : "
                        + Parents[1].Rulesets.Length + " ).");
                }

                // for each set of rules (assume both individuals have the same number of rule sets)
                for (var x = 0; x < Parents[0].Rulesets.Length; x++)
                {
                    var temp = new RuleSet[2];
                    while (true)
                    {
                        // create two new Rulesets (initially empty)
                        for (var i = 0; i < 2; i++)
                            temp[i] = new RuleSet();

                        // split the ruleset indexed x in parent 1
                        temp = Parents[0].Rulesets[x].SplitIntoTwo(state, thread, temp, RuleCrossProbability);
                        // now temp[0] contains rules to that must go to parent[1]

                        // split the ruleset indexed x in parent 2 (append after the splitted result from previous operation)
                        temp = Parents[1].Rulesets[x].SplitIntoTwo(state, thread, temp, RuleCrossProbability);
                        // now temp[1] contains rules that must go to parent[0]

                        // ensure that there are enough rules
                        if (temp[0].RuleCount >= Parents[0].Rulesets[x].Constraints(initializer).MinSize
                            && temp[0].RuleCount <= Parents[0].Rulesets[x].Constraints(initializer).MaxSize
                            && temp[1].RuleCount >= Parents[1].Rulesets[x].Constraints(initializer).MinSize
                            && temp[1].RuleCount <= Parents[1].Rulesets[x].Constraints(initializer).MaxSize)
                            break;

                        temp = new RuleSet[2];
                    }

                    // copy the results in the Rulesets of the Parents
                    Parents[0].Rulesets[x].CopyNoClone(temp[1]);
                    Parents[1].Rulesets[x].CopyNoClone(temp[0]);
                }

                Parents[0].PostprocessIndividual(state, thread);
                Parents[1].PostprocessIndividual(state, thread);

                Parents[0].Evaluated = false;
                Parents[1].Evaluated = false;

                // add 'em to the population
                inds[q] = Parents[0];
                q++;
                if (q < n + start && !TossSecondParent)
                {
                    inds[q] = Parents[1];
                    q++;
                }
            }
            return n;
        }
        
        #endregion // Operations
        #region Cloning

        public override object Clone()
        {
            var c = (RuleCrossoverPipeline)base.Clone();

            // deep-cloned stuff
            c.Parents = new RuleIndividual[Parents.Length];
            Parents.CopyTo(c.Parents, 0);

            return c;
        }

        #endregion // Cloning
    }
}