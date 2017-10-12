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
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Logging;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// GPNode is a IGPNodeParent which is the abstract superclass of
    /// all GP function nodes in trees.  GPNode contains quite a few functions
    /// for cloning subtrees in special ways, counting the number of nodes
    /// in subtrees in special ways, and finding specific nodes in subtrees.
    /// 
    /// GPNode's LightClone() method does not clone its Children (it copies the
    /// array, but that's it).  If you want to deep-clone a tree or subtree, you
    /// should use one of the CloneReplacing(...) methods instead.
    /// 
    /// <p/>GPNodes contain a number of important items:
    /// <ul><li/>A <i>Constraints</i> object which defines the name of the node,
    /// its arity, and its type Constraints. This
    /// object is shared with all GPNodes of the same function name/arity/returntype/childtypes.
    /// <li/>A <i>Parent</i>.  This is either another GPNode, or (if this node
    /// is the root) a GPTree.
    /// <li/>Zero or more <i>Children</i>, which are GPNodes.
    /// <li/>An argument position in its Parent.
    /// </ul>
    /// 
    /// <p/>In addition to serialization for checkpointing, GPNodes may read and write themselves to streams in three ways.
    /// 
    /// <ul>
    /// <li/><b>WriteNode(...,BinaryWriter) / ReadNode(...,BinaryReader)</b>&nbsp;&nbsp;&nbsp;
    /// This method transmits or receives a GPNode in binary.  It is the most efficient approach to sending
    /// GPNodes over networks, etc.  The default versions of WriteNode/ReadNode both generate errors.
    /// GPNode subclasses should override them to provide more functionality, particularly if you're planning on using
    /// ECJ in a distributed fashion.  Both of these functions are called by GPNode's ReadRootedTree/WriteRootedTree
    /// respectively, which handle the reading/printing of the trees as a whole.
    /// 
    /// <li/><b>PrintNode(...,StreamWriter) / ReadNode(...,StreamReader)</b>&nbsp;&nbsp;&nbsp;
    /// This approach transmits or receives a GPNode in text encoded such that the GPNode is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  To do this, these methods will typically encode numbers
    /// using the <tt>ec.util.Code</tt> class.  These methods are mostly used to write out populations to
    /// files for inspection, slight modification, then reading back in later on.  
    /// Both of these functions are called by GPNode's ReadRootedTree/WriteRootedTree
    /// respectively, which handle the reading/printing of the trees as a whole.  Notably ReadRootedNode
    /// will try to determine what kind of node is next, then call <b>ReadNode</b> on the prototype for that
    /// node to generate the node.  <b>PrintNode</b> by default calls ToString() and
    /// prints the result, though subclasses often override this to provide additional functionality (notably
    /// ERCs).
    /// 
    /// <li/><b>PrintNodeForHumans(...,StreamWriter)</b>&nbsp;&nbsp;&nbsp;
    /// This approach prints a GPNode in a fashion intended for human consumption only.
    /// <b>PrintNodeForHumans</b> by default calls ToStringForHumans() (which by default calls ToString()) and
    /// prints the result.  PrintNodeForHumans is called by <b>PrintRootedTreeForHumans</b>, which handles
    /// printing of the entire GPNode tree.
    /// </ul>
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>nc</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of the node Constraints for the GPNode)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.Node
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPNode")]
    public abstract class GPNode : IGPNodeParent
    {
        #region Constants

        public const string P_NODE = "node";
        public const string P_NODECONSTRAINTS = "nc";
        public const string GPNODEPRINTTAB = "    ";
        public const int MAXPRINTBYTES = 40;
        
        public const int NODESEARCH_ALL = 0;
        public const int NODESEARCH_TERMINALS = 1;
        public const int NODESEARCH_NONTERMINALS = 2;
        public const int NODESEARCH_CUSTOM = 3;
        
        public const int CHILDREN_UNKNOWN = -1;

        #endregion // Constants
        #region Properties

        /// <summary>
        /// The default base for GPNodes -- defined even though
        /// GPNode is abstract so you don't have to in subclasses. 
        /// </summary>
        public virtual IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_NODE); }
        }

        /// <summary>
        /// Returns a Lisp-like atom for the node which can be read in again by computer.
        /// If you need to encode an integer or a float or whatever for some reason
        /// (perhaps if it's an ERC), you should use the ec.util.Code library.  
        /// </summary>
        public virtual string Name
        {
            get { return ToString(); }
        }

        /// <summary>
        /// The GPNode's Parent.  4 bytes.  :-(  But it really helps simplify breeding. 
        /// </summary>
        public IGPNodeParent Parent { get; set; }

        public GPNode[] Children { get; set; }

        /// <summary>
        /// The argument position of the child in its Parent. 
        /// </summary>
        public int ArgPosition { get; set; }

        /// <summary>
        /// Returns the depth of the tree, which is a value >= 1.  O(n). 
        /// </summary>
        public virtual int Depth
        {
            get
            {
                var d = 0;
                foreach (var newdepth in Children.Select(t => t.Depth).Where(newdepth => newdepth > d))
                {
                    d = newdepth;
                }
                return d + 1;
            }
        }

        /// <summary>
        /// Returns the number of children this node expects to have.  This method is
        /// only called by the default implementation of checkConstraints(...), and by default
        /// it returns CHILDREN_UNKNOWN.  You can override this method to return a value >= 0,
        /// which will be checked for in the default checkConstraints(...), or you can leave
        /// this method alone and override checkConstraints(...) to check for more complex constraints
        /// as you see fit.
        /// </summary>
        public virtual int ExpectedChildren { get { return CHILDREN_UNKNOWN; } }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Sets up a <i>prototypical</i> GPNode with those features all nodes of that
        /// prototype share, and nothing more.  So no filled-in Children, 
        /// no ArgPosition, no Parent.  Yet.
        /// This must be called <i>after</i> the GPTypes and GPNodeConstraints 
        /// have been set up.  Presently they're set up in GPInitializer,
        /// which gets called before this does, so we're safe. 
        /// <p/>
        /// You should override this if you need to load some special features on
        /// a per-function basis.  Note that base hangs off of a function set, so
        /// this method may get called for different instances in the same GPNode
        /// class if they're being set up as prototypes for different GPFunctionSets.
        /// If you absolutely need some global base, then you should use something
        /// hanging off of GPDefaults.ParamBase.
        /// The ultimate caller of this method must guarantee that he will eventually
        /// call state.Output.ExitIfErrors(), so you can freely use state.Output.Error
        /// instead of state.Output.Fatal(), which will help a lot.
        /// </summary>		
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            var def = DefaultBase;

            // determine my Constraints -- at this point, the Constraints should have been loaded.
            var s = state.Parameters.GetString(paramBase.Push(P_NODECONSTRAINTS), def.Push(P_NODECONSTRAINTS));
            if (s == null)
                state.Output.Fatal("No node Constraints are defined for the GPNode "
                    + ToStringForError(), paramBase.Push(P_NODECONSTRAINTS), def.Push(P_NODECONSTRAINTS));
            else
                ConstraintsIndex = GPNodeConstraints.ConstraintsFor(s, state).ConstraintIndex;

            // The number of Children is determined by the Constraints.  Though
            // for some special versions of GPNode, we may have to enforce certain
            // rules, checked in Children versions of Setup(...)

            var constraintsObj = Constraints(((GPInitializer)state.Initializer));
            var len = constraintsObj.ChildTypes.Length;
            if (len == 0) Children = constraintsObj.ZeroChildren;
            else Children = new GPNode[len];
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Verification of validity of the node in the tree -- strictly for debugging purposes only. 
        /// </summary>
        public int Verify(IEvolutionState state, GPFunctionSet funcs, int index)
        {
            if (!(state.Initializer is GPInitializer))
            {
                state.Output.Error("" + index + ": Initializer is not a GPInitializer");
                return index + 1;
            }

            var initializer = (GPInitializer)(state.Initializer);

            // 1. Is the Parent and ArgPosition right?
            if (Parent == null)
            {
                state.Output.Error("" + index + ": null Parent");
                return index + 1;
            }
            if (ArgPosition < 0)
            {
                state.Output.Error("" + index + ": negative ArgPosition");
                return index + 1;
            }
            if (Parent is GPTree && ((GPTree)Parent).Child != this)
            {
                state.Output.Error("" + index + ": I think I am a root node, but my GPTree does not think I am a root node");
                return index + 1;
            }
            if (Parent is GPTree && ArgPosition != 0)
            {
                state.Output.Error("" + index + ": I think I am a root node, but my ArgPosition is not 0");
                return index + 1;
            }
            if (Parent is GPNode && ArgPosition >= ((GPNode)Parent).Children.Length)
            {
                state.Output.Error("" + index + ": ArgPosition outside range of Parent's Children array");
                return index + 1;
            }
            if (Parent is GPNode && ((GPNode)Parent).Children[ArgPosition] != this)
            {
                state.Output.Error("" + index + ": I am not found in the provided ArgPosition (" + ArgPosition + ") of my Parent's Children array");
                return index + 1;
            }

            // 2. Are the Parents and ArgPositions right for my kids? [need to double check]
            if (Children == null)
            {
                state.Output.Error("" + index + ": Null Children Array");
                return index + 1;
            }
            for (var x = 0; x < Children.Length; x++)
            {
                if (Children[x] == null)
                {
                    state.Output.Error("" + index + ": Null Child (#" + x + " )");
                    return index + 1;
                }
                if (Children[x].Parent != this)
                {
                    state.Output.Error("" + index + ": child #" + x + " does not have me as a Parent");
                    return index + 1;
                }
                if (Children[x].ArgPosition < 0)
                {
                    state.Output.Error("" + index + ": child #" + x + " ArgPosition is negative");
                    return index + 1;
                }
                if (Children[x].ArgPosition != x)
                {
                    state.Output.Error("" + index + ": child #" + x + " ArgPosition does not match position in the Children array");
                    return index + 1;
                }
            }

            // 3. Do I have valid Constraints?
            if (ConstraintsIndex < 0 || ConstraintsIndex >= initializer.NumNodeConstraints)
            {
                state.Output.Error("" + index + ": Preposterous node Constraints (" + ConstraintsIndex + ")");
                return index + 1;
            }

            // 4. Am I swap-compatable with my Parent?
            if (Parent is GPNode && !Constraints(initializer).ReturnType.CompatibleWith(initializer, ((GPNode)(Parent))
                                                                                                         .Constraints(initializer).ChildTypes[ArgPosition]))
            {
                state.Output.Error("" + index + ": Incompatable GP type between me and my Parent");
                return index + 1;
            }
            if (Parent is GPTree && !Constraints(initializer).ReturnType.CompatibleWith(initializer, ((GPTree)(Parent))
                                                                                .Constraints(initializer).TreeType))
            {
                state.Output.Error("" + index + ": I am root, but incompatable GP type between me and my tree return type");
                return index + 1;
            }

            // 5. Is my class in the GPFunctionSet?
            var nodes = funcs.NodesByArity[Constraints(initializer).ReturnType.Type][Children.Length];
            var there = false;

            for (var x = 0; x < nodes.Length; x++)
                if (nodes[x].GetType() == GetType())
                {
                    there = true;
                    break;
                }
            if (!there)
            {
                state.Output.Error("" + index + ": I'm not in the function set.");
                return index + 1;
            }

            // otherwise we've passed -- go to next node
            index++;
            for (var x = 0; x < Children.Length; x++)
                index = Children[x].Verify(state, funcs, index);

            state.Output.ExitIfErrors();
            return index;
        }

        #region Constraints

        /// <summary>
        /// The GPNode's Constraints.  You typically shouldn't access the Constraints 
        /// through this variable -- use the Constraints(state) method instead. 
        /// </summary>
        protected int ConstraintsIndex { get; set; }

        /// <summary>
        /// Returns the GPNode's Constraints.  A good JIT compiler should inline this.
        /// </summary>
        public GPNodeConstraints Constraints(GPInitializer initializer)
        {
            return initializer.NodeConstraints[ConstraintsIndex];
        }

        /// <summary>
        /// You ought to override this method to check to make sure that the
        /// Constraints are valid as best you can tell.  Things you might
        /// check for:
        /// <ul>
        /// <li/> Children.Length is correct
        /// <li/> certain arguments in Constraints.ChildTypes are 
        /// swap-compatible with each other
        /// <li/> Constraints.ReturnType is swap-compatible with appropriate 
        /// arguments in Constraints.ChildTypes
        /// </ul>
        /// <p/>You can't check for everything, of course, but you might try some
        /// obvious checks for blunders.  The default version of this method
        /// simply calls numChildren() if it's defined (it returns something >= 0).
        /// If the value doesn't match the current number of children, an error is raised.
        /// This is a simple constraints check.
        /// <p/>
        /// The ultimate caller of this method must guarantee that he will eventually
        /// call state.Output.ExitIfErrors(), so you can freely use state.Output.Error
        /// instead of state.Output.Fatal(), which will help a lot.
        /// </summary>
        /// <remarks>Warning: this method may get called more than once.</remarks>
        public virtual void CheckConstraints(IEvolutionState state, int tree, GPIndividual typicalIndividual, IParameter individualBase)
        {
            int numChildren = ExpectedChildren;
            if (numChildren >= 0 && Children.Length != numChildren)  // uh oh
                state.Output.Error("Incorrect number of children for node " + ToStringForError() + " at " + individualBase +
                ", was expecting " + numChildren + " but got " + Children.Length);
        }
        
        #endregion // Constraints
        #region Tree

        #region Parent

        /// <summary>
        /// Returns the argument type of the slot that I fit into in my Parent.  
        /// If I'm the root, returns the treetype of the GPTree. 
        /// </summary>
        public GPType ParentType(GPInitializer initializer)
        {
            if (Parent is GPNode)
                return ((GPNode)Parent).Constraints(initializer).ChildTypes[ArgPosition];

            // else it's a tree root
            return ((GPTree)Parent).Constraints(initializer).TreeType;
        }

        /// <summary>
        /// Returns the root ancestor of this node.  O(ln n) average case, O(n) worst case. 
        /// </summary>		
        public virtual IGPNodeParent RootParent()
        {

            // -- new code, no need for recursion
            IGPNodeParent cParent = this;
            while (cParent != null && cParent is GPNode)
                cParent = ((GPNode)cParent).Parent;
            return cParent;
        }

        #endregion // Parent
        #region RootedTree

        /// <summary>
        /// Returns a hashcode associated with all the nodes in the tree.  
        /// The default version adds the hash of the node plus its child
        /// trees, rotated one-off each time, which seems reasonable. 
        /// </summary>
        public virtual int RootedTreeHashCode()
        {
            var hash = NodeHashCode();

            return Children.Aggregate(hash, (current, t) => (current << 1 | BitShifter.URShift(current, 31)) ^ t.RootedTreeHashCode());
        }

        /// <summary>
        /// Returns true if the two rooted trees are "genetically" equal, though
        /// they may have different Parents.  O(n). 
        /// </summary>
        public virtual bool RootedTreeEquals(GPNode node)
        {
            if (!NodeEquals(node))
                return false;
            for (var x = 0; x < Children.Length; x++)
                if (!(Children[x].RootedTreeEquals(node.Children[x])))
                    return false;
            return true;
        }

        #endregion // RootedTree
        #region PathLength

        /// <summary>
        /// Returns the path length of the tree, which is the sum of all paths from all nodes to the root.   O(n).
        /// </summary>
        public int PathLength(int nodesearch) { return pathLength(NODESEARCH_ALL, 0); }

        private int pathLength(int nodesearch, int currentDepth)
        {
            var sum = currentDepth;
            if (nodesearch == NODESEARCH_NONTERMINALS && Children.Length == 0 ||  // I'm a leaf, don't include me
                nodesearch == NODESEARCH_TERMINALS && Children.Length > 0)  // I'm a nonleaf, don't include me
                sum = 0;

            sum += Children.Sum(t => pathLength(nodesearch, currentDepth + 1));
            return sum;
        }

        #endregion // PathLength
        #region Depth

        /// <summary>
        /// Returns the mean depth of the tree,  which is path length 
        /// (sum of all paths from all nodes to the root)  divided by the number of nodes.  O(n).
        /// </summary>
        public int MeanDepth(int nodesearch)
        {
            return PathLength(nodesearch) / NumNodes(nodesearch);
        }

        /// <summary>
        /// Returns the depth at which I appear in the tree, which is a value >= 0. O(ln n) avg.
        /// </summary>
        public virtual int AtDepth()
        {
            // -- new code, no need for recursion
            var cParent = Parent;
            var count = 0;

            while (cParent != null && cParent is GPNode)
            {
                count++;
                cParent = ((GPNode)(cParent)).Parent;
            }
            return count;
        }

        #endregion // Depth
        #region Nodes

        /// <summary>
        /// Returns the number of nodes, constrained by g.Test(...)
        /// in the subtree for which this GPNode is root.  This might
        /// be sped up by caching the value.  O(n). 
        /// </summary>
        public virtual int NumNodes(GPNodeGatherer g)
        {
            var s = 0;
            for (var x = 0; x < Children.Length; x++)
                s += Children[x].NumNodes(g);

            return s + (g.Test(this) ? 1 : 0);
        }

        /// <summary>
        /// Returns the number of nodes, constrained by nodesearch,
        /// in the subtree for which this GPNode is root.
        /// This might be sped up by cacheing the value somehow.  O(n). 
        /// </summary>
        public virtual int NumNodes(int nodesearch)
        {
            var s = 0;
            for (var x = 0; x < Children.Length; x++)
                s += Children[x].NumNodes(nodesearch);
            return s + ((nodesearch == NODESEARCH_ALL
                     || (nodesearch == NODESEARCH_TERMINALS && Children.Length == 0)
                     || (nodesearch == NODESEARCH_NONTERMINALS && Children.Length > 0)) ? 1 : 0);
        }

        /// <summary>
        /// Returns the p'th node, constrained by nodesearch, in the subtree for which this GPNode is root.
        /// Use NumNodes(nodesearch) to determine the total number.  Or if you used numNodes(g), then when
        /// nodesearch == NODESEARCH_CUSTOM, g.Test(...) is used as the constraining predicate.
        /// p ranges from 0 to this number minus 1. O(n). The resultant node is returned in <i>g</i>.
        /// </summary>
        public virtual int NodeInPosition(int p, GPNodeGatherer g, int nodesearch)
        {

            // am I of the type I'm looking for?
            if (nodesearch == NODESEARCH_ALL
                || (nodesearch == NODESEARCH_TERMINALS && Children.Length == 0)
                || (nodesearch == NODESEARCH_NONTERMINALS && Children.Length > 0)
                || (nodesearch == NODESEARCH_CUSTOM && g.Test(this)))
            {
                // is the count now at 0?  Is it me?
                if (p == 0)
                {
                    g.Node = this;
                    return -1; // found it
                }
                // if it's not me, drop the count by 1
                p--;
            }

            // regardless, check my Children if I've not returned by now
            foreach (var t in Children)
            {
                p = t.NodeInPosition(p, g, nodesearch);
                if (p == -1)
                    return -1; // found it
            }
            return p;
        }

        /// <summary>
        /// Returns true if I am the "genetically" identical to this node, and our
        /// Children arrays are the same length, though
        /// we may have different Parents and Children.  The default form
        /// of this method simply calls the much weaker NodeEquivalentTo(node).  
        /// You may need to override this to perform exact comparisons, if you're
        /// an ERC, ADF, or ADM for example.  Here's an example of how NodeEquivalentTo(node)
        /// differs from NodeEquals(node): two ERCs, both of
        /// the same class, but one holding '1.23' and the other holding '2.45', which
        /// came from the same prototype node in the same function set.
        /// They should NOT be NodeEquals(...) but <i>should</i> be NodeEquivalent(...).  
        /// </summary>
        public virtual bool NodeEquals(GPNode node)
        {
            return NodeEquivalentTo(node);
        }

        /// <summary>
        /// Returns true if I and the provided node are the same kind of
        /// node -- that is, we could have both been Cloned() and Reset() from
        /// the same prototype node.  The default form of this function returns
        /// true if I and the node have the same class, the same length Children
        /// array, and the same Constraints.  You may wish to override this in
        /// certain circumstances.   Here's an example of how NodeEquivalentTo(node)
        /// differs from NodeEquals(node): two ERCs, both of
        /// the same class, but one holding '1.23' and the other holding '2.45', which
        /// came from the same prototype node in the same function set.
        /// They should NOT be NodeEquals(...) but <i>should</i> be NodeEquivalent(...). 
        /// </summary>
        public virtual bool NodeEquivalentTo(GPNode node)
        {
            return (GetType().Equals(node.GetType()) && Children.Length == node.Children.Length && ConstraintsIndex == node.ConstraintsIndex);
        }

        /// <summary>
        /// Returns a hashcode usually associated with all nodes that are 
        /// equal to you (using NodeEquals(...)).  The default form
        /// of this method returns the hashcode of the node's class.
        /// ERCs in particular probably will want to override this method.
        /// </summary>
        public virtual int NodeHashCode()
        {
            return GetHashCode();
        }

        /// <summary>
        /// Starts a node in a new life immediately after it has been cloned.
        /// The default version of this function does nothing.  The purpose of
        /// this function is to give ERCs a chance to set themselves to a new
        /// random value after they've been cloned from the prototype.
        /// You should not assume that the node is properly connected to other
        /// nodes in the tree at the point this method is called. 
        /// </summary>		
        public virtual void ResetNode(IEvolutionState state, int thread)
        {
        }

        /// <summary>
        /// Returns true if the subtree rooted at this node contains subnode.  O(n). 
        /// </summary>
        public virtual bool Contains(GPNode subnode)
        {
            return subnode == this || Children.Any(t => t.Contains(subnode));
        }

        /// <summary>
        /// Replaces the node with another node in its position in the tree. 
        /// newNode should already have been cloned and ready to go.
        /// We presume that the other node is type-compatible and
        /// of the same arity (these things aren't checked).  
        /// </summary>	
        public void ReplaceWith(GPNode newNode)
        {
            // copy the Parent and ArgPosition
            newNode.Parent = Parent;
            newNode.ArgPosition = ArgPosition;

            // replace the Parent pointer
            if (Parent is GPNode)
                ((GPNode)(Parent)).Children[ArgPosition] = newNode;
            else
                ((GPTree)(Parent)).Child = newNode;

            // replace the child pointers
            for (var x = 0; x < Children.Length; x++)
            {
                newNode.Children[x] = Children[x];
                newNode.Children[x].Parent = newNode;
                newNode.Children[x].ArgPosition = x;
            }
        }

        /// <summary>
        /// Returns true if I can swap into node's position. 
        /// </summary>		
        public bool SwapCompatibleWith(GPInitializer initializer, GPNode node)
        {
            // I'm atomically compatible with him; a fast check
            if (Constraints(initializer).ReturnType == node.Constraints(initializer).ReturnType)
                // no need to check for compatibility
                return true;

            // I'm set compatible with his Parent's swap-position
            GPType type;
            if (node.Parent is GPNode)
                // it's a GPNode
                type = ((GPNode)(node.Parent)).Constraints(initializer).ChildTypes[node.ArgPosition];
            // it's a tree root; I'm set compatible with the GPTree type
            else
                type = ((GPTree)(node.Parent)).Constraints(initializer).TreeType;

            return Constraints(initializer).ReturnType.CompatibleWith(initializer, type);
        }

        /// <summary>
        /// Evaluates the node with the given thread, state, individual, problem, and stack.
        /// Your random number generator will be state.Random[thread].  
        /// The node should, as appropriate, evaluate child nodes with these same items
        /// passed to Eval(...).
        /// <p/>About <b>input</b>: <tt>input</tt> is special; it is how data is passed between
        /// Parent and child nodes.  If Children "receive" data from their Parent node when
        /// it evaluates them, they should receive this data stored in <tt>input</tt>.
        /// If (more likely) the Parent "receives" results from its Children, it should
        /// pass them an <tt>input</tt> object, which they'll fill out, then it should
        /// check this object for the returned value.
        /// <p/>A tree is typically evaluated by dropping a GPData into the root.  When the
        /// root returns, the resultant <tt>input</tt> should hold the return value.
        /// <p/>In general, you should not be creating new GPDatas.  
        /// If you think about it, in most conditions (excepting ADFs and ADMs) you 
        /// can use and reuse <tt>input</tt> for most communications purposes between
        /// Parents and Children.  
        /// <p/>So, let's say that your GPNode function implements the boolean AND function,
        /// and expects its Children to return return boolean values (as it does itself).
        /// You've implemented your GPData subclass to be, uh, <b>BooleanData</b>, which
        /// looks like:
        ///  
        /// <code>
        ///         public class BooleanData extends GPData 
        ///         {
        ///             public boolean result;
        ///             public GPData copyTo(GPData gpd)
        ///             {
        ///                 ((BooleanData)gpd).result = result;
        ///             }
        ///         }
        /// </code>
        /// <p/>...so, you might implement your Eval(...) function as follows:
        /// <code>
        ///         public void Eval(final IEvolutionState state,
        ///             final int thread,
        ///             final GPData input,
        ///             final ADFStack stack,
        ///             final GPIndividual individual,
        ///             final IProblem problem
        ///         {
        ///             BooleanData dat = (BooleanData)input;
        ///             boolean x;
        /// 
        ///             // evaluate the first child
        ///             Children[0].eval(state,thread,input,stack,individual,problem);
        /// 
        ///             // store away its result
        ///             x = dat.result;
        /// 
        ///             // evaluate the second child
        ///             Children[1].eval(state,thread,input,stack,individual,problem);
        /// 
        ///             // return (in input) the result of the two ANDed
        /// 
        ///             dat.result = dat.result && x;
        ///             return;
        ///         }
        /// </code>
        /// </summary>		
        public abstract void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem);

        #endregion // Nodes

        #endregion // Tree

        #endregion // Operations
        #region Cloning

        public virtual GPNode LightClone()
        {
            try
            {
                var node = (GPNode) (MemberwiseClone());
                var len = Children.Length;
                // we'll share arrays -- probably just using GPNodeConstraints.ZeroChildren anyway
                node.Children = len == 0 ? Children : new GPNode[len];
                return node;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cloning Error!", ex);
            } // never happens
        }
        
        /// <summary>
        /// Deep-clones the tree rooted at this node, and returns the entire
        /// copied tree.  The result has everything set except for the root
        /// node's Parent and ArgPosition.  This method is identical to
        /// cloneReplacing for historical reasons, except that it returns
        /// the object as an Object, not a GPNode. 
        /// </summary>		
        public virtual object Clone()
        {
            var newnode = LightClone();
            for (var x = 0; x < Children.Length; x++)
            {
                newnode.Children[x] = (GPNode)Children[x].Clone();
                // if you think about it, the following CAN'T be implemented by
                // the children's clone method.  So it's set here.
                newnode.Children[x].Parent = newnode;
                newnode.Children[x].ArgPosition = x;
            }
            return newnode;
        }
        
        /// <summary>
        /// Deep-clones the tree rooted at this node, and returns the entire
        /// copied tree.  If the node oldSubtree is located somewhere in this
        /// tree, then its subtree is replaced with a deep-cloned copy of
        /// newSubtree.  The result has everything set except for the root
        /// node's Parent and ArgPosition. 
        /// </summary>		
        public GPNode CloneReplacing(GPNode newSubtree, GPNode oldSubtree)
        {
            if (this == oldSubtree) return (GPNode)newSubtree.Clone();

            var newnode = LightClone();
            for (var x = 0; x < Children.Length; x++)
            {
                newnode.Children[x] = Children[x].CloneReplacing(newSubtree, oldSubtree);
                // if you think about it, the following CAN'T be implemented by
                // the Children's clone method.  So it's set here.
                newnode.Children[x].Parent = newnode;
                newnode.Children[x].ArgPosition = x;
            }
            return newnode;
        }		
        
        /// <summary>
        /// Deep-clones the tree rooted at this node, and returns the entire
        /// copied tree.  If the node oldSubtree is located somewhere in this
        /// tree, then its subtree is replaced with
        /// newSubtree (<i>not</i> a copy of newSubtree).  
        /// The result has everything set except for the root
        /// node's Parent and ArgPosition. 
        /// </summary>		
        public GPNode CloneReplacingNoSubclone(GPNode newSubtree, GPNode oldSubtree)
        {
            if (this == oldSubtree) return newSubtree;

            var newnode = LightClone();
            for (var x = 0; x < Children.Length; x++)
            {
                newnode.Children[x] = Children[x].CloneReplacingNoSubclone(newSubtree, oldSubtree);
                // if you think about it, the following CAN'T be implemented by
                // the Children's clone method.  So it's set here.
                newnode.Children[x].Parent = newnode;
                newnode.Children[x].ArgPosition = x;
            }
            return newnode;
        }
                   
        /// <summary>
        /// Deep-clones the tree rooted at this node, and returns the entire
        /// copied tree.  If a node in oldSubtrees is located somewhere in this
        /// tree, then its subtree is replaced with a deep-cloned copy of the
        /// subtree rooted at its equivalent number in 
        /// newSubtrees.  The result has everything set except for the root
        /// node's Parent and ArgPosition. 
        /// </summary>		
        public GPNode CloneReplacing(GPNode[] newSubtrees, GPNode[] oldSubtrees)
        {
            // am I a candidate?
            var candidate = - 1;
            for (var x = 0; x < oldSubtrees.Length; x++)
                if (this == oldSubtrees[x])
                {
                    candidate = x; 
                    break;
                }
            
            if (candidate >= 0)
                return newSubtrees[candidate].CloneReplacing(newSubtrees, oldSubtrees);

            var newnode = LightClone();
            for (var x = 0; x < Children.Length; x++)
            {
                newnode.Children[x] = Children[x].CloneReplacing(newSubtrees, oldSubtrees);
                // if you think about it, the following CAN'T be implemented by
                // the Children's clone method.  So it's set here.
                newnode.Children[x].Parent = newnode;
                newnode.Children[x].ArgPosition = x;
            }
            return newnode;
        }
                
        /// <summary>
        /// Clones a new subtree, but with the single node oldNode 
        /// (which may or may not be in the subtree) 
        /// replaced with a newNode (not a clone of newNode).  
        /// These nodes should be
        /// type-compatible both in argument and return types, and should have
        /// the same number of arguments obviously.  This function will <i>not</i>
        /// check for this, and if they are not the result is undefined. 
        /// </summary>		
        public GPNode CloneReplacingAtomic(GPNode newNode, GPNode oldNode)
        {
            int numArgs;
            GPNode currNode;

            if (this == oldNode)
            {
                numArgs = Math.Max(newNode.Children.Length, Children.Length);
                currNode = newNode;
            }
            else
            {
                numArgs = Children.Length;
                currNode = LightClone();
            }
            
            // populate
            for (var x = 0; x < numArgs; x++)
            {
                currNode.Children[x] = Children[x].CloneReplacingAtomic(newNode, oldNode);
                // if you think about it, the following CAN'T be implemented by
                // the Children's clone method.  So it's set here.
                currNode.Children[x].Parent = currNode;
                currNode.Children[x].ArgPosition = x;
            }
            return currNode;
        }		
        
        /// <summary>
        /// Clones a new subtree, but with each node in oldNodes[] respectively
        /// (which may or may not be in the subtree) replaced with
        /// the equivalent
        /// nodes in newNodes[] (and not clones).  
        /// The length of oldNodes[] and newNodes[] should
        /// be the same of course.  These nodes should be
        /// type-compatible both in argument and return types, and should have
        /// the same number of arguments obviously.  This function will <i>not</i>
        /// check for this, and if they are not the result is undefined. 
        /// </summary>		
        public GPNode CloneReplacingAtomic(GPNode[] newNodes, GPNode[] oldNodes)
        {
            int numArgs;
            GPNode currNode;
            var found = - 1;
            
            for (var x = 0; x < newNodes.Length; x++)
            {
                if (this != oldNodes[x]) continue;
                found = x;
                break;
            }
            
            if (found > -1)
            {
                numArgs = Math.Max(newNodes[found].Children.Length, Children.Length);
                currNode = newNodes[found];
            }
            else
            {
                numArgs = Children.Length;
                currNode = LightClone();
            }
            
            // populate
            for (var x = 0; x < numArgs; x++)
            {
                currNode.Children[x] = Children[x].CloneReplacingAtomic(newNodes, oldNodes);
                // if you think about it, the following CAN'T be implemented by
                // the Children's clone method.  So it's set here.
                currNode.Children[x].Parent = currNode;
                currNode.Children[x].ArgPosition = x;
            }
            return currNode;
        }

        #endregion // Cloning
        #region ToString

        abstract public override string ToString();

        /// <summary>
        /// Returns a Lisp-like atom for the node which is intended for human
        /// consumption, and not to be read in again.  The default version
        /// just calls ToString(). 
        /// </summary>		
        public virtual string ToStringForHumans()
        {
            return ToString();
        }

        /// <summary>
        /// Returns a description of the node that can make it easy to identify
        /// in error messages (by default, at least its name and the tree it's found in).
        /// It's okay if this is a reasonably expensive procedure -- it won't be called a lot.  
        /// </summary>		
        public virtual string ToStringForError()
        {
            var rootp = (GPTree)RootParent();
            if (rootp != null)
            {
                var tnum = ((GPTree)(RootParent())).TreeNumber;
                return ToString() + (tnum == GPTree.NO_TREENUM ? "" : " in tree " + tnum);
            }
            return ToString();
        }

        /// <summary>
        /// A convenience function for identifying a GPNode in an error message 
        /// </summary>
        public virtual string ErrorInfo()
        {
            return "GPNode " + ToString() + " in the function set for tree " + ((GPTree)(RootParent())).TreeNumber;
        }

        #endregion // ToString
        #region IO

        #region Print

        /// <summary>
        /// Prints out a human-readable and Lisp-like atom for the node, 
        /// and returns the number of bytes in the string that you sent
        /// to the log (use Print(), not PrintLn()).  
        /// The default version gets the atom from ToStringForHumans(). 
        /// </summary>
        public int PrintNodeForHumans(IEvolutionState state, int log)
        {
            var n = ToStringForHumans();
            state.Output.Print(n, log);
            return n.Length;
        }

        /// <summary>
        /// Prints out a COMPUTER-readable and Lisp-like atom for the node, 
        /// which is also suitable for readNode to read, and returns the number 
        /// of bytes in the string that you sent to the log (use Print(), not PrintLn()).  
        /// The default version gets the atom from ToString(). O(1). 
        /// </summary>
        public int PrintNode(IEvolutionState state, int log)
        {
            var n = ToString();
            state.Output.Print(n, log);
            return n.Length;
        }

        /// <summary>
        /// Prints out a COMPUTER-readable and Lisp-like atom for the node, which
        /// is also suitable for ReadNode to read, and returns the number 
        /// of bytes in the string that you sent to the log (use Print(), not PrintLn()).  
        /// The default version gets the atom from ToString(). O(1). 
        /// </summary>		
        public virtual int PrintNode(IEvolutionState state, StreamWriter writer)
        {
            var n = ToString();
            writer.Write(n);
            return n.Length;
        }

        /// <summary>
        /// Prints out the tree on a single line, with no ending \n, in a fashion that can
        /// be read in later by computer. O(n).  
        /// You should call this method with printbytes == 0. 
        /// </summary>
        public int PrintRootedTree(IEvolutionState state, int log, int printbytes)
        {
            if (Children.Length > 0)
            {
                state.Output.Print(" (", log); printbytes += 2;
            }
            else
            {
                state.Output.Print(" ", log); printbytes += 1;
            }
            printbytes += PrintNode(state, log);

            printbytes = Children.Aggregate(printbytes, (current, t) => t.PrintRootedTree(state, log, current));

            if (Children.Length > 0)
            {
                state.Output.Print(")", log); printbytes += 1;
            }
            return printbytes;
        }

        /// <summary>
        /// Prints out the tree on a single line, with no ending \n, in a fashion that can
        /// be read in later by computer. O(n).  Returns the number of bytes printed.
        /// You should call this method with printbytes == 0. 
        /// </summary>		
        public virtual int PrintRootedTree(IEvolutionState state, StreamWriter writer, int printbytes)
        {
            if (Children.Length > 0)
            {
                writer.Write(" (");
                printbytes += 2;
            }
            else
            {
                writer.Write(" ");
                printbytes += 1;
            }

            printbytes += PrintNode(state, writer);

            printbytes = Children.Aggregate(printbytes, (current, t) => t.PrintRootedTree(state, writer, current));

            if (Children.Length > 0)
            {
                writer.Write(")");
                printbytes += 1;
            }
            return printbytes;
        }

        /// <summary>
        /// Prints out the tree in a readable Lisp-like multi-line fashion. O(n).  
        /// You should call this method with tablevel and printbytes == 0.  
        /// No ending '\n' is printed.  
        /// </summary>
        public int PrintRootedTreeForHumans(IEvolutionState state, int log, int tablevel, int printbytes)
        {
            if (printbytes > MAXPRINTBYTES)
            {
                state.Output.Print("\n", log);
                tablevel++;
                printbytes = 0;
                for (var x = 0; x < tablevel; x++)
                    state.Output.Print(GPNODEPRINTTAB, log);
            }

            if (Children.Length > 0)
            {
                state.Output.Print(" (", log);
                printbytes += 2;
            }
            else
            {
                state.Output.Print(" ", log);
                printbytes += 1;
            }
            printbytes += PrintNodeForHumans(state, log);

            printbytes = Children.Aggregate(printbytes, (current, t) => t.PrintRootedTreeForHumans(state, log, tablevel, current));

            if (Children.Length > 0)
            {
                state.Output.Print(")", log);
                printbytes += 1;
            }
            return printbytes;
        }

        #region Graphviz

        /// <summary>
        /// Produces the Graphviz code for a Graphviz tree of the subtree rooted at this node.
        /// For this to work, the output of ToString() must not contain a double-quote. 
        /// Note that this isn't particularly efficient and should only be used to generate
        /// occasional trees for display, not for storing individuals or sending them over networks.
        /// </summary>
        /// <returns></returns>
        public string MakeGraphvizTree()
        {
            return "digraph g {\nnode [shape=rectangle];\n" + MakeGraphvizSubtree("n") + "}\n";
        }

        /// <summary>
        /// Produces the inner code for a graphviz subtree.  Called from makeGraphvizTree(). 
        /// Note that this isn't particularly efficient and should only be used to generate
        /// occasional trees for display, not for storing individuals or sending them over networks.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected string MakeGraphvizSubtree(string prefix)
        {
            var body = prefix + "[label = \"" + ToStringForHumans() + "\"];\n";
            for (var x = 0; x < Children.Length; x++)
            {
                string newprefix;
                if (x < 10) newprefix = prefix + x;
                else newprefix = prefix + "n" + x; // to distinguish it

                body = body + Children[x].MakeGraphvizSubtree(newprefix);
                body = body + prefix + " -> " + newprefix + ";\n";
            }
            return body;
        }

        #endregion // Graphviz
        #region Latex

        /// <summary>
        /// Produces the LaTeX code for a LaTeX tree of the subtree rooted at this node, 
        /// using the <tt>epic</tt> and <tt>fancybox</tt> packages, as described in sections 
        /// 10.5.2 (page 307) and 10.1.3 (page 278) of <i>The LaTeX Companion</i>, respectively.  
        /// For this to work, the output of ToString() must not contain any weird latex characters, 
        /// notably { or } or % or \, unless you know what you're doing. 
        /// See the documentation for ec.gp.GPTree for information on how to take 
        /// this code snippet and insert it into your LaTeX file. 
        /// </summary>		
        public virtual string MakeLatexTree()
        {
            if (Children.Length == 0)
            {
                return "\\gpbox{" + ToStringForHumans() + "}";
            }

            var s = "\\begin{bundle}{\\gpbox{" + ToStringForHumans() + "}}";
            for (var x = 0; x < Children.Length; x++)
                s = s + "\\chunk{" + Children[x].MakeLatexTree() + "}";
            s = s + "\\end{bundle}";
            return s;
        }

        #endregion // Latex
        #region CTree

        /// <summary>
        /// Producess a String consisting of the tree in pseudo-C form, given that the Parent already will wrap the
        /// expression in Parentheses (or not).  In pseudo-C form, functions with one child are printed out as a(b), 
        /// functions with more than two Children are printed out as a(b, c, d, ...), and functions with exactly two
        /// Children are either printed as a(b, c) or in operator form as (b a c) -- for example, (b * c).  Whether
        /// or not to do this depends on the setting of <tt>useOperatorForm</tt>.  Additionally, terminals will be
        /// printed out either in variable form -- a -- or in zero-argument function form -- a() -- depending on
        /// the setting of <tt>PrintTerminalsAsVariables</tt>.
        /// </summary>
        public virtual string MakeCTree(bool parentMadeParens, bool printTerminalsAsVariables, bool useOperatorForm)
        {
            if (Children.Length == 0)
            {
                return (printTerminalsAsVariables ? ToStringForHumans() : ToStringForHumans() + "()");
            }
            if (Children.Length == 1)
            {
                return ToStringForHumans() + "(" + Children[0].MakeCTree(true, printTerminalsAsVariables, useOperatorForm) + ")";
            }
            if (Children.Length == 2 && useOperatorForm)
            {
                return (parentMadeParens ? "" : "(")
                    + Children[0].MakeCTree(false, printTerminalsAsVariables, true) + " " + ToStringForHumans() + " "
                    + Children[1].MakeCTree(false, printTerminalsAsVariables, true) + (parentMadeParens ? "" : ")");
            }
            var s = ToStringForHumans() + "(" + Children[0].MakeCTree(true, printTerminalsAsVariables, useOperatorForm);
            for (var x = 1; x < Children.Length; x++)
                s = s + ", " + Children[x].MakeCTree(true, printTerminalsAsVariables, useOperatorForm);
            return s + ")";
        }

        #endregion // CTree
        #region Lisp

        private StringBuilder MakeLispTree(StringBuilder buf)
        {
            if (Children.Length == 0)
                return buf.Append(ToStringForHumans());

            buf.Append("(");
            buf.Append(ToStringForHumans());
            // String s = "(" + toStringForHumans();
            for (var x = 0; x < Children.Length; x++)
            {
                buf.Append(" ");
                Children[x].MakeLispTree(buf);
                //s = s + " " + children[x].makeLispTree();
            }
            buf.Append(")");
            return buf;
            //return s + ")";
        }

        /// <summary>
        /// Produces a tree for human consumption in Lisp form similar to that generated by printTreeForHumans().
        /// Note that this isn't particularly efficient and should only be used to generate
        /// occasional trees for display, not for storing individuals or sending them over networks.
        /// </summary>
        public String MakeLispTree()
        {
            //if (Children.Length == 0)
            //    return ToStringForHumans();

            //var s = "(" + ToStringForHumans();
            //s = Children.Aggregate(s, (current, t) => current + " " + t.MakeLispTree());
            //return s + ")";

            return MakeLispTree(new StringBuilder()).ToString();
        }

        #endregion // Lisp

        #endregion // Print
        #region Binary

        public virtual void WriteRootedTree(IEvolutionState state, GPType expectedType, GPFunctionSet funcs, BinaryWriter writer)
        {
            writer.Write(Children.Length);
            var isTerminal = (Children.Length == 0);

            // identify the node
            var gpfi = isTerminal ? funcs.Terminals[expectedType.Type] : funcs.Nonterminals[expectedType.Type];

            var index = 0;
            for (; index < gpfi.Length; index++)
                if (gpfi[index].NodeEquivalentTo(this))
                    break;

            if (index == gpfi.Length)
            // uh oh
            {
                state.Output.Fatal("No node in the function set can be found that is equivalent to the node "
                    + this + " when performing WriteRootedTree(EvolutionState, GPType, GPFunctionSet, DataOutput).");
            }
            writer.Write(index); // what kind of node it is
            WriteNode(state, writer);

            var initializer = ((GPInitializer)state.Initializer);
            for (var x = 0; x < Children.Length; x++)
                Children[x].WriteRootedTree(state, Constraints(initializer).ChildTypes[x], funcs, writer);
        }

        public static GPNode ReadRootedTree(IEvolutionState state, BinaryReader reader,
                        GPType expectedType, GPFunctionSet funcs, IGPNodeParent parent, int argPosition)
        {
            var len = reader.ReadInt32(); // num Children
            var index = reader.ReadInt32(); // index in function set

            var isTerminal = (len == 0);
            var gpfi = isTerminal
                ? funcs.Terminals[expectedType.Type]
                : funcs.Nonterminals[expectedType.Type];

            var node = gpfi[index].LightClone();

            if (node.Children == null || node.Children.Length != len)
            {
                state.Output.Fatal("Mismatch in number of Children (" + len + ") when performing readRootedTree(...DataInput...) on " + node);
            }

            node.Parent = parent;
            node.ArgPosition = argPosition;
            node.ReadNode(state, reader);

            // do its Children
            var initializer = ((GPInitializer)state.Initializer);
            for (var x = 0; x < node.Children.Length; x++)
                node.Children[x] = ReadRootedTree(state, reader, node.Constraints(initializer).ChildTypes[x], funcs, node, x);

            return node;
        }

        #endregion // Binary
        #region From DecodeReturn

        /// <summary>
        /// Reads the node and its Children from the form printed out by printRootedTree. 
        /// </summary>
        public static GPNode ReadRootedTree(int linenumber, DecodeReturn dret, GPType expectedType,
            GPFunctionSet funcs, IGPNodeParent parent, int argPosition, IEvolutionState state)
        {
            const char REPLACEMENT_CHAR = '@';

            // eliminate whitespace if any
            var isTerminal = true;
            var len = dret.Data.Length;
            for (; dret.Pos < len && Char.IsWhiteSpace(dret.Data[dret.Pos]); dret.Pos++)
                ;

            // if I'm out of space, complain

            if (dret.Pos >= len)
                state.Output.Fatal("Reading line " + linenumber + ": "
                    + "Premature end of tree structure -- did you forget a close-Parenthesis?\nThe tree was" + dret.Data);

            // if I've found a ')', complain
            if (dret.Data[dret.Pos] == ')')
            {
                var sb = new StringBuilder(dret.Data);
                sb[dret.Pos] = REPLACEMENT_CHAR;
                dret.Data = sb.ToString();
                state.Output.Fatal("Reading line " + linenumber + ": "
                    + "Premature ')' which I have replaced with a '" + REPLACEMENT_CHAR + "', in tree:\n" + dret.Data);
            }

            // determine if I'm a terminal or not
            if (dret.Data[dret.Pos] == '(')
            {
                isTerminal = false;
                dret.Pos++;
                // strip following whitespace
                for (; dret.Pos < len && Char.IsWhiteSpace(dret.Data[dret.Pos]); dret.Pos++)
                    ;
            }

            // check again if I'm out of space

            if (dret.Pos >= len)
                state.Output.Fatal("Reading line " + linenumber + ": "
                    + "Premature end of tree structure -- did you forget a close-Parenthesis?\nThe tree was" + dret.Data);

            // check again if I found a ')'
            if (dret.Data[dret.Pos] == ')')
            {
                var sb = new StringBuilder(dret.Data);
                sb[dret.Pos] = REPLACEMENT_CHAR;
                dret.Data = sb.ToString();
                state.Output.Fatal("Reading line " + linenumber + ": "
                    + "Premature ')' which I have replaced with a '" + REPLACEMENT_CHAR + "', in tree:\n" + dret.Data);
            }


            // find that node!
            var gpfi = isTerminal
                ? funcs.Terminals[expectedType.Type]
                : funcs.Nonterminals[expectedType.Type];

            GPNode node = null;
            foreach (var t in gpfi.Where(t => (node = t.ReadNode(dret)) != null))
            {
                break;
            }

            // did I find one?

            if (node == null)
            {
                if (dret.Pos != 0)
                {
                    var sb = new StringBuilder(dret.Data);
                    sb[dret.Pos] = REPLACEMENT_CHAR;
                    dret.Data = sb.ToString();
                }
                else
                    dret.Data = "" + REPLACEMENT_CHAR + dret.Data;

                var msg = "Reading line " + linenumber + ": " 
                        + "I came across a symbol which I could not match up with a type-valid node.\n"
                        + "I have replaced the position immediately before the node in question with a '"
                        + REPLACEMENT_CHAR + "':\n" + dret.Data;

                state.Output.Fatal(msg);
                throw new InvalidOperationException(msg);
            }

            node.Parent = parent;
            node.ArgPosition = argPosition;
            var initializer = ((GPInitializer)state.Initializer);

            // do its Children
            for (var x = 0; x < node.Children.Length; x++)
                node.Children[x] = ReadRootedTree(linenumber, dret, node.Constraints(initializer).ChildTypes[x], funcs, node, x, state);

            // if I'm not a terminal, look for a ')'

            if (!isTerminal)
            {
                // clear whitespace
                for (; dret.Pos < len && Char.IsWhiteSpace(dret.Data[dret.Pos]); dret.Pos++){ }

                if (dret.Pos >= len)
                    state.Output.Fatal("Reading line " + linenumber + ": "
                        + "Premature end of tree structure -- did you forget a close-Parenthesis?\nThe tree was" + dret.Data);

                if (dret.Data[dret.Pos] != ')')
                {
                    if (dret.Pos != 0)
                    {
                        var sb = new StringBuilder(dret.Data);
                        sb[dret.Pos] = REPLACEMENT_CHAR;
                        dret.Data = sb.ToString();
                    }
                    else
                    {
                        dret.Data = "" + REPLACEMENT_CHAR + dret.Data;
                    }
                    state.Output.Fatal("Reading line " + linenumber + ": "
                        + "A nonterminal node has too many arguments.  I have put a '"
                        + REPLACEMENT_CHAR + "' just before the offending argument.\n" + dret.Data);
                }
                else
                {
                    dret.Pos++; // get rid of the ')'
                }
            }

            // return the node
            return node;
        }

        #endregion // From DecodeReturn

        #region Write and Read Node

        /// <summary>
        /// Override this to write any additional node-specific information to <i>writer</i> besides: the number of arguments, 
        /// the specific node class, the Children, and the Parent.  The default version of this method does nothing. 
        /// </summary>
        public virtual void WriteNode(IEvolutionState state, BinaryWriter writer)
        {
            // do nothing
        }

        /// <summary>
        /// Override this to read any additional node-specific information from <i>reader</i> besides: the number of arguments,
        /// the specific node class, the Children, and the Parent.  The default version of this method does nothing. 
        /// </summary>
        public virtual void ReadNode(IEvolutionState state, BinaryReader reader)
        {
            // do nothing
        }

        /// <summary>
        /// Reads the node symbol,
        /// advancing the DecodeReturn to the first character in the string
        /// beyond the node symbol, and returns a new, empty GPNode of the
        /// appropriate class representing that symbol, else null if the
        /// node symbol is not of the correct type for your GPNode class. You may
        /// assume that initial whitespace has been eliminated.  Generally should
        /// be case-SENSITIVE, unlike in Lisp.  The default
        /// version usually works for "simple" function names, that is, not ERCs
        /// or other stuff where you have to encode the symbol. 
        /// </summary>
        public virtual GPNode ReadNode(DecodeReturn dret)
        {
            var len = dret.Data.Length;

            // get my name
            var str2 = ToString();
            var len2 = str2.Length;

            if (dret.Pos + len2 > len)
                // uh oh, not enough space
                return null;

            // check it out
            for (var x = 0; x < len2; x++)
                if (dret.Data[dret.Pos + x] != str2[x])
                    return null;

            // looks good!  Check to make sure that
            // the symbol's all there is
            if (dret.Data.Length > dret.Pos + len2)
            {
                var c = dret.Data[dret.Pos + len2];
                if (!Char.IsWhiteSpace(c) && c != ')' && c != '(')
                    // uh oh
                    return null;
            }

            // we're happy!
            dret.Pos += len2;
            return LightClone();
        }

        #endregion // Write and Read Node

        #endregion // IO
    }
}