using System;
using System.IO;
using System.Diagnostics;

using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.SteadyState;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP.Koza
{
    ///// <summary> 
    ///// A simple Koza-style statistics generator.  Prints the mean fitness 
    ///// (raw,adjusted,hits) and best individual of each generation.
    ///// At the end, prints the best individual of the run and the number of
    ///// individuals processed.
    ///// 
    ///// <p/>If gather-full is true, then final timing information, number of nodes
    ///// and depths of trees, approximate final memory utilization, etc. are also given.
    ///// 
    ///// <p/>Compressed files will be overridden on restart from checkpoint; uncompressed files will be 
    ///// appended on restart.
    ///// 
    ///// <p/>KozaStatistics implements a simple version of steady-state statistics in the
    ///// same fashion that SimpleStatistics does: if it quits before a generation boundary,
    ///// it will include the best individual discovered, even if the individual was discovered
    ///// after the last boundary.  This is done by using individualsEvaluatedStatistics(...)
    ///// to update best-individual-of-generation in addition to doing it in
    ///// postEvaluationStatistics(...).
    ///// 
    ///// <p/><b>Parameters</b><br/>
    ///// <table>
    ///// <tr><td valign="top"><i>base.</i><tt>gzip</tt><br/>
    ///// <font size="-1">boolean</font></td>
    ///// <td valign="top">(whether or not to compress the file (.gz suffix added)</td></tr>
    ///// <tr><td valign="top"><i>base.</i><tt>file</tt><br/>
    ///// <font size="-1">String (a filename), or nonexistant (signifies stdout)</font></td>
    ///// <td valign="top">(the log for statistics)</td></tr>
    ///// <tr><td valign="top"><i>base</i>.<tt>gather-full</tt><br/>
    ///// <font size="-1">bool = <tt>true</tt> or <tt>false</tt> (default)</font></td>
    ///// <td valign="top">(should we full statistics on individuals (will run slower, 
    ///// though the slowness is due to off-line processing that won't mess up timings)</td></tr>
    ///// </table>
    ///// </summary>
    //[Serializable]
    //[Obsolete("Use SimpleStatistics instead.")]
    //[ECConfiguration("ec.gp.koza.KozaStatistics")]
    //public class KozaStatistics : Statistics, ISteadyStateStatistics
    //{
    //    /// <summary>
    //    /// Log file parameter 
    //    /// </summary>
    //    public const string P_STATISTICS_FILE = "file";
        
    //    /// <summary>
    //    /// The Statistics' log 
    //    /// </summary>
    //    public int StatisticsLog;
        
    //    /// <summary>
    //    /// The best individual we've found so far 
    //    /// </summary>
    //    public Individual[] BestOfRun;
    //    public Individual[] GetBestSoFar() { return BestOfRun; }

        
    //    /// <summary>
    //    /// Compress? 
    //    /// </summary>
    //    public const string P_COMPRESS = "gzip";
        
    //    public const string P_FULL = "gather-full";
        
    //    internal bool DoFull;
        
    //    /// <summary>
    //    /// Total number of individuals
    //    /// </summary>
    //    internal long NumInds;
        
    //    // timings
    //    internal long LastTime;
    //    internal long InitializationTime;
    //    internal long BreedingTime;
    //    internal long EvaluationTime;
    //    internal long NodesInitialized;
    //    internal long NodesEvaluated;
    //    internal long NodesBred;
        
    //    // memory usage info
    //    internal long LastUsage = 0;
    //    internal long InitializationUsage = 0;
    //    internal long BreedingUsage = 0;
    //    internal long EvaluationUsage = 0;
        
    //    public KozaStatistics()
    //    {
    //        BestOfRun = null; 
    //        StatisticsLog = 0; /* stdout */
    //    }
        
    //    public override void Setup(IEvolutionState state, IParameter paramBase)
    //    {
    //        base.Setup(state, paramBase);

    //        state.Output.WarnOnce("KozaStatistics is deprecated and will soon be deleted.  Use SimpleStatistics instead.");

    //        var statisticsFile = state.Parameters.GetFile(paramBase.Push(P_STATISTICS_FILE), null);
            
    //        if (statisticsFile != null)
    //            try
    //            {
    //                StatisticsLog = state.Output.AddLog(statisticsFile,
    //                              !state.Parameters.GetBoolean(paramBase.Push(P_COMPRESS), null, false), 
    //                               state.Parameters.GetBoolean(paramBase.Push(P_COMPRESS), null, false));
    //            }
    //            catch (IOException i)
    //            {
    //                state.Output.Fatal("An IOException occurred while trying to create the log " + statisticsFile + ":\n" + i);
    //            }
            
    //        DoFull = state.Parameters.GetBoolean(paramBase.Push(P_FULL), null, false);
    //        NodesInitialized = NodesEvaluated = NodesBred = 0;
    //        BreedingTime = EvaluationTime = 0;
    //    }		
        
    //    public override void PreInitializationStatistics(IEvolutionState state)
    //    {
    //        base.PreInitializationStatistics(state);
    //        if (DoFull)
    //        {
    //            var currentProcess = Process.GetCurrentProcess();
    //            LastTime = (DateTime.Now.Ticks - 621355968000000000) / 10000;
    //            // BRS : TODO : Check whether or not this performance counter is the right one to use here.
    //            LastUsage = currentProcess.WorkingSet64;
    //        }
    //    }
        
    //    public override void PostInitializationStatistics(IEvolutionState state)
    //    {
    //        base.PostInitializationStatistics(state);
            
    //        // set up our Best_Of_Run array -- can't do this in Setup, because
    //        // we don't know if the number of subpops has been determined yet
    //        BestOfRun = new Individual[state.Population.Subpops.Length];
            
    //        // gather timings       
    //        if (DoFull)
    //        {
    //            var currentProcess = Process.GetCurrentProcess();
    //            // BRS : TODO : Select an appropriate performance counter (WorkingSet64 may not be the most relevant).
    //            var curU = currentProcess.WorkingSet64;
    //            if (curU > LastUsage)
    //                InitializationUsage = curU - LastUsage;
    //            InitializationTime = (DateTime.Now.Ticks - 621355968000000000) / 10000 - LastTime;
                
    //            // Determine how many nodes we have
    //            for (var x = 0; x < state.Population.Subpops.Length; x++)
    //            {
    //                // check to make sure they're the right class
    //                if (!(state.Population.Subpops[x].Species is GPSpecies))
    //                    state.Output.Fatal("Subpopulation " + x + " is not of the species form GPSpecies." 
    //                                                + "  Cannot do timing statistics with KozaStatistics.");
                    
    //                for (var y = 0; y < state.Population.Subpops[x].Individuals.Length; y++)
    //                {
    //                    var i = (GPIndividual) (state.Population.Subpops[x].Individuals[y]);
    //                    for (var z = 0; z < i.Trees.Length; z++)
    //                        NodesInitialized += i.Trees[z].Child.NumNodes(GPNode.NODESEARCH_ALL);
    //                }
    //            }
    //        }
    //    }
        
    //    public override void PreBreedingStatistics(IEvolutionState state)
    //    {
    //        base.PreBreedingStatistics(state);
    //        if (DoFull)
    //        {
    //            var r = Process.GetCurrentProcess();
    //            LastTime = (DateTime.Now.Ticks - 621355968000000000) / 10000;
    //            LastUsage = r.WorkingSet64;
    //        }
    //    }
        
    //    public override void  PostBreedingStatistics(IEvolutionState state)
    //    {
    //        base.PostBreedingStatistics(state);
    //        // gather timings
    //        if (DoFull)
    //        {
    //            var r = Process.GetCurrentProcess();
    //            var curU = r.WorkingSet64;
    //            if (curU > LastUsage)
    //                BreedingUsage += curU - LastUsage;
    //            BreedingTime += (DateTime.Now.Ticks - 621355968000000000) / 10000 - LastTime;
                
    //            // Determine how many nodes we have
    //            for (var x = 0; x < state.Population.Subpops.Length; x++)
    //            {
    //                // check to make sure they're the right class
    //                if (!(state.Population.Subpops[x].Species is GPSpecies))
    //                    state.Output.Fatal("Subpopulation " + x + " is not of the species form GPSpecies." 
    //                                                + "  Cannot do timing statistics with KozaStatistics.");
                    
    //                for (var y = 0; y < state.Population.Subpops[x].Individuals.Length; y++)
    //                {
    //                    var i = (GPIndividual) (state.Population.Subpops[x].Individuals[y]);
    //                    for (var z = 0; z < i.Trees.Length; z++)
    //                        NodesBred += i.Trees[z].Child.NumNodes(GPNode.NODESEARCH_ALL);
    //                }
    //            }
    //        }
    //    }
        
    //    public override void PreEvaluationStatistics(IEvolutionState state)
    //    {
    //        base.PreEvaluationStatistics(state);
    //        if (DoFull)
    //        {
    //            var r = Process.GetCurrentProcess();
    //            LastTime = (DateTime.Now.Ticks - 621355968000000000) / 10000;
    //            LastUsage = r.WorkingSet64;
    //        }
    //    }
        
    //    public override void PostEvaluationStatistics(IEvolutionState state)
    //    {
    //        base.PostEvaluationStatistics(state);
            
    //        // Gather statistics
    //        var r = Process.GetCurrentProcess();
    //        var curU = r.WorkingSet64;
    //        if (curU > LastUsage)
    //            EvaluationUsage += curU - LastUsage;
    //        if (DoFull)
    //            EvaluationTime += (DateTime.Now.Ticks - 621355968000000000) / 10000 - LastTime;
            
            
    //        state.Output.PrintLn("\n\n\nGeneration " + state.Generation + "\n================", StatisticsLog);
            
    //        var best_i = new Individual[state.Population.Subpops.Length];
    //        for (var x = 0; x < state.Population.Subpops.Length; x++)
    //        {
    //            state.Output.PrintLn("\nSubpopulation " + x + "\n----------------", StatisticsLog);
                
    //            // gather timings
    //            if (DoFull)
    //            {
    //                long totNodesPerGen = 0;
    //                long totDepthPerGen = 0;
                    
    //                // check to make sure they're the right class
    //                if (!(state.Population.Subpops[x].Species is GPSpecies))
    //                    state.Output.Fatal("Subpopulation " + x + " is not of the species form GPSpecies." 
    //                                                + "  Cannot do timing statistics with KozaStatistics.");
                    
    //                var numNodes = new long[((GPIndividual) (state.Population.Subpops[x].Species.I_Prototype)).Trees.Length];
    //                var numDepth = new long[((GPIndividual) (state.Population.Subpops[x].Species.I_Prototype)).Trees.Length];
                    
    //                for (var y = 0; y < state.Population.Subpops[x].Individuals.Length; y++)
    //                {
    //                    var i = (GPIndividual) (state.Population.Subpops[x].Individuals[y]);
    //                    for (var z = 0; z < i.Trees.Length; z++)
    //                    {
    //                        NodesEvaluated += i.Trees[z].Child.NumNodes(GPNode.NODESEARCH_ALL);
    //                        numNodes[z] += i.Trees[z].Child.NumNodes(GPNode.NODESEARCH_ALL);
    //                        numDepth[z] += i.Trees[z].Child.Depth;
    //                    }
    //                }
                    
    //                for (var tr = 0; tr < numNodes.Length; tr++)
    //                    totNodesPerGen += numNodes[tr];

    //                state.Output.PrintLn("Avg Nodes: " + ((double) totNodesPerGen) 
    //                                                   / state.Population.Subpops[x].Individuals.Length, 
    //                                                   StatisticsLog);

    //                state.Output.Print("Nodes/tree: [", StatisticsLog);
    //                for (var tr = 0; tr < numNodes.Length; tr++)
    //                {
    //                    if (tr > 0)
    //                        state.Output.Print("|", StatisticsLog);

    //                    state.Output.Print("" + ((double) numNodes[tr]) 
    //                                          / state.Population.Subpops[x].Individuals.Length, StatisticsLog);
    //                }
    //                state.Output.PrintLn("]", StatisticsLog);
                    
    //                for (var tr = 0; tr < numDepth.Length; tr++)
    //                    totDepthPerGen += numDepth[tr];

    //                state.Output.PrintLn("Avg Depth: " + ((double) totDepthPerGen) 
    //                                                   / (state.Population.Subpops[x].Individuals.Length * numDepth.Length), 
    //                                                   StatisticsLog);

    //                state.Output.Print("Depth/tree: [", StatisticsLog);
    //                for (var tr = 0; tr < numDepth.Length; tr++)
    //                {
    //                    if (tr > 0)
    //                        state.Output.Print("|", StatisticsLog);
    //                    state.Output.Print("" + ((double) numDepth[tr]) 
    //                                          / state.Population.Subpops[x].Individuals.Length, StatisticsLog);
    //                }
    //                state.Output.PrintLn("]", StatisticsLog);
    //            }
                                
    //            var meanStandardized = 0.0f;
    //            var meanAdjusted = 0.0f;
    //            var hits = 0;
                
    //            if (!(state.Population.Subpops[x].Species.F_Prototype is KozaFitness))
    //                state.Output.Fatal("Subpopulation " + x + " is not of the fitness KozaFitness. "
    //                                            + " Cannot do timing statistics with KozaStatistics.");
                
                
    //            best_i[x] = state.Population.Subpops[x].Individuals[0];
    //            for (var y = 0; y < state.Population.Subpops[x].Individuals.Length; y++)
    //            {
    //                // best individual
    //                if (state.Population.Subpops[x].Individuals[y].Fitness.BetterThan(best_i[x].Fitness))
    //                    best_i[x] = state.Population.Subpops[x].Individuals[y];
    //                // mean for population
    //                meanStandardized += ((KozaFitness) (state.Population.Subpops[x].Individuals[y].Fitness)).StandardizedFitness;
    //                meanAdjusted += ((KozaFitness) (state.Population.Subpops[x].Individuals[y].Fitness)).AdjustedFitness;
    //                hits += ((KozaFitness) (state.Population.Subpops[x].Individuals[y].Fitness)).Hits;
    //            }
                
    //            // compute fitness stats
    //            meanStandardized /= state.Population.Subpops[x].Individuals.Length;
    //            meanAdjusted /= state.Population.Subpops[x].Individuals.Length;

    //            state.Output.Print("Mean fitness raw: " + meanStandardized + " adjusted: " + meanAdjusted 
    //                            + " hits: " + ((double) hits) / state.Population.Subpops[x].Individuals.Length, 
    //                                                                        StatisticsLog);
                
    //            state.Output.PrintLn("", StatisticsLog);
                
    //            // compute inds stats
    //            NumInds += state.Population.Subpops[x].Individuals.Length;
    //        }
            
    //        // now test to see if it's the new Best_Of_Run
    //        for (var x = 0; x < state.Population.Subpops.Length; x++)
    //        {
    //            if (BestOfRun[x] == null || best_i[x].Fitness.BetterThan(BestOfRun[x].Fitness))
    //                BestOfRun[x] = (Individual) (best_i[x].Clone());
                
    //            // print the best-of-generation individual
    //            state.Output.PrintLn("\nBest Individual of Generation:", StatisticsLog);
    //            best_i[x].PrintIndividualForHumans(state, StatisticsLog);
    //            state.Output.Message("Subpop " + x + " best fitness of generation: " + best_i[x].Fitness.FitnessToStringForHumans());
    //        }
    //    }
               
    //    /// <summary>
    //    /// Logs the best individual of the run. 
    //    /// </summary>
    //    public override void FinalStatistics(IEvolutionState state, int result)
    //    {
    //        base.FinalStatistics(state, result);
            
    //        state.Output.PrintLn("\n\n\nFinal Statistics\n================", StatisticsLog);
            
    //        state.Output.PrintLn("Total Individuals Evaluated: " + NumInds, StatisticsLog);
    //        // for now we just print the best fitness 
            
    //        state.Output.PrintLn("\nBest Individual of Run:", StatisticsLog);
    //        for (var x = 0; x < state.Population.Subpops.Length; x++)
    //        {
    //            BestOfRun[x].PrintIndividualForHumans(state, StatisticsLog);
    //            state.Output.Message("Subpop " + x + " best fitness of run: " + BestOfRun[x].Fitness.FitnessToStringForHumans());
                
    //            // finally describe the winner if there is a description
    //            ((ISimpleProblem)(state.Evaluator.p_problem.Clone())).Describe(state, BestOfRun[x], x, 0, StatisticsLog);
    //        }
            
    //        // Output timings
    //        if (DoFull)
    //        {
    //            state.Output.PrintLn("\n\n\nTimings\n=======", StatisticsLog);
                
    //            state.Output.PrintLn("Initialization: " + ((float) InitializationTime) / 1000 + " secs total, " 
    //                + NodesInitialized + " nodes, " + NodesInitialized / (((float) InitializationTime) / 1000) 
    //                + " nodes/sec", StatisticsLog);

    //            state.Output.PrintLn("Evaluating: " + ((float) EvaluationTime) / 1000 + " secs total, " + NodesEvaluated + " nodes, " 
    //                + NodesEvaluated / (((float) EvaluationTime) / 1000) + " nodes/sec", StatisticsLog);

    //            state.Output.PrintLn("Breeding: " + ((float) BreedingTime) / 1000 + " secs total, " + NodesBred + " nodes, " 
    //                + NodesBred / (((float) BreedingTime) / 1000) + " nodes/sec", StatisticsLog);
                
    //            state.Output.PrintLn("\n\n\nMemory Usage\n==============", StatisticsLog);

    //            state.Output.PrintLn("Initialization: " + ((float) InitializationUsage) / 1024 + " KB total, " + NodesInitialized + " nodes, " 
    //                                + NodesInitialized / (((float) InitializationUsage) / 1024) + " nodes/KB", StatisticsLog);

    //            state.Output.PrintLn("Evaluating: " + ((float) EvaluationUsage) / 1024 + " KB total, " + NodesEvaluated + " nodes, " 
    //                              + NodesEvaluated / (((float) EvaluationUsage) / 1024) + " nodes/KB", StatisticsLog);

    //            state.Output.PrintLn("Breeding: " + ((float) BreedingUsage) / 1024 + " KB total, " + NodesBred + " nodes, " 
    //                                 + NodesBred / (((float) BreedingUsage) / 1024) + " nodes/KB", StatisticsLog);
    //        }
    //    }
    //}
}