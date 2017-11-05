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
using System.Threading;
using System.Threading.Tasks.Dataflow;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Simple;

namespace BraneCloud.Evolution.EC.CoEvolve
{
    /// <summary> 
    /// CompetitiveEvaluator
    /// 
    /// <p/>CompetitiveEvaluator is a Evaluator which performs <i>competitive fitness evaluations</i>.  
    /// Competitive fitness is where individuals' fitness is determined by testing them against 
    /// other members of the same subpop.  Competitive fitness topologies differ from
    /// co-evolution topologies in that co-evolution is a term I generally reserve for 
    /// multiple sbupopulations which breed separately but compete against other subpops 
    /// during evaluation time.  Individuals are evaluated regardless of whether or not they've
    /// been evaluated in the past.
    /// <p/>Your Problem is responsible for setting up the fitness appropriately.  
    /// CompetitiveEvaluator expects to use Problems which adhere to the IGroupedProblem interface, 
    /// which defines a new evaluate(...) function, plus a preprocess(...) and postprocess(...) function.
    /// <p/>This competitive fitness evaluator is single-threaded -- maybe we'll hack in multithreading later. 
    /// And it only has two individuals competing during any fitness evaluation.  The order of individuals in the 
    /// subpop will be changed during the evaluation process.  There are seven evaluation topologies
    /// presently supported:
    /// <p/><dl>
    /// <dt/><b>Single Elimination Tournament</b><dd/>
    /// All members of the population are paired up and evaluated.  In each pair, the "winner" is the individual
    /// which winds up with the superior fitness.  If neither fitness is superior, then the "winner" is picked
    /// at random.  Then all the winners are paired up and evaluated, and so on, just like in a single elimination
    /// tournament.  It is important that the <b>population size be a <i>power of two</i></b>, else some individuals
    /// will not have the same number of "wins" as others and may lose the tournament as a result.
    /// <dt/><b>Round Robin</b><dd/>
    /// Every member of the population are paired up and evaluated with all other members of the population, not
    /// not including the member itself (we might add in self-play as a future later if people ask for it, it's
    /// easy to hack in).
    /// <dt/><b>K-Random-Opponents-One-Way</b><dd/>
    /// Each individual's fitness is calculated based on K competitions against random opponents.
    /// For details, see "A Comparison of Two Competitive Fitness Functions" by Liviu Panait and
    /// Sean Luke in the Proceedings of GECCO 2002.
    /// <dt/><b>K-Random-Opponents-Two-Ways</b><dd/>
    /// Each individual's fitness is calculated based on K competitions against random opponents. The advantage of
    /// this method over <b>K-Random-Opponents-One-Way</b> is a reduced number of competitions (when I competes
    /// against J, both I's and J's fitnesses are updated, while in the previous method only one of the individuals
    /// has its fitness updated).
    /// For details, see "A Comparison of Two Competitive Fitness Functions" by Liviu Panait and
    /// Sean Luke in the Proceedings of GECCO 2002.
    /// </dl> 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>style</tt><br/>
    /// <font size="-1">string with possible values: </font></td>
    /// <td valign="top">(the Style of the tournament)<br/>
    /// <i>single-elim-tournament</i> (a single elimination tournament)<br/>
    /// <i>round-robin</i> (a round robin tournament)<br/>
    /// <i>rand-1-way</i> (K-Random-Opponents, each game counts for only one of the players)<br/>
    /// <i>rand-2-ways</i> (K-Random-Opponents, each game counts for both players)<br/>
    /// </td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>group-size</tt><br/>
    /// <font size="-1"> int</font></td>
    /// <td valign="top">(how many individuals per group, used in <i>rand-1-way</i> and <i>rand-2-ways</i> tournaments)<br/>
    /// <i>group-size</i> &gt;= 1 for <i>rand-1-way</i> or <i>rand-2-ways</i><br/>
    /// </td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>over-eval</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(if the tournament Style leads to an individual playing more games than others, should the extra games be used for his fitness evaluatiuon?)</td></tr>
    /// </table>
    /// </summary>		
    [Serializable]
    [ECConfiguration("ec.coevolve.CompetitiveEvaluator")]
    public class CompetitiveEvaluator : Evaluator
    {
        #region Constants

