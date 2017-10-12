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
using System.IO;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// GPTree is a IGPNodeParent which holds the root GPNode of a tree of GPNodes.  
    /// GPTrees typically fill out an array held in a GPIndividual (their "owner") 
    /// and their roots are evaluated to evaluate a Genetic programming tree.
    /// 
    /// GPTrees also have <i>constraints</i>, which are shared, and define items shared among several GPTrees.
    /// <p/>In addition to serialization for checkpointing, GPTrees may read and write themselves to streams in three ways.
    /// 
    /// <ul>
    /// <li/><b>WriteTree(...,BinaryWriter) / ReadTree(...,BinaryReader)</b>&nbsp;&nbsp;&nbsp;
    /// This method transmits or receives a GPTree in binary.  It is the most efficient approach to sending
    /// GPTrees over networks, etc.  The default versions of WriteTree/ReadTree call WriteRootedTree/ReadRootedTree
    /// on their respective GPNode roots.
    /// 
    /// <li/><b>PrintTree(...,StreamWriter) / ReadTree(...,StreamReader)</b>&nbsp;&nbsp;&nbsp;
    /// This approach transmits or receives a GPTree in text encoded such that the GPTree is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  To do this, these methods will typically encode numbers
    /// using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
    /// files for inspection, slight modification, then reading back in later on.  <b>ReadTree</b>
    /// largely calls ReadRootedTree on the GPNode root.  Likewise, <b>PrintTree</b> calls PrintRootedTree
    /// 
    /// <li/><b>PrintTreeForHumans(...,StreamWriter)</b>&nbsp;&nbsp;&nbsp;
    /// This approach prints a GPTree in a fashion intended for human consumption only.
    /// <b>PrintTreeForHumans</b> calls either <b>MakeCTree</b> and prints the result,
    /// calls <b>MakeLatexTree</b> and prints the result, or calls <b>PrintRootedTreeForHumans</b> on the root.
    /// Which one is called depends on the kind of tree you have chosen to print for humans, as is discussed next.
    /// </ul>
    /// <p/>GPTrees can print themselves for humans in one of three ways:
    /// <ol><li/>A GPTree can print the tree as a Koza-style Lisp s-expression, which is the default.  
    /// <li/> A GPTree can print itself in pseudo-C format:
    /// <ol><li/>Terminals can be printed either as variables "a" or as zero-argument functions "a()"
    /// <li/>One-argument nonterminals are printed as functions "a(b)"
    /// <li/>Two-argument nonterminals can be printed either as operators "b a c" or as functions "a(b, c)"
    /// <li/>Nonterminals with more arguments are printed as functions "a(b, c, d, ...)"
    /// </ol>
    /// <li/>A GPTree can print the tree as a LaTeX2e code snippet, which can be inserted
    /// into a LaTeX2e file and will result in a picture of the tree!  Cool, no?
    /// </ol>
    /// 
    /// <p/>You turn the C-printing feature on with the <b>c</b> parameter, plus certain
    /// optional parameters (<b>c-operators</b>, <b>c-variables</b>) as described below.
    /// You turn the latex-printing <b>latex</b> parameter below.  The C-printing parameter
    /// takes precedence.
    /// <p/>
    /// Here's how the latex system works.  To insert the code, you'll need to include the
    /// <tt>epic</tt>,<tt>ecltree</tt>, and probably the <tt>fancybox</tt> packages,
    /// in that order.  You'll also need to define the command <tt>\gpbox</tt>, which
    /// takes one argument (the string name for the GPNode) and draws a box with that
    /// node.  Lastly, you might want to set a few parameters dealing with the 
    /// <tt>ecltree</tt> package.
    /// 
    /// <p/>Here's an example which looks quite good (pardon the double-backslashes
    /// in front of the usepackage statements -- javadoc is freaking out if I put
    /// a single backslash.  So you'll need to remove the extra backslash in order
    /// to try out this example):
    /// 
    /// <p/><table width="100%" border="0" cellpadding="0" cellspacing="0">
    /// <tr><td bgcolor="#DDDDDD"><font size="-1"><tt>
    /// <pre>
    /// \documentclass[]{article}
    /// \\usepackage{epic}     <b>% required by ecltree and fancybox packages</b>
    /// \\usepackage{ecltree}  <b>% to draw the GP trees</b>
    /// \\usepackage{fancybox} <b>% required by \Ovalbox</b>
    /// \begin{document}
    /// <b>% minimum distance between nodes on the same line</b>
    /// \setlength{\GapWidth}{1em}    
    /// <b>% draw with a thick dashed line, very nice looking</b>
    /// \thicklines \drawwith{\dottedline{2}}   
    /// <b>% draw an oval and center it with the rule.  You may want to fool with the
    /// % rule values, though these seem to work quite well for me.  If you make the
    /// % rule smaller than the text height, then the GP nodes may not line up with
    /// % each other horizontally quite right, so watch out.</b>
    /// \newcommand{\gpbox}[1]{\Ovalbox{#1\rule[-.7ex]{0ex}{2.7ex}}}
    /// 
    /// <b>% Here's the tree which the GP system spat out</b>
    /// \begin{bundle}{\gpbox{progn3}}\chunk{\begin{bundle}{\gpbox{if-food-ahead}}
    /// \chunk{\begin{bundle}{\gpbox{progn3}}\chunk{\gpbox{right}}
    /// \chunk{\gpbox{left}}\chunk{\gpbox{move}}\end{bundle}}
    /// \chunk{\begin{bundle}{\gpbox{if-food-ahead}}\chunk{\gpbox{move}}
    /// \chunk{\gpbox{left}}\end{bundle}}\end{bundle}}\chunk{\begin{bundle}{\gpbox{progn2}}
    /// \chunk{\begin{bundle}{\gpbox{progn2}}\chunk{\gpbox{move}}
    /// \chunk{\gpbox{move}}\end{bundle}}\chunk{\begin{bundle}{\gpbox{progn2}}
    /// \chunk{\gpbox{right}}\chunk{\gpbox{left}}\end{bundle}}\end{bundle}}
    /// \chunk{\begin{bundle}{\gpbox{if-food-ahead}}\chunk{\begin{bundle}{\gpbox{if-food-ahead}}
    /// \chunk{\gpbox{move}}\chunk{\gpbox{left}}\end{bundle}}
    /// \chunk{\begin{bundle}{\gpbox{if-food-ahead}}\chunk{\gpbox{left}}\chunk{\gpbox{right}}
    /// \end{bundle}}\end{bundle}}\end{bundle}
    /// \end{document}
    /// </pre></tt></font></td></tr></table>
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>tc</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(The tree's constraints)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>latex</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(print for humans using latex?)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>c</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> or <tt>false</tt> (default)</td>
    /// <td valign="top">(print for humans using c?  Takes precedence over latex)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>c-operators</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> (default) or <tt>false</tt></td>
    /// <td valign="top">(when printing using c, print two-argument functions operators "b a c"?  The alternative is functions "a(b, c)."</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>c-variables</tt><br/>
    /// <font size="-1"/>bool = <tt>true</tt> (default) or <tt>false</tt></td>
    /// <td valign="top">(when printing using c, print zero-argument functions as variables "a"?  The alternative is functions "a()".)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.tree
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.GPTree")]
    public class GPTree : IGPNodeParent, IPrototype
    {
        #region Constants

        public const string P_TREE = "tree";
        public const string P_TREECONSTRAINTS = "tc";
        public const string P_USEGRAPHVIZ = "graphviz";
        public const string P_USELATEX = "latex";
        public const string P_USEC = "c";
        public const string P_USEOPS = "c-operators";
        public const string P_USEVARS = "c-variables";
        public const int NO_TREENUM = -1;

        public const string P_PRINT_STYLE = "print-style";
        public const string V_LISP = "lisp";
        public const string V_DOT = "dot";
        public const string V_LATEX = "latex";
        public const string V_C = "c";

        public const int PRINT_STYLE_LISP = 0;
        public const int PRINT_STYLE_DOT = 1;
        public const int PRINT_STYLE_LATEX = 2;
        public const int PRINT_STYLE_C = 3;

        #endregion // Constants
        #region Properties

        public virtual IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_TREE); }
        }

        /// <summary>
        /// The root GPNode in the GPTree 
        /// </summary>
        public GPNode Child { get; set; }

        /// <summary>
        /// The owner of the GPTree 
        /// </summary>
        public GPIndividual Owner { get; set; }

        /// <summary>
        /// An expensive function which determines my tree number -- only
        /// use for errors, etc. Returns ec.gp.GPTree.NO_TREENUM if the 
        /// tree number could not be
        /// determined (might happen if it's not been assigned yet). 
        /// </summary>
        public virtual int TreeNumber
        {
            get
            {
                if (Owner == null)
                    return NO_TREENUM;

                if (Owner.Trees == null)
                    return NO_TREENUM;

                for (var x = 0; x < Owner.Trees.Length; x++)
                    if (Owner.Trees[x] == this)
                        return x;

                return NO_TREENUM;
            }
        }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Sets up a prototypical GPTree with those features it shares with
        /// other GPTrees in its position in its GPIndividual, and nothhing more.
        /// This must be called <i>after</i> the GPTypes and GPNodeConstraints 
        /// have been set up.  Presently they're set up in GPInitializer,
        /// which gets called before this does, so we're safe. 
        /// </summary>
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            // get rid of deprecated values
            if (state.Parameters.ParameterExists(paramBase.Push(P_USEGRAPHVIZ), def.Push(P_USEGRAPHVIZ)))
                state.Output.Error("Parameter no longer used.  See GPTree.cs for details.", paramBase.Push(P_USEGRAPHVIZ), def.Push(P_USEGRAPHVIZ));
            if (state.Parameters.ParameterExists(paramBase.Push(P_USELATEX), def.Push(P_USELATEX)))
                state.Output.Error("Parameter no longer used.  See GPTree.cs for details.", paramBase.Push(P_USELATEX), def.Push(P_USELATEX));
            if (state.Parameters.ParameterExists(paramBase.Push(P_USEC), def.Push(P_USEC)))
                state.Output.Error("Parameter no longer used.  See GPTree.cs for details.", paramBase.Push(P_USEC), def.Push(P_USEC));
            state.Output.ExitIfErrors();

            var style = state.Parameters.GetString(paramBase.Push(P_PRINT_STYLE), def.Push(P_PRINT_STYLE));
            if (style == null)  // assume Lisp
                PrintStyle = PRINT_STYLE_LISP;
            else if (style.Equals(V_C))
                PrintStyle = PRINT_STYLE_C;
            else if (style.Equals(V_DOT))
                PrintStyle = PRINT_STYLE_DOT;
            else if (style.Equals(V_LATEX))
                PrintStyle = PRINT_STYLE_LATEX;

            // in C, treat terminals as variables?  By default, yes.
            PrintTerminalsAsVariablesInC = state.Parameters.GetBoolean(paramBase.Push(P_USEVARS), def.Push(P_USEVARS), true);

            // in C, treat two-child functions as operators?  By default, yes.
            PrintTwoArgumentNonterminalsAsOperatorsInC = state.Parameters.GetBoolean(paramBase.Push(P_USEOPS), def.Push(P_USEOPS), true);

            // determine my constraints -- at this point, the constraints should have been loaded.
            var s = state.Parameters.GetString(paramBase.Push(P_TREECONSTRAINTS), def.Push(P_TREECONSTRAINTS));
            if (s == null)
            {
                state.Output.Fatal("No tree constraints are defined for the GPTree " + paramBase + ".");
            }
            else
                ConstraintsIndex = GPTreeConstraints.ConstraintsFor(s, state).ConstraintsIndex;

            state.Output.ExitIfErrors(); // because I promised
            // we're not loading the nodes at this point
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Builds a new randomly-generated rooted tree and attaches it to the GPTree. 
        /// </summary>
        public virtual void BuildTree(IEvolutionState state, int thread)
        {
            var initializer = ((GPInitializer)state.Initializer);
            Child = Constraints(initializer).Init.NewRootedTree(state, Constraints(initializer).TreeType, thread, this,
                                                                       Constraints(initializer).FunctionSet,
                                                                       0, GPNodeBuilder.NOSIZEGIVEN);
        }

        /// <summary>
        /// Verification of validity of the tree -- strictly for debugging purposes only.
        /// </summary>
        public void Verify(IEvolutionState state)
        {
            if (!(state.Initializer is GPInitializer))
            {
                state.Output.Error("Initializer is not a GPInitializer");
                return;
            }

            var initializer = (GPInitializer)(state.Initializer);

            if (Child == null)
            {
                state.Output.Error("Null root child of GPTree.");
                return;
            }
            if (Owner == null)
            {
                state.Output.Error("Null owner of GPTree.");
                return;
            }
            if (Owner.Trees == null)
            {
                state.Output.Error("Owner has null trees.");
                return;
            }
            if (TreeNumber == NO_TREENUM)
            {
                state.Output.Error("No Tree Number! I appear to be an orphan GPTree.");
                return;
            }
            if (ConstraintsIndex < 0 || ConstraintsIndex >= initializer.NumTreeConstraints)
            {
                state.Output.Error("Preposterous tree constraints (" + ConstraintsIndex + ")");
                return;
            }

            Child.Verify(state, Constraints(initializer).FunctionSet, 0);
            state.Output.ExitIfErrors();
        }

        #region Constraints

        /// <summary>
        /// Constraints on the GPTree  -- don't access the constraints through
        /// this variable -- use the Constraints() method instead, which will give
        /// the actual constraints object. 
        /// </summary>
        /// <remarks>NOTE: Changed visibility to protected.</remarks>
        protected int ConstraintsIndex { get; set; }

        public GPTreeConstraints Constraints(GPInitializer initializer)
        {
            if (ConstraintsIndex < 0 || ConstraintsIndex > initializer.TreeConstraints.Count - 1)
            {
                throw new InvalidOperationException("ConstraintsIndex is outside of the allowable range: [0, initializer.TreeConstraints.Length - 1]");
            }
            return initializer.TreeConstraints[ConstraintsIndex];
        }

        #endregion // Constraints

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// Returns a hash code for comparing different GPTrees.  In
        /// general, two trees which are TreeEquals(...) should have the
        /// same hash code. 
        /// </summary>
        public virtual int TreeHashCode()
        {
            return Child.RootedTreeHashCode();
        }

        /// <summary>
        /// Returns true if I am "genetically" the same as tree, though we may have different owners. 
        /// </summary>
        public virtual bool TreeEquals(GPTree tree)
        {
            return Child.RootedTreeEquals(tree.Child);
        }

        #endregion // Comparison
        #region Cloning

        /// <summary>
        /// Like clone() but doesn't copy the tree. 
        /// </summary>
        public virtual GPTree LightClone()
        {
            try
            {
                return (GPTree)(MemberwiseClone()); // note that the root child reference is copied, not cloned
            }
            catch (Exception)
            {
                throw new ApplicationException("Cloning Error!");
            } // never happens
        }

        /// <summary>
        /// Deep-clones the tree.  Note that you should not deep-clone trees attached to the
        /// prototypical GPIndividual: they are blank trees with no root, and this method
        /// will generate a NullPointerException as a result. 
        /// </summary>
        public virtual object Clone()
        {
            var newtree = LightClone();
            newtree.Child = (GPNode)Child.Clone(); // force a deep copy
            newtree.Child.Parent = newtree;
            newtree.Child.ArgPosition = 0;
            return newtree;
        }

        #endregion // Cloning
        #region IO

        /// <summary>
        /// The print style of the GPTree.
        /// </summary>
        public int PrintStyle { get; set; }

        /// <summary>
        /// When using c to print for humans, do we print terminals as variables? 
        /// (as opposed to zero-argument functions)? 
        /// </summary>
        public bool PrintTerminalsAsVariablesInC { get; set; }

        /// <summary>
        /// When using c to print for humans, do we print two-argument nonterminals in operator form "a op b"? 
        /// (as opposed to functions "op(a, b)")? 
        /// </summary>
        public bool PrintTwoArgumentNonterminalsAsOperatorsInC { get; set; }

        /// <summary>
        /// Prints out the tree in single-line fashion suitable for reading in later by computer. O(n). 
        /// The default version of this method simply calls child's PrintRootedTree(...) method. 
        /// </summary>
        public void PrintTree(IEvolutionState state, int log)
        {
            Child.PrintRootedTree(state, log, 0);
            // printRootedTree doesn't print a '\n', so I need to do so here
            state.Output.PrintLn("", log);
        }

        /// <summary>
        /// Prints out the tree in single-line fashion suitable for reading
        /// in later by computer. O(n). 
        /// The default version of this method simply calls child's 
        /// PrintRootedTree(...) method. 
        /// </summary>
        public virtual void PrintTree(IEvolutionState state, StreamWriter writer)
        {
            Child.PrintRootedTree(state, writer, 0);
            // PrintRootedTree doesn't print a '\n', so I need to do so here
            writer.WriteLine();
        }

        /// <summary>
        /// Prints out the tree in a readable Lisp-like fashion. O(n). 
        /// The default version of this method simply calls child's 
        /// PrintRootedTreeForHumans(...) method.
        /// </summary>
        public void PrintTreeForHumans(IEvolutionState state, int log)
        {
            if (PrintStyle == PRINT_STYLE_C)
                state.Output.Print(Child.MakeCTree(true, PrintTerminalsAsVariablesInC,
                                                         PrintTwoArgumentNonterminalsAsOperatorsInC),
                                                         log);
            else if (PrintStyle == PRINT_STYLE_LATEX)
                state.Output.Print(Child.MakeLatexTree(), log);
            else if (PrintStyle == PRINT_STYLE_DOT)
                state.Output.Print(Child.MakeGraphvizTree(), log);
            else
                Child.PrintRootedTreeForHumans(state, log, 0, 0);
            // PrintRootedTreeForHumans doesn't print a '\n', so I need to do so here
            state.Output.PrintLn("", log);
        }

        /// <summary>
        /// Reads in the tree from a form printed by PrintTree. 
        /// </summary>
        public virtual void ReadTree(IEvolutionState state, StreamReader reader)
        {
            // BRS : TODO : The .NET StreamReader does not provide a way to get the current linenumber. 
            // For now we'll use the byte offset. (could pass in a linenumber tracked by the caller)
            var linenumber = (int)reader.BaseStream.Position;

            // the next line will be the child
            var s = reader.ReadLine();
            if (s == null)
                // uh oh
                state.Output.Fatal("Reading at stream position " + linenumber + " : " + "No Tree found.");
            else
            {
                var initializer = ((GPInitializer)state.Initializer);

                Child = GPNode.ReadRootedTree(linenumber, new DecodeReturn(s),
                                                Constraints(initializer).TreeType,
                                                Constraints(initializer).FunctionSet,
                                                this, 0, state);
            }
        }

        public virtual void WriteTree(IEvolutionState state, BinaryWriter writer)
        {
            var initializer = ((GPInitializer)state.Initializer);
            Child.WriteRootedTree(state, Constraints(initializer).TreeType, Constraints(initializer).FunctionSet, writer);
        }

        public virtual void ReadTree(IEvolutionState state, BinaryReader reader)
        {
            var initializer = ((GPInitializer)state.Initializer);

            Child = GPNode.ReadRootedTree(state, reader,
                                            Constraints(initializer).TreeType,
                                            Constraints(initializer).FunctionSet,
                                            this, 0);
        }

        #endregion // IO
    }
}