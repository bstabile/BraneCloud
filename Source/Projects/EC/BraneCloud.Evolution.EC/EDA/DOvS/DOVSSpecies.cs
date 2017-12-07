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
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.EDA.DOvS
{
    /**
     * DOVSSpecies is a IntegerVectorSpecies which implements DOVS algorithm. The
     * two most important method for a Species in DOVS problem is
     * updateMostPromisingArea(...) and mostPromisingAreaSamples(...). We call these
     * two methods in sequence to first determine an area around best individual and
     * sample new individual from that area. However, there are several ways to
     * implements these two methods, thus, we let the subclasses to determine the
     * implementation of these two method, e.g. HyperboxSpecies.
     * 
     * 
     * <p>
     * DOVSSpecies must be used in combination with DOVSBreeder, which will call it
     * at appropriate times to reproduce new individuals for next generations. It
     * must also be used in combination with DOVSInitializer and DOVSEvaluator. The
     * former will be used to generate the initial population, and the later will
     * determine a suitable number of evaluation for each individual.
     *
     *
     * <p>
     * <b>Parameters</b><br>
     * <table>
     * <tr>
     * <td valign=top><tt><i>base</i>.initial-reps</tt><br>
     * <font size=-1>Integer &gt; 1</font></td>
     * <td valign=top>Base value of number of evaluations for each individual.</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.warmup</tt><br>
     * <font size=-1>Integer &gt; 1</font></td>
     * <td valign=top>Number of trial we want to randomize one dimension of the
     * individual, used for sampling.</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.constraints-size</tt><br>
     * <font size=-1>Integer</font></td>
     * <td valign=top>Number of constraints for the initial optimization problem.
     * link</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.constraints-A</tt><br>
     * <font size=-1>String</font></td>
     * <td valign=top>A string of double number separate by whitespace specified the
     * left hand side coefficients of the constraint Ax<=b.</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.constraints-B</tt><br>
     * <font size=-1>Double</font></td>
     * <td valign=top>A double number specified the right hand side of the constraint Ax&lt;=b.</td>
     * </tr>
     * <tr>
     * <td valign=top><tt><i>base</i>.stochastic</tt><br>
     * <font size=-1>Boolean (default = false)</font></td>
     * <td valign=top>Is it the problem a stochastic problem?</td>
     * </tr>
     * </table>
     * 
     * 
     * 
     * <p>
     * <b>Default Base</b><br>
     * dovs.species
     * 
     * <p>
     * <b>Parameter bases</b><br>
     * <table>
     * <tr>
     * <td valign=top><i>base</i>.<tt>species</tt></td>
     * <td>species (the subpopulations' species)</td>
     * </tr>
     *
     *
     * 
     * @author Ermo Wei and David Freelan
     * 
     */

    [ECConfiguration("ec.eda.dovs.DOVSSpecies")]
    public class DOVSSpecies : IntegerVectorSpecies
    {

        public const string P_DOVS_SPECIES = "species";
        public const string P_INITIAL_REPETITION = "initial-reps";

        public const string P_STOCHASTIC = "stochastic";

        //public const string P_OCBA = "ocba";
        public const string P_CONSTRAINTS_SIZE = "constraints-size";

        public const string P_A = "constraints-A";
        public const string P_B = "constraints-b";
        public const string P_WARM_UP = "warmup";

        /**
         * This integer indicate the index of optimal individual in the visited
         * array.
         */
        public int OptimalIndex = -1;

        /** warm up period for RMD sampling. */
        public int WarmUp { get; set; }

        /**
         * This list contains all the sample we have visited during current
         * algorithm run.
         */
        public IList<Individual> Visited { get; set; }

        /**
         * Given a individual, return the index of this individual in ArrayList
         * visited
         */
        public IDictionary<Individual, int> VisitedIndexMap { get; set; }

        /**
         * CornerMaps for the all the visisted individuals. This record the
         * key-value pair for each individuals, where key is the coordinates and
         * value is individual itself.
         */
        public IList<CornerMap> Corners { get; set; }

        /**
         * activeSolutions contains all the samples that is on the boundary of the
         * most promising area.
         */
        public IList<Individual> ActiveSolutions { get; set; }

        /**
         * This is the Ek in original paper, where is the collection all the
         * individuals evaluated in generation k.
         */
        public IList<Individual> Ek { get; set; }

    /* Ocba flag. */
        //public boolean ocba;

        /** Is the problem a stochastic problem. */
        public bool Stochastic { get; set; }

        /** Base value of number evaluation for each individual. */
        public int InitialReps { get; set; }

        /**
         * This value will be updated at each generation to determine how many
         * evaluation is needed for one individual. It make use of the initialReps.
         */
        public int Repetition { get; set; }

        /** This is for future using. */
        public long NumOfTotalSamples { get; set; } = 0;

        /** Constraint coefficients */
        public IList<double[]> A { get; set; }

        /** Constraint coefficients */
        public double[] B { get; set; }

        public override IParameter DefaultBase => DOVSDefaults.ParamBase.Push(P_DOVS_SPECIES);

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            ActiveSolutions = new List<Individual>();
            Ek = new List<Individual>();
            Visited = new List<Individual>();
            VisitedIndexMap = new Dictionary<Individual, int>();
            Corners = new List<CornerMap>();
            // initialize corner map
            for (int i = 0; i < GenomeSize; ++i)
                Corners.Add(new CornerMap());

            IParameter def = DefaultBase;

            Stochastic = state.Parameters.GetBoolean(paramBase.Push(P_STOCHASTIC), def.Push(P_STOCHASTIC), true);
            //ocba = state.parameters.GetBoolean(paramBase.Push(P_OCBA), def.Push(P_OCBA), true);

            InitialReps =
                state.Parameters.GetInt(paramBase.Push(P_INITIAL_REPETITION), def.Push(P_INITIAL_REPETITION), 1);
            if (InitialReps < 1)
                state.Output.Fatal("Initial number of repetitions must be >= 1", paramBase.Push(P_INITIAL_REPETITION),
                    def.Push(P_INITIAL_REPETITION));

            WarmUp = state.Parameters.GetInt(paramBase.Push(P_WARM_UP), def.Push(P_WARM_UP), 1);
            if (WarmUp < 1)
                state.Output.Fatal("Warm-up Period must be >= 1", paramBase.Push(P_WARM_UP), def.Push(P_WARM_UP));

            // read in the constraint
            int size = state.Parameters.GetInt(paramBase.Push(P_CONSTRAINTS_SIZE), def.Push(P_CONSTRAINTS_SIZE), 0);

            A = new List<double[]>();
            B = new double[size];

            if (size > 0)
            {
                // Set up the constraints for A
                for (int x = 0; x < size; x++)
                {
                    IParameter p = paramBase.Push(P_A).Push("" + x);
                    IParameter defp = def.Push(P_A).Push("" + x);

                    double[] d = state.Parameters.GetDoublesUnconstrained(p, defp, this.GenomeSize);
                    if (d == null)
                        state.Output.Fatal(
                            "Row " + x +
                            " of DOVSSpecies constraints array A must be a space- or tab-delimited list of exactly " +
                            this.GenomeSize + " numbers.",
                            p, defp);
                    A.Add(d);
                }
                {
                    // Block to prevent variable name collision
                    IParameter p = paramBase.Push(P_B);
                    IParameter defp = def.Push(P_B);

                    B = state.Parameters.GetDoublesUnconstrained(p, defp, size);
                    if (B == null)
                        state.Output.Fatal(
                            "DOVSSpecies constraints vector b must be a space- or tab-delimited list of exactly " +
                            size + " numbers.",
                            p, defp);
                }
            }

            Repetition = Stochastic ? InitialReps : 1;

        }

        /**
         * Define a most promising area for search of next genertion of individuals.
         */
        public virtual void UpdateMostPromisingArea(IEvolutionState state)
        {
            throw new NotImplementedException("updateMostPromisingArea method not implementd!");
        }

        /**
         * Sample from the most promising area to get new generation of individual
         * for evaluation.
         */
        public virtual IList<Individual> MostPromisingAreaSamples(IEvolutionState state, int size)
        {
            throw new NotImplementedException("mostPromisingAreaSamples method not implementd!");
        }

        /**
         * To find the best sample for each generation, we need to go through each
         * individual in the current population, and also best individual and
         * individuals in actionSolutions. These three type of individuals are
         * exactly the individuals evaluated in DOVSEvaluator.
         */
        public void FindBestSample(IEvolutionState state, Subpopulation subpop)
        {
            // clear Ek
            Ek.Clear();

            IList<Individual> individuals = subpop.Individuals;
            foreach (Individual i in individuals)
                Ek.Add(i);
            foreach (Individual i in ActiveSolutions)
                Ek.Add(i);
            Ek.Add(Visited[OptimalIndex]);
            OptimalIndex = FindOptimalIndividual(Ek);
        }

        /**
         * Given a list of individuals, it will find the one with highest fitness
         * value and retrieve its index in visited solution list.
         */
        private int FindOptimalIndividual(IList<Individual> list)
        {
            double maximum = int.MinValue;
            IntegerVectorIndividual bestInd = null;
            for (int i = 0; i < list.Count; ++i)
            {
                IntegerVectorIndividual ind = (IntegerVectorIndividual) list[i];
                if (((DOVSFitness) ind.Fitness).Mean > maximum)
                {
                    maximum = ((DOVSFitness) ind.Fitness).Mean;
                    bestInd = ind;
                }
            }

            return VisitedIndexMap[bestInd];
        }

        /**
         * This method will take a candidate list and identify is there is redundant
         * individual in it. If yes, it will get rid of the redundant individuals.
         * After that, it will check if all the samples from this generation have
         * been visited in previous generation. If yes, it will retrieve the samples
         * from previous generations.
         */
        public IList<Individual> UniqueSamples(IEvolutionState state, IList<Individual> candidates)
        {
            // first filter out the redundant sample with in the set of candidates
            HashSet<Individual> set = new HashSet<Individual>();
            foreach (Individual i in candidates)
            {
                if (!set.Contains(i))
                    set.Add(i);
            }
            // now all the individual in candidates are unique with in the set
            candidates = new List<Individual>(set);

            // Sk will be the new population
            IList<Individual> sk = new List<Individual>();

            // see if we have these individual in visted array before
            for (int i = 0; i < candidates.Count; ++i)
            {

                IntegerVectorIndividual individual = (IntegerVectorIndividual) candidates[i];

                if (VisitedIndexMap.ContainsKey(individual))
                {
                    // we have this individual before, retrieve that
                    int index = VisitedIndexMap[individual];
                    // get the original individual
                    individual = (IntegerVectorIndividual) Visited[index];
                }
                else
                {
                    Visited.Add(individual);
                    VisitedIndexMap[individual] = Visited.Count - 1;

                    // We add the new individual into the CornerMap
                    // NOTE: if the individual already, we still need to do this?
                    // original code says yes, but it seems to be wrong
                    // so we do this only the new individual is new
                    for (int j = 0; j < GenomeSize; ++j)
                    {
                        // The individual is the content. The key is its
                        // coordinate position
                        Corners[j].Insert(individual.genome[j], individual);
                    }
                }

                sk.Add(individual);
            }

            return sk;
        }
    }
}