        public const int STYLE_SINGLE_ELIMINATION = 1;
        public const int STYLE_ROUND_ROBIN = 2;
        public const int STYLE_N_RANDOM_COMPETITORS_ONEWAY = 3;
        public const int STYLE_N_RANDOM_COMPETITORS_TWOWAY = 4;

        public const string P_COMPETE_STYLE = "style";
        public const string P_GROUP_SIZE = "group-size";
        public const string P_OVER_EVAL = "over-eval";

        #endregion // Constants
        #region Properties

        public int Style { get; set; }
        public int GroupSize { get; set; }
        public bool AllowOverEvaluation { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var temp = state.Parameters.GetStringWithDefault(paramBase.Push(P_COMPETE_STYLE), null, "");
            if (temp.ToUpper().Equals("single-elim-tournament".ToUpper()))
            {
                Style = STYLE_SINGLE_ELIMINATION;
            }
            else if (temp.ToUpper().Equals("round-robin".ToUpper()))
            {
                Style = STYLE_ROUND_ROBIN;
            }
            else if (temp.ToUpper().Equals("rand-1-way".ToUpper()))
            {
                Style = STYLE_N_RANDOM_COMPETITORS_ONEWAY;
            }
            else if (temp.ToUpper().Equals("rand-2-way".ToUpper()))
            {
                Style = STYLE_N_RANDOM_COMPETITORS_TWOWAY;
            }
            else if (temp.ToUpper().Equals("rand-2-ways".ToUpper()))
            {
                state.Output.Fatal("'rand-2-ways' is no longer a valid style name: use 'rand-2-way'",
                    paramBase.Push(P_COMPETE_STYLE), null);
            }
            else
            {
                state.Output.Fatal("Incorrect value for parameter. Acceptable values: "
                    + "single-elim-tournament, round-robin, rand-1-way, rand-2-way", paramBase.Push(P_COMPETE_STYLE));
            }

            if (Style == STYLE_N_RANDOM_COMPETITORS_ONEWAY || Style == STYLE_N_RANDOM_COMPETITORS_TWOWAY)
            {
                GroupSize = state.Parameters.GetInt(paramBase.Push(P_GROUP_SIZE), null, 1);
                if (GroupSize < 1)
                {
                    state.Output.Fatal("Incorrect value for parameter", paramBase.Push(P_GROUP_SIZE));
                }
            }

            AllowOverEvaluation = state.Parameters.GetBoolean(paramBase.Push(P_OVER_EVAL), null, false);
        }
        
        #endregion // Setup
        #region Operations

        public override bool RunComplete(IEvolutionState state)
        {
            return false;
        }

        public virtual void RandomizeOrder(IEvolutionState state, Individual[] individuals)
        {
            // copy the inds into a new array, then dump them randomly into the
            // subpop again
            var queue = new Individual[individuals.Length];
            var len = queue.Length;
            Array.Copy(individuals, 0, queue, 0, len);

            for (var x = len; x > 0; x--)
            {
                var i = state.Random[0].NextInt(x);
                individuals[x - 1] = queue[i];
                // get rid of queue[i] by swapping the highest guy there and then
                // decreasing the highest value  :-)
                queue[i] = queue[x - 1];
            }
        }

