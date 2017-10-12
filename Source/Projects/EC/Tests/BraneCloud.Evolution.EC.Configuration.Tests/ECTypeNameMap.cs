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

using System.Collections.Generic;


namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    // As ugly as the hacks in this file might seem, they are only meant to provide temporary
    // mapping of configuration type names to the corresponding real types in BraneCloud.Evolution.EC
    // and BraneCloud.Evolution.EC.Util namespaces. At some piont the legacy examples will be converted to
    // use a new configuration scheme, and that will allow us to do this in cleaner way.
    //
    // In the meantime, this is a weakly-typed mapping that requires MAINTENANCE if types 
    // or namespaces are added, moved, or renamed!
    // Good practice demands that tests built for this mapping be maintained and you should
    // consider them a MANDATORY part of build validation!

    /// <summary>
    /// This is similar to the "TypeMap" found in the Model namespace except here we must use
    /// strings instead of direct mapping of type in order to avoid circular referencing of assemblies.
    /// Mapping of "normalized" configuration type names to actual "BraneCloud.Evolution.EC" .NET CLR type names.
    /// This is meant to allow for more flexibility in constructing valid configurations, and for translating
    /// legacy names to types that can be activated as the namespaces and types "evolve". 
    /// </summary>
    public static class ECTypeNameMap
    {
        #region Map
        private static readonly Dictionary<string, string> _map
            = new Dictionary<string, string>()
            {
                #region (root)
                { "ec.Breeder", ECNamespace.Root + "." + "Breeder" },
                { "ec.IBreeder", ECNamespace.Root + "." + "IBreeder" },
                { "ec.BreedingPipeline", ECNamespace.Root + "." + "BreedingPipeline" },
                { "ec.IBreedingPipeline", ECNamespace.Root + "." + "IBreedingPipeline" },
                { "ec.BreedingSource", ECNamespace.Root + "." + "BreedingSource" },
                { "ec.Clique", ECNamespace.Root + "." + "IClique" },
                { "ec.IBreedingSource", ECNamespace.Root + "." + "IBreedingSource" },
                { "ec.IClique", ECNamespace.Root + "." + "IClique" },
                { "ec.ECDefaults", ECNamespace.Root + "." + "ECDefaults" },
                { "ec.IDefaults", ECNamespace.Root + "." + "IDefaults" },
                { "ec.Evaluator", ECNamespace.Root + "." + "Evaluator" },
                { "ec.IEvaluator", ECNamespace.Root + "." + "IEvaluator" },
                { "ec.EvolutionState", ECNamespace.Root + "." + "EvolutionState" },
                { "ec.IEvolutionState", ECNamespace.Root + "." + "IEvolutionState" },
                { "ec.Evolve", ECNamespace.Root + "." + "Evolve" },
                { "ec.Exchanger", ECNamespace.Root + "." + "Exchanger" },
                { "ec.Finisher", ECNamespace.Root + "." + "Finisher" },
                { "ec.IFinisher", ECNamespace.Root + "." + "IFinisher" },
                { "ec.Fitness", ECNamespace.Root + "." + "Fitness" },
                { "ec.IFitness", ECNamespace.Root + "." + "IFitness" },
                { "ec.IGroup", ECNamespace.Root + "." + "IGroup" },
                { "ec.Individual", ECNamespace.Root + "." + "Individual" },
                { "ec.Initializer", ECNamespace.Root + "." + "Initializer" },
                { "ec.ModelFactory", ECNamespace.Root + "." + "ModelFactory" },
                { "ec.Population", ECNamespace.Root + "." + "Population" },
                { "ec.IProblem", ECNamespace.Root + "." + "IProblem" },
                { "ec.Problem", ECNamespace.Root + "." + "Problem" },
                { "ec.IPrototype", ECNamespace.Root + "." + "IPrototype" },
                { "ec.SelectionMethod", ECNamespace.Root + "." + "SelectionMethod" },
                { "ec.ISetup", ECNamespace.Root + "." + "ISetup" },
                { "ec.ISingleton", ECNamespace.Root + "." + "ISingleton" },
                { "ec.Singleton", ECNamespace.Root + "." + "ISingleton" },
                { "ec.Species", ECNamespace.Root + "." + "Species" },
                { "ec.Statistics", ECNamespace.Root + "." + "Statistics" },
                { "ec.Subpopulation", ECNamespace.Root + "." + "Subpopulation" },
                #endregion // (root)
                #region Breed
                { "ec.breed.BreedDefaults", ECNamespace.Breed + "." + "BreedDefaults" },
                { "ec.breed.BufferedBreedingPipeline", ECNamespace.Breed + "." + "BufferedBreedingPipeline" },
                { "ec.breed.ForceBreedingPipeline", ECNamespace.Breed + "." + "ForceBreedingPipeline" },
                { "ec.breed.GenerationSwitchPipeline", ECNamespace.Breed + "." + "GenerationSwitchPipeline" },
                { "ec.breed.MultiBreedingPipeline", ECNamespace.Breed + "." + "MultiBreedingPipeline" },
                { "ec.breed.ReproductionPipeline", ECNamespace.Breed + "." + "ReproductionPipeline" },
                #endregion // Breed
                #region CoEvolve
                { "ec.coevolve.CompetitiveEvaluator", ECNamespace.CoEvolve + "." + "CompetitiveEvaluator" },
                { "ec.coevolve.EliteComparator", ECNamespace.CoEvolve + "." + "EliteComparator" },
                { "ec.coevolve.EncapsulatedIndividual", ECNamespace.CoEvolve + "." + "EncapsulatedIndividual" },
                { "ec.coevolve.IndividualAndVictories", ECNamespace.CoEvolve + "." + "IndividualAndVictories" },
                { "ec.coevolve.IndComparator", ECNamespace.CoEvolve + "." + "IndComparator" },
                { "ec.coevolve.CompetitiveEvaluatorThread", ECNamespace.CoEvolve + "." + "CompetitiveEvaluatorThread" },
                { "ec.coevolve.RoundRobinCompetitiveEvaluatorThread", ECNamespace.CoEvolve + "." + "RoundRobinCompetitiveEvaluatorThread" },
                { "ec.coevolve.NRandomOneWayCompetitiveEvaluatorThread", ECNamespace.CoEvolve + "." + "NRandomOneWayCompetitiveEvaluatorThread" },
                { "ec.coevolve.NRandomTwoWayCompetitiveEvaluatorThread", ECNamespace.CoEvolve + "." + "NRandomTwoWayCompetitiveEvaluatorThread" },
                { "ec.coevolve.IGroupedProblem", ECNamespace.CoEvolve + "." + "IGroupedProblem" },
                { "ec.coevolve.MultiPopCoevolutionaryEvaluator", ECNamespace.CoEvolve + "." + "MultiPopCoevolutionaryEvaluator" },
                #endregion // CoEvolve 
                #region DE
                { "ec.de.Best1BinDEBreeder", ECNamespace.DE + "." + "Best1BinDEBreeder" },
                { "ec.de.DEBreeder", ECNamespace.DE + "." + "DEBreeder" },
                { "ec.de.DEEvaluator", ECNamespace.DE + "." + "DEEvaluator" },
                //{ "ec.de.DEStatistics", ECNamespace.DE + "." + "DEStatistics" },
                { "ec.de.Rand1EitherOrDEBreeder", ECNamespace.DE + "." + "Rand1EitherOrDEBreeder" },
                //{ "ec.de.Rand1ExpDEBreeder", ECNamespace.DE + "." + "Rand1ExpDEBreeder" },
                #endregion // DE
                #region ES
                { "ec.es.ESDefaults", ECNamespace.ES + "." + "ESDefaults" },
                { "ec.es.ESSelection", ECNamespace.ES + "." + "ESSelection" },
                { "ec.es.MuCommaLambdaBreeder", ECNamespace.ES + "." + "MuCommaLambdaBreeder" },
                { "ec.es.MuLambdaBreederThread", ECNamespace.ES + "." + "MuLambdaBreederThread" },
                { "ec.es.MuPlusLambdaBreeder", ECNamespace.ES + "." + "MuPlusLambdaBreeder" },
                #endregion // ES
                #region Eval
                { "ec.eval.IJob", ECNamespace.Eval + "." + "IJob" },
                //{ "ec.eval.Job", ECNamespace.Eval + "." + "Job" },
                { "ec.eval.IMasterProblem", ECNamespace.Eval + "." + "IMasterProblem" },
                //{ "ec.eval.MasterProblem", ECNamespace.Eval + "." + "MasterProblem" },
                //{ "ec.eval.ISlave", ECNamespace.Eval + "." + "ISlave" },
                //{ "ec.eval.Slave", ECNamespace.Eval + "." + "Slave" },
                { "ec.eval.ISlaveConnection", ECNamespace.Eval + "." + "ISlaveConnection" },
                //{ "ec.eval.SlaveConnection", ECNamespace.Eval + "." + "SlaveConnection" },
                { "ec.eval.ISlaveMonitor", ECNamespace.Eval + "." + "ISlaveMonitor" },
                //{ "ec.eval.SlaveMonitor", ECNamespace.Eval + "." + "SlaveMonitor" },
                #endregion // Eval
                #region GP
                { "ec.gp.ADF", ECNamespace.GP + "." + "ADF" },
                { "ec.gp.ADFArgument", ECNamespace.GP + "." + "ADFArgument" },
                { "ec.gp.ADFContext", ECNamespace.GP + "." + "ADFContext" },
                { "ec.gp.ADFStack", ECNamespace.GP + "." + "ADFStack" },
                { "ec.gp.ADM", ECNamespace.GP + "." + "ADM" },
                { "ec.gp.ERC", ECNamespace.GP + "." + "ERC" },
                { "ec.gp.GPAtomicType", ECNamespace.GP + "." + "GPAtomicType" },
                { "ec.gp.GPBreedingPipeline", ECNamespace.GP + "." + "GPBreedingPipeline" },
                { "ec.gp.GPData", ECNamespace.GP + "." + "GPData" },
                { "ec.gp.GPDefaults", ECNamespace.GP + "." + "GPDefaults" },
                { "ec.gp.GPFunctionSet", ECNamespace.GP + "." + "GPFunctionSet" },
                { "ec.gp.GPIndividual", ECNamespace.GP + "." + "GPIndividual" },
                { "ec.gp.GPInitializer", ECNamespace.GP + "." + "GPInitializer" },
                { "ec.gp.GPNode", ECNamespace.GP + "." + "GPNode" },
                { "ec.gp.GPNodeBuilder", ECNamespace.GP + "." + "GPNodeBuilder" },
                { "ec.gp.GPNodeConstraints", ECNamespace.GP + "." + "GPNodeConstraints" },
                { "ec.gp.GPNodeGatherer", ECNamespace.GP + "." + "GPNodeGatherer" },
                { "ec.gp.IGPNodeParent", ECNamespace.GP + "." + "IGPNodeParent" },
                { "ec.gp.IGPNodeSelector", ECNamespace.GP + "." + "IGPNodeSelector" },
                { "ec.gp.GPProblem", ECNamespace.GP + "." + "GPProblem" },
                { "ec.gp.GPSetType", ECNamespace.GP + "." + "GPSetType" },
                { "ec.gp.GPSpecies", ECNamespace.GP + "." + "GPSpecies" },
                { "ec.gp.GPTree", ECNamespace.GP + "." + "GPTree" },
                { "ec.gp.GPTreeConstraints", ECNamespace.GP + "." + "GPTreeConstraints" },
                { "ec.gp.GPType", ECNamespace.GP + "." + "GPType" },
                #endregion // EC.GP
                #region GP.Breed
                { "ec.gp.breed.GPBreedDefaults", ECNamespace.GPBreed + "." + "GPBreedDefaults" },
                { "ec.gp.breed.InternalCrossoverPipeline", ECNamespace.GPBreed + "." + "InternalCrossoverPipeline" },
                { "ec.gp.breed.MutateAllNodesPipeline", ECNamespace.GPBreed + "." + "MutateAllNodesPipeline" },
                { "ec.gp.breed.MutateDemotePipeline", ECNamespace.GPBreed + "." + "MutateDemotePipeline" },
                { "ec.gp.breed.MutateERCPipeline", ECNamespace.GPBreed + "." + "MutateERCPipeline" },
                { "ec.gp.breed.MutateOneNodePipeline", ECNamespace.GPBreed + "." + "MutateOneNodePipeline" },
                { "ec.gp.breed.MutatePromotePipeline", ECNamespace.GPBreed + "." + "MutatePromotePipeline" },
                { "ec.gp.breed.MutateSwapPipeline", ECNamespace.GPBreed + "." + "MutateSwapPipeline" },
                { "ec.gp.breed.RehangPipeline", ECNamespace.GPBreed + "." + "RehangPipeline" },
                #endregion // GP.Breed
                #region GP.Build
                { "ec.gp.build.GPBuildDefaults", ECNamespace.GPBuild + "." + "GPBuildDefaults" },
                { "ec.gp.build.PTC1", ECNamespace.GPBuild + "." + "PTC1" },
                { "ec.gp.build.PTC2", ECNamespace.GPBuild + "." + "PTC2" },
                { "ec.gp.build.PTCFunctionSet", ECNamespace.GPBuild + "." + "PTCFunctionSet" },
                { "ec.gp.build.IPTCFunctionSet", ECNamespace.GPBuild + "." + "IPTCFunctionSet" },
                { "ec.gp.build.RandomBranch", ECNamespace.GPBuild + "." + "RandomBranch" },
                { "ec.gp.build.RandTree", ECNamespace.GPBuild + "." + "RandTree" },
                { "ec.gp.build.Uniform", ECNamespace.GPBuild + "." + "Uniform" },
                { "ec.gp.build.UniformGPNodeStorage", ECNamespace.GPBuild + "." + "UniformGPNodeStorage" },
                #endregion // GP.Build
                #region GP.GE
                { "ec.gp.ge.GEDefaults", ECNamespace.GPGE + "." + "GEDefaults" },
                { "ec.gp.ge.GEIndividual", ECNamespace.GPGE + "." + "GEIndividual" },
                { "ec.gp.ge.GEProblem", ECNamespace.GPGE + "." + "GEProblem" },
                { "ec.gp.ge.GESpecies", ECNamespace.GPGE + "." + "GESpecies" },
                { "ec.gp.ge.GrammarFunctionNode", ECNamespace.GPGE + "." + "GrammarFunctionNode" },
                { "ec.gp.ge.GrammarNode", ECNamespace.GPGE + "." + "GrammarNode" },
                { "ec.gp.ge.GrammarParser", ECNamespace.GPGE + "." + "GrammarParser" },
                { "ec.gp.ge.GrammarRuleNode", ECNamespace.GPGE + "." + "GrammarRuleNode" },
                #endregion // GP.GE
                #region GP.Koza
                { "ec.gp.koza.CrossoverPipeline", ECNamespace.GPKoza + "." + "CrossoverPipeline" },
                { "ec.gp.koza.FullBuilder", ECNamespace.GPKoza + "." + "FullBuilder" },
                { "ec.gp.koza.GPKozaDefaults", ECNamespace.GPKoza + "." + "GPKozaDefaults" },
                { "ec.gp.koza.GrowBuilder", ECNamespace.GPKoza + "." + "GrowBuilder" },
                { "ec.gp.koza.HalfBuilder", ECNamespace.GPKoza + "." + "HalfBuilder" },
                { "ec.gp.koza.KozaBuilder", ECNamespace.GPKoza + "." + "KozaBuilder" },
                { "ec.gp.koza.KozaFitness", ECNamespace.GPKoza + "." + "KozaFitness" },
                { "ec.gp.koza.KozaNodeSelector", ECNamespace.GPKoza + "." + "KozaNodeSelector" },
                { "ec.gp.koza.KozaShortStatistics", ECNamespace.GPKoza + "." + "KozaShortStatistics" },
                { "ec.gp.koza.MutationPipeline", ECNamespace.GPKoza + "." + "MutationPipeline" },
                #endregion // GP.Koza
                #region MultiObjective
                { "ec.multiobjective.MultiObjectiveDefaults", ECNamespace.MultiObjective + "." + "MultiObjectiveDefaults" },
                { "ec.multiobjective.MultiObjectiveFitness", ECNamespace.MultiObjective + "." + "MultiObjectiveFitness" },
                { "ec.multiobjective.MultiObjectiveFitnessComparator", ECNamespace.MultiObjective + "." + "MultiObjectiveFitnessComparator" },
                { "ec.multiobjective.MultiObjectiveStatistics", ECNamespace.MultiObjective + "." + "MultiObjectiveStatistics" },
                #endregion // MultiObjective
                #region MultiObjective.NSGA2
                 { "ec.multiobjective.nsga2.NSGA2Breeder", ECNamespace.MultiObjectiveNSGA2 + "." + "NSGA2Breeder" },
                 { "ec.multiobjective.nsga2.NSGA2Evaluator", ECNamespace.MultiObjectiveNSGA2 + "." + "NSGA2Evaluator" },
                 { "ec.multiobjective.nsga2.NSGA2MultiObjectiveFitness", ECNamespace.MultiObjectiveNSGA2 + "." + "NSGA2MultiObjectiveFitness" },
                 { "ec.multiobjective.nsga2.NSGA2MultiObjectiveFitnessComparator", ECNamespace.MultiObjectiveNSGA2 + "." + "NSGA2MultiObjectiveFitnessComparator" },
               #endregion // MultiObjective.NSGA2
                #region MultiObjective.SPEA2
                { "ec.multiobjective.spea2.SPEA2Breeder", ECNamespace.MultiObjectiveSPEA2 + "." + "SPEA2Breeder" },
                { "ec.multiobjective.spea2.SPEA2Evaluator", ECNamespace.MultiObjectiveSPEA2 + "." + "SPEA2Evaluator" },
                { "ec.multiobjective.spea2.SPEA2MultiObjectiveFitness", ECNamespace.MultiObjectiveSPEA2 + "." + "SPEA2MultiObjectiveFitness" },
                //{ "ec.multiobjective.spea2.SPEA2Subpopulation", ECNamespace.MultiObjectiveSPEA2 + "." + "SPEA2Subpopulation" },
                { "ec.multiobjective.spea2.SPEA2TournamentSelection", ECNamespace.MultiObjectiveSPEA2 + "." + "SPEA2TournamentSelection" },
                #endregion // MultiObjective.SPEA2
                #region Parsimony
                { "ec.parsimony.BucketTournamentSelection", ECNamespace.Parsimony + "." + "BucketTournamentSelection" },
                { "ec.parsimony.DoubleTournamentSelection", ECNamespace.Parsimony + "." + "DoubleTournamentSelection" },
                { "ec.parsimony.LexicographicTournamentSelection", ECNamespace.Parsimony + "." + "LexicographicTournamentSelection" },
                { "ec.parsimony.ProportionalTournamentSelection", ECNamespace.Parsimony + "." + "ProportionalTournamentSelection" },
                { "ec.parsimony.RatioBucketTournamentSelection", ECNamespace.Parsimony + "." + "RatioBucketTournamentSelection" },
                { "ec.parsimony.TarpeianStatistics", ECNamespace.Parsimony + "." + "TarpeianStatistics" },
                #endregion // Parsimony
                #region PSO
                { "ec.pso.PSOBreeder", ECNamespace.PSO + "." + "PSOBreeder" },
                { "ec.pso.PSOSubpopulation", ECNamespace.PSO + "." + "PSOSubpopulation" },
                #endregion // PSO
                #region Rule
                { "ec.rule.Rule", ECNamespace.Rule + "." + "Rule" },
                { "ec.rule.RuleConstraints", ECNamespace.Rule + "." + "RuleConstraints" },
                { "ec.rule.RuleDefaults", ECNamespace.Rule + "." + "RuleDefaults" },
                { "ec.rule.RuleIndividual", ECNamespace.Rule + "." + "RuleIndividual" },
                { "ec.rule.RuleInitializer", ECNamespace.Rule + "." + "RuleInitializer" },
                { "ec.rule.RuleSet", ECNamespace.Rule + "." + "RuleSet" },
                { "ec.rule.RuleSetConstraints", ECNamespace.Rule + "." + "RuleSetConstraints" },
                { "ec.rule.RuleSpecies", ECNamespace.Rule + "." + "RuleSpecies" },
                #endregion // Rule
                #region Rule.Breed
                { "ec.rule.breed.RuleCrossoverPipeline", ECNamespace.RuleBreed + "." + "RuleCrossoverPipeline" },
                { "ec.rule.breed.RuleMutationPipeline", ECNamespace.RuleBreed + "." + "RuleMutationPipeline" },
                #endregion // Rule.Breed
                #region Select
                { "ec.select.BestSelection", ECNamespace.Select + "." + "BestSelection" },
                { "ec.select.BoltzmannSelection", ECNamespace.Select + "." + "BoltzmannSelection" },
                { "ec.select.FirstSelection", ECNamespace.Select + "." + "FirstSelection" },
                { "ec.select.FitProportionateSelection", ECNamespace.Select + "." + "FitProportionateSelection" },
                { "ec.select.GreedyOverselection", ECNamespace.Select + "." + "GreedyOverselection" },
                { "ec.select.MultiSelection", ECNamespace.Select + "." + "MultiSelection" },
                { "ec.select.RandomSelection", ECNamespace.Select + "." + "RandomSelection" },
                { "ec.select.SelectDefaults", ECNamespace.Select + "." + "SelectDefaults" },
                { "ec.select.SigmaScalingSelection", ECNamespace.Select + "." + "SigmaScalingSelection" },
                { "ec.select.SUSSelection", ECNamespace.Select + "." + "SUSSelection" },
                { "ec.select.TournamentSelection", ECNamespace.Select + "." + "TournamentSelection" },
                #endregion // Select
                #region Simple
                { "ec.simple.SimpleBreeder", ECNamespace.Simple + "." + "SimpleBreeder" },
                { "ec.simple.SimpleBreederThread", ECNamespace.Simple + "." + "SimpleBreederThread" },
                { "ec.simple.SimpleDefaults", ECNamespace.Simple + "." + "SimpleDefaults" },
                { "ec.simple.SimpleEvaluator", ECNamespace.Simple + "." + "SimpleEvaluator" },
                { "ec.simple.SimpleEvaluatorThread", ECNamespace.Simple + "." + "SimpleEvaluatorThread" },
                { "ec.simple.SimpleEvolutionState", ECNamespace.Simple + "." + "SimpleEvolutionState" },
                { "ec.simple.SimpleExchanger", ECNamespace.Simple + "." + "SimpleExchanger" },
                { "ec.simple.SimpleFinisher", ECNamespace.Simple + "." + "SimpleFinisher" },
                { "ec.simple.SimpleFitness", ECNamespace.Simple + "." + "SimpleFitness" },
                { "ec.simple.SimpleInitializer", ECNamespace.Simple + "." + "SimpleInitializer" },
                { "ec.simple.ISimpleProblem", ECNamespace.Simple + "." + "ISimpleProblem" },
                { "ec.simple.SimpleShortStatistics", ECNamespace.Simple + "." + "SimpleShortStatistics" },
                { "ec.simple.SimpleStatistics", ECNamespace.Simple + "." + "SimpleStatistics" },
                #endregion // Simple
                #region Spatial
                { "ec.spatial.ISpace", ECNamespace.Spatial + "." + "ISpace" },
                { "ec.spatial.Spatial1DSubpopulation", ECNamespace.Spatial + "." + "Spatial1DSubpopulation" },
                { "ec.spatial.SpatialBreeder", ECNamespace.Spatial + "." + "SpatialBreeder" },
                { "ec.spatial.SpatialDefaults", ECNamespace.Spatial + "." + "SpatialDefaults" },
                { "ec.spatial.SpatialMultiPopCoevolutionaryEvaluator", ECNamespace.Spatial + "." + "SpatialMultiPopCoevolutionaryEvaluator" },
                { "ec.spatial.SpatialTournamentSelection", ECNamespace.Spatial + "." + "SpatialTournamentSelection" },
                #endregion // Spatial
                #region SteadyState
                { "ec.steadystate.ISteadyStateExchanger", ECNamespace.SteadyState + "." + "ISteadyStateExchanger" },
                { "ec.steadystate.QueueIndividual", ECNamespace.SteadyState + "." + "QueueIndividual" },
                { "ec.steadystate.SteadyStateBreeder", ECNamespace.SteadyState + "." + "SteadyStateBreeder" },
                { "ec.steadystate.ISteadyStateBSource", ECNamespace.SteadyState + "." + "ISteadyStateBSource" },
                { "ec.steadystate.SteadyStateDefaults", ECNamespace.SteadyState + "." + "SteadyStateDefaults" },
                { "ec.steadystate.SteadyStateEvaluator", ECNamespace.SteadyState + "." + "SteadyStateEvaluator" },
                { "ec.steadystate.SteadyStateEvolutionState", ECNamespace.SteadyState + "." + "SteadyStateEvolutionState" },
                { "ec.steadystate.ISteadyStateStatistics", ECNamespace.SteadyState + "." + "ISteadyStateStatistics" },
                #endregion // SteadyState
                #region Util
                {"ec.util.Checkpoint", ECNamespace.Util + "." + "Checkpoint" },
                {"ec.util.Code", ECNamespace.Util + "." + "Code" },
                //{"ec.util.IDataPipe", ECNamespace.Util + "." + "IDataPipe" },
                //{"ec.util.DataPipe", ECNamespace.Util + "." + "DataPipe" },
                {"ec.util.DecodeReturn", ECNamespace.Util + "." + "DecodeReturn" },
                {"ec.util.IGELexer", ECNamespace.Util + "." + "IGELexer" },
                {"ec.util.GELexer", ECNamespace.Util + "." + "GELexer" },
                //{"ec.util.LocalHost", ECNamespace.Util + "." + "LocalHost" },
                {"ec.util.QuickSort", ECNamespace.Util + "." + "QuickSort" },
                //{"ec.util.ReflectedObject", ECNamespace.Util + "." + "ReflectedObject" },
                {"ec.util.ISortComparator", ECNamespace.Util + "." + "ISortComparator" },
                {"ec.util.ISortComparatorL", ECNamespace.Util + "." + "ISortComparatorL" },
                {"ec.util.TensorFactory", ECNamespace.Util + "." + "TensorFactory" },
                {"ec.util.ECVersion", ECNamespace.Util + "." + "ECVersion" },
                {"ec.util.Version", ECNamespace.Util + "." + "ECVersion" },
                #endregion // Util
                #region Vector
                { "ec.vector.BitVectorIndividual", ECNamespace.Vector + "." + "BitVectorIndividual" },
                { "ec.vector.ByteVectorIndividual", ECNamespace.Vector + "." + "ByteVectorIndividual" },
                { "ec.vector.DoubleVectorIndividual", ECNamespace.Vector + "." + "DoubleVectorIndividual" },
                { "ec.vector.FloatVectorIndividual", ECNamespace.Vector + "." + "FloatVectorIndividual" },
                { "ec.vector.FloatVectorSpecies", ECNamespace.Vector + "." + "FloatVectorSpecies" },
                { "ec.vector.GeneVectorIndividual", ECNamespace.Vector + "." + "GeneVectorIndividual" },
                { "ec.vector.GeneVectorSpecies", ECNamespace.Vector + "." + "GeneVectorSpecies" },
                { "ec.vector.IntegerVectorIndividual", ECNamespace.Vector + "." + "IntegerVectorIndividual" },
                { "ec.vector.IntegerVectorSpecies", ECNamespace.Vector + "." + "IntegerVectorSpecies" },
                { "ec.vector.LongVectorIndividual", ECNamespace.Vector + "." + "LongVectorIndividual" },
                { "ec.vector.ShortVectorIndividual", ECNamespace.Vector + "." + "ShortVectorIndividual" },
                { "ec.vector.VectorDefaults", ECNamespace.Vector + "." + "VectorDefaults" },
                { "ec.vector.VectorGene", ECNamespace.Vector + "." + "VectorGene" },
                { "ec.vector.VectorIndividual", ECNamespace.Vector + "." + "VectorIndividual" },
                { "ec.vector.VectorSpecies", ECNamespace.Vector + "." + "VectorSpecies" },
                #endregion // Vector
                #region Vector.Breed
                { "ec.vector.breed.GeneDuplicationPipeline", ECNamespace.VectorBreed + "." + "GeneDuplicationPipeline" },
                { "ec.vector.breed.ListCrossoverPipeline", ECNamespace.VectorBreed + "." + "ListCrossoverPipeline" },
                { "ec.vector.breed.MultipleVectorCrossoverPipeline", ECNamespace.VectorBreed + "." + "MultipleVectorCrossoverPipeline" },
                { "ec.vector.breed.VectorCrossoverPipeline", ECNamespace.VectorBreed + "." + "VectorCrossoverPipeline" },
                { "ec.vector.breed.VectorMutationPipeline", ECNamespace.VectorBreed + "." + "VectorMutationPipeline" },
                #endregion // Vector.Breed
            };
        #endregion // Map

        public static Dictionary<string, string>.KeyCollection Keys
        {
            get { return _map.Keys; }
        }
        public static Dictionary<string, string>.ValueCollection Values
        {
            get { return _map.Values; }
        }
        public static bool TryGetTypeName(string configTypeName, out string result)
        {
            return _map.TryGetValue(configTypeName, out result);
        }
    }

    public static class ECUtilTypeNameMap
    {
        #region Map
        private static readonly Dictionary<string, string> _map
            = new Dictionary<string, string>()
                  {
                      #region Util
                      {"ec.util.Checkpoint", ECNamespace.Util + "." + "Checkpoint" },
                      {"ec.util.Code", ECNamespace.Util + "." + "Code" },
                      {"ec.util.IDataPipe", ECNamespace.Util + "." + "IDataPipe" },
                      {"ec.util.DataPipe", ECNamespace.Util + "." + "DataPipe" },
                      {"ec.util.DecodeReturn", ECNamespace.Util + "." + "DecodeReturn" },
                      {"ec.util.IGELexer", ECNamespace.Util + "." + "IGELexer" },
                      {"ec.util.GELexer", ECNamespace.Util + "." + "GELexer" },
                      //{"ec.util.LocalHost", ECNamespace.Util + "." + "LocalHost" },
                      {"ec.util.QuickSort", ECNamespace.Util + "." + "QuickSort" },
                      {"ec.util.ReflectedObject", ECNamespace.Util + "." + "ReflectedObject" },
                      {"ec.util.SortComparator", ECNamespace.Util + "." + "SortComparator" },
                      {"ec.util.SortComparatorL", ECNamespace.Util + "." + "SortComparatorL" },
                      {"ec.util.TensorFactory", ECNamespace.Util + "." + "TensorFactory" },
                      {"ec.util.ECVersion", ECNamespace.Util + "." + "ECVersion" },
                      {"ec.util.Version", ECNamespace.Util + "." + "ECVersion" },

                      #endregion // Util
                  };
        #endregion // Map

        public static Dictionary<string, string>.KeyCollection Keys
        {
            get { return _map.Keys; }
        }

        public static Dictionary<string, string>.ValueCollection Values
        {
            get { return _map.Values; }
        }

        public static bool TryGetTypeName(string configTypeName, out string result)
        {
            return _map.TryGetValue(configTypeName, out result);
        }
    }

}