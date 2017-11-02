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
using BraneCloud.Evolution.EC.Select;
using BraneCloud.Evolution.EC.SteadyState;

namespace BraneCloud.Evolution.EC.Parsimony
{
    /// <summary> 
    /// DoubleTournamentSelection
    /// 
    /// There are 2 tournaments for each selection of an individual. In the first
    /// ("qualifying") tournament, <i>size</i> individuals
    /// are selected and the <i>best</i> one (based on individuals' length if <i>do-length-first</i>
    /// is true, or based on individual's fitness otherwise). This process repeat <i>size2</i> times,
    /// so we end up with <i>size2</i> winners on one criteria. Then, there is second "champion" tournament
    /// on the other criteria (fitness if <i>do-length-first</i> is true, size otherwise) among the
    /// <i>size2</i> individuals, and the best one is the one returned by this selection method.
    /// 
    /// <p/><b>Typical Number of Individuals Produced Per <tt>produce(...)</tt> call</b><br/>
    /// Always 1.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1 (default 7)</font></td>
    /// <td valign="top">(the tournament size for the initial ("qualifying") tournament)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>size2</tt><br/>
    /// <font size="-1">int &gt;= 1 (default 7)</font></td>
    /// <td valign="top">(the tournament size for the final ("champion") tournament)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>pick-worst</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should we pick the <i>worst</i> individual in the initial ("qualifying") tournament instead of the <i>best</i>?)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>pick-worst2</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    /// <td valign="top">(should we pick the <i>worst</i> individual in the final ("champion") tournament instead of the <i>best</i>?)</td></tr>
    /// <tr><td valign="top"><i>base.</i><tt>do-length-first</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> (default) or <tt>false</tt></font></td>
    /// <td valign="top">(should the initial ("qualifying") tournament be based on the length of the individual or (if false) the fitness of the individual?  The final ("champion") tournament will be based on the alternative option)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// select.double-tournament
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.parsimony.DoubleTournamentSelection")]
    public class DoubleTournamentSelection : SelectionMethod, ISteadyStateBSource
    {
        #region Constants

        /// <summary>default base </summary>
        public const string P_TOURNAMENT = "double-tournament";
        
        public const string P_PICKWORST = "pick-worst";
        public const string P_PICKWORST2 = "pick-worst2";
        
        public const string P_DOLENGTHFIRST = "do-length-first";
        