        /// <summary> 
        /// An evaluator that performs coevolutionary evaluation.  Like SimpleEvaluator,
        /// it applies evolution pipelines, one per thread, to various subchunks of a new population.
        /// </summary>
        public override void EvaluatePopulation(IEvolutionState state)
        {
            var numinds = new int[state.EvalThreads];
            var fromThreads = new int[state.EvalThreads];

            var assessFitness = new bool[state.Population.Subpops.Length];
            for (var i = 0; i < assessFitness.Length; i++)
                assessFitness[i] = true;					// update everyone's fitness in preprocess and postprocess

            for (var y = 0; y < state.EvalThreads; y++)
            {
                // figure numinds
                if (y < state.EvalThreads - 1)
                    // not last one
                    numinds[y] = state.Population.Subpops[0].Individuals.Length / state.EvalThreads;
                else
                    numinds[y] = state.Population.Subpops[0].Individuals.Length / state.EvalThreads
                        + (state.Population.Subpops[0].Individuals.Length
                        - (state.Population.Subpops[0].Individuals.Length / state.EvalThreads) * state.EvalThreads);
                // figure from
                fromThreads[y] = (state.Population.Subpops[0].Individuals.Length / state.EvalThreads) * y;
            }

            RandomizeOrder(state, state.Population.Subpops[0].Individuals);

            var prob = (IGroupedProblem)p_problem.Clone();

            prob.PreprocessPopulation(state, state.Population, assessFitness, Style == STYLE_SINGLE_ELIMINATION);

            switch (Style)
            {
                case STYLE_SINGLE_ELIMINATION:
                    EvalSingleElimination(state, state.Population.Subpops[0].Individuals, 0, prob);
                    break;
                case STYLE_ROUND_ROBIN:
                    EvalRoundRobin(state, fromThreads, numinds, state.Population.Subpops[0].Individuals, 0, prob);
                    break;
                case STYLE_N_RANDOM_COMPETITORS_ONEWAY:
                    EvalNRandomOneWay(state, fromThreads, numinds, state.Population.Subpops[0].Individuals, 0, prob);
                    break;
                case STYLE_N_RANDOM_COMPETITORS_TWOWAY:
                    EvalNRandomTwoWay(state, fromThreads, numinds, state.Population.Subpops[0].Individuals, 0, prob);
                    break;
                default:
                    state.Output.Fatal("Invalid competition style in CompetitiveEvaluator.evaluatePopulation()");
                    break;
            }

            prob.PostprocessPopulation(state, state.Population, assessFitness, Style == STYLE_SINGLE_ELIMINATION);
        }

        public virtual void EvalSingleElimination(IEvolutionState state, Individual[] individuals, int subpop, IGroupedProblem prob)
        {
            // for a single-elimination tournament, the subpop[0] size must be 2^n for
            // some value n.  We don't check that here!  Check it in Setup.

            // create the tournament array
            var tourn = new Individual[individuals.Length];
            Array.Copy(individuals, 0, tourn, 0, individuals.Length);
            var len = tourn.Length;
            var competition = new Individual[2];
            var subpops = new[] { subpop, subpop };
            var updates = new bool[2];
            updates[0] = updates[1] = true;

            // the "top half" of our array will be losers.
            // the bottom half will be winners.  Then we cut our array in half and repeat.
            while (len > 1)
            {
                for (var x = 0; x < len / 2; x++)
                {
                    competition[0] = tourn[x];
                    competition[1] = tourn[len - x - 1];

                    prob.Evaluate(state, competition, updates, true, subpops, 0);
                }

                for (var x = 0; x < len / 2; x++)
                {
                    // if the second individual is better, or coin flip if equal, than we switch them around
                    if (tourn[len - x - 1].Fitness.BetterThan(tourn[x].Fitness) 
                        || tourn[len - x - 1].Fitness.EquivalentTo(tourn[x].Fitness) && state.Random[0].NextBoolean())
                    {
                        Individual temp = tourn[x];
                        tourn[x] = tourn[len - x - 1];
                        tourn[len - x - 1] = temp;
                    }
                }

                // last part of the tournament: deal with odd values of len!
                if (len % 2 != 0)
                    len = 1 + len / 2;
                else
                    len /= 2;
            }
        }

        public virtual void EvalRoundRobin(IEvolutionState state, int[] origins, int[] numinds, Individual[] individuals, int subpop, IGroupedProblem prob)
        {
            if (state.EvalThreads == 1)
                EvalRoundRobinPopChunk(state, origins[0], numinds[0], 0, individuals, subpop, prob);
            else
            {
                ParallelEvaluation<RoundRobinCompetitiveEvaluatorThread>(
                    state, origins, numinds, individuals, subpop, prob);
            }
        }

        void ParallelEvaluation<TEvalThread>(
            IEvolutionState state, 
            int[] origins, 
            int[] numinds,
            Individual[] individuals, 
            int subpop, 
            IGroupedProblem prob)
            where TEvalThread: CompetitiveEvaluatorThread, new()
        {
            // BRS: TPL DataFlow is cleaner and safer than using raw threads.

            // Limit the concurrency in case the user has gone overboard!
            var maxDegree = Math.Min(Environment.ProcessorCount, state.BreedThreads);
            var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegree };

            Action<CompetitiveEvaluatorThread> act = t => t.Run();
            var actionBlock = new ActionBlock<CompetitiveEvaluatorThread>(act, options);

            for (var i = 0; i < state.EvalThreads; i++)
            {
                var runnable = new TEvalThread
                {
                    ThreadNum = i,
                    State = state,
                    Subpop = subpop,
                    NumInds = numinds[i],
                    From = origins[i],
                    Problem = prob,
                    Evaluator = this,
                    Inds = individuals
                };
                actionBlock.Post(runnable);
            }
            actionBlock.Complete();
            actionBlock.Completion.Wait();
        }

        /// <summary> 
        /// A private helper function for evalutatePopulation which evaluates a chunk
        /// of individuals in a subpop for a given thread.
        /// 
        /// Although this method is declared public (for the benefit of a private
        /// helper class in this file), you should not call it.
        /// </summary>
        public virtual void EvalRoundRobinPopChunk(IEvolutionState state, int origin, int numinds, int threadnum, Individual[] individuals, int subpop, IGroupedProblem prob)
        {
            var competition = new Individual[2];
            var subpops = new[] { subpop, subpop };
            var updates = new bool[2];
            updates[0] = updates[1] = true;
            var upperBound = origin + numinds;

            // evaluate chunk of population against entire population
            // since an individual x will be evaluated against all 
            // other individuals <x in other threads, only evaluate it against
            // individuals >x in this thread.
            for (var x = origin; x < upperBound; x++)
            {
                for (var y = x + 1; y < individuals.Length; y++)
                {
                    competition[0] = individuals[x];
                    competition[1] = individuals[y];
                    prob.Evaluate(state, competition, updates, false, subpops, 0);
                }
            }
        }

        public virtual void EvalNRandomOneWay(IEvolutionState state, int[] origins, int[] numinds, Individual[] individuals, int subpop, IGroupedProblem prob)
        {
            if (state.EvalThreads == 1)
                EvalNRandomOneWayPopChunk(state, origins[0], numinds[0], 0, individuals, subpop, prob);
            else
            {
                ParallelEvaluation<NRandomOneWayCompetitiveEvaluatorThread>(
                    state, origins, numinds, individuals, subpop, prob);
            }
        }

        public virtual void EvalNRandomOneWayPopChunk(IEvolutionState state, int origin, int numinds, int threadnum, Individual[] individuals, int subpop, IGroupedProblem prob)
        {
            var queue = new Individual[individuals.Length];
            var len = queue.Length;
            Array.Copy(individuals, 0, queue, 0, len);

            var competition = new Individual[2];
            var subpops = new[] { subpop, subpop };
            var updates = new bool[2];
            updates[0] = true;
            updates[1] = false;
            var upperBound = origin + numinds;

            for (var x = origin; x < upperBound; x++)
            {
                competition[0] = individuals[x];
                // fill up our tournament
                for (var y = 0; y < GroupSize; )
                {
                    // swap to end and remove
                    var index = state.Random[0].NextInt(len - y);
                    competition[1] = queue[index];
                    queue[index] = queue[len - y - 1];
                    queue[len - y - 1] = competition[1];
                    // if the opponent is not the actual individual, we can
                    // have a competition
                    if (competition[1] != individuals[x])
                    {
                        prob.Evaluate(state, competition, updates, false, subpops, 0);
                        y++;
                    }
                }
            }
        }

