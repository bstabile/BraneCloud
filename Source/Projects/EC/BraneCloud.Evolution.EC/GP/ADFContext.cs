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
using BraneCloud.Evolution.EC.GP.GE;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{    
    /// <summary> 
    /// ADFContext is the object pushed onto an ADF stack which represents
    /// the current context of an ADM or ADF function call, that is, how to
    /// get the argument values that argument_terminals need to return.
    /// 
    /// <p/><i>adf</i> contains the relevant ADF/ADM node. 
    /// If it's an ADF
    /// function call, then <i>Arguments[]</i> contains the evaluated arguments
    /// to the ADF.  If it's an ADM function call,
    /// then <i>Arguments[]</i> is set to false.
    /// 
    /// <p/>You set up the ADFContext object for a given ADF or ADM node with
    /// the PrepareADF(...) and prepareADM(...) functions.
    /// 
    /// <p/>To evaluate an argument number from an ADFContext, call evaluate(...),
    /// and the results are evaluated and copied into input.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i><tt>.Data</tt><br/>
    /// <font size="-1">classname, inherits and != ec.GPData</font></td>
    /// <td valign="top">(the class for the ADFContext's basic GPData type -- typically this is the same as GPProblem's GPData type)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.adf-context
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i><tt>.Data</tt><br/>
    /// <td valign="top"/>(the ADFContext's basic GPData type)</tr> 
    /// </table>
    /// </summary>    
    [Serializable]
    [ECConfiguration("ec.gp.ADFContext")]
    public class ADFContext : IPrototype
    {
        #region Constants

        public const string P_DATA = "data";
        public const string P_ADFCONTEXT = "adf-context";        
        public const int INITIAL_ARGUMENT_SIZE = 2; // seems reasonable

        #endregion // Constants
        #region Properties

        public virtual IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_ADFCONTEXT); }
        }

        /// <summary>
        /// The ADF/ADM node proper 
        /// </summary>
        public ADF Adf { get; set; }

        /// <summary>
        /// A prototypical GPData node. 
        /// </summary>
        public GPData ArgProto { get; set; }

        /// <summary>
        /// An array of GPData nodes (none of the null, when it's used) 
        /// holding an ADF's Arguments' return results 
        /// </summary>
        public GPData[] Arguments { get; set; }

        #endregion // Properties
        #region Setup

        public ADFContext()
        {
            Arguments = new GPData[INITIAL_ARGUMENT_SIZE];
        }

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // load the prototype
            var p = paramBase.Push(P_DATA);
            var def = DefaultBase;
            var d = def.Push(P_DATA);

            if (state.Parameters.ParameterExists(p, d))
            {
                //Arg_Proto = (GPData) (state.Parameters.GetInstanceForParameter(p, d, typeof(GPData)));
                //Arg_Proto.Setup(state, p);
                state.Output.Warning("ADF Data is deprecated -- this parameter is no longer used.  Instead, we directly use the GPData.", p, d);
            }
            //else
            //{
            // snarf it from eval.problem.Data, hacked up from the Problem's data type,
            // 'cause I'm not sure if Problem's been set up yet

            // BRS : This has been circumvented by setting up the GPData before the ADFStack in GPProblem's setup()
            //var pp = new Parameter(EvolutionState.P_EVALUATOR).Push(Evaluator.P_PROBLEM).Push(GPProblem.P_DATA);
            //var dd = GPDefaults.ParamBase.Push(GPProblem.P_GPPROBLEM).Push(GPProblem.P_DATA);

            ////state.Output.Warning("No ADF GPData specified; using (and setting up) from\n " + pp + "\nor default base " + dd, p, d);
            //ArgProto = (GPData)(state.Parameters.GetInstanceForParameter(pp, dd, typeof(GPData)));
            //ArgProto.Setup(state, pp); // note setting up from Problem's base!
            // END BRS comment

            //}

            // BRS : TODO : Determine if this is a reasonable way to do this!
            // Currently, only GEProblem implements IGPProblemParent. 
            // But for all I know, there could be other uses for this kind of "aggregate problem".
            // Could the parentage ever likely be more than one level deep? If so this would have to be recursive.
            if (state.Evaluator.p_problem is IGPProblemParent)
                ArgProto = (GPData)((IGPProblemParent) state.Evaluator.p_problem).Problem.Data.Clone();
            else
                ArgProto = (GPData)((IGPProblem)state.Evaluator.p_problem).Data.Clone();

            // clone off the prototype
            for (var x = 0; x < INITIAL_ARGUMENT_SIZE; x++)
                Arguments[x] = (GPData)(ArgProto.Clone());
        }

        /// <summary>
        /// Increases Arguments to accommodate space if necessary. Sets adf to a.
        /// You need to then fill out the Arguments yourself. 
        /// </summary>
        public void PrepareADF(ADF a)
        {
            // set to the length requested or longer
            if (a.Children.Length > Arguments.Length)
            {
                var newArguments = new GPData[a.Children.Length];
                Array.Copy(Arguments, 0, newArguments, 0, Arguments.Length);
                // fill gap -- ugh, luckily this doesn't happen but a few times
                for (var x = Arguments.Length; x < newArguments.Length; x++)
                    newArguments[x] = (GPData)(ArgProto.Clone());
                Arguments = newArguments;
            }
            Adf = a;
        }

        /// <summary>
        /// Sets adf to the specified ADM 
        /// </summary>
        public void PrepareADM(ADM a)
        {
            Adf = a;
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Evaluates the argument number in the current context 
        /// </summary>
        public virtual void Evaluate(IEvolutionState state, int thread, GPData input, ADFStack stack,
            GPIndividual individual, IProblem problem, int argument)
        {
            // do I have that many Arguments?
            if (argument >= Adf.Children.Length || argument < 0)
            // uh oh 
            {
                individual.PrintIndividual(state, 0);
                state.Output.Fatal("Invalid argument number for " + Adf.ErrorInfo());
            }

            // Am I an ADM or an ADF?
            if (Adf == null)
                state.Output.Fatal("ADF is null for " + Adf.ErrorInfo());
            else if (!(Adf is ADM))
                // it's an ADF
                Arguments[argument].CopyTo(input);
            // it's an ADM
            else
            {
                // get rid of my context temporarily
                if (stack.MoveOntoSubstack(1) != 1)
                    state.Output.Fatal("Substack prematurely empty for " + Adf.ErrorInfo());

                // Call the GPNode
                Adf.Children[argument].Eval(state, thread, input, stack, individual, problem);

                // restore my context
                if (stack.MoveFromSubstack(1) != 1)
                    state.Output.Fatal("Stack prematurely empty for " + Adf.ErrorInfo());
            }
        }

        #endregion // Operations
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                var myobj = (ADFContext) MemberwiseClone();
                
                // deep-clone the context proto
                myobj.ArgProto = (GPData) (ArgProto.Clone());
                
                // deep-clone the contexts
                myobj.Arguments = new GPData[Arguments.Length];
                for (var x = 0; x < myobj.Arguments.Length; x++)
                    myobj.Arguments[x] = (GPData) (Arguments[x].Clone());
                
                return myobj;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            }
        }

        #endregion // Cloning
    }
}