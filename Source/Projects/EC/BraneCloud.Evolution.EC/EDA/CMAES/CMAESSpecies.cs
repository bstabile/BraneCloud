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
using BraneCloud.Evolution.EC.Vector;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.EDA.CMAES
{
    /**
     * CMAESSpecies is a FloatVectorSpecies which implements a faithful version of the
     * CMA-ES algorithm.  The class has two basic methods.  The newIndividual(...)
     * method generates a new random individual underneath the current CMA-ES
     * gaussian distribution.  The updateDistribution(...) method revises the
     * gaussian distribution to reflect the fitness results of the population.
     * 
     * <p>CMAESSpecies must be used in combination with CMAESBreeder, which will
     * call it at appropriate times to revise the distribution and to generate a new
     * subpopulation of individuals.  It must also be used in combination with
     * CMAESInitializer, which will use it to generate the initial population.
     *
     * <p>Note importantly that CMAESSpecies <b>ignores the subpopulation size</b>.
     * Instead the first thing it will do is revise the subpopulation size to reflect
     * the "lambda" parameter.  This is a consequence of another feature of
     * CMAESSpecies: many of its parameters do not have fixed default values, but
     * rather values which are computed on the fly if the user does not provide them.
     * For this reason, it also prints out these values when running so the user may
     * see what values it used for that given run.  The computed default values 
     * use equations which are common in the CMA-ES literature and are described
     * below.
     *
     * <p>CMAESSpecies also has an "alternative termination" option.  Normally ECJ
     * terminates when the optimal individual is discovered or when the generations
     * or maximum number of evaluations has been exceeded.  CMA-ES can optionally
     * terminate when the eigenvalues of the covariance matrix of its gaussian
     * distribution are too small.  Among other things, this will generally prevent
     * CMA-ES from terminating abnormally because an eigenvalue has gone negative
     * (due to floating point underflows).  But by default this alternative termination
     * is turned off, and CMA-ES will simply terminate in that case.
     *
     * <p>CMAESSpecies needs initial values of sigma (the scaling parameter for its
     * covariance matrix) and the mean of the gaussian distribution.  By default
     * sigma's initial value is 1.0.  The mean must be set to one of "zero"
     * (meaning the origin), "center" (meaning the center of the genome space defined
     * by the min and max gene values for each gene), or "random" (meaning a randomly-
     * chosen point in the space".  If it is not set to any of these, you may
     * alternatively set the initial mean values by hand.  But you must do one of
     * the two.
     *
     * <p>Initializing the covariance matrix can be a problem in in CMA-ES, particularly
     * if it is large relative to the gene bounds.  If CMA-ES generates a random individual
     * under its current distribution and that individual violates the bounds of just a 
     * single gene, it is invalid and must be regenerated.  If you have a lot of genes,
     * and the covariance matrix is large relative to their bounds, then the probability
     * that this will occur rapidly approaches 1.0, so CMA-ES will be trapped in an effectively
     * infinite loop endlessly producing invalid individuals.
     *
     * <p>This can be remedied in a few ways.  First there is an option available to force
     * the initial covariance matrix to NOT be the identity matrix (the default) but instead
     * be scaled according to the gene bounds.  That may help.  You can also of course reduce
     * sigma.  Last, you can turn on an alternative individual generation mechanism; here,
     * if a specific gene bound is violated, then *for that gene only* the value is chosen at
     * random uniformly from within the gene bounds.
     *
     * <p>CMAESSpecies relies on the EJML matrix library, available at 
     * <a href="http://ejml.org/">http://ejml.org/</a>

     <p><b>Parameters</b><br>
     <table>
     <tr><td valign=top><i>base</i>.<tt>mean</tt><br>
     <font size=-1>String, one of center, zero, or random</font></td>
     <td valign=top>(the initial mean for the distribution)<br>
     If not provided, <i>base</i>.<tt>mean</tt>.0 and so on must be given    
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>mean</tt>.<i>i</i><br>
     <font size=-1>Floating-point value</font></td>
     <td valign=top>(the value of dimension i of the initial mean vector)<br>
     These values will override those set as a result of <i>base</i>.<tt>mean</tt> 
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>sigma</tt><br>
     <font size=-1>Floating-point value > 0.0</font></td>
     <td valign=top>(the sigma scaling parameter)<br>
     If not provided, this defaults to 1.0.
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>lambda</tt><br>
     <font size=-1>Integer > <i>base</i>.<tt>mu</tt></font></td>
     <td valign=top>(lambda population size)<br>
     If not provided, this defaults to 4 + Math.Floor(3 * Math.Log(n)).
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>lambda</tt><br>
     <font size=-1>Integer > 1</td>
     <td valign=top>(mu truncated population size)<br>
     If not provided, this defaults to Math.Floor(lambda / 2.0).
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>weight</tt>.<i>i</i><br>
     <font size=-1>Float >= 0</td>
     <td valign=top>(weight for individual i (i from 0 to mu-1))<br>
     If not provided, this defaults to Math.Log((lambda + 1.0) / (2.0 * (i + 1))).
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>cc</tt><br>
     <font size=-1>0 &lt;= Float &lt; 1</td>
     <td valign=top>(c_c parameter)<br>
     If not provided, this defaults to (4.0+mueff/n) / (n+4.0 + 2.0*mueff/n)<br>
     Where mueff is defined in the variables below, and n is the genome size
     </td></tr>
     
     <tr><td valign=top><i>base</i>.<tt>cs</tt><br>
     <font size=-1>0 &lt;= Float &lt; 1</td>
     <td valign=top>(c_sigma parameter)<br>
     If not provided, this defaults to (mueff+2.0)/(n+mueff+5.0)<br>
     Where mueff is defined in the variables below, and n is the genome size
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>c1</tt><br>
     <font size=-1>0 &lt;= Float &lt; 1 (and c1 > (1-cmu))</td>
     <td valign=top>(c_1 parameter)<br>
     If not provided, this defaults to 2.0 / ((n+1.3)*(n+1.3)+mueff)<br>
     Where mueff is defined in the variables below, and n is the genome size
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>cmu</tt><br>
     <font size=-1>0 &lt;= Float &lt; 1 (and cmu > (1-c1))</td>
     <td valign=top>(c_mu parameter)<br>
     If not provided, this defaults to Math.Min(1.0-c1, 2.0*(mueff-2.0+1.0/mueff) / ((n+2.0)*(n+2.0)+mueff))<br>
     Where mueff is defined in the variables below, and n is the genome size
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>damps</tt><br>
     <font size=-1>0 &lt;= Float &lt; 1</td>
     <td valign=top>(d_sigma dampening parameter)<br>
     If not provided, this defaults to 1.0 + 2.0*Math.Max(0.0, Math.Sqrt((mueff-1.0)/(n+1.0))-1.0) + cs<br>
     Where mueff is defined in the variables below, and n is the genome size
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>covariance</tt><br>
     <font size=-1>String, either "identity" (default) or "scaled"</td>
     <td valign=top>Covariance matrix initialization procedure.<br>
     If "identity", then the covariance matrix is initialized to the
     identity matrix.  If "scaled", then the covariance matrix is
     initialized to a diagonal matrix whose values are squares of
     each gene range (max - min).
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>alternative-generator</tt><br>
     <font size=-1>true or false (default)</td>
     <td valign=top>Whether or not to use the alternative indivdiual-generation procedure.<br>
     If "true", then if, in the process of generating an individual, we have failed 
     alternative-generator-tries times to create an individual which falls within the min and
     max gene values for each gene, then whenever a gene value violates those constraints
     we will simply randomize it to something uniformly chosen between the min and max.
     If "false", then whenever an individual violates constraints, we will try again as
     necessary.
     </td></tr>

     <tr><td valign=top><i>base</i>.<tt>alternative-generator-tries</tt><br>
     <font size=-1>Integer > 1 (default is 100)</td>
     <td valign=top>How many times we try to generate a valid individual before
     possibly using the alternative-generator approach.
     </td></tr>

     </table>


     <p><b>Default Base</b><br>
     eda.cma-es.species


     * @author Sam McKay and Sean Luke
     * @version 1.0 
     */

    public class CMAESSpecies : FloatVectorSpecies
    {
        public const string P_CMAES_SPECIES = "cma-es.species";

        public const string P_LAMBDA = "lambda";
        public const string P_MU = "mu";
        public const string P_SIGMA = "sigma";
        public const string P_MEAN = "mean";
        public const string P_WEIGHTS = "weight";

        public const string P_CC = "cc";
        public const string P_CS = "cs";
        public const string P_C1 = "c1";
        public const string P_CMU = "cmu";
        public const string P_DAMPS = "damps";

        public const string V_CENTER = "center";
        public const string V_ZERO = "zero";
        public const string V_RANDOM = "random";

        public const string P_COVARIANCE = "covariance";
        public const string V_IDENTITY = "identity";
        public const string V_SCALED = "scaled";

        public const string P_ALTERNATIVE_TERMINATION = "alternative-termination";
        public const string P_ALTERNATIVE_GENERATOR = "alternative-generator";
        public const string P_ALTERNATIVE_GENERATOR_TRIES = "alternative-generator-tries";

        /** The individuals generated from the distribution. 
            If not specified in the parameters, by default 
            lambda = 4+(int)Math.Floor(3*Math.Log(n));
        */
        public int lambda;

        /** The truncated individuals used to update the distribution. 
            If not specified in the parameters, by default 
            mu = (int)Math.Floor(lambda/2.0);
        */
        public int mu;

        /** The ranked fitness weights for the mu individuals. 
            If not specified in the parameters, by default 
            weights[i] = ln((lambda + 1) / 2i).
            Then all the weights are normalized so that they sum to 1. */
        public double[] weights;

        /** The "mu_{eff}" constant in CMA-ES.   This is set to 
            1.0 / the sum of the squares of each of the weights[...] */
        public double mueff;

        /** The c_{\mu} rank-mu update learning rate.
            If not specified in the parameters, by default
            cmu = Math.Min(1-c1, 2 * (mueff - 2 + 1/mueff) / ((n+2)*(n+2) + mueff))
            where n is the genome size.
        */
        public double cmu;

        /** The c_1 rank-1 update learning rate.
            If not specified in the parameters, by default
            c1 = 2 / ((n + 1.3) * (n + 1.3) + mueff)
            where n is the genome size.
        */
        public double c1;

        /** The c_c rank-one evolution path cumulation parameter.
            If not specified in the parameters, by default
            cc = (4 + mueff / n) / (n + 4 + 2 * mueff/n)
            where n is the genome size.
        */
        public double cc;

        /** The c_{\sigma} mutation rate evolution path learning rate.
            If not specified in the parameters, by default
            cs = (mueff + 2) / (n + mueff + 5)
            where n is the genome size.
        */
        public double cs;

        /** The d_{\sigma} dampening for the mutation rate update.
            If not specified in the parameters, by default
            damps = cs + 2 * Math.Max(1, Math.Sqrt((mueff - 1) / (n + 1))) - 1
            otherwise known as
            damps = 1 + 2 * Math.Max(0, Math.Sqrt((mueff - 1) / (n + 1)) - 1) + cs
            where n is the genome size.
        */
        public double damps;

        /** The "sigma" scaling factor for the covariance matrix. */
        public double sigma;

        /** The mean of the distribution. */
        public SimpleMatrix xmean;

        /** The "C" covariance matrix of the distribution. */
        public SimpleMatrix c;

        /** The "B" matrix, eigendecomposed from the "C" covariance matrix of the distribution. */
        public SimpleMatrix b;

        /** The "C" matrix, eigendecomposed from the "C" covariance matrix of the distribution. */
        public SimpleMatrix d;

        /** b x d */
        public DenseMatrix64F bd;

        /** bd x sigma */
        public DenseMatrix64F sbd;

        /** C^{-1/2}.  This is equal to B x D^{-1} x B^T */
        public SimpleMatrix invsqrtC;

        /** The p_{\sigma} evolution path vector. */
        public SimpleMatrix ps;

        /** The p_c evolution path vector. */
        public SimpleMatrix pc;

        /** An estimate of the expected size of the standard multivariate gaussian N(0,I). 
            This is chiN = Math.Sqrt(n)*(1.0-1.0/(4.0*n)+1.0/(21.0*n*n))
        */
        public double chiN;

        /** The most recent generation where an eigendecomposition on C was performed into B and D */
        public int lastEigenDecompositionGeneration = -1;

        /** Should we terminate when the eigenvalues get too small?  If we don't, they might go negative and the eigendecomposition will fail. */
        public bool useAltTermination;

        /** If, after trying altGeneratorTries to build an indiviual, we are still building one which violates min/max gene constraints, should
            we instead fill those violated genes with uniformly-selected values between the min and max? */
        public bool useAltGenerator;

        /** How many times should we try to generate a valid individual before we give up and use the useAltGenerator approach? */
        public int altGeneratorTries = DEFAULT_ALT_GENERATOR_TRIES;

        /** Default value (100) for altGeneratorTries. */
        public static int DEFAULT_ALT_GENERATOR_TRIES = 100;



        public override IParameter DefaultBase => CMAESDefaults.ParamBase.Push(P_CMAES_SPECIES);

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            IMersenneTwister random = state.Random[0];

            IParameter def = DefaultBase;

            IParameter subpopBase = paramBase.Pop();
            IParameter subpopDefaultBase = ECDefaults.ParamBase.Push(Subpopulation.P_SUBPOPULATION);

            if (!state.Parameters.ParameterExists(paramBase.Push(P_SIGMA), def.Push(P_SIGMA)))
            {
                state.Output.Message("CMA-ES sigma was not provided, defaulting to 1.0");
                sigma = 1.0;
            }
            else
            {
                sigma = state.Parameters.GetDouble(paramBase.Push(P_SIGMA), def.Push(P_SIGMA), 0.0);
                if (sigma <= 0)
                    state.Output.Fatal("If CMA-ES sigma is provided, it must be > 0.0", paramBase.Push(P_SIGMA),
                        def.Push(P_SIGMA));
            }

            double[] cvals = new double[GenomeSize];
            String covarianceInitialization =
                state..Parameters.GetStringWithDefault(paramBase.Push(P_COVARIANCE), def.Push(P_COVARIANCE), V_IDENTITY);
            String covs = "Initial Covariance: <";
            for (int i = 0; i < GenomeSize; i++)
            {
                if (i > 0) covs += ", ";
                if (covarianceInitialization.Equals(V_SCALED))
                {
                    cvals[i] = (MaxGenes[i] - MinGenes[i]);
                }
                else if (covarianceInitialization.Equals(V_IDENTITY))
                {
                    cvals[i] = 1.0;
                }
                else
                {
                    state.Output.Fatal("Invalid covariance initialization type " + covarianceInitialization,
                        paramBase.Push(P_COVARIANCE), def.Push(P_COVARIANCE));
                }

                // cvals is standard deviations, so we change them to variances now
                cvals[i] *= cvals[i];
                covs += cvals[i];
            }
            state.Output.Message(covs + ">");

            // set myself up and define my initial distribution here
            int n = GenomeSize;
            b = SimpleMatrix.identity(n);
            c = new SimpleMatrix(CommonOps.diag(cvals));

            d = SimpleMatrix.identity(n);
            bd = CommonOps.identity(n, n);
            sbd = CommonOps.identity(n, n);
            invsqrtC = SimpleMatrix.identity(n);


            // Here we do one FIRST round of eigendecomposition, because newIndividual needs
            // a valid version of sbd.  If c is initially the identity matrix (and sigma = 1), 
            // then sbd is too, and we're done.  But if c is scaled in any way, we need to compute
            // the proper value of sbd.  Along the way we'll wind up computing b, d, bd, and invsqrtC

            EigenDecomposition<DenseMatrix64F> eig = DecompositionFactory.eig(GenomeSize, true, true);
            if (eig.decompose(c.copy().GetMatrix()))
            {
                SimpleMatrix dinv = new SimpleMatrix(GenomeSize, GenomeSize);
                for (int i = 0; i < GenomeSize; i++)
                {
                    double eigrt = Math.Sqrt(eig.GetEigenvalue(i).real);
                    d.set(i, i, eigrt);
                    dinv.set(i, i, 1 / eigrt);
                    CommonOps.insert(eig.GetEigenVector(i), b.GetMatrix(), 0, i);
                }

                invsqrtC = b.mult(dinv.mult(b.transpose()));
                CommonOps.mult(b.GetMatrix(), d.GetMatrix(), bd);
            }
            else
            {
                state.Output.Fatal("CMA-ES eigendecomposition failed. ");
            }
            CommonOps.scale(sigma, bd, sbd);

            // End FIRST round of eigendecomposition



            // Initialize dynamic (internal) strategy parameters and constants
            pc = new SimpleMatrix(n, 1);
            ps = new SimpleMatrix(n, 1); // evolution paths for C and sigma
            chiN = Math.Sqrt(n) *
                   (1.0 - 1.0 / (4.0 * n) + 1.0 / (21.0 * n * n)); // expectation of ||N(0,I)|| == norm(randn(N,1))

            xmean = new SimpleMatrix(GenomeSize, 1);

            bool meanSpecified = false;
            String val = state.Parameters.GetString(paramBase.Push(P_MEAN), def.Push(P_MEAN));
            if (val != null)
            {
                meanSpecified = true;
                if (val.Equals(V_CENTER))
                {
                    for (int i = 0; i < GenomeSize; i++)
                    {
                        xmean.set(i, 0, (MaxGenes[i] + MinGenes[i]) / 2.0);
                    }
                }
                else if (val.Equals(V_ZERO))
                {
                    for (int i = 0; i < GenomeSize; i++)
                    {
                        xmean.set(i, 0, 0); // it is this anyway
                    }
                }
                else if (val.Equals(V_RANDOM))
                {
                    for (int i = 0; i < GenomeSize; i++)
                    {
                        xmean.set(i, 0,
                            state.Random[0].NextDouble(true, true) * (MaxGenes[i] - MinGenes[i]) + MinGenes[i]);
                    }
                }
                else
                {
                    state.Output.Fatal("Unknown mean value specified: " + val, paramBase.Push(P_MEAN), def.Push(P_MEAN));
                }
            }
            else
            {
                state.Output.Fatal("No default mean value specified.  Loading full mean from parameters.",
                    paramBase.Push(P_MEAN), def.Push(P_MEAN));
            }

            bool nonDefaultMeanSpecified = false;
            for (int i = 0; i < GenomeSize; i++)
            {
                double m_i = 0;
                try
                {
                    m_i = state.Parameters.GetDouble(paramBase.Push(P_MEAN).Push("" + i), def.Push(P_MEAN).Push("" + i));
                    xmean.set(i, 0, m_i);
                    nonDefaultMeanSpecified = true;
                }
                catch (FormatException e)
                {
                    if (!meanSpecified)
                        state.Output.Error(
                            "No default mean value was specified, but CMA-ES mean index " + i +
                            " is missing or not a number.", paramBase.Push(P_MEAN).Push("" + i),
                            def.Push(P_MEAN).Push("" + i));
                }
            }

            state.Output.ExitIfErrors();
            if (nonDefaultMeanSpecified && meanSpecified)
            {
                state.Output.Warning("A default mean value was specified, but certain mean values were overridden.");
            }

            String mes = "Initial Mean: <";
            for (int i = 0; i < GenomeSize - 1; i++)
                mes = mes + xmean.Get(i, 0) + ", ";
            mes = mes + xmean.Get(GenomeSize - 1, 0) + ">";
            state.Output.Message(mes);

            if (!state.Parameters.ParameterExists(paramBase.Push(P_LAMBDA), def.Push(P_LAMBDA)))
            {
                lambda = 4 + (int) Math.Floor(3 * Math.Log(n));
            }
            else
            {
                lambda = state.Parameters.GetInt(paramBase.Push(P_LAMBDA), def.Push(P_LAMBDA), 1);
                if (lambda <= 0)
                    state.Output.Fatal("If the CMA-ES lambda parameter is provided, it must be a valid integer > 0",
                        paramBase.Push(P_LAMBDA), def.Push(P_LAMBDA));
            }

            if (!state.Parameters.ParameterExists(paramBase.Push(P_MU), def.Push(P_MU)))
            {
                mu = (int) (Math.Floor(lambda / 2.0));
            }
            else
            {
                mu = state.Parameters.GetInt(paramBase.Push(P_MU), def.Push(P_MU), 1);
                if (mu <= 0)
                    state.Output.Fatal("If the CMA-ES mu parameter is provided, it must be a valid integer > 0",
                        paramBase.Push(P_MU), def.Push(P_MU));
            }

            if (mu > lambda) // uh oh
                state.Output.Fatal("CMA-ES mu must be <= lambda.  Presently mu=" + mu + " and lambda=" + lambda);

            weights = new double[mu];
            bool weightsSpecified = false;
            for (int i = 0; i < mu; i++)
                if (state.Parameters.ParameterExists(paramBase.Push(P_WEIGHTS).Push("" + i), def.Push(P_WEIGHTS).Push("" + i)))
                {
                    state.Output.Message("CMA-ES weight index " + i +
                                         " specified.  Loading all weights from parameters.");
                    weightsSpecified = true;
                    break;
                }

            if (weightsSpecified)
            {
                for (int i = 0; i < mu; i++)
                {
                    double m_i = 0;
                    try
                    {
                        weights[i] = state.Parameters.GetDouble(paramBase.Push(P_WEIGHTS).Push("" + i),
                            def.Push(P_WEIGHTS).Push("" + i));
                    }
                    catch (FormatException e)
                    {
                        state.Output.Error("CMA-ES weight index " + i + " missing or not a number.",
                            paramBase.Push(P_WEIGHTS).Push("" + i), def.Push(P_WEIGHTS).Push("" + i));
                    }
                }
                state.Output.ExitIfErrors();
            }
            else
            {
                for (int i = 0; i < mu; i++)
                    weights[i] = Math.Log((lambda + 1.0) / (2.0 * (i + 1)));
            }

            // normalize
            double sum = 0.0;
            for (int i = 0; i < mu; i++)
                sum += weights[i];
            for (int i = 0; i < mu; i++)
                weights[i] /= sum;

            // compute mueff
            double sumSqr = 0.0;
            for (int i = 0; i < mu; i++)
                sumSqr += weights[i] * weights[i];
            mueff = 1.0 / sumSqr;

            mes = "Weights: <";
            for (int i = 0; i < weights.Length - 1; i++)
                mes = mes + weights[i] + ", ";
            mes = mes + (weights.Length - 1) + ">";
            state.Output.Message(mes);

            useAltTermination = state.Parameters.GetBoolean(paramBase.Push(P_ALTERNATIVE_TERMINATION),
                def.Push(P_ALTERNATIVE_TERMINATION), false);
            useAltGenerator = state.Parameters.GetBoolean(paramBase.Push(P_ALTERNATIVE_GENERATOR),
                def.Push(P_ALTERNATIVE_GENERATOR), false);
            altGeneratorTries = state.Parameters.GetIntWithDefault(paramBase.Push(P_ALTERNATIVE_GENERATOR_TRIES),
                def.Push(P_ALTERNATIVE_GENERATOR_TRIES), DEFAULT_ALT_GENERATOR_TRIES);
            if (altGeneratorTries < 1)
                state.Output.Fatal(
                    "If specified (the default is " + DEFAULT_ALT_GENERATOR_TRIES +
                    "), alt-generation-tries must be >= 1",
                    paramBase.Push(P_ALTERNATIVE_GENERATOR_TRIES), def.Push(P_ALTERNATIVE_GENERATOR_TRIES));

            if (!state.Parameters.ParameterExists(paramBase.Push(P_CC), def.Push(P_CC)))
            {
                cc = (4.0 + mueff / n) / (n + 4.0 + 2.0 * mueff / n); // time constant for cumulation for C
            }
            else
            {
                cc = state.Parameters.GetDoubleWithMax(paramBase.Push(P_CC), def.Push(P_CC), 0.0, 1.0);
                if (cc < 0.0)
                    state.Output.Fatal(
                        "If the CMA-ES cc parameter is provided, it must be a valid number in the range [0,1]",
                        paramBase.Push(P_CC), def.Push(P_CC));
            }

            if (!state.Parameters.ParameterExists(paramBase.Push(P_CS), def.Push(P_CS)))
            {
                cs = (mueff + 2.0) / (n + mueff + 5.0); // t-const for cumulation for sigma control
            }
            else
            {
                cs = state.Parameters.GetDoubleWithMax(paramBase.Push(P_CS), def.Push(P_CS), 0.0, 1.0);
                if (cs < 0.0)
                    state.Output.Fatal(
                        "If the CMA-ES cs parameter is provided, it must be a valid number in the range [0,1]",
                        paramBase.Push(P_CS), def.Push(P_CS));
            }

            if (!state.Parameters.ParameterExists(paramBase.Push(P_C1), def.Push(P_C1)))
            {
                c1 = 2.0 / ((n + 1.3) * (n + 1.3) + mueff); // learning rate for rank-one update of C
            }
            else
            {
                c1 = state.Parameters.GetDouble(paramBase.Push(P_C1), def.Push(P_C1), 0.0);
                if (c1 < 0)
                    state.Output.Fatal("If the CMA-ES c1 parameter is provided, it must be a valid number >= 0.0",
                        paramBase.Push(P_C1), def.Push(P_C1));
            }

            if (!state.Parameters.ParameterExists(paramBase.Push(P_CMU), def.Push(P_CMU)))
            {
                cmu = Math.Min(1.0 - c1, 2.0 * (mueff - 2.0 + 1.0 / mueff) / ((n + 2.0) * (n + 2.0) + mueff));
            }
            else
            {
                cmu = state.Parameters.GetDouble(paramBase.Push(P_CMU), def.Push(P_CMU), 0.0);
                if (cmu < 0)
                    state.Output.Fatal("If the CMA-ES cmu parameter is provided, it must be a valid number >= 0.0",
                        paramBase.Push(P_CMU), def.Push(P_CMU));
            }

            if (c1 > (1 - cmu)) // uh oh
                state.Output.Fatal("CMA-ES c1 must be <= 1 - cmu.  You are using c1=" + c1 + " and cmu=" + cmu);
            if (cmu > (1 - c1)) // uh oh
                state.Output.Fatal("CMA-ES cmu must be <= 1 - c1.  You are using cmu=" + cmu + " and c1=" + c1);

            if (!state.Parameters.ParameterExists(paramBase.Push(P_DAMPS), def.Push(P_DAMPS)))
            {
                damps = 1.0 + 2.0 * Math.Max(0.0, Math.Sqrt((mueff - 1.0) / (n + 1.0)) - 1.0) + cs; // damping for sigma
            }
            else
            {
                damps = state.Parameters.GetDouble(paramBase.Push(P_DAMPS), def.Push(P_DAMPS), 0.0);
                if (damps <= 0)
                    state.Output.Fatal("If the CMA-ES damps parameter is provided, it must be a valid number > 0.0",
                        paramBase.Push(P_DAMPS), def.Push(P_DAMPS));
            }

            double damps_min = 0.5;
            double damps_max = 2.0;
            if (damps > damps_max || damps < damps_min)
                state.Output.Warning("CMA-ES damps ought to be close to 1.  You are using damps = " + damps);

            state.Output.Message("lambda: " + lambda);
            state.Output.Message("mu:     " + mu);
            state.Output.Message("mueff:  " + mueff);
            state.Output.Message("cmu:    " + cmu);
            state.Output.Message("c1:     " + c1);
            state.Output.Message("cc:     " + cc);
            state.Output.Message("cs:     " + cs);
            state.Output.Message("damps:  " + damps);
        }



        public override Object Clone()
        {
            CMAESSpecies myobj = (CMAESSpecies) (base.Clone());

            // clone the distribution and other variables here
            myobj.c = c.copy();
            myobj.b = b.copy();
            myobj.d = d.copy();
            myobj.bd = bd.copy();
            myobj.sbd = sbd.copy();
            myobj.invsqrtC = invsqrtC.copy();

            myobj.xmean = xmean.copy();
            myobj.ps = ps.copy();
            myobj.pc = pc.copy();

            return myobj;
        }


        public static int MAX_TRIES_BEFORE_WARNING = 100000;

        public override Individual NewIndividual(IEvolutionState state, int thread)
        {
            Individual newind = base.NewIndividual(state, thread);
            IMersenneTwister random = state.Random[thread];

            if (!(newind is DoubleVectorIndividual)) // uh oh
            state.Output.Fatal(
                "To use CMAESSpecies, the species must be initialized with a DoubleVectorIndividual.  But it contains a " +
                newind);

            DoubleVectorIndividual dvind = (DoubleVectorIndividual) (newind);

            DenseMatrix64F genome = DenseMatrix64F.wrap(GenomeSize, 1, dvind.genome);
            DenseMatrix64F temp = new DenseMatrix64F(GenomeSize, 1);

            // arz(:,k) = randn(N,1); % standard normally distributed vector
            // arx(:,k) = xmean + sigma*(B*D*arz(:,k));
            int tries = 0;
            while (true)
            {
                for (int i = 0; i < GenomeSize; i++)
                    dvind.genome[i] = random.NextGaussian();

                CommonOps.mult(sbd, genome, temp); // temp = sigma*b*d*genome;
                CommonOps.add(temp, xmean.GetMatrix(), genome); // genome = temp + xmean;

                bool invalid_value = false;
                for (int i = 0; i < GenomeSize; i++)
                    if (dvind.genome[i] < MinGenes[i] || dvind.genome[i] > MaxGenes[i])
                    {
                        if (useAltGenerator && tries > altGeneratorTries)
                        {
                            // instead of just failing, we're going to select uniformly from
                            // possible values for this particular gene.
                            dvind.genome[i] = state.Random[thread].NextDouble() * (MaxGenes[i] - MinGenes[i]) +
                                              MinGenes[i];
                        }
                        else
                        {
                            invalid_value = true;
                            break;
                        }
                    }

                if (invalid_value)
                {
                    if (++tries > MAX_TRIES_BEFORE_WARNING)
                        state.Output.WarnOnce(
                            "CMA-ES may be slow because many individuals are being generated which\n" +
                            "are outside the min/max gene bounds.  If an individual violates a single\n" +
                            "gene bounds, it is rejected, so as the number of genes grows, the\n" +
                            "probability of this happens increases exponentially.  You can deal\n" +
                            "with this by decreasing sigma.  Alternatively you can use set\n" +
                            "pop.subpop.0.alternative-generation=true (see the manual).\n" +
                            "Finally, if this is happening during initialization, you might also\n" +
                            "change pop.subpop.0.species.covariance=scaled.\n");
                    continue;
                }

                return newind;
            }
        }


        /** Revises the CMA-ES distribution to reflect the current fitness results in the provided subpopulation. */
        public void UpdateDistribution(IEvolutionState state, Subpopulation subpop)
        {
            // % Sort by fitness and compute weighted mean into xmean
            // [arfitness, arindex] = sort(arfitness); % minimization
            // xmean = arx(:,arindex(1:mu))*weights;   % recombination            % Eq.39
            // counteval += lambda;

            // only need partial sort?
            ((List<Individual>) subpop.Individuals).Sort();

            SimpleMatrix artmp = new SimpleMatrix(GenomeSize, mu);
            SimpleMatrix xold = xmean;
            xmean = new SimpleMatrix(GenomeSize, 1);

            for (int i = 0; i < mu; i++)
            {
                DoubleVectorIndividual dvind = (DoubleVectorIndividual) subpop.Individuals[i];

                // won't modify the genome
                SimpleMatrix arz = new SimpleMatrix(GenomeSize, 1, true, dvind.genome);
                arz = (arz.minus(xold).divide(sigma));

                for (int j = 0; j < GenomeSize; j++)
                {
                    xmean.set(j, 0, xmean.Get(j, 0) + weights[i] * dvind.genome[j]);
                    artmp.set(j, i, arz.Get(j, 0));
                }
            }

            // % Cumulation: Update evolution paths

            SimpleMatrix y = xmean.minus(xold).divide(sigma);
            SimpleMatrix bz = invsqrtC.mult(y);
            SimpleMatrix bz_scaled = bz.scale(Math.Sqrt(cs * (2.0 - cs) * mueff));
            ps = ps.scale(1.0 - cs).plus(bz_scaled);

            double h_sigma_value =
                ((ps.dot(ps) / (1.0 - Math.Pow(1.0 - cs, 2.0 * (state.Generation + 1)))) / GenomeSize);
            int hsig = (h_sigma_value < (2.0 + (4.0 / (GenomeSize + 1)))) ? 1 : 0;

            SimpleMatrix y_scaled = y.scale(hsig * Math.Sqrt(cc * (2.0 - cc) * mueff));
            pc = pc.scale(1.0 - cc).plus(y_scaled);

            // % Adapt covariance matrix C
            c = c.scale(1.0 - c1 - cmu);
            c = c.plus(pc.mult(pc.transpose()).plus(c.scale((1.0 - hsig) * cc * (2.0 - cc))).scale(c1));
            c = c.plus((artmp.mult(SimpleMatrix.diag(weights).mult(artmp.transpose()))).scale(cmu));

            // % Adapt step-size sigma
            sigma = sigma * Math.Exp((cs / damps) * (ps.normF() / chiN - 1.0));

            // % Update B and D from C
            if ((state.Generation - lastEigenDecompositionGeneration) > 1.0 / ((c1 + cmu) * GenomeSize * 10.0))
            {
                lastEigenDecompositionGeneration = state.Generation;

                // make sure the matrix is symmetric (it should be already)
                // not sure if this is necessary           
                for (int i = 0; i < GenomeSize; i++)
                for (int j = 0; j < i; j++)
                    c.set(j, i, c.Get(i, j));

                // this copy gets modified by the decomposition
                DenseMatrix64F copy = c.copy().GetMatrix();
                EigenDecomposition<DenseMatrix64F> eig = DecompositionFactory.eig(GenomeSize, true, true);
                if (eig.decompose(copy))
                {
                    SimpleMatrix dinv = new SimpleMatrix(GenomeSize, GenomeSize);
                    for (int i = 0; i < GenomeSize; i++)
                    {
                        double eigrt = Math.Sqrt(eig.GetEigenvalue(i).real);
                        d.set(i, i, eigrt);
                        dinv.set(i, i, 1 / eigrt);
                        CommonOps.insert(eig.GetEigenVector(i), b.GetMatrix(), 0, i);
                    }

                    invsqrtC = b.mult(dinv.mult(b.transpose()));
                    CommonOps.mult(b.GetMatrix(), d.GetMatrix(), bd);
                }
                else
                {
                    state.Output.Fatal("CMA-ES eigendecomposition failed. ");
                }
            }

            CommonOps.scale(sigma, bd, sbd);

            // % Break, if fitness is good enough or condition exceeds 1e14, better termination methods are advisable 
            // if arfitness(1) <= stopfitness || max(D) > 1e7 * min(D)
            //   break;
            // end
            if (useAltTermination && CommonOps.elementMax(d.extractDiag().GetMatrix()) >
                1e7 * CommonOps.elementMin(d.extractDiag().GetMatrix()))
            {
                state.Evaluator.SetRunCompleted("CMAESSpecies: Stopped because matrix condition exceeded limit.");
            }
        }


    }
}
