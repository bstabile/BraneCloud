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
using System.Collections;
using System.IO;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.Runtime.Eval
{

    /// <summary>
    /// MetaProblem
    /// 
    /// <p/>MetaProblem is a special class for implenting so-called "Meta-Evolutionary Algorithms",
    /// a topic related to "HyperHeuristics".  In a Meta-EA, an evolutionary system is used to
    /// optimize the parameters for another evolutionary system.
    ///
    /// <p/>We will refer to the EA used to optimize the parameters as the<i> Meta-EA</i> or<i>meta-level EA</i>.
    /// The EA whose parameters are getting optimized will be called the<i> Base EA</i>.
    ///
    /// <p/>In order to optimize the parameters for a base EA, one must be able to
    /// test those parameters, and this is done by running a base evolutionary system using those 
    /// parameters and seeing how well it performs.This means generating a second instance
    /// of an ECJ system.ECJ does this in the same Java process as the original system: so you must
    /// account for this in your memory consumption.
    ///
    /// <p/>The way it works is as follows.First, you set up the base-level ECJ system as normal, with a
    /// parameter file defining all of its parameters (ideally including default settings for the
    /// parameters you'll be optimizing).  You will probably also want to prevent statistics from writing
    /// anything to files or printing anything to the screen, which would be very inefficient when
    /// doing meta-level stuff.
    ///
    /// <tt><p/><pre>     stat.silent = true </pre></tt>
    ///
    /// <p/>Next you set up the meta-level ECJ system.Here you define the problem class as a MetaProblem:
    ///
    /// <tt><pre>    eval.problem = ec.eval.MetaProblem</pre></tt>
    ///
    /// <p/>Next you tell the meta-level ECJ system where the parameter file for the base-level ECJ system is,
    /// so it can set up the base-level system from that file:
    ///
    /// <tt><pre>    eval.problem.file = base-ec.params    </pre></tt>
    ///
    /// <p/>MetaProblem assesses the fitness of its individuals(the parameter settings for the base ECJ
    /// system) by running the base ECJ system some N times using those parameter settings, gathering
    /// the best-of-run fitnesses from each of those N times, and taking the mean of the fitnesses.This
    /// means that the base ECJ system must use a fitness facility which can be reduced to a single
    /// number.Further, both ECJ systems (meta-level and base) should use the same Fitness class.
    /// The value of N is important: if you make it too high, you're wasting valuable time in testing.  But
    /// if you make it too low, you will get inaccurate fitness results and you'll have a lot of noise in
    /// your testing.To specify the N number of tests to 10 (for example), you say:
    ///
    /// <tt><pre>    eval.problem.runs = 10    </pre></tt>
    ///
    /// <p/>Because evaluations are noisy and random, you will probably want to guarantee that individuals
    /// are reevaluated if they show up again in a later generation.To do this, you say:
    /// 
    /// <tt><pre>    eval.problem.reevaluate = true </pre></tt>
    ///
    /// <p/> Actually you don't need to say this because it's the default value.But if you want to avoid
    /// reevaluating individuals, you must explicitly set it to false.
    ///
    /// <p/>Now we get to specifying the actual parameters which will be optimized.  The outer ECJ system
    /// must use a DoubleVectorIndividual.  The genome size of that individual must be exactly the number
    /// of parameters you're trying to optimize in the base ECJ system.  If there are 5 parameters,
    /// for example, you might say:
    ///
    /// <tt><pre>    pop.subpop.0.species = ec.vector.FloatVectorSpecies </pre></tt>
    /// <tt><pre> pop.subpop.0.species.ind = ec.vector.DoubleVectorIndividual </pre></tt>
    /// <tt><pre> pop.subpop.0.species.genome-size = 5 </pre></tt>
    ///
    /// <p/> We suggest you keep the default min-gene and max-gene values something simple, such as 0.0 and 1.0
    /// respectively, and likewise some default mutation data.  And a crossover parameter perhaps:
    ///
    /// <tt><pre>    pop.subpop.0.species.min-gene = 0.0 </pre></tt>
    /// <tt><pre> pop.subpop.0.species.max-gene = 1.0 </pre></tt>
    /// <tt><pre> pop.subpop.0.species.mutation-prob = 0.25 </pre></tt>
    /// <tt><pre> pop.subpop.0.species.mutation-type = gauss </pre></tt>
    /// <tt><pre> pop.subpop.0.species.mutation-stdev = 0.1 </pre></tt>
    /// <tt><pre> pop.subpop.0.species.mutation-bounded = true </pre></tt>
    /// <tt><pre> pop.subpop.0.species.out-of-bounds-retries = 100 </pre></tt>
    /// <tt><pre> pop.subpop.0.species.crossover-type = one </pre></tt>
    ///
    /// <p/> The genes in this genome are not necessarily going to be treated as doubles, however.This
    /// is because not all parameters are doubles.  While something like gaussian mutation variance
    /// is a double, population size is an integer.  Furthermore, some parameters are booleans, and
    /// others are values chosen from a set of possible strings, such as one-point ("one"), two-point ("two"), and
    /// uniform ("any") crossover (3 strings).  To handle this, we will encode in the double vector 
    /// any of the following kinds of data:
    ///
    /// <ul>
    /// <li/> Double values between a min and max value inclusive.
    /// <li/> Integer values between a min and a max value inclusive.
    /// <li/> Integer values from 0 through n-1, representing some N possible string values.
    /// <li/> Boolean values, represented as the integers 0 and 1
    /// </ul>
    ///
    /// <p/>For example, let's say you're trying to optimize the following parameters.
    /// 
    /// <p/><ol>
    /// <li/> Mutation probability (a double)
    /// <li/> Mutation type (one of "reset", "gauss", or "polynomial")
    /// <li/> Mutation standard deviation for gauss mutaton (a double)
    /// <li/> Mutation distribution index for polynomial mutation (an integer)
    /// <li/> Use of the alternative polynomial mutation version (a boolean)
    /// </ol>
    ///
    /// <p/>For each of these we need to specify the mutation type used for the gene
    /// responsible for that parameter.  In our example, we will use our default mutation
    /// (gaussian, probability 0.25, stdev 0.1, bounded) for our two double parameters,
    /// "reset" mutation for the second and fifth parameters, and integer random walk mutation for
    /// the fourth parameter.  Declaring these mutation types will also determine the
    /// initialization procedure for those genes: see "Heterogeneous Vectors" in the manual
    /// for more information.
    /// 
    /// <p/>Finally we need to specify the parameter associated with each gene.  We do that with
    /// a parameter like this:
    ///
    /// <tt><pre>    eval.problem.param.0 = pop.subpop.0.species.mutation-prob    </pre></tt>
    ///
    /// If the parameter value is numerical or boolean, MetaProblem will create the right value for 
    /// it automatically. If the parameter value is a string, you need which string value
    /// corresponds to the number stored in the gene.  For example:
    ///
    /// <tt><pre>    eval.problem.param.1 = pop.subpop.0.species.mutation-type    </pre></tt>
    /// <tt><pre>    eval.problem.param.1.num-vals = 3    </pre></tt>
    /// <tt><pre>    eval.problem.param.1.val.0 = reset    </pre></tt>
    /// <tt><pre>    eval.problem.param.1.val.1 = gauss    </pre></tt>
    /// <tt><pre>    eval.problem.param.1.val.2 = polynomial    </pre></tt>
    ///
    /// <p/>So we need to specify two things: information about how the gene is mutated
    /// (and hence initialized), and information about how it is to be interpreted as a parameter.
    /// In the parameters below note that we often omit mutation information when we are relying
    /// on some default we defined above:
    ///
    /// <tt><pre>    pop.subpop.0.species.genome-size = 5    </pre></tt>
    /// <tt><pre>    eval.problem.num-params = 5    </pre></tt>
    /// <br/>
    /// <tt><pre>    eval.problem.param.0 = pop.subpop.0.species.mutation-prob    </pre></tt>
    /// <tt><pre>    eval.problem.param.0.type = float    </pre></tt>
    /// <br/>
    /// <tt><pre>    eval.problem.param.1 = pop.subpop.0.species.mutation-type    </pre></tt>
    /// <tt><pre>    eval.problem.param.1.num-vals = 3    </pre></tt>
    /// <tt><pre>    eval.problem.param.1.val.0 = reset    </pre></tt>
    /// <tt><pre>    eval.problem.param.1.val.1 = gauss    </pre></tt>
    /// <tt><pre>    eval.problem.param.1.val.2 = polynomial    </pre></tt>
    /// <tt><pre>    pop.subpop.0.species.max-gene.1 = 2    </pre></tt>
    /// <tt><pre>    pop.subpop.0.species.mutation-type.1 = integer-reset    </pre></tt>
    /// <br/> 
    /// <tt><pre>    eval.problem.param.2 = pop.subpop.0.species.mutation-stdev    </pre></tt>
    /// <tt><pre>    eval.problem.param.2.type = float    </pre></tt>
    /// <br/>
    /// <tt><pre>    eval.problem.param.3 = pop.subpop.0.species.mutation-distribution-index    </pre></tt>
    /// <tt><pre>    eval.problem.param.3.type = integer    </pre></tt>
    /// <tt><pre>    pop.subpop.0.species.max-gene.3 = 10    </pre></tt>
    /// <tt><pre>    pop.subpop.0.species.mutation-type.3 = integer-random-walk   </pre></tt>
    /// <tt><pre>    pop.subpop.0.species.random-walk-probability.3 = 0.8    </pre></tt>
    /// <br/>
    /// <tt><pre>    eval.problem.param.4 = pop.subpop.0.species.alternative-polynomial-version    </pre></tt>
    /// <tt><pre>    eval.problem.param.4.type = boolean    </pre></tt>
    /// <tt><pre>    pop.subpop.0.species.mutation-type.4 = integer-reset    </pre></tt>
    ///
    /// <p/>If the mappings above are insufficient for you, you can create your own by overriding
    /// two methods: 
    ///
    /// <ul>
    /// <p/><li/>
    ///     loadDomain(...) sets up the domain from the parameters above.  You could override this
    ///     to interpret your own parameters as you saw fit, or simply to turn off parameter loading
    ///     entirely.
    ///
    /// <p/><li/>
    ///     map(...) actually maps a gene into a parameter value (a string).  You could override this
    ///     to provide your own mapping, either hard coded, or from some version of loadDomain(...)
    ///     you created.  If you override this method, you'll want to override loadDomain(...) for
    ///     sure, if only to turn it off.
    /// </ul>
    ///
    /// <p/><b>Preparing for the final run</b> Once you've got everything working, you probably want
    /// to eliminate all output at the base level before starting the big meta-level run.You can
    /// do this in a base-level parameter file like this:
    ///
    /// <tt><pre>    silent = true </pre></tt>
    ///
    /// <p/><b>Caveats.</b> A meta-level individual is tested by setting a base-level EA with its
    /// parameters, then running the base-level EA, then extracting the best individual of the run and
    /// getting its fitness.  This is done some N times, and the fitness is combined from these
    /// N, using the method<b> combine(...)</b>.  By default this method simply does<b> setToMeanOf(...)</b>,
    /// but you might want to do something else.  Note that this means that by default it's going
    /// to be difficult to have multiobjective fitness at either level without overriding the combine()
    /// method.  Furthermore, because the fitness is extracted from just the first subpopulation of the
    /// base-level EA, this implies that you probably only want one subpopulation at the base-level,
    /// except in the case of competitive coevolution where you ultimately don't care about the fitness of othe
    /// other subpopulations.  If you have more than one subpopulation at the base level, you will 
    /// receive a one-time warning.
    ///
    /// <p/>MetaProblem gathers two kinds of statistics of interest to you.  First, it gathers the
    /// best individual of run, mearning the DoubleVectorIndividual whose parameters on average
    /// produced the highest best-fitness-of-run runs in the base system.  This is gathered
    /// using the standard statistics procedures.Second it gathers the best individual discovered
    /// among the various<i>base runs</i>.This individual is reported, at the end, during
    /// the <b>describe(...)</b> method, and appears at the end of the statistics file
    /// if you are using SimpleStatistics at the meta-level.
    ///
    ///
    /// <p/>Finally, <b>yes, MetaProblem can be recursive.</b>  You can set things up so that you're
    /// evolving the parameters for an EC system which evolves the parameters for an EC system which
    /// evolves the parameters for an EC system.
    ///
    ///
    /// <p/><b>Parameters</b><br/>
    /// 
    /// <table>
    /// <tr><td valign="top" ><tt><i> base </i>.file </tt><br/>
    ///
    /// <font size="-1"> filename </font></td>
    ///
    /// <td valign="top" > (the filename of the "base" (lower-level) parameter file<i>i</i>)</td></tr>
    /// <tr><td valign="top" ><tt><i> base </i>.runs </tt><br/>
    /// < font size="-1">int >= 1 (default=1)</font></td>
    /// <td valign="top" > (the number of base-level evolutionary runs performed to assess the fitness of a meta individual)</td></tr>
    /// <tr><td valign="top" ><tt><i> base </i>.reevaluate </tt><br/>
    /// <font size="-1">boolean(default=true)</font></td>
    /// <td valign="top" > (when a meta individual has its evaluated flag set, should we reevaluate it anyway?)</td></tr>
    /// <tr><td valign="top" ><tt><i> base </i>.set - random </tt><br/>
    /// <font size="-1">boolean(default=false)</font></td>
    /// <td valign="top" > (Should we silence the stdout and stderr logs of the Output of the base EA?)</td></tr>
    /// <tr><td valign="top" ><tt><i> base </i>.num -params</tt><br/>
    /// <font size = "-1" > int >= 1 </font></td>
    /// <td valign="top">(How many parameters are being evolved? This should match the genome length of the meta-level EA individuals)</td></tr>
    ///
    /// <tr><td valign="top" ><tt><i> base </i>.param.< i > number </i></tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top" > (The parameter name)</td></tr>
    ///
    /// <tr><td valign="top" ><tt><i> base </i>.param.< i > number </i>.type </tt><br/>
    /// <font size="-1">String, one of: <tt> integer boolean float</tt> (or not defined if <tt>num-vals</tt> is defined)</font></td>
    /// <td valign="top" > The parameter type</td></tr>
    ///
    /// <tr><td valign="top" ><tt><i> base </i>.param.< i > number </i>.num - vals </tt><br/>
    ///    
    ///     <font size= "-1" > int >= 1 </font></td>
    ///
    ///     <td valign= "top" > (The number of values (Strings) a parameter may take on, if it is a multi-string type)</td></tr>
    ///
    /// <tr><td valign="top" ><tt><i> base </i>.param.< i > number </i>.val.< i > val - number </i></tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top" > (A possible value that a parameter may take on, if it is a multi-string type)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.eval.MetaProblem")]
    public class MetaProblem : Problem, ISimpleProblem
    {
        public const String P_FILE = "file";
        public const String P_RUNS = "runs";
        public const String P_REEVALUATE_INDIVIDUALS = "reevaluate";
        public const String P_NUM_PARAMS = "num-params";
        public const String P_PARAM = "param";
        public const String P_TYPE = "type";
        public const String V_INTEGER = "integer";
        public const String V_BOOLEAN = "boolean";
        public const String V_FLOAT = "float";
        public const String P_NUM_VALS = "num-vals";
        public const String P_VAL = "val";
        public const String P_MUZZLE = "muzzle";
        public const String P_SET_RANDOM = "set-random";


        /** The parameter base from which the MetaProblem was loaded. */
        public IParameter ParamBase { get; set; }

        /** A prototypical parameter database for the underlying (base-level) evolutionary computation system.  This is never directly used, just cloned. */
        public IParameterDatabase p_database { get; set; }

        /** This points to the database presently used by the underlying (base-level) evolutionary computation system.  It is a cloned and modified version
            of p_database. */
        public IParameterDatabase CurrentDatabase { get; set; }

        /** The number of base-level evolutionary runs to perform to evaluate an individual.  */
        public int Runs { get; set; }

        /** Whether to reevaluate individuals if and when they appear for evaluation in the future.  */
        public bool ReevaluateIndividuals { get; set; }


        /** The best underlying individual array, one per subpopulation.
            We retain the best underlying individual here rather than
            storing it in (say) the associated fitness because fitnesses
            are *averaged* over trials, so we wouldn't be able to keep track
            of the *max* fitness and associated individual that way.  So we
            do it here.  
        */

        // Note that this requires a lock to synchronize on
        // because multiple MetaProblems, perhaps in different threads, may
        // be accessing this array simultaneously trying to update statistics.

        public Individual[] BestUnderlyingIndividual { get; set; } // not deep cloned

        /** Acquire this lock before accessing bestUnderlyingIndividual */

        public Object Lock = new Object(); // not deep cloned



        /** A list of domain information, one per parameter in the genome. */

        // Domain information is represented by arrays, each at present one of:
        // 
        // 1. double[0].   Float values.  Note array is 0 length.
        // 2. int[0].         Integer values.  Note array is 0 length.
        // 3. String[n].  An integer-valued interval from 0 ... n-1 inclusive, 
        //                mapping to the parameter strings string[0] through string[n-1]
        // 4. bool[0]. bool values.  Note array is 0 length.
        //
        // We may need more than this but it'll suffice for now.

        public Object[] Domain { get; set; } // not deep cloned

        private bool SetRandom { get; set; }

        // default form does nothing
        public void Setup(EvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            this.ParamBase = paramBase;
            var file = state.Parameters.GetFile(paramBase.Push(P_FILE), null);
            try
            {
                p_database =
                    new ParameterDatabase(file,
                        new[] {"-file", file.FullName}); // command line has just the parameter database
            }
            catch (IOException e)
            {
                state.Output.Fatal("Exception loading meta-parameter-database:\n" + e,
                    paramBase.Push(P_FILE));
            }
            Runs = state.Parameters.GetInt(paramBase.Push(P_RUNS), null, 1);
            if (Runs < 1)
                state.Output.Fatal("Number of runs must be >= 1",
                    paramBase.Push(P_RUNS));

            ReevaluateIndividuals = state.Parameters.GetBoolean(paramBase.Push(P_REEVALUATE_INDIVIDUALS), null, true);
            if (state.Parameters.ParameterExists(paramBase.Push(P_MUZZLE), null))
                state.Output.Warning("" + paramBase.Push(P_MUZZLE) + " no longer exists.  Use 'silent' in the lower-level EA parameters instead.");

            IParameter pop = new Parameter(Initializer.P_POP);
            int subpopsLength = state.Parameters.GetInt(pop.Push(Population.P_SIZE), null, 1);
            BestUnderlyingIndividual = new Individual[subpopsLength];

            SetRandom = state.Parameters.GetBoolean(paramBase.Push(P_SET_RANDOM), null, false);

            LoadDomain(state, paramBase);
        }


        protected void LoadDomain(EvolutionState state, IParameter paraBase)
        {
            // Load domain and check for parameters

            int numParams = state.Parameters.GetInt(ParamBase.Push(P_NUM_PARAMS), null, 1);
            if (numParams < 1)
                state.Output.Fatal("Number of parameters must be >= 1",
                    ParamBase.Push(P_NUM_PARAMS));

            Domain = new Object[numParams];

            IParameter pb = ParamBase.Push(P_PARAM);
            for (int i = 0; i < numParams; i++) // just keep rising
            {
                // check parameter
                IParameter p = pb.Push("" + i);
                if (!state.Parameters.ParameterExists(p, null)) // guess that's it
                    break;

                // load parameter domain
                else if (state.Parameters.ParameterExists(p.Push(P_TYPE), null))
                {
                    String type = state.Parameters.GetString(p.Push(P_TYPE), null);
                    if (type.Equals(V_INTEGER, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Domain[i] = new int[0];
                    }
                    else if (type.Equals(V_FLOAT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Domain[i] = new double[0];
                    }
                    else if (type.Equals(V_BOOLEAN, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Domain[i] = new bool[0];
                    }
                    else
                        state.Output.Fatal("Meta parameter number " + i + " has a malformed type declaration.",
                            p.Push(P_TYPE), null);

                    // double-check
                    if (state.Parameters.ParameterExists(p.Push(P_NUM_VALS), null))
                        state.Output.Fatal(
                            "Meta parameter number " + i + " has both a type declaration and a num-vals declaration.",
                            p.Push(P_TYPE), p.Push(P_NUM_VALS));
                }
                else if (state.Parameters.ParameterExists(p.Push(P_NUM_VALS), null))
                {
                    int len = state.Parameters.GetInt(p.Push(P_NUM_VALS), null, 1);
                    if (len > 0)
                    {
                        var tags = new String[len];
                        for (int j = 0; j < len; j++)
                        {
                            tags[j] = state.Parameters.GetString(p.Push(P_VAL).Push("" + j), null);
                            if (tags[j] == null)
                                state.Output.Fatal("Meta parameter number " + i + " is missing value number " + j + ".",
                                    p.Push(P_VAL).Push("" + j));
                        }
                        Domain[i] = tags;
                    }
                    else
                        state.Output.Fatal("Meta parameter number " + i + " has a malformed domain.",
                            p.Push(P_NUM_VALS));
                }
                else
                    state.Output.Fatal(
                        "Meta parameter number " + i + " has no type declaration or num-vals declaration.",
                        p.Push(P_TYPE), p.Push(P_NUM_VALS));
            }
        }


        protected String Map(IEvolutionState state, double[] genome, FloatVectorSpecies species, int index)
        {
            if (index < 0 || index >= Domain.Length)
                state.Output.Fatal("No domain provided for meta parameter number " + index + ".");

            Object d = Domain[index];
            double min = species.GetMinGene(index);
            double max = species.GetMaxGene(index);
            double gene = genome[index];

            if (d is bool[])
            {
                if (gene < min || gene > max)
                    state.Output.Fatal("Gene index " + index + " has a value (" + gene +
                                       ") outside the min-max range (from " + min + " to " + max +
                                       " inclusive).  Did you forget to bound the mutation?");
                else if (gene < (min + max) / 2.0) return "false";
                else return "true";
            }
            else if (d is int[])
            {
                return "" + (int) (Math.Floor(gene));
            }
            else if (d is double[])
            {
                return "" + gene;
            }
            else if (d is String[])
            {
                var dom = (String[]) d;
                if (min != 0)
                    state.Output.Fatal("Invalid min-gene value (" + min +
                                       ") for a string type in MetaProblem.  Gene index was " + index +
                                       ".  Should have been 0.");
                else if (max != dom.Length - 1)
                    state.Output.Fatal("Invalid max-gene value (" + max +
                                       ") for a string type in MetaProblem.  Gene index was " + index +
                                       ".  Should have been " + (dom.Length - 1) +
                                       ", that is, the number of vals - 1.");
                else if (gene < min || gene > max)
                    state.Output.Fatal("Gene index " + index + " has a value (" + gene +
                                       ") outside the min-max range (from " + min + " to " + max +
                                       " inclusive).  Did you forget to bound the mutation?");
                else return dom[(int) (Math.Floor(gene))];
            }
            else
                state.Output.Fatal("INTERNAL ERROR.  Invalid mapping for domain of meta parameter number " + index +
                                   " in MetaProblem.");
            return null; // never happens
        }


        /** Override this method to revise the provided parameter database to reflect the "parameters" specified in the 
            given meta-individual.  'Run' is the current run number for this individual's evaluation.  */
        public void ModifyParameters(IEvolutionState state, IParameterDatabase database, int run,
            Individual metaIndividual)
        {
            if (!(metaIndividual is DoubleVectorIndividual))
                state.Output.Fatal("Meta-individual is not a DoubleVectorIndividual.");
            var individual = (DoubleVectorIndividual) metaIndividual;
            var species = (FloatVectorSpecies) individual.Species;
            double[] genome = individual.genome;

            IParameter pb = ParamBase.Push(P_PARAM);
            for (int i = 0; i < genome.Length; i++)
            {
                IParameter p = pb.Push("" + i);
                String param = state.Parameters.GetString(p, null);
                if (param == null)
                    state.Output.Fatal("Meta parameter number " + i + " missing.", p);
                // load it
                database.SetParameter(new Parameter(param), "" + Map(state, genome, species, i));
            }
        }


        public void Evaluate(IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum)
        {
            if (ind.Evaluated && !ReevaluateIndividuals) return;

            var fits = new ArrayList();

            Individual bestOfRuns = null;
            for (int run = 0; run < Runs; run++)
            {
                // too annoying
                //state.Output.message("Thread " + threadnum + " Run " + run);
                try
                {
                    // The following uses BinaryFormatter to create a separate copy of the ParameterDatabase.
                    CurrentDatabase = (ParameterDatabase) p_database.DeepClone();
                }
                catch (Exception e)
                {
                    state.Output.Fatal("Exception copying database.\n" + e);
                }
                ModifyParameters(state, CurrentDatabase, run, ind);

                var output = new Output(false); // do not store messages, just print them
                output.AddLog(Log.D_STDOUT, false);
                output.AddLog(Log.D_STDERR, true);
                output.ThrowsErrors = true; // don't do System.exit(1);

                EvolutionState evaluatedState = null;
                try
                {
                    evaluatedState = Evolve.Initialize(CurrentDatabase, 0, output);

                    // should we override the seeds?
                    if (SetRandom)
                    {
                        // we use the random number generator to seed the generators
                        // of the underlying process.  This isn't optimal but it should
                        // probably do okay.  To be extra careful we prime the generators.

                        for (int i = 0; i < evaluatedState.Random.Length; i++)
                        {
                            int seed = state.Random[threadnum].NextInt();
                            evaluatedState.Random[i] = Evolve.PrimeGenerator(new MersenneTwisterFast(seed));
                        }
                    }

                    evaluatedState.Run(EvolutionState.C_STARTED_FRESH);

                    // Issue a warning if there's more than one subpopulation
                    if (evaluatedState.Population.Subpops.Length > 1)
                        state.Output.WarnOnce(
                            "MetaProblem used, but underlying evolution state has more than one subpopulation: only the results from subpopulation 0 will be considered.");


                    // Identify the best fitness of the underlying EvolutionState run, 

                    // we can only easily detect if the underlying EvolutionState has a proper Statistics
                    // object we can use AFTER we've run it because the Statistics object is set up during
                    // run().  We could modify this but I'm too lazy to do so, so...

                    Individual[] inds = null; // will get set, don't worry
                    if (evaluatedState.Statistics != null &&
                        (evaluatedState.Statistics is SimpleStatistics ||
                         evaluatedState.Statistics is SimpleShortStatistics))
                    {
                        inds = null;

                        // obviously we need an interface here rather than this nonsense
                        if (evaluatedState.Statistics is SimpleStatistics)
                            inds = ((SimpleStatistics) evaluatedState.Statistics).GetBestSoFar();
                        else inds = ((SimpleShortStatistics) evaluatedState.Statistics).GetBestSoFar();
                        if (inds == null)
                            state.Output.Fatal(
                                "Underlying evolution state has a Statistics object which provides a null best-so-far array.  Can't extract fitness.");
                        fits.Add((Fitness) (inds[0].Fitness));
                        //System.err.println("" + inds[0] + " " + inds[0].fitness);
                    }
                    else if (evaluatedState.Statistics == null)
                        state.Output.Fatal(
                            "Underlying evolution state has a null Statistics object.  Can't extract fitness.");
                    else
                        state.Output.Fatal(
                            "Underlying evolution state has a Statistics object which doesn't implement ProvidesBestSoFar.  Can't extract fitness.");


                    // Now we need to suck out the best individual discovered so far.  If the underlying
                    // evoluationary system itself has a MetaProblem, we need to do this recursively.
                    // We presume that the MetaProblem exists in subpopulation 0.

                    if (evaluatedState.Evaluator.p_problem is MetaProblem)
                    {
                        var mp = (MetaProblem) evaluatedState.Evaluator.p_problem;
                        lock (mp.Lock)
                        {
                            Individual bestind = mp.BestUnderlyingIndividual[0];

                            if (bestOfRuns == null || bestind.Fitness.BetterThan(bestOfRuns.Fitness))
                                bestOfRuns = (Individual) bestind.Clone();
                        }
                    }
                    // otherwise we grab the best individual found in the underlying evolutionary run,
                    // gathered from the inds array we used earlier.
                    else
                    {
                        // gather the best individual found during the runs
                        if (bestOfRuns == null || inds[0].Fitness.BetterThan(bestOfRuns.Fitness))
                            bestOfRuns = (Individual) (inds[0].Clone());
                    }


                    // now clean up
                    Evolve.Cleanup(evaluatedState);
                }
                catch (OutputExitException e)
                {
                    // looks like an error occurred.
                    state.Output.Warning(
                        "Error occurred in underlying evolutionary run.  NOTE: multiple threads may still be running:\n" +
                        e.Message);
                }
                catch (OutOfMemoryException e)
                {
                    // Let's try fixing things
                    evaluatedState = null;
                    //System.gc();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    state.Output.Warning(
                        "An Out of Memory error occurred in underlying evolutionary run.  Attempting to recover and reset.  NOTE: multiple threads may still be running:\n" +
                        e.Message);
                }
            }


            // Load the fitness into our individual 
            var fits2 = new IFitness[fits.Count];
            for (var i = 0; i < fits2.Length; i++)
                fits2[i] = (IFitness) fits[i];
            Combine(state, fits2, ind.Fitness);
            ind.Evaluated = true;

            // store the best individual found during the runs if it's superior.  
            // We need to do a lock here, which is rare in ECJ.  This is because the
            // bestUnderlyingIndividual array is shared among MetaProblem instances
            lock (Lock)
            {
                if (bestOfRuns != null &&
                    (BestUnderlyingIndividual[subpopulation] == null ||
                     bestOfRuns.Fitness.BetterThan(BestUnderlyingIndividual[subpopulation].Fitness)))
                {
                    BestUnderlyingIndividual[subpopulation] = bestOfRuns; // no clone necessary
                }
            }

        }

        /// <summary>
        /// Combines fitness results from multiple runs into a final Fitness.  By default this
        /// is done by using setToMeanOf.
        /// </summary>
        public void Combine(IEvolutionState state, IFitness[] runs, IFitness finalFitness)
        {
            finalFitness.SetToMeanOf(state, runs);
        }

        public void Describe(EvolutionState state, Individual ind, int subpopulation, int threadnum, int log)
        {
            // the default implementation works just like the default implementation of modifyParameters(...)

            state.Output.PrintLn("\nParameters:", log);
            if (!(ind is DoubleVectorIndividual))
                state.Output.Fatal("Meta-individual is not a DoubleVectorIndividual.");
            var individual = (DoubleVectorIndividual) ind;
            var species = (FloatVectorSpecies) individual.Species;
            double[] genome = individual.genome;

            IParameter pb = ParamBase.Push(P_PARAM);
            for (int i = 0; i < genome.Length; i++)
            {
                IParameter p = pb.Push("" + i);
                String param = state.Parameters.GetString(p, null);
                if (param == null)
                    state.Output.Fatal("Meta parameter number " + i + " missing.", p);
                // print it
                state.Output.PrintLn("" + param + " = " + Map(state, genome, species, i), log);
            }


            // We need to do a lock here, which is rare in ECJ.  This is because the
            // bestUnderlyingIndividual array is shared among MetaProblem instances
            lock (Lock)
            {
                if (BestUnderlyingIndividual[subpopulation] != null)
                {
                    state.Output.PrintLn("\nUnderlying Individual:", log);
                    BestUnderlyingIndividual[subpopulation].PrintIndividualForHumans(state, log);
                }
            }
        }
    }


}