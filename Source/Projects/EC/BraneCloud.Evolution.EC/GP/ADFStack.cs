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

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// ADFStack is a special data object used to hold ADF data.
    /// This object is a weird beast and takes some explaining.
    /// It consists of a main stack, a secondary "substack", and a "reserve" area 
    /// (also implemented as a stack,
    /// but it doesn't have to be).  The reserve is used to "recycle" objects
    /// rather than having to create then new every time.
    /// 
    /// <p/>When an ADF is evaluated, it first
    /// evaluates its children, then it calls Push() on the ADFstack.
    /// Push() either creates a new ADFContext, or it fetches one from the
    /// reserve if possible.  It then pushes the context on the main stack,
    /// and also returns the context.  The ADF fills the context's arguments
    /// with the results of its childrens' evaluation, and sets numargs to 
    /// the number of
    /// arguments, then evaluates the
    /// ADF's associated function tree, 
    /// 
    /// <p/>When an ADM is evaluated, it calls Push() on the ADFstack.
    /// The ADM then fills the context's adm node with itself, and sets numargs
    /// to the number of children it has.  Then it calls the ADM's associated
    /// function tree.
    /// 
    /// <p/>In that tree, if an argument terminal of value <i>n</i> is evaluated,
    /// the argument terminal calls evaluate(...) on the top context 
    /// on the ADF stack and returns the result.
    /// This method does different things depending on whether the top context
    /// represented an ADF or an ADM.  If it was an ADF, the context simply sets
    /// input to the value of argument <i>n</i> in the context's argument list,
    /// and returns input.  If it was an ADM, the context pops itself off the
    /// stack and pushes itself on the substack (to set up the right context
    /// for evaluating an original child of the ADM), then evaluates child <i>n</i>
    /// of the ADM, then pops itself off the substack and pushes itself back
    /// on the stack to restore the context.  Input is set to the evaluated
    /// results, and input is returned.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>context</tt><br/>
    /// <font size="-1">classname, inherits and != ec.gp.GPContext</font></td>
    /// <td valign="top">(the stack's GPContext class)</td></tr> 
    /// </table>
    /// <p/><b>Parameters</b><br/>
    /// gp.adf-stack
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><i>base</i>.<tt>context</tt><br/>
    /// <td valign="top"/>(context_proto)</tr> 
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.ADFStack")]
    public class ADFStack : IPrototype
    {
        #region Constants

        public const string P_ADFSTACK = "adf-stack";
        public const string P_ADF = "adf";
        public const string P_CONTEXT = "context";
        public const int INITIAL_STACK_SIZE = 2; // seems like a good size

        #endregion // Constants
        #region Fields

        protected internal int OnStack;
        protected internal int OnSubstack;
        protected internal int InReserve;

        protected internal ADFContext[] Stack = new ADFContext[INITIAL_STACK_SIZE];
        protected internal ADFContext[] Substack = new ADFContext[INITIAL_STACK_SIZE];
        protected internal ADFContext[] Reserve = new ADFContext[INITIAL_STACK_SIZE];

        #endregion // Fields
        #region Properties

        public virtual IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_ADFSTACK); }
        }

        public ADFContext ContextProto { get; set; }

        #endregion // Properties
        #region Setup

        public virtual void  Setup(IEvolutionState state, IParameter paramBase)
        {
            // load our prototype
            
            var p = paramBase.Push(P_CONTEXT);
            var d = DefaultBase.Push(P_CONTEXT);
            
            ContextProto = (ADFContext) (state.Parameters.GetInstanceForParameterEq(p, d, typeof(ADFContext)));
            ContextProto.Setup(state, p);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns an ADFContext from the stack's reserve, or creates one
        /// fresh if there are none in reserve.  While you can throw this
        /// ADFContext away if you like, it'd be good if you actually didn't
        /// call this function unless you expected to push the 
        /// context onto the stack with Push(ADFContext obj) -- karma!
        /// </summary>
        public ADFContext Take()
        {
            // Remove one from reserve

            ADFContext obj;
            if (InReserve > 0)
                obj = Reserve[--InReserve];
            else
                obj = (ADFContext)ContextProto.Clone(); // hopefully that doesn't have to happen too many times
            return obj;
        }

        /// <summary>
        /// Pushes an ADFContext onto the main stack.  The best way to get an
        /// ADFContext to push onto the stack is with get(). Returns obj. 
        /// </summary>     
        public ADFContext Push(ADFContext obj)
        {
            // Double stack if necessary
            if (OnStack == Stack.Length)
            {
                var newstack = new ADFContext[Stack.Length * 2];
                Array.Copy(Stack, 0, newstack, 0, Stack.Length);
                Stack = newstack;
            }

            // Add to stack
            Stack[OnStack++] = obj;

            // return it
            return obj;
        }

        /// <summary>
        /// Pops off <i>n</i> items from the stack, if possible. Returns the number of items actually popped off. 
        /// </summary>
        public int Pop(int n)
        {
            int x;
            for (x = 0; x < n; x++)
            {
                // Anything left on the stack?
                if (OnStack == 0)
                    break;

                // Remove one from stack
                var obj = Stack[--OnStack];

                // Double reserve if necessary
                if (InReserve == Reserve.Length)
                {
                    var newreserve = new ADFContext[Reserve.Length * 2];
                    Array.Copy(Reserve, 0, newreserve, 0, Reserve.Length);
                    Reserve = newreserve;
                }

                // Add to reserve
                Reserve[InReserve++] = obj;
            }
            return x;
        }

        /// <summary>
        /// Returns the <i>n</i>th item in the stack (0-indexed), or null if this goes to the bottom of the stack. 
        /// </summary>
        public ADFContext Top(int n)
        {
            // is this beyond the stack?
            if (OnStack - n <= 0)
                return null;

            return Stack[OnStack - n - 1];
        }

        /// <summary>
        /// Moves <i>n</i> items onto the substack (pops them off the stack and pushes them onto the substack).  
        /// Returns the actual number of items for which this was done. 
        /// </summary>
        public int MoveOntoSubstack(int n)
        {
            int x;
            for (x = 0; x < n; x++)
            {
                // is the stack empty?
                if (OnStack == 0)
                    break; // uh oh

                // Remove one from stack
                var obj = Stack[--OnStack];

                // Double substack if necessary
                if (OnSubstack == Substack.Length)
                {
                    var newsubstack = new ADFContext[Substack.Length * 2];
                    Array.Copy(Substack, 0, newsubstack, 0, Substack.Length);
                    Substack = newsubstack;
                }

                // Add to substack
                Substack[OnSubstack++] = obj;
            }
            return x;
        }

        /// <summary>
        /// Moves <i>n</i> items onto the stack (popss them off the substack and pushes them onto the stack). 
        /// Returns the actual number of items moved from the Substack onto the main stack 
        /// </summary>
        public int MoveFromSubstack(int n)
        {
            int x;
            for (x = 0; x < n; x++)
            {
                // is the substack empty?
                if (OnSubstack == 0)
                    break; // uh oh

                // Remove one from stack
                var obj = Substack[--OnSubstack];

                // Double stack if necessary
                if (OnStack == Stack.Length)
                {
                    var newstack = new ADFContext[Stack.Length * 2];
                    Array.Copy(Stack, 0, newstack, 0, Stack.Length);
                    Stack = newstack;
                }

                // Add to stack
                Stack[OnStack++] = obj;
            }
            return x;
        }

        /// <summary>
        /// Pops off all items on the stack and the substack. 
        /// </summary>
        public void Reset()
        {
            if (OnSubstack > 0)
                MoveFromSubstack(OnSubstack);
            if (OnStack > 0)
                Pop(OnStack);
        }

        #endregion // Operations
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                var myobj = (ADFStack) MemberwiseClone();
                
                // deep-cloned stuff
                myobj.ContextProto = (ADFContext) ContextProto.Clone();
                
                // clone the stack arrays -- dunno if this is faster than new ADFContext[...]
                myobj.Stack = (ADFContext[]) Stack.Clone();
                myobj.Substack = (ADFContext[]) Substack.Clone();
                myobj.Reserve = (ADFContext[]) Reserve.Clone();
                
                // fill 'em up
                for (var x = 0; x < OnStack; x++)
                    myobj.Stack[x] = (ADFContext) Stack[x].Clone();
                for (var x = 0; x < OnSubstack; x++)
                    myobj.Substack[x] = (ADFContext) Substack[x].Clone();
                for (var x = 0; x < InReserve; x++)
                    myobj.Reserve[x] = (ADFContext) Reserve[x].Clone();
                return myobj;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }

        #endregion // Cloning
    }
}