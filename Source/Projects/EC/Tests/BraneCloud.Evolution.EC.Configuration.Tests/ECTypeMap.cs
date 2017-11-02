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
using System.Globalization;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    /// <summary>
    /// Mapping of "canonical" configuration type names to actual "BraneCloud.Evolution.EC" .NET CLR types.
    /// This is meant to allow for more flexibility in constructing valid configurations, and for translating
    /// legacy names to types that can be activated as the namespaces and types "evolve". 
    /// </summary>
    public static class ECTypeMap
    {
        #region Map
        private static readonly Dictionary<string, Type> _map
            = new Dictionary<string, Type>()
            {
                #region (root)
                { "ec.Breeder", typeof(BraneCloud.Evolution.EC.Breeder) },
                { "ec.IBreeder", typeof(BraneCloud.Evolution.EC.IBreeder) },
                { "ec.BreedingPipeline", typeof(BraneCloud.Evolution.EC.BreedingPipeline) },
                { "ec.IBreedingPipeline", typeof(BraneCloud.Evolution.EC.IBreedingPipeline) },
                { "ec.BreedingSource", typeof(BraneCloud.Evolution.EC.BreedingSource) },
                { "ec.IBreedingSource", typeof(BraneCloud.Evolution.EC.IBreedingSource) },
                { "ec.Clique", typeof(BraneCloud.Evolution.EC.IClique) },
                { "ec.IClique", typeof(BraneCloud.Evolution.EC.IClique) },
                { "ec.ECDefaults", typeof(BraneCloud.Evolution.EC.ECDefaults) },
                { "ec.IDefaults", typeof(BraneCloud.Evolution.EC.IDefaults) },
                { "ec.Evaluator", typeof(BraneCloud.Evolution.EC.Evaluator) },
                { "ec.IEvaluator", typeof(BraneCloud.Evolution.EC.IEvaluator) },
                { "ec.EvolutionState", typeof(BraneCloud.Evolution.EC.EvolutionState) },
                { "ec.IEvolutionState", typeof(BraneCloud.Evolution.EC.IEvolutionState) },
                { "ec.Exchanger", typeof(BraneCloud.Evolution.EC.Exchanger) },
                { "ec.Finisher", typeof(BraneCloud.Evolution.EC.Finisher) },
                { "ec.IFinisher", typeof(BraneCloud.Evolution.EC.IFinisher) },
                { "ec.Fitness", typeof(BraneCloud.Evolution.EC.Fitness) },
                { "ec.IFitness", typeof(BraneCloud.Evolution.EC.IFitness) },
                { "ec.IGroup", typeof(BraneCloud.Evolution.EC.IGroup) },
                { "ec.Individual", typeof(BraneCloud.Evolution.EC.Individual) },
                { "ec.Initializer", typeof(BraneCloud.Evolution.EC.Initializer) },
                { "ec.ModelFactory", typeof(BraneCloud.Evolution.EC.ModelFactory) },
                { "ec.Population", typeof(BraneCloud.Evolution.EC.Population) },
                { "ec.IProblem", typeof(BraneCloud.Evolution.EC.IProblem) },
                { "ec.Problem", typeof(BraneCloud.Evolution.EC.Problem) },
                { "ec.IPrototype", typeof(BraneCloud.Evolution.EC.IPrototype) },
                { "ec.SelectionMethod", typeof(BraneCloud.Evolution.EC.SelectionMethod) },
                { "ec.ISetup", typeof(BraneCloud.Evolution.EC.ISetup) },
                { "ec.ISingleton", typeof(BraneCloud.Evolution.EC.ISingleton) },
                { "ec.Singleton", typeof(BraneCloud.Evolution.EC.ISingleton) },
                { "ec.Species", typeof(BraneCloud.Evolution.EC.Species) },
                { "ec.Statistics", typeof(BraneCloud.Evolution.EC.Statistics) },
                { "ec.Subpopulation", typeof(BraneCloud.Evolution.EC.Subpopulation) },
                #endregion // (root)
                #region Breed
                { "ec.gp.breed.BreederThread", typeof(BraneCloud.Evolution.EC.Breed.BreederThread) },
                { "ec.breed.BreedDefaults", typeof(BraneCloud.Evolution.EC.Breed.BreedDefaults) },
                { "ec.breed.BufferedBreedingPipeline", typeof(BraneCloud.Evolution.EC.Breed.BufferedBreedingPipeline) },
                { "ec.breed.ForceBreedingPipeline", typeof(BraneCloud.Evolution.EC.Breed.ForceBreedingPipeline) },
                { "ec.breed.GenerationSwitchPipeline", typeof(BraneCloud.Evolution.EC.Breed.GenerationSwitchPipeline) },
                { "ec.breed.MultiBreedingPipeline", typeof(BraneCloud.Evolution.EC.Breed.MultiBreedingPipeline) },
                { "ec.breed.ReproductionPipeline", typeof(BraneCloud.Evolution.EC.Breed.ReproductionPipeline) },
                #endregion // Breed
                #region CoEvolve
                { "ec.coevolve.CompetitiveEvaluator", typeof(BraneCloud.Evolution.EC.CoEvolve.CompetitiveEvaluator) },
                { "ec.coevolve.EliteComparator", typeof(BraneCloud.Evolution.EC.CoEvolve.EliteComparator) },
                { "ec.coevolve.EncapsulatedIndividual", typeof(BraneCloud.Evolution.EC.CoEvolve.EncapsulatedIndividual) },
                { "ec.coevolve.IndividualAndVictories", typeof(BraneCloud.Evolution.EC.CoEvolve.IndividualAndVictories) },
                { "ec.coevolve.IndComparator", typeof(BraneCloud.Evolution.EC.CoEvolve.IndComparator) },
                { "ec.coevolve.CompetitiveEvaluatorThread", typeof(BraneCloud.Evolution.EC.CoEvolve.CompetitiveEvaluatorThread) },
                { "ec.coevolve.RoundRobinCompetitiveEvaluatorThread", typeof(BraneCloud.Evolution.EC.CoEvolve.RoundRobinCompetitiveEvaluatorThread) },
                { "ec.coevolve.NRandomOneWayCompetitiveEvaluatorThread", typeof(BraneCloud.Evolution.EC.CoEvolve.NRandomOneWayCompetitiveEvaluatorThread) },
                { "ec.coevolve.NRandomTwoWayCompetitiveEvaluatorThread", typeof(BraneCloud.Evolution.EC.CoEvolve.NRandomTwoWayCompetitiveEvaluatorThread) },
                { "ec.coevolve.IGroupedProblem", typeof(BraneCloud.Evolution.EC.CoEvolve.IGroupedProblem) },
                { "ec.coevolve.MultiPopCoevolutionaryEvaluator", typeof(BraneCloud.Evolution.EC.CoEvolve.MultiPopCoevolutionaryEvaluator) },
                #endregion // CoEvolve 
                #region DE
                { "ec.de.Best1BinDEBreeder", typeof(BraneCloud.Evolution.EC.DE.Best1BinDEBreeder) },
                { "ec.de.DEBreeder", typeof(BraneCloud.Evolution.EC.DE.DEBreeder) },
                { "ec.de.DEEvaluator", typeof(BraneCloud.Evolution.EC.Evaluation.DEEvaluator) },
                //{ "ec.de.DEStatistics", typeof(BraneCloud.Evolution.EC.DE.DEStatistics) },
                { "ec.de.Rand1EitherOrDEBreeder", typeof(BraneCloud.Evolution.EC.DE.Rand1EitherOrDEBreeder) },
                //{ "ec.de.Rand1ExpDEBreeder", typeof(BraneCloud.Evolution.EC.DE.Rand1ExpDEBreeder) },
                #endregion // DE
                #region ES
                { "ec.es.ESDefaults", typeof(BraneCloud.Evolution.EC.ES.ESDefaults) },
                { "ec.es.ESSelection", typeof(BraneCloud.Evolution.EC.ES.ESSelection) },
                { "ec.es.MuCommaLambdaBreeder", typeof(BraneCloud.Evolution.EC.ES.MuCommaLambdaBreeder) },
                { "ec.es.MuPlusLambdaBreeder", typeof(BraneCloud.Evolution.EC.ES.MuPlusLambdaBreeder) },
                #endregion // ES
                #region Eval
                { "ec.eval.IJob", typeof(BraneCloud.Evolution.EC.Eval.IJob) },
                //{ "ec.eval.Job", typeof(BraneCloud.Evolution.EC.Eval.Job) },
                { "ec.eval.IMasterProblem", typeof(BraneCloud.Evolution.EC.Eval.IMasterProblem) },
                //{ "ec.eval.MasterProblem", typeof(BraneCloud.Evolution.EC.Eval.MasterProblem) },
                //{ "ec.eval.ISlave", typeof(BraneCloud.Evolution.EC.Eval.ISlave) },
                //{ "ec.eval.Slave", typeof(BraneCloud.Evolution.EC.Eval.Slave) },
                { "ec.eval.ISlaveConnection", typeof(BraneCloud.Evolution.EC.Eval.ISlaveConnection) },
                //{ "ec.eval.SlaveConnection", typeof(BraneCloud.Evolution.EC.Eval.SlaveConnection) },
                { "ec.eval.ISlaveMonitor", typeof(BraneCloud.Evolution.EC.Eval.ISlaveMonitor) },
                //{ "ec.eval.SlaveMonitor", typeof(BraneCloud.Evolution.EC.Eval.SlaveMonitor) },
                #endregion // Eval
                #region Exchange
                //{ "ec.exchange.InterPopulationExchange", typeof(BraneCloud.Evolution.EC.Exchange.InterPopulationExchange) },
                //{ "ec.exchange.IslandExchange", typeof(BraneCloud.Evolution.EC.Exchange.IslandExchange) },
                //{ "ec.exchange.IslandExchangeIslandInfo", typeof(BraneCloud.Evolution.EC.Exchange.IslandExchangeIslandInfo) },
                //{ "ec.exchange.IslandExchangeMailbox", typeof(BraneCloud.Evolution.EC.Exchange.IslandExchangeMailbox) },
                //{ "ec.exchange.IslandExchangeServer", typeof(BraneCloud.Evolution.EC.Exchange.IslandExchangeServer) },
                #endregion // Exchange
                #region GP
                { "ec.gp.ADF", typeof(BraneCloud.Evolution.EC.GP.ADF) },
                { "ec.gp.ADFArgument", typeof(BraneCloud.Evolution.EC.GP.ADFArgument) },
                { "ec.gp.ADFContext", typeof(BraneCloud.Evolution.EC.GP.ADFContext) },
                { "ec.gp.ADFStack", typeof(BraneCloud.Evolution.EC.GP.ADFStack) },
                { "ec.gp.ADM", typeof(BraneCloud.Evolution.EC.GP.ADM) },
                { "ec.gp.ERC", typeof(BraneCloud.Evolution.EC.GP.ERC) },
                { "ec.gp.GPAtomicType", typeof(BraneCloud.Evolution.EC.GP.GPAtomicType) },
                { "ec.gp.GPBreedingPipeline", typeof(BraneCloud.Evolution.EC.GP.GPBreedingPipeline) },
                { "ec.gp.GPData", typeof(BraneCloud.Evolution.EC.GP.GPData) },
                { "ec.gp.GPDefaults", typeof(BraneCloud.Evolution.EC.GP.GPDefaults) },
                { "ec.gp.GPFunctionSet", typeof(BraneCloud.Evolution.EC.GP.GPFunctionSet) },
                { "ec.gp.GPIndividual", typeof(BraneCloud.Evolution.EC.GP.GPIndividual) },
                { "ec.gp.GPInitializer", typeof(BraneCloud.Evolution.EC.GP.GPInitializer) },
                { "ec.gp.GPNode", typeof(BraneCloud.Evolution.EC.GP.GPNode) },
                { "ec.gp.GPNodeBuilder", typeof(BraneCloud.Evolution.EC.GP.GPNodeBuilder) },
                { "ec.gp.GPNodeConstraints", typeof(BraneCloud.Evolution.EC.GP.GPNodeConstraints) },
                { "ec.gp.GPNodeGatherer", typeof(BraneCloud.Evolution.EC.GP.GPNodeGatherer) },
                { "ec.gp.IGPNodeParent", typeof(BraneCloud.Evolution.EC.GP.IGPNodeParent) },
                { "ec.gp.IGPNodeSelector", typeof(BraneCloud.Evolution.EC.GP.IGPNodeSelector) },
                { "ec.gp.GPProblem", typeof(BraneCloud.Evolution.EC.GP.GPProblem) },
                { "ec.gp.GPSetType", typeof(BraneCloud.Evolution.EC.GP.GPSetType) },
                { "ec.gp.GPSpecies", typeof(BraneCloud.Evolution.EC.GP.GPSpecies) },
                { "ec.gp.GPTree", typeof(BraneCloud.Evolution.EC.GP.GPTree) },
                { "ec.gp.GPTreeConstraints", typeof(BraneCloud.Evolution.EC.GP.GPTreeConstraints) },
                { "ec.gp.GPType", typeof(BraneCloud.Evolution.EC.GP.GPType) },
                #endregion // EC.GP
                #region GP.Breed
                { "ec.gp.breed.GPBreedDefaults", typeof(BraneCloud.Evolution.EC.GP.Breed.GPBreedDefaults) },
                { "ec.gp.breed.InternalCrossoverPipeline", typeof(BraneCloud.Evolution.EC.GP.Breed.InternalCrossoverPipeline) },
                { "ec.gp.breed.MutateAllNodesPipeline", typeof(BraneCloud.Evolution.EC.GP.Breed.MutateAllNodesPipeline) },
                { "ec.gp.breed.MutateDemotePipeline", typeof(BraneCloud.Evolution.EC.GP.Breed.MutateDemotePipeline) },
                { "ec.gp.breed.MutateERCPipeline", typeof(BraneCloud.Evolution.EC.GP.Breed.MutateERCPipeline) },
                { "ec.gp.breed.MutateOneNodePipeline", typeof(BraneCloud.Evolution.EC.GP.Breed.MutateOneNodePipeline) },
                { "ec.gp.breed.MutatePromotePipeline", typeof(BraneCloud.Evolution.EC.GP.Breed.MutatePromotePipeline) },
                { "ec.gp.breed.MutateSwapPipeline", typeof(BraneCloud.Evolution.EC.GP.Breed.MutateSwapPipeline) },
                { "ec.gp.breed.RehangPipeline", typeof(BraneCloud.Evolution.EC.GP.Breed.RehangPipeline) },
                #endregion // GP.Breed
                #region GP.Build
                { "ec.gp.build.GPBuildDefaults", typeof(BraneCloud.Evolution.EC.GP.Build.GPBuildDefaults) },
                { "ec.gp.build.PTC1", typeof(BraneCloud.Evolution.EC.GP.Build.PTC1) },
                { "ec.gp.build.PTC2", typeof(BraneCloud.Evolution.EC.GP.Build.PTC2) },
                { "ec.gp.build.PTCFunctionSet", typeof(BraneCloud.Evolution.EC.GP.Build.PTCFunctionSet) },
                { "ec.gp.build.IPTCFunctionSet", typeof(BraneCloud.Evolution.EC.GP.Build.IPTCFunctionSet) },
                { "ec.gp.build.RandomBranch", typeof(BraneCloud.Evolution.EC.GP.Build.RandomBranch) },
                { "ec.gp.build.RandTree", typeof(BraneCloud.Evolution.EC.GP.Build.RandTree) },
                { "ec.gp.build.Uniform", typeof(BraneCloud.Evolution.EC.GP.Build.Uniform) },
                { "ec.gp.build.UniformGPNodeStorage", typeof(BraneCloud.Evolution.EC.GP.Build.UniformGPNodeStorage) },
                #endregion // GP.Build
                #region GP.GE
                { "ec.gp.ge.GEDefaults", typeof(BraneCloud.Evolution.EC.GP.GE.GEDefaults) },
                { "ec.gp.ge.GEIndividual", typeof(BraneCloud.Evolution.EC.GP.GE.GEIndividual) },
                { "ec.gp.ge.GEProblem", typeof(BraneCloud.Evolution.EC.GP.GE.GEProblem) },
                { "ec.gp.ge.GESpecies", typeof(BraneCloud.Evolution.EC.GP.GE.GESpecies) },
                { "ec.gp.ge.GrammarFunctionNode", typeof(BraneCloud.Evolution.EC.GP.GE.GrammarFunctionNode) },
                { "ec.gp.ge.GrammarNode", typeof(BraneCloud.Evolution.EC.GP.GE.GrammarNode) },
                { "ec.gp.ge.GrammarParser", typeof(BraneCloud.Evolution.EC.GP.GE.GrammarParser) },
                { "ec.gp.ge.GrammarRuleNode", typeof(BraneCloud.Evolution.EC.GP.GE.GrammarRuleNode) },
                #endregion // GP.GE
                #region GP.Koza
                { "ec.gp.koza.CrossoverPipeline", typeof(BraneCloud.Evolution.EC.GP.Koza.CrossoverPipeline) },
                { "ec.gp.koza.FullBuilder", typeof(BraneCloud.Evolution.EC.GP.Koza.FullBuilder) },
                { "ec.gp.koza.GPKozaDefaults", typeof(BraneCloud.Evolution.EC.GP.Koza.GPKozaDefaults) },
                { "ec.gp.koza.GrowBuilder", typeof(BraneCloud.Evolution.EC.GP.Koza.GrowBuilder) },
                { "ec.gp.koza.HalfBuilder", typeof(BraneCloud.Evolution.EC.GP.Koza.HalfBuilder) },
                { "ec.gp.koza.KozaBuilder", typeof(BraneCloud.Evolution.EC.GP.Koza.KozaBuilder) },
                { "ec.gp.koza.KozaFitness", typeof(BraneCloud.Evolution.EC.GP.Koza.KozaFitness) },
                { "ec.gp.koza.KozaNodeSelector", typeof(BraneCloud.Evolution.EC.GP.Koza.KozaNodeSelector) },
                { "ec.gp.koza.KozaShortStatistics", typeof(BraneCloud.Evolution.EC.GP.Koza.KozaShortStatistics) },
                { "ec.gp.koza.MutationPipeline", typeof(BraneCloud.Evolution.EC.GP.Koza.MutationPipeline) },
                #endregion // GP.Koza
                #region MultiObjective
                { "ec.multiobjective.MultiObjectiveDefaults", typeof(BraneCloud.Evolution.EC.MultiObjective.MultiObjectiveDefaults) },
                { "ec.multiobjective.MultiObjectiveFitness", typeof(BraneCloud.Evolution.EC.MultiObjective.MultiObjectiveFitness) },
                { "ec.multiobjective.MultiObjectiveFitnessComparator", typeof(BraneCloud.Evolution.EC.MultiObjective.MultiObjectiveFitnessComparator) },
                { "ec.multiobjective.MultiObjectiveStatistics", typeof(BraneCloud.Evolution.EC.MultiObjective.MultiObjectiveStatistics) },
                #endregion // MultiObjective
                #region MultiObjective.NSGA2
                { "ec.multiobjective.nsga2.NSGA2Breeder", typeof(BraneCloud.Evolution.EC.MultiObjective.NSGA2.NSGA2Breeder) },
                { "ec.multiobjective.nsga2.NSGA2Evaluator", typeof(BraneCloud.Evolution.EC.Evaluation.NSGA2Evaluator) },
                { "ec.multiobjective.nsga2.NSGA2MultiObjectiveFitness", typeof(BraneCloud.Evolution.EC.MultiObjective.NSGA2.NSGA2MultiObjectiveFitness) },
                { "ec.multiobjective.nsga2.NSGA2MultiObjectiveFitnessComparator", typeof(BraneCloud.Evolution.EC.MultiObjective.NSGA2.NSGA2MultiObjectiveFitnessComparator) },
                #endregion // MultiObjective.NSGA2
                #region MultiObjective.SPEA2
                { "ec.multiobjective.spea2.SPEA2Breeder", typeof(BraneCloud.Evolution.EC.MultiObjective.SPEA2.SPEA2Breeder) },
                { "ec.multiobjective.spea2.SPEA2Evaluator", typeof(BraneCloud.Evolution.EC.MultiObjective.SPEA2.SPEA2Evaluator) },
                { "ec.multiobjective.spea2.SPEA2MultiObjectiveFitness", typeof(BraneCloud.Evolution.EC.MultiObjective.SPEA2.SPEA2MultiObjectiveFitness) },
                //{ "ec.multiobjective.spea2.SPEA2Subpopulation", typeof(BraneCloud.Evolution.EC.MultiObjective.SPEA2.SPEA2Subpopulation) },
                { "ec.multiobjective.spea2.SPEA2TournamentSelection", typeof(BraneCloud.Evolution.EC.MultiObjective.SPEA2.SPEA2TournamentSelection) },
                #endregion // MultiObjective.SPEA2
                #region Parsimony
                { "ec.parsimony.BucketTournamentSelection", typeof(BraneCloud.Evolution.EC.Parsimony.BucketTournamentSelection) },
                { "ec.parsimony.DoubleTournamentSelection", typeof(BraneCloud.Evolution.EC.Parsimony.DoubleTournamentSelection) },
                { "ec.parsimony.LexicographicTournamentSelection", typeof(BraneCloud.Evolution.EC.Parsimony.LexicographicTournamentSelection) },
                { "ec.parsimony.ProportionalTournamentSelection", typeof(BraneCloud.Evolution.EC.Parsimony.ProportionalTournamentSelection) },
                { "ec.parsimony.RatioBucketTournamentSelection", typeof(BraneCloud.Evolution.EC.Parsimony.RatioBucketTournamentSelection) },
                { "ec.parsimony.TarpeianStatistics", typeof(BraneCloud.Evolution.EC.Parsimony.TarpeianStatistics) },
                #endregion // Parsimony
                #region PSO
                { "ec.pso.PSOBreeder", typeof(BraneCloud.Evolution.EC.PSO.PSOBreeder) },
                { "ec.pso.PSOSubpopulation", typeof(BraneCloud.Evolution.EC.PSO.PSOSubpopulation) },
                #endregion // PSO
                #region Rule
                { "ec.rule.Rule", typeof(BraneCloud.Evolution.EC.Rule.Rule) },
                { "ec.rule.RuleConstraints", typeof(BraneCloud.Evolution.EC.Rule.RuleConstraints) },
                { "ec.rule.RuleDefaults", typeof(BraneCloud.Evolution.EC.Rule.RuleDefaults) },
                { "ec.rule.RuleIndividual", typeof(BraneCloud.Evolution.EC.Rule.RuleIndividual) },
                { "ec.rule.RuleInitializer", typeof(BraneCloud.Evolution.EC.Rule.RuleInitializer) },
                { "ec.rule.RuleSet", typeof(BraneCloud.Evolution.EC.Rule.RuleSet) },
                { "ec.rule.RuleSetConstraints", typeof(BraneCloud.Evolution.EC.Rule.RuleSetConstraints) },
                { "ec.rule.RuleSpecies", typeof(BraneCloud.Evolution.EC.Rule.RuleSpecies) },
                #endregion // Rule
                #region Rule.Breed
                { "ec.rule.breed.RuleCrossoverPipeline", typeof(BraneCloud.Evolution.EC.Rule.Breed.RuleCrossoverPipeline) },
                { "ec.rule.breed.RuleMutationPipeline", typeof(BraneCloud.Evolution.EC.Rule.Breed.RuleMutationPipeline) },
                #endregion // Rule.Breed
                #region Select
                { "ec.select.BestSelection", typeof(BraneCloud.Evolution.EC.Select.BestSelection) },
                { "ec.select.BoltzmannSelection", typeof(BraneCloud.Evolution.EC.Select.BoltzmannSelection) },
                { "ec.select.FirstSelection", typeof(BraneCloud.Evolution.EC.Select.FirstSelection) },
                { "ec.select.FitProportionateSelection", typeof(BraneCloud.Evolution.EC.Select.FitProportionateSelection) },
                { "ec.select.GreedyOverselection", typeof(BraneCloud.Evolution.EC.Select.GreedyOverselection) },
                { "ec.select.MultiSelection", typeof(BraneCloud.Evolution.EC.Select.MultiSelection) },
                { "ec.select.RandomSelection", typeof(BraneCloud.Evolution.EC.Select.RandomSelection) },
                { "ec.select.SelectDefaults", typeof(BraneCloud.Evolution.EC.Select.SelectDefaults) },
                { "ec.select.SigmaScalingSelection", typeof(BraneCloud.Evolution.EC.Select.SigmaScalingSelection) },
                { "ec.select.SUSSelection", typeof(BraneCloud.Evolution.EC.Select.SUSSelection) },
                { "ec.select.TournamentSelection", typeof(BraneCloud.Evolution.EC.Select.TournamentSelection) },
                #endregion // Select
                #region Simple
                { "ec.simple.SimpleBreeder", typeof(BraneCloud.Evolution.EC.Simple.SimpleBreeder) },
                { "ec.simple.SimpleDefaults", typeof(BraneCloud.Evolution.EC.Simple.SimpleDefaults) },
                { "ec.simple.SimpleEvaluator", typeof(BraneCloud.Evolution.EC.Simple.SimpleEvaluator) },
                { "ec.simple.SimpleEvaluator.SimpleEvaluatorThread", typeof(BraneCloud.Evolution.EC.Simple.SimpleEvaluator.SimpleEvaluatorThread) },
                { "ec.simple.SimpleEvolutionState", typeof(BraneCloud.Evolution.EC.Simple.SimpleEvolutionState) },
                { "ec.simple.SimpleExchanger", typeof(BraneCloud.Evolution.EC.Simple.SimpleExchanger) },
                { "ec.simple.SimpleFinisher", typeof(BraneCloud.Evolution.EC.Simple.SimpleFinisher) },
                { "ec.simple.SimpleFitness", typeof(BraneCloud.Evolution.EC.Simple.SimpleFitness) },
                { "ec.simple.SimpleInitializer", typeof(BraneCloud.Evolution.EC.Simple.SimpleInitializer) },
                { "ec.simple.ISimpleProblem", typeof(BraneCloud.Evolution.EC.Simple.ISimpleProblem) },
                { "ec.simple.SimpleShortStatistics", typeof(BraneCloud.Evolution.EC.Simple.SimpleShortStatistics) },
                { "ec.simple.SimpleStatistics", typeof(BraneCloud.Evolution.EC.Simple.SimpleStatistics) },
                #endregion // Simple
                #region Spatial
                { "ec.spatial.ISpace", typeof(BraneCloud.Evolution.EC.Spatial.ISpace) },
                { "ec.spatial.Spatial1DSubpopulation", typeof(BraneCloud.Evolution.EC.Spatial.Spatial1DSubpopulation) },
                { "ec.spatial.SpatialBreeder", typeof(BraneCloud.Evolution.EC.Spatial.SpatialBreeder) },
                { "ec.spatial.SpatialDefaults", typeof(BraneCloud.Evolution.EC.Spatial.SpatialDefaults) },
                { "ec.spatial.SpatialMultiPopCoevolutionaryEvaluator", typeof(BraneCloud.Evolution.EC.Spatial.SpatialMultiPopCoevolutionaryEvaluator) },
                { "ec.spatial.SpatialTournamentSelection", typeof(BraneCloud.Evolution.EC.Spatial.SpatialTournamentSelection) },
                #endregion // Spatial
                #region SteadyState
                { "ec.steadystate.ISteadyStateExchanger", typeof(BraneCloud.Evolution.EC.SteadyState.ISteadyStateExchanger) },
                { "ec.steadystate.QueueIndividual", typeof(BraneCloud.Evolution.EC.SteadyState.QueueIndividual) },
                { "ec.steadystate.SteadyStateBreeder", typeof(BraneCloud.Evolution.EC.SteadyState.SteadyStateBreeder) },
                { "ec.steadystate.ISteadyStateBSource", typeof(BraneCloud.Evolution.EC.SteadyState.ISteadyStateBSource) },
                { "ec.steadystate.SteadyStateDefaults", typeof(BraneCloud.Evolution.EC.SteadyState.SteadyStateDefaults) },
                { "ec.steadystate.SteadyStateEvaluator", typeof(BraneCloud.Evolution.EC.SteadyState.SteadyStateEvaluator) },
                { "ec.steadystate.SteadyStateEvolutionState", typeof(BraneCloud.Evolution.EC.SteadyState.SteadyStateEvolutionState) },
                { "ec.steadystate.ISteadyStateStatistics", typeof(BraneCloud.Evolution.EC.SteadyState.ISteadyStateStatistics) },
                #endregion // SteadyState
                #region Util
                {"ec.util.Checkpoint", typeof (BraneCloud.Evolution.EC.Util.Checkpoint)},
                {"ec.util.Code", typeof (BraneCloud.Evolution.EC.Util.Code)},
                //{"ec.util.IDataPipe", typeof (BraneCloud.Evolution.EC.Util.IDataPipe)},
                //{"ec.util.DataPipe", typeof (BraneCloud.Evolution.EC.Util.DataPipe)},
                {"ec.util.DecodeReturn", typeof (BraneCloud.Evolution.EC.Util.DecodeReturn)},
                {"ec.util.IGELexer", typeof (BraneCloud.Evolution.EC.Util.IGELexer)},
                {"ec.util.GELexer", typeof (BraneCloud.Evolution.EC.Util.GELexer)},
                //{"ec.util.LocalHost", typeof (BraneCloud.Evolution.EC.Util.LocalHost)},
                {"ec.util.QuickSort", typeof (BraneCloud.Evolution.EC.Util.QuickSort)},
                //{"ec.util.ReflectedObject", typeof (BraneCloud.Evolution.EC.Util.ReflectedObject)},
                {"ec.util.ISortComparator", typeof (BraneCloud.Evolution.EC.Util.ISortComparator)},
                {"ec.util.ISortComparatorL", typeof (BraneCloud.Evolution.EC.Util.ISortComparatorL)},
                {"ec.util.TensorFactory", typeof (BraneCloud.Evolution.EC.Util.TensorFactory)},
                {"ec.util.ECVersion", typeof (BraneCloud.Evolution.EC.Util.ECVersion)},
                {"ec.util.Version", typeof (BraneCloud.Evolution.EC.Util.ECVersion)},
                #endregion // Util
                #region Vector
                { "ec.vector.BitVectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.BitVectorIndividual) },
                { "ec.vector.ByteVectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.ByteVectorIndividual) },
                { "ec.vector.DoubleVectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.DoubleVectorIndividual) },
                { "ec.vector.FloatVectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.FloatVectorIndividual) },
                { "ec.vector.FloatVectorSpecies", typeof(BraneCloud.Evolution.EC.Vector.FloatVectorSpecies) },
                { "ec.vector.Gene", typeof(BraneCloud.Evolution.EC.Vector.Gene) },
                { "ec.vector.GeneVectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.GeneVectorIndividual) },
                { "ec.vector.GeneVectorSpecies", typeof(BraneCloud.Evolution.EC.Vector.GeneVectorSpecies) },
                { "ec.vector.IntegerVectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.IntegerVectorIndividual) },
                { "ec.vector.IntegerVectorSpecies", typeof(BraneCloud.Evolution.EC.Vector.IntegerVectorSpecies) },
                { "ec.vector.LongVectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.LongVectorIndividual) },
                { "ec.vector.ShortVectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.ShortVectorIndividual) },
                { "ec.vector.VectorDefaults", typeof(BraneCloud.Evolution.EC.Vector.VectorDefaults) },
                { "ec.vector.VectorIndividual", typeof(BraneCloud.Evolution.EC.Vector.VectorIndividual) },
                { "ec.vector.VectorSpecies", typeof(BraneCloud.Evolution.EC.Vector.VectorSpecies) },
                #endregion // Vector
                #region Vector.Breed
                { "ec.vector.breed.GeneDuplicationPipeline", typeof(BraneCloud.Evolution.EC.Vector.Breed.GeneDuplicationPipeline) },
                { "ec.vector.breed.ListCrossoverPipeline", typeof(BraneCloud.Evolution.EC.Vector.Breed.ListCrossoverPipeline) },
                { "ec.vector.breed.MultipleVectorCrossoverPipeline", typeof(BraneCloud.Evolution.EC.Vector.Breed.MultipleVectorCrossoverPipeline) },
                { "ec.vector.breed.VectorCrossoverPipeline", typeof(BraneCloud.Evolution.EC.Vector.Breed.VectorCrossoverPipeline) },
                { "ec.vector.breed.VectorMutationPipeline", typeof(BraneCloud.Evolution.EC.Vector.Breed.VectorMutationPipeline) },
                #endregion // Vector.Breed
            };
        #endregion // Map

        public static Dictionary<string, Type>.KeyCollection Keys => _map.Keys;

        public static Dictionary<string, Type>.ValueCollection Values => _map.Values;

        public static bool TryGetType(string configTypeName, out Type type)
        {
            return _map.TryGetValue(configTypeName, out type);
        }
    }

    //public static class ECUtilTypeMap
    //{
    //    #region Map
    //    private static readonly Dictionary<string, Type> _map
    //        = new Dictionary<string, Type>()
    //              {
    //                  #region Util
    //                  {"ec.util.Checkpoint", typeof (BraneCloud.Evolution.EC.Util.Checkpoint)},
    //                  {"ec.util.Code", typeof (BraneCloud.Evolution.EC.Util.Code)},
    //                  {"ec.util.IDataPipe", typeof (BraneCloud.Evolution.EC.Util.IDataPipe)},
    //                  {"ec.util.DataPipe", typeof (BraneCloud.Evolution.EC.Util.DataPipe)},
    //                  {"ec.util.DecodeReturn", typeof (BraneCloud.Evolution.EC.Util.DecodeReturn)},
    //                  {"ec.util.IGELexer", typeof (BraneCloud.Evolution.EC.Util.IGELexer)},
    //                  {"ec.util.GELexer", typeof (BraneCloud.Evolution.EC.Util.GELexer)},
    //                  //{"ec.util.LocalHost", typeof (BraneCloud.Evolution.EC.Util.LocalHost)},
    //                  {"ec.util.QuickSort", typeof (BraneCloud.Evolution.EC.Util.QuickSort)},
    //                  {"ec.util.ReflectedObject", typeof (BraneCloud.Evolution.EC.Util.ReflectedObject)},
    //                  {"ec.util.SortComparator", typeof (BraneCloud.Evolution.EC.Util.SortComparator)},
    //                  {"ec.util.SortComparatorL", typeof (BraneCloud.Evolution.EC.Util.SortComparatorL)},
    //                  {"ec.util.TensorFactory", typeof (BraneCloud.Evolution.EC.Util.TensorFactory)},
    //                  {"ec.util.ECVersion", typeof (BraneCloud.Evolution.EC.Util.ECVersion)},
    //                  {"ec.util.Version", typeof (BraneCloud.Evolution.EC.Util.ECVersion)},

    //                  #endregion // Util
    //              };
    //    #endregion // Map

    //    public static Dictionary<string, Type>.KeyCollection Keys
    //    {
    //        get { return _map.Keys; }
    //    }
    //    public static Dictionary<string, Type>.ValueCollection Values
    //    {
    //        get { return _map.Values; }
    //    }
    //    public static Type TryGetType(string configTypeName, out Type type)
    //    {
    //        return _map.TryGetValue(configTypeName, out type);
    //    }
    //}
}