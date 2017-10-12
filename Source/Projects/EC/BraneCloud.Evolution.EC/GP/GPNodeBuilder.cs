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
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// GPNodeBuilder is a IPrototype which defines the superclass for objects
    /// which create ("grow") GP trees, whether for population initialization,
    /// subtree mutation, or whatnot.  It defines a single abstract method, 
    /// NewRootedTree(...), which must be implemented to grow the tree.
    /// 
    /// <p/>GPNodeBuilder also provides some facilities for user-specification
    /// of probabilities of various tree sizes, which the tree builder can use
    /// as it sees fit (or totally ignore).  
    /// There are two such facilities.  First, the user might
    /// specify a minimum and maximum range for tree sizes to be picked from;
    /// trees would likely be picked uniformly from this range.  Second, the
    /// user might specify an array, <tt>num-sizes</tt> long, of probabilities of 
    /// tree sizes, in order to give a precise probability distribution. 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>min-size</tt><br/>
    /// <font size="-1">int &gt;= 1, or undefined</font></td>
    /// <td valign="top">(smallest valid size, see discussion above)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>max-size</tt><br/>
    /// <font size="-1">int &gt;= <tt>min-size</tt>, or undefined</font></td>
    /// <td valign="top">(largest valid size, see discussion above)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>num-sizes</tt><br/>
    /// <font size="-1">int &gt;= 1, or underfined</font></td>
    /// <td valign="top">(number of sizes in the size distribution, see discussion above)
    /// </td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt>.<i>n</i><br/>
    /// <font size="-1">0.0 &lt;= float &lt;= 1.0</font>, or undefined</td>
    /// <td valign="top">(probability of choosing size <i>n</i>.  See discussion above)
    /// </td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPNodeBuilder")]
    public abstract class GPNodeBuilder : IPrototype
    {
        #region Constants

        public const int NOSIZEGIVEN = - 1;
        public const int CHECK_BOUNDARY = 8;
        public const string P_MINSIZE = "min-size";
        public const string P_MAXSIZE = "max-size";
        public const string P_NUMSIZES = "num-sizes";
        public const string P_SIZE = "size";

        #endregion // Constants
        #region Properties

        public abstract IParameter DefaultBase { get; }

        /// <summary>
        /// The minium possible size  -- if unused, it's 0 
        /// </summary>
        public int MinSize { get; set; }

        /// <summary>
        /// The maximum possible size  -- if unused, it's 0 
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary> 
        /// SizeDistribution[x] represents the likelihood of size x appearing -- if unused, it's null 
        /// </summary>
        public float[] SizeDistribution { get; set; }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            // min and max size

            if (state.Parameters.ParameterExists(paramBase.Push(P_MINSIZE), def.Push(P_MINSIZE)))
            {
                if (!(state.Parameters.ParameterExists(paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE))))
                    state.Output.Fatal("This GPNodeBuilder has a " + P_MINSIZE + " but not a " + P_MAXSIZE + ".");

                MinSize = state.Parameters.GetInt(paramBase.Push(P_MINSIZE), def.Push(P_MINSIZE), 1);
                if (MinSize == 0)
                    state.Output.Fatal("The GPNodeBuilder must have a min size >= 1.", paramBase.Push(P_MINSIZE), def.Push(P_MINSIZE));

                MaxSize = state.Parameters.GetInt(paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE), 1);
                if (MaxSize == 0)
                    state.Output.Fatal("The GPNodeBuilder must have a max size >= 1.", paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE));

                if (MinSize > MaxSize)
                    state.Output.Fatal("The GPNodeBuilder must have min size <= max size.", paramBase.Push(P_MINSIZE), def.Push(P_MINSIZE));
            }
            else if (state.Parameters.ParameterExists(paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE)))
                state.Output.Fatal("This GPNodeBuilder has a " + P_MAXSIZE + " but not a " + P_MINSIZE + ".", paramBase.Push(P_MAXSIZE), def.Push(P_MAXSIZE));
            // load sizeDistribution
            else if (state.Parameters.ParameterExists(paramBase.Push(P_NUMSIZES), def.Push(P_NUMSIZES)))
            {
                var siz = state.Parameters.GetInt(paramBase.Push(P_NUMSIZES), def.Push(P_NUMSIZES), 1);
                if (siz == 0)
                    state.Output.Fatal("The number of sizes in the GPNodeBuilder's distribution must be >= 1. ");
                SizeDistribution = new float[siz];
                if (state.Parameters.ParameterExists(paramBase.Push(P_SIZE).Push("0"), def.Push(P_SIZE).Push("0")))
                    state.Output.Warning("GPNodeBuilder does not use size #0 in the distribution",
                        paramBase.Push(P_SIZE).Push("0"),
                        def.Push(P_SIZE).Push("0"));

                var sum = 0.0f;
                for (var x = 0; x < siz; x++)
                {
                    SizeDistribution[x] = state.Parameters.GetFloat(paramBase.Push(P_SIZE).Push("" + (x + 1)), def.Push(P_SIZE).Push("" + (x + 1)), 0.0f);
                    if (SizeDistribution[x] < 0.0)
                    {
                        state.Output.Warning("Distribution value #" + x + " negative or not defined, assumed to be 0.0",
                            paramBase.Push(P_SIZE).Push("" + (x + 1)),
                            def.Push(P_SIZE).Push("" + (x + 1)));
                        SizeDistribution[x] = 0.0f;
                    }
                    sum += SizeDistribution[x];
                }
                if (sum > 1.0)
                    state.Output.Warning("Distribution sums to greater than 1.0", paramBase.Push(P_SIZE), def.Push(P_SIZE));
                if (sum == 0.0)
                    state.Output.Fatal("Distribution is all 0's", paramBase.Push(P_SIZE), def.Push(P_SIZE));

                // normalize and prepare
                RandomChoice.OrganizeDistribution(SizeDistribution);
            }
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns true if some size distribution (either minSize and maxSize,
        /// or sizeDistribution) is set up by the user in order to pick sizes randomly. 
        /// </summary>
        public virtual bool CanPick()
        {
            return (MinSize != 0 || SizeDistribution != null);
        }
        
        /// <summary>
        /// Assuming that either minSize and maxSize, or sizeDistribution, is defined,
        /// picks a random size from minSize...maxSize inclusive, or randomly
        /// from sizeDistribution. 
        /// </summary>
        public virtual int PickSize(IEvolutionState state, int thread)
        {
            if (MinSize > 0)
            {
                // pick from minSize...maxSize
                return state.Random[thread].NextInt(MaxSize - MinSize + 1) + MinSize;
            }
            if (SizeDistribution != null)
            {
                // pick from distribution
                return RandomChoice.PickFromDistribution(SizeDistribution, state.Random[thread].NextFloat(), CHECK_BOUNDARY) + 1;
            }
            throw new ApplicationException("Neither minSize nor sizeDistribution is defined in GPNodeBuilder");
        }
            
        /// <summary>
        /// Produces a new rooted tree of GPNodes whose root's return type is
        /// swap-compatible with <i>type</i>.  When you build a brand-new
        /// tree out of GPNodes cloned from the
        /// prototypes stored in the GPNode[] arrays, you must remember
        /// to call resetNode() on each cloned GPNode.  This gives ERCs a chance
        /// to randomize themselves and set themselves up. 
        /// <p/>requestedSize is an
        /// optional argument which differs based on the GPNodeBuilder used.
        /// Typically it is set to a tree size that the calling method wants
        /// the GPNodeBuilder to produce; the GPNodeBuilder is not obligated to
        /// produce a tree of this size, but it should attempt to interpret this
        /// argument as appropriate for the given algorithm.  To indicate that
        /// you don't care what size the tree should be, you can pass NOSIZEGIVEN. 
        /// However if the algorithm <i>requires</i> you to provide a size, it
        /// will generate a fatal error to let you know. 
        /// </summary>
        public abstract GPNode NewRootedTree(IEvolutionState state, GPType type, int thread, 
                                    IGPNodeParent parent, GPFunctionSet funcs, int argPosition, int requestedSize);

        #endregion // Operations
        #region Warnings and Errors
        /// <summary>
        /// Issues a warning that no terminal was found with a return type of the given type, and that an algorithm
        /// had requested one.  If fail is true, then a fatal is issued rather than a warning.  The warning takes
        /// the form of a one-time big explanatory message, followed by a one-time-per-type message. 
        /// </summary>
        protected internal virtual void  WarnAboutNoTerminalWithType(GPType type, bool fail, IEvolutionState state)
        {
            // big explanation -- appears only once
            state.Output.WarnOnce("A GPNodeBuilder has been requested at least once to generate a one-node tree with " 
                + "a return value type-compatable with a certain type; but there is no TERMINAL which is type-compatable " 
                + "in this way.  As a result, the algorithm was forced to use a NON-TERMINAL, making the tree larger than " 
                + "requested, and exposing more child slots to fill, which if not carefully considered, could " 
                + "recursively repeat this problem and eventually fill all memory.");
            
            // shorter explanation -- appears for each node builder and type combo
            if (fail)
            {
                state.Output.Fatal("" + GetType() + " can't find a terminal type-compatable with " 
                    + type + " and cannot replace it with a nonterminal.  You may need to try a different node-builder algorithm.");
            }
            else
            {
                state.Output.WarnOnce("" + GetType() + " can't find a terminal type-compatable with " + type);
            }
        }
        
        /// <summary>
        /// If the given test is true, issues a warning that no terminal was found with a return type of the given type, and that an algorithm
        /// had requested one.  If fail is true, then a fatal is issued rather than a warning.  The warning takes
        /// the form of a one-time big explanatory message, followed by a one-time-per-type message. Returns the value of the test.
        /// This form makes it easy to insert warnings into if-statements.  
        /// </summary>
        protected internal virtual bool WarnAboutNonterminal(bool test, GPType type, bool fail, IEvolutionState state)
        {
            if (test)
                WarnAboutNonTerminalWithType(type, fail, state);
            return test;
        }
        
        /// <summary>
        /// Issues a warning that no nonterminal was found with a return type of the given type, and that an algorithm
        /// had requested one.  If fail is true, then a fatal is issued rather than a warning.  The warning takes
        /// the form of a one-time big explanatory message, followed by a one-time-per-type message. 
        /// </summary>
        protected internal virtual void  WarnAboutNonTerminalWithType(GPType type, bool fail, IEvolutionState state)
        {
            // big explanation -- appears only once
            state.Output.WarnOnce("A GPNodeBuilder has been requested at least once to generate a one-node tree with " 
                + "a return value type-compatable with a certain type; but there is no NON-TERMINAL which is type-compatable " 
                + "in this way.  As a result, the algorithm was forced to use a TERMINAL, making the tree larger than " 
                + "requested, and exposing more child slots to fill, which if not carefully considered, could " 
                + "recursively repeat this problem and eventually fill all memory.");
            
            // shorter explanation -- appears for each node builder and type combo
            if (fail)
            {
                state.Output.Fatal("" + GetType() + " can't find a terminal type-compatable with " 
                    + type + " and cannot replace it with a nonterminal.  You may need to try a different node-builder algorithm.");
            }
            else
            {
                state.Output.WarnOnce("" + GetType() + " can't find a terminal type-compatable with " + type);
            }
        }
        
        /// <summary>
        /// Issues a fatal error that no node (nonterminal or terminal) 
        /// was found with a return type of the given type, and that an algorithm
        /// had requested one.  
        /// </summary>
        protected internal virtual void  ErrorAboutNoNodeWithType(GPType type, IEvolutionState state)
        {
            state.Output.Fatal("" + GetType() + " could find no terminal or nonterminal type-compatable with " + type);
        }

        #endregion // Warnings and Errors
        #region Cloning

        public virtual object Clone()
        {
            try
            {
                var c = (GPNodeBuilder) MemberwiseClone();

                if (SizeDistribution != null)
                    c.SizeDistribution = (float[]) SizeDistribution.Clone();

                return c;
            }
            catch (Exception)
            {
                throw new ApplicationException("Cloning Error!");
            } // never happens
        }

        #endregion // Cloning
    }
}