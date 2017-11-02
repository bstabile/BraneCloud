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
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.CoEvolve
{
    /// <summary> 
    /// MultiPopCoevolutionaryEvaluator
    /// 
    /// <p/>MultiPopCoevolutionaryEvaluator is an Evaluator which performs <i>competitive or cooperative multi-population
    /// coevolution</i>.  Competitive coevolution is where individuals' fitness is determined by
    /// testing them against individuals from other subpop.  Cooperative coevolution is where individuals
    /// form teams together with members of other subpops, and the individuals' fitness is computed based
    /// on the performance of such teams.  This evaluator assumes that the problem can only evaluate groups of
    /// individuals containing one individual from each subpop.  Individuals are evaluated regardless of
    /// whether or not they've been evaluated in the past.
    /// 
    /// <p/>Your Problem is responsible for updating up the fitness appropriately with values usually obtained
    /// from teaming up the individual with different partners from the other subpops.
    /// MultiPopCoevolutionaryEvaluator expects to use Problems which adhere to the IGroupedProblem
    /// interface, which defines a new evaluate(...) function, plus a preprocess(...) and postprocess(...) function.
    /// 
    /// <p/>This coevolutionary evaluator is single-threaded -- maybe we'll hack in multithreading later.  It allows
    /// any number of subpops (implicitly, any number of individuals being evaluated together). The order of
    /// individuals in the subpop may be changed during the evaluation process.
    /// 
    /// <p/>Ordinarily MultiPopCoevolutionaryEvaluator does "parallel" coevolution: all subpopulations are evaluated
    /// simultaneously, then bred simultaneously.  But if you set the "sequential" parameter in the class 
    /// ec.simple.SimpleBreeder, then MultiPopCoevolutionary behaves in a sequential fashion common in the "classic"
    /// version of cooperative coevolution: only one subpopulation is evaluated and bred per generation.
    /// The subpopulation index to breed is determined by taking the generation number, modulo the
    /// total number of subpopulations.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><b>breed</b><tt>.sequential</tt><br/>
    /// <font size="-1">boolean (default = false)</font></td>
    /// <td valign="top">(should we evaluate and breed a single subpopulation each generation?  Note that this is a SimpleBreeder parameter. )
    /// </td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>subpop.num-current</tt><br/>
    /// <font size="-1"> int &gt;= 0</font></td>
    /// <td valign="top">(the number of random individuals from any given subpopulation from the current population to be selected as collaborators)
    /// </td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>subpop.num-elites</tt><br/>
    /// <font size="-1"> int &gt;= 0</font></td>
    /// <td valign="top">(the number of elite individuals from any given subpopulation from the previous population to be selected as collaborators. For generation 0, random individuals from the current population will be used.)
    /// </td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>subpop.num-prev</tt><br/>
    /// <font size="-1"> int &gt;= 0</font></td>
    /// <td valign="top">(the number of random individuals from any given subpopulation from the previous population to be selected as collaborators.   For generation 0, random individuals from the current population will be used)
    /// </td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>subpop.X.select-prev</tt><br/>
    /// <font size="-1"> instance of ec.SelectionMethod</font></td>
    /// <td valign="top">(the SelectionMethod used to select partners from the individuals in subpopulation X at the previous generation)
    /// </td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>subpop.X.select-current</tt><br/>
    /// <font size="-1"> instance of ec.SelectionMethod</font></td>
    /// <td valign="top">(the SelectionMethod used to select partners from the individuals in subpopulation X at the current generation.
    /// <b>WARNING.</b>  This SelectionMethod must not select based on fitness, since fitness hasn't been set yet.
    /// RandomSelection is a good choice. )</td></tr>
    /// 
    /// <tr><td valign="top"><i>base.</i><tt>shuffling</tt><br/>
    /// <font size="-1"> boolean (default = false)</font></td>
    /// <td valign="top">(instead of selecting individuals from )
    /// </td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.coevolve.MultiPopCoevolutionaryEvaluator")]
    public class MultiPopCoevolutionaryEvaluator : Evaluator
    {
        #region Constants

        private const long SerialVersionUID = 1;

        /// <summary>
        /// The preamble for selecting partners from each subpop.
        /// </summary>
        public const string P_SUBPOP = "subpop";
        
        /// <summary>
        /// The number of random partners selected from the current generation.
        /// </summary>
        public const string P_NUM_RAND_IND = "num-current";

        // the number of shuffled random partners selected from the current generation
        public const string P_NUM_SHUFFLED = "num-shuffled";

        /// <summary>
        /// The number of elite partners selected from the previous generation.
        /// </summary>
        public const string P_NUM_ELITE = "num-elites";
        protected int NumElite;
        Individual[/*subpopulation*/][/*the elites*/] _eliteIndividuals;

        /// <summary>
        /// The number of random partners selected from the current and previous generations.
        /// </summary>
        public const string P_NUM_IND = "num-prev";

        /// <summary>
        /// The selection method used to select the other partners from the previous generation.
        /// </summary>
        public const string P_SELECTION_METHOD_PREV = "select-prev";

        /// <summary>
        /// The selection method used to select the other partners from the current generation.
        /// </summary>
        public const string P_SELECTION_METHOD_CURRENT = "select-current";

        #endregion // Constants
        #region Fields

        protected int NumCurrent;
        protected int NumShuffled;
        protected int NumPrev;

        Population _previousPopulation;

        SelectionMethod[] _selectionMethodPrev;
        SelectionMethod[] _selectionMethodCurrent;

        /// <summary>
        /// Individuals to evaluate together.
        /// </summary>
        Individual[] _inds;

        /// <summary>
        /// Which individual should have its fitness updated as a result.
        /// </summary>
        bool[] _updates;

        #endregion // Fields
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // evaluators are set up AFTER breeders, so I can check this now
            if (state.Breeder is SimpleBreeder &&
                ((SimpleBreeder)state.Breeder).SequentialBreeding) // we're going sequentil
                state.Output.Message(
                    "The Breeder is breeding sequentially, so the MultiPopCoevolutionaryEvaluator is also evaluating sequentially.");

            // at this point, we do not know the number of subpops, so we read it as well from the parameters file
            var tempSubpop = new Parameter(Initializer.P_POP).Push(Population.P_SIZE);
            var numSubpops = state.Parameters.GetInt(tempSubpop, null, 0);
            if (numSubpops <= 0)
                state.Output.Fatal("Parameter not found, or it has a non-positive value.", tempSubpop);

            NumElite = state.Parameters.GetInt(paramBase.Push(P_NUM_ELITE), null, 0);
            if (NumElite < 0)
                state.Output.Fatal("Parameter not found, or it has an incorrect value.", paramBase.Push(P_NUM_ELITE));

            NumShuffled = state.Parameters.GetInt(paramBase.Push(P_NUM_SHUFFLED), null, 0);
            if (NumShuffled < 0)
                state.Output.Fatal("Parameter not found, or it has an incorrect value.", paramBase.Push(P_NUM_SHUFFLED));

            NumCurrent = state.Parameters.GetInt(paramBase.Push(P_NUM_RAND_IND), null, 0);
            _selectionMethodCurrent = new SelectionMethod[numSubpops];
            if (NumCurrent < 0)
                state.Output.Fatal("Parameter not found, or it has an incorrect value.", paramBase.Push(P_NUM_RAND_IND));
            else if (NumCurrent == 0)
                state.Output.Message("Not testing against current individuals:  Current Selection Methods will not be loaded.");
            else if (NumCurrent > 0)
            {
                for (var i = 0; i < numSubpops; i++)
                {
                    _selectionMethodCurrent[i] = (SelectionMethod)
                                                (state.Parameters.GetInstanceForParameter(
                                                    paramBase.Push(P_SUBPOP).Push("" + i).Push(
                                                        P_SELECTION_METHOD_CURRENT),
                                                    paramBase.Push(P_SELECTION_METHOD_CURRENT), typeof(SelectionMethod)));
                    if (_selectionMethodCurrent[i] == null)
                        state.Output.Error("No selection method provided for subpopulation " + i,
                                           paramBase.Push(P_SUBPOP).Push("" + i).Push(P_SELECTION_METHOD_CURRENT),
                                           paramBase.Push(P_SELECTION_METHOD_CURRENT));
                    else
                        _selectionMethodCurrent[i].Setup(state,
                                                        paramBase.Push(P_SUBPOP).Push("" + i).Push(
                                                            P_SELECTION_METHOD_CURRENT));
                }
            }

            NumPrev = state.Parameters.GetInt(paramBase.Push(P_NUM_IND), null, 0);
            _selectionMethodPrev = new SelectionMethod[numSubpops];

            if (NumPrev < 0)
                state.Output.Fatal("Parameter not found, or it has an incorrect value.", paramBase.Push(P_NUM_IND));
            else if (NumPrev == 0)
                state.Output.Message("Not testing against previous individuals:  Previous Selection Methods will not be loaded.");
            else if (NumPrev > 0)
            {
                for (var i = 0; i < numSubpops; i++)
                {
                    _selectionMethodPrev[i] = (SelectionMethod)
                                             (state.Parameters.GetInstanceForParameter(
                                                 paramBase.Push(P_SUBPOP).Push("" + i).Push(P_SELECTION_METHOD_PREV),
                                                 paramBase.Push(P_SELECTION_METHOD_PREV), typeof(SelectionMethod)));
                    if (_selectionMethodPrev[i] == null)
                        state.Output.Error("No selection method provided for subpopulation " + i,
                                           paramBase.Push(P_SUBPOP).Push("" + i).Push(P_SELECTION_METHOD_PREV),
                                           paramBase.Push(P_SELECTION_METHOD_PREV));
                    else
                        _selectionMethodPrev[i].Setup(state,
                                                     paramBase.Push(P_SUBPOP).Push("" + i).Push(P_SELECTION_METHOD_PREV));
                }
            }

            if (NumElite + NumCurrent + NumPrev + NumShuffled <= 0)
                state.Output.Error("The total number of partners to be selected should be > 0.");
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Operations

        public override bool RunComplete(IEvolutionState state)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the subpopulation should be evaluated.  This will happen if the Breeder
        /// believes that the subpopulation should be breed afterwards.
        /// </summary>
        public bool ShouldEvaluateSubpop(IEvolutionState state, int subpop, int threadnum)
        {
            return state.Breeder is SimpleBreeder &&
                   ((SimpleBreeder) state.Breeder).ShouldBreedSubpop(state, subpop, threadnum);
        }

        public override void EvaluatePopulation(IEvolutionState state)
        {
            // determine who needs to be evaluated
            var preAssessFitness = new bool[state.Population.Subpops.Length];
            var postAssessFitness = new bool[state.Population.Subpops.Length];
            for (var i = 0; i < state.Population.Subpops.Length; i++)
            {
                postAssessFitness[i] = ShouldEvaluateSubpop(state, i, 0);
                preAssessFitness[i] = postAssessFitness[i] || (state.Generation == 0);  // always prepare (set up trials) on generation 0
            }


            // do evaluation            
            BeforeCoevolutionaryEvaluation(state, state.Population, (IGroupedProblem) p_problem);
            
            ((IGroupedProblem) p_problem).PreprocessPopulation(state, state.Population, preAssessFitness, false);
            PerformCoevolutionaryEvaluation(state, state.Population, (IGroupedProblem) p_problem);
            ((IGroupedProblem) p_problem).PostprocessPopulation(state, state.Population, postAssessFitness, false);
            
            AfterCoevolutionaryEvaluation(state, state.Population, (IGroupedProblem) p_problem);
        }
        
        public virtual void  BeforeCoevolutionaryEvaluation(IEvolutionState state, Population pop, IGroupedProblem prob)
        {			
            if (state.Generation == 0)
            {
                // create arrays for the elite individuals in the population at the previous generation.
                // deep clone the elite individuals as random individuals (in the initial generation, nobody has been evaluated yet).
                
                // deal with the elites
                _eliteIndividuals = TensorFactory.Create<Individual>(state.Population.Subpops.Length, NumElite);

                // copy the first individuals in each subpop (they are already randomly generated)
                for (var i = 0; i < _eliteIndividuals.Length; i++)
                {
                    if (NumElite > state.Population.Subpops[i].Individuals.Length)
                        state.Output.Fatal("Number of elite partners is greater than the size of the subpop.");
                    for (var j = 0; j < NumElite; j++)
                        _eliteIndividuals[i][j] = (Individual)(state.Population.Subpops[i].Individuals[j].Clone());  // just take the first N individuals of each subpopulation
                }

                // test for shuffled
                if (NumShuffled > 0)
                {
                    var size = state.Population.Subpops[0].Individuals.Length;
                    for (var i = 0; i < state.Population.Subpops.Length; i++)
                    {
                        if (state.Population.Subpops[i].Individuals.Length != size)
                            state.Output.Fatal("Shuffling was requested in MultiPopCoevolutionaryEvaluator, but the subpopulation sizes are not the same.  " +
                            "Specifically, subpopulation 0 has size " + size + " but subpopulation " + i + " has size " + state.Population.Subpops[i].Individuals.Length);
                    }
                }
            }
        }

        protected void Shuffle(IEvolutionState state, int[] a)
        {
            var mtf = state.Random[0];
            for (var x = a.Length - 1; x >= 1; x--)
            {
                var rand = mtf.NextInt(x + 1);
                var obj = a[x];
                a[x] = a[rand];
                a[rand] = obj;
            }
        }

        public virtual void PerformCoevolutionaryEvaluation(IEvolutionState state, Population pop, IGroupedProblem prob)
        {
            var evaluations = 0;

            _inds = new Individual[pop.Subpops.Length];
            _updates = new bool[pop.Subpops.Length];

            // we start by warming up the selection methods
            if (NumCurrent > 0)
                for (var i = 0; i < _selectionMethodCurrent.Length; i++)
                    _selectionMethodCurrent[i].PrepareToProduce(state, i, 0);

            if (NumPrev > 0)
                for (var i = 0; i < _selectionMethodPrev.Length; i++)
                {
                    // do a hack here
                    var currentPopulation = state.Population;
                    state.Population = _previousPopulation;
                    _selectionMethodPrev[i].PrepareToProduce(state, i, 0);
                    state.Population = currentPopulation;
                }

            // build subpopulation array to pass in each time
            var subpops = new int[state.Population.Subpops.Length];
            for (var j = 0; j < subpops.Length; j++)
                subpops[j] = j;

            // handle shuffled always

            if (NumShuffled > 0)
            {
                int[ /*numShuffled*/][ /*subpop*/][ /*shuffledIndividualIndexes*/] ordering = null;
                // build shuffled orderings
                ordering = TensorFactory.Create<Int32>(NumShuffled, state.Population.Subpops.Length,
                                                       state.Population.Subpops[0].Individuals.Length);
                for (var c = 0; c < NumShuffled; c++)
                {
                    for (var m = 0; m < state.Population.Subpops.Length; m++)
                    {
                        for (var i = 0; i < state.Population.Subpops[0].Individuals.Length; i++)
                            ordering[c][m][i] = i;
                        if (m != 0)
                            Shuffle(state, ordering[c][m]);
                    }
                }

                // for each individual
                for (var i = 0; i < state.Population.Subpops[0].Individuals.Length; i++)
                {
                    for (var k = 0; k < NumShuffled; k++)
                    {
                        for (var ind = 0; ind < _inds.Length; ind++)
                        {
                            _inds[ind] = state.Population.Subpops[ind].Individuals[ordering[k][ind][i]];
                            _updates[ind] = true;
                        }
                        prob.Evaluate(state, _inds, _updates, false, subpops, 0);
                        evaluations++;
                    }
                }
            }

            // for each subpopulation
            for (var j = 0; j < state.Population.Subpops.Length; j++)
            {
                // now do elites and randoms

                if (!ShouldEvaluateSubpop(state, j, 0)) continue;  // don't evaluate this subpopulation

                // for each individual
                for (var i = 0; i < state.Population.Subpops[j].Individuals.Length; i++)
                {
                    var individual = state.Population.Subpops[j].Individuals[i];
                
                    // Test against all the elites
                    for (var k = 0; k < _eliteIndividuals[j].Length; k++)
                    {
                        for (var ind = 0; ind < _inds.Length; ind++)
                        {
                            if (ind == j)
                            {
                                _inds[ind] = individual;
                                _updates[ind] = true;
                            }
                            else
                            {
                                _inds[ind] = _eliteIndividuals[ind][k];
                                _updates[ind] = false;
                            }
                        }
                        prob.Evaluate(state, _inds, _updates, false, subpops, 0);
                        evaluations++;
                    }

                    // test against random selected individuals of the current population
                    for (var k = 0; k < NumCurrent; k++)
                    {
                        for (var ind = 0; ind < _inds.Length; ind++)
                        {
                            if (ind == j)
                            {
                                _inds[ind] = individual; 
                                _updates[ind] = true;
                            }
                            else
                            {
                                _inds[ind] = ProduceCurrent(ind, state, 0); 
                                _updates[ind] = true;
                            }
                        }
                        prob.Evaluate(state, _inds, _updates, false, subpops, 0);
                        evaluations++;
                    }

                    // Test against random selected individuals of previous population
                    for (int k = 0; k < NumPrev; k++)
                    {
                        for (int ind = 0; ind < _inds.Length; ind++)
                        {
                            if (ind == j)
                            {
                                _inds[ind] = individual; 
                                _updates[ind] = true;
                            }
                            else
                            {
                                _inds[ind] = ProducePrevious(ind, state, 0); 
                                _updates[ind] = false;
                            }
                        }
                        prob.Evaluate(state, _inds, _updates, false, subpops, 0);
                        evaluations++;
                    }
                }
            }

            // now shut down the selection methods
            if (NumCurrent > 0)
                for (var i = 0; i < _selectionMethodCurrent.Length; i++)
                    _selectionMethodCurrent[i].FinishProducing(state, i, 0);

            if (NumPrev > 0)
            {
                for (var i = 0; i < _selectionMethodPrev.Length; i++)
                {
                    // do a hack here
                    var currentPopulation = state.Population;
                    state.Population = _previousPopulation;
                    _selectionMethodPrev[i].FinishProducing(state, i, 0);
                    state.Population = currentPopulation;
                }
            }

            state.Output.Message("Evaluations: " + evaluations);
        }

        /// <summary>
        /// Selects one individual from the previous subpopulation.  If there is no previous
        /// population, because we're at generation 0, then an individual from the current
        /// population is selected at random.
        /// </summary>
        protected Individual ProducePrevious(int subpopulation, IEvolutionState state, int thread)
        {
            if (state.Generation == 0)
            {
                // pick current at random.  Can't use a selection method because they may not have fitness assigned
                return state.Population.Subpops[subpopulation].Individuals[
                    state.Random[0].NextInt(state.Population.Subpops[subpopulation].Individuals.Length)];
            }

            // do a hack here -- back up population, replace with the previous population, run the selection method, replace again
            var currentPopulation = state.Population;
            state.Population = _previousPopulation;
            var selected = state.Population.Subpops[subpopulation].Individuals[
                    _selectionMethodPrev[subpopulation].Produce(subpopulation, state, thread)];
            state.Population = currentPopulation;
            return selected;
        }

        /// <summary>
        /// Selects one individual from the given subpopulation.
        /// </summary>
        protected Individual ProduceCurrent(int subpopulation, IEvolutionState state, int thread)
        {
            return state.Population.Subpops[subpopulation].Individuals[
                            _selectionMethodCurrent[subpopulation].Produce(subpopulation, state, thread)];
        }

        public virtual void AfterCoevolutionaryEvaluation(IEvolutionState state, Population pop, IGroupedProblem prob)
        {
            if (NumElite > 0)
            {
                for (var i = 0; i < state.Population.Subpops.Length; i++)
                    if (ShouldEvaluateSubpop(state, i, 0))		// only load elites for subpopulations which are actually changing
                        LoadElites(state, i);
            }

            // copy over the previous population
            if (NumPrev > 0)
            {
                _previousPopulation = (Population)(state.Population.EmptyClone());
                for (var i = 0; i < _previousPopulation.Subpops.Length; i++)
                    for (var j = 0; j < _previousPopulation.Subpops[i].Individuals.Length; j++)
                        _previousPopulation.Subpops[i].Individuals[j] = (Individual)(state.Population.Subpops[i].Individuals[j].Clone());
            }
        }
        
        public virtual void  LoadElites(IEvolutionState state, int whichSubpop)
        {
            var subpop = state.Population.Subpops[whichSubpop];

            if (NumElite == 1)
            {
                var best = 0;
                var oldinds = subpop.Individuals;
                for (var x = 1; x < oldinds.Length; x++)
                    if (oldinds[x].Fitness.BetterThan(oldinds[best].Fitness))
                        best = x;
                _eliteIndividuals[whichSubpop][0] = (Individual)(state.Population.Subpops[whichSubpop].Individuals[best].Clone());
            }
            else if (NumElite > 0)  // we'll need to sort
            {
                var orderedPop = new int[subpop.Individuals.Length];
                for (var x = 0; x < subpop.Individuals.Length; x++) orderedPop[x] = x;

                // sort the best so far where "<" means "more fit than"
                QuickSort.QSort(orderedPop, new EliteComparator(subpop.Individuals));

                // load the top N individuals
                for (var j = 0; j < NumElite; j++)
                    _eliteIndividuals[whichSubpop][j] = (Individual)(state.Population.Subpops[whichSubpop].Individuals[orderedPop[j]].Clone());
            }
        }

        #endregion // Operations
    }
    
    class EliteComparator : ISortComparatorL
    {
        internal Individual[] Inds;
        public EliteComparator(Individual[] inds)
        {
            Inds = inds;
        }
        public virtual bool lt(long a, long b)
        {
            return Inds[(int) a].Fitness.BetterThan(Inds[(int) b].Fitness);
        }
        public virtual bool gt(long a, long b)
        {
            return Inds[(int) b].Fitness.BetterThan(Inds[(int) a].Fitness);
        }
    }
}