        public virtual void EvalNRandomTwoWay(IEvolutionState state, int[] origins, int[] numinds, Individual[] individuals, int subpop, IGroupedProblem prob)
        {
            if (state.EvalThreads == 1)
                EvalNRandomTwoWayPopChunk(state, origins[0], numinds[0], 0, individuals, subpop, prob);
            else
            {
                ParallelEvaluation<NRandomTwoWayCompetitiveEvaluatorThread>(
                    state, origins, numinds, individuals, subpop, prob);
            }
        }

        public virtual void EvalNRandomTwoWayPopChunk(IEvolutionState state, int origin, int numinds, int threadnum,
                                                        Individual[] individuals, int subpop, IGroupedProblem prob)
        {

            // the number of games played for each player
            var individualsOrdered = new EncapsulatedIndividual[individuals.Length];
            var queue = new EncapsulatedIndividual[individuals.Length];
            for (var i = 0; i < individuals.Length; i++)
                individualsOrdered[i] = new EncapsulatedIndividual(individuals[i], 0);

            var competition = new Individual[2];
            var subpops = new[] { subpop, subpop };
            var updates = new bool[2];
            updates[0] = true;
            var upperBound = origin + numinds;

            for (var x = origin; x < upperBound; x++)
            {
                Array.Copy(individualsOrdered, 0, queue, 0, queue.Length);
                competition[0] = queue[x].Ind;

                // if the rest of individuals is not enough to fill
                // all games remaining for the current individual
                // (meaning that the current individual has left a
                // lot of games to play versus players with index
                // greater than his own), then it should play with
                // all. In the end, we should check that he finished
                // all the games he needs. If he did, everything is
                // ok, otherwise he should play with some other players
                // with index smaller than his own, but all these games
                // will count only for his fitness evaluation, and
                // not for the opponents' (unless allowOverEvaluations is set to true)

                // if true, it means that he has to play against all opponents with greater index
                if (individuals.Length - x - 1 <= GroupSize - queue[x].NumOpponentsMet)
                {
                    for (var y = x + 1; y < queue.Length; y++)
                    {
                        competition[1] = queue[y].Ind;
                        updates[1] = (queue[y].NumOpponentsMet < GroupSize) || AllowOverEvaluation;
                        prob.Evaluate(state, competition, updates, false, subpops, 0);
                        queue[x].NumOpponentsMet++;
                        if (updates[1])
                            queue[y].NumOpponentsMet++;
                    }
                }
                // here he has to play against a selection of the opponents with greater index
                else
                {
                    // we can use the queue structure because we'll just rearrange the indexes
                    // but we should make sure we also rearrange the other vectors referring to the individuals

                    for (var y = 0; GroupSize > queue[x].NumOpponentsMet; y++)
                    {
                        // swap to the end and remove from list
                        var index = state.Random[0].NextInt(queue.Length - x - 1 - y) + x + 1;
                        competition[1] = queue[index].Ind;

                        updates[1] = (queue[index].NumOpponentsMet < GroupSize) || AllowOverEvaluation;
                        prob.Evaluate(state, competition, updates, false, subpops, 0);
                        queue[x].NumOpponentsMet++;
                        if (updates[1])
                            queue[index].NumOpponentsMet++;

                        // swap the players (such that a player will not be considered twice)
                        var temp = queue[index];
                        queue[index] = queue[queue.Length - y - 1];
                        queue[queue.Length - y - 1] = temp;
                    }
                }

                // if true, it means that the current player needs to play some games with other players with lower indexes.
                // this is an unfortunate situation, since all those players have already had their groupSize games for the evaluation
                if (queue[x].NumOpponentsMet < GroupSize)
                {
                    for (var y = queue[x].NumOpponentsMet; y < GroupSize; y++)
                    {
                        // select a random opponent with smaller index (don't even care for duplicates)
                        int index;
                        if (x > 0)
                            // if x is 0, then there are no players with smaller index, therefore pick a random one
                            index = state.Random[0].NextInt(x);
                        else
                            index = state.Random[0].NextInt(queue.Length - 1) + 1;
                        // use the opponent for the evaluation
                        competition[1] = queue[index].Ind;
                        updates[1] = (queue[index].NumOpponentsMet < GroupSize) || AllowOverEvaluation;
                        prob.Evaluate(state, competition, updates, false, subpops, 0);
                        queue[x].NumOpponentsMet++;
                        if (updates[1])
                            queue[index].NumOpponentsMet++;
                    }
                }
            }
        }