        /// <summary>
        /// Size parameter 
        /// </summary>
        public const string P_SIZE = "size";
        public const string P_SIZE2 = "size2";
        
        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return SelectDefaults.ParamBase.Push(P_TOURNAMENT); }
        }

        /// <summary>
        /// Size of the tournament
        /// </summary>
        public int Size { get; set; }
        public int Size2 { get; set; }

        /// <summary>
        /// What's our probability of selection? If 1.0, we always pick the "good" individual. 
        /// </summary>
        public double ProbabilityOfSelection { get; set; }
        public double ProbabilityOfSelection2 { get; set; }

        /// <summary>
        /// Do we pick the worst instead of the best? 
        /// </summary>
        public bool PickWorst { get; set; }
        public bool PickWorst2 { get; set; }
        public bool DoLengthFirst { get; set; }

        #endregion // Properties
        #region Setup

        public override void  Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            var def = DefaultBase;
            
            var val = state.Parameters.GetDouble(paramBase.Push(P_SIZE), def.Push(P_SIZE), 1.0);
            if (val < 1.0)
                state.Output.Fatal("Tournament size must be >= 1.", paramBase.Push(P_SIZE), def.Push(P_SIZE));
            else if (val > 1 && val < 2)
            // pick with probability
            {
                Size = 2;
                ProbabilityOfSelection = (val / 2);
            }
            else
            {
                if (val != (int) val)
                // it's not an integer
                    state.Output.Fatal("If >= 2, Tournament size must be an integer.", paramBase.Push(P_SIZE), def.Push(P_SIZE));
                else
                {
                    Size = (int) val;
                    ProbabilityOfSelection = 1.0;
                }
            }
            
            val = state.Parameters.GetDouble(paramBase.Push(P_SIZE2), def.Push(P_SIZE2), 1.0);
            if (val < 1.0)
                state.Output.Fatal("Tournament size2 must be >= 1.", paramBase.Push(P_SIZE2), def.Push(P_SIZE2));
            else if (val > 1 && val < 2)
            // pick with probability
            {
                Size2 = 2;
                ProbabilityOfSelection2 = (val / 2);
            }
            else
            {
                if (val != (int) val)
                // it's not an integer
                    state.Output.Fatal("If >= 2, Tournament size2 must be an integer.", paramBase.Push(P_SIZE2), def.Push(P_SIZE2));
                else
                {
                    Size2 = (int) val;
                    ProbabilityOfSelection2 = 1.0;
                }
            }
            
            DoLengthFirst = state.Parameters.GetBoolean(paramBase.Push(P_DOLENGTHFIRST), def.Push(P_DOLENGTHFIRST), true);
            PickWorst = state.Parameters.GetBoolean(paramBase.Push(P_PICKWORST), def.Push(P_PICKWORST), false);
            PickWorst2 = state.Parameters.GetBoolean(paramBase.Push(P_PICKWORST2), def.Push(P_PICKWORST2), false);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Produces the index of a person selected from among several by a tournament.
        /// The tournament's criteria is fitness of individuals if doLengthFirst is true,
        /// otherwise the size of the individuals.
        /// </summary>
        public override int Produce(int subpop, IEvolutionState state, int thread)
        {
            var inds = new int[Size2];
            for (var x = 0; x < Size2; x++)
                inds[x] = Make(subpop, state, thread);
            
            if (!DoLengthFirst)
            {
                // pick size random individuals, then pick the best.
                var oldinds = state.Population.Subpops[subpop].Individuals;
                var i = inds[0];
                var bad = i;
                
                for (var x = 1; x < Size2; x++)
                {
                    var j = inds[x];
                    if (PickWorst2)
                    {
                        if (oldinds[j].Size > oldinds[i].Size)
                        {
                            bad = i; 
                            i = j;
                        }
                        else
                            bad = j;
                    }
                    else
                    {
                        if (oldinds[j].Size < oldinds[i].Size)
                        {
                            bad = i; 
                            i = j;
                        }
                        else
                            bad = j;
                    }
                }
                
                if (ProbabilityOfSelection2 != 1.0 && !state.Random[thread].NextBoolean(ProbabilityOfSelection2))
                    i = bad;
                return i;
            }
            else
            {
                // pick size random individuals, then pick the best.
                var oldinds = state.Population.Subpops[subpop].Individuals;
                var i = inds[0];
                var bad = i;
                
                for (var x = 1; x < Size2; x++)
                {
                    var j = inds[x];
                    if (PickWorst2)
                    {
                        if (!(oldinds[j].Fitness.BetterThan(oldinds[i].Fitness)))
                        {
                            bad = i; 
                            i = j;
                        }
                        else
                            bad = j;
                    }
                    else
                    {
                        if (oldinds[j].Fitness.BetterThan(oldinds[i].Fitness))
                        {
                            bad = i; 
                            i = j;
                        }
                        else
                            bad = j;
                    }
                }
                
                if (ProbabilityOfSelection2 != 1.0 && !state.Random[thread].NextBoolean(ProbabilityOfSelection2))
                    i = bad;
                return i;
            }
        }
        
        /// <summary>
        /// Produces the index of a person selected from among several by a tournament.
        /// The tournament's criteria is size of individuals if doLengthFirst is true,
        /// otherwise the fitness of the individuals.
        /// </summary>
        public virtual int Make(int subpop, IEvolutionState state, int thread)
        {
            if (DoLengthFirst)
            // if length first, the first tournament is based on size
            {
                // pick size random individuals, then pick the best.
                var oldinds = state.Population.Subpops[subpop].Individuals;
                var i = state.Random[thread].NextInt(oldinds.Length);
                var bad = i;
                
                for (var x = 1; x < Size; x++)
                {
                    var j = state.Random[thread].NextInt(oldinds.Length);
                    if (PickWorst)
                    {
                        if (oldinds[j].Size > oldinds[i].Size) { bad = i; i = j; } else bad = j;
                    }
                    else
                    {
                        if (oldinds[j].Size < oldinds[i].Size) { bad = i; i = j; } else bad = j;
                    }
                }
                
                if (ProbabilityOfSelection != 1.0 && !state.Random[thread].NextBoolean(ProbabilityOfSelection))
                    i = bad;
                return i;
            }
            else
            {
                // pick size random individuals, then pick the best.
                var oldinds = state.Population.Subpops[subpop].Individuals;
                var i = state.Random[thread].NextInt(oldinds.Length);
                var bad = i;
                
                for (var x = 1; x < Size; x++)
                {
                    var j = state.Random[thread].NextInt(oldinds.Length);
                    if (PickWorst)
                    {
                        if (! oldinds[j].Fitness.BetterThan(oldinds[i].Fitness)) { bad = i; i = j; } else bad = j;
                    }
                    else
                    {
                        if ( oldinds[j].Fitness.BetterThan(oldinds[i].Fitness)) { bad = i; i = j; } else bad = j;
                    }
                }
                
                if (ProbabilityOfSelection != 1.0 && !state.Random[thread].NextBoolean(ProbabilityOfSelection))
                    i = bad;
                return i;
            }
        }
                
        public virtual void  IndividualReplaced(SteadyStateEvolutionState state, int subpop, int thread, int individual)
        {
            return ;
        }
        
        public virtual void  SourcesAreProperForm(SteadyStateEvolutionState state)
        {
            return ;
        }

        #endregion // Operations
    }
}