        internal virtual int NextPowerOfTwo(int N)
        {
            var i = 1;
            while (i < N)
                i *= 2;
            return i;
        }

        internal int WhereToPutInformation;

        internal virtual void FillPositions(int[] positions, int who, int totalPerDepth, int total)
        {
            if (totalPerDepth >= total - 1)
            {
                positions[WhereToPutInformation] = who;
                WhereToPutInformation++;
            }
            else
            {
                FillPositions(positions, who, totalPerDepth * 2 + 1, total);
                FillPositions(positions, totalPerDepth - who, totalPerDepth * 2 + 1, total);
            }
        }

        #endregion // Operations
    }

    /// <summary>
    /// Used by the K-Random-Opponents-One-Way and K-Random-Opponents-Two-Ways evaluations
    /// </summary>
    class EncapsulatedIndividual
    {
        public Individual Ind;
        public int NumOpponentsMet;
        public EncapsulatedIndividual(Individual ind, int value)
        {
            Ind = ind;
            NumOpponentsMet = value;
        }
    }

    ///// <summary>
    ///// Used by the Single-Elimination-Tournament, (Double-Elimination-Tournament and World-Cup) evaluations
    ///// </summary>
    //class IndividualAndVictories
    //{
    //    public Individual Ind;
    //    public int Victories;
    //    public IndividualAndVictories(Individual ind, int value)
    //    {
    //        Ind = ind;
    //        Victories = value;
    //    }
    //}

    /// <summary>
    /// SortComparer for Inidividuals
    /// </summary>   
    class IndComparator
    {
        public virtual bool lt(object a, object b)
        {
            return ((Individual)a).Fitness.BetterThan(((Individual)b).Fitness);
        }
        public virtual bool gt(object a, object b)
        {
            return ((Individual)b).Fitness.BetterThan(((Individual)a).Fitness);
        }
    }

    abstract class CompetitiveEvaluatorThread : IThreadRunnable
    {
        protected object SyncLock = new object();

        public int NumInds;
        public int From;
        public CompetitiveEvaluator Evaluator;
        public IEvolutionState State;
        public int ThreadNum;
        public IGroupedProblem Problem;
        public int Subpop;
        public Individual[] Inds;

        public abstract void Run();
    }

    class RoundRobinCompetitiveEvaluatorThread : CompetitiveEvaluatorThread
    {
        public override void Run()
        {
            lock (SyncLock)
            {
                Evaluator.EvalRoundRobinPopChunk(State, From, NumInds, ThreadNum, Inds, Subpop, Problem);
            }
        }
    }
    class NRandomOneWayCompetitiveEvaluatorThread : CompetitiveEvaluatorThread
    {
        public override void Run()
        {
            lock (SyncLock)
            {
                Evaluator.EvalNRandomOneWayPopChunk(State, From, NumInds, ThreadNum, Inds, Subpop, Problem);
            }
        }
    }
    class NRandomTwoWayCompetitiveEvaluatorThread : CompetitiveEvaluatorThread
    {
        public override void Run()
        {
            lock (SyncLock)
            {
                Evaluator.EvalNRandomTwoWayPopChunk(State, From, NumInds, ThreadNum, Inds, Subpop, Problem);
            }
        }
    }
}