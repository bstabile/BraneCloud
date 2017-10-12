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
using System.Numerics;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.GP.Build
{
    /// <summary>
    /// Uniform implements the algorithm described in 
    /// <p/>Bohm, Walter and Andreas Geyer-Schulz. 1996. "Exact Uniform Initialization for Genetic Programming".  
    /// In <i>Foundations of Genetic Algorithms IV,</i> Richard Belew and Michael Vose, eds.  Morgan Kaufmann.  379-407. (ISBN 1-55860-460-X) 
    /// <p/> The user-provided requested tree size is either provided directly to the Uniform algorithm, 
    /// or if the size is NOSIZEGIVEN, then Uniform will pick one at random from the GPNodeBuilder probability 
    /// distribution system (using either max-depth and min-depth, or using num-sizes).  
    /// <p/>Further, if the user sets the <tt>true-dist</tt> parameter, the Uniform will ignore the user's specified 
    /// probability distribution and instead pick from a distribution between the minimum size and the maximum size 
    /// the user specified, where the sizes are distributed according to the <i>actual</i> number of trees that can 
    /// be created with that size.  Since many more trees of size 10 than size 3 can be created, for example, size 10 
    /// will be picked that much more often.
    /// <p/>Uniform also prints out the actual number of trees that exist for a given size, return type, and function set.  
    /// As if this were useful to you.  :-)
    /// <p/> The algorithm, which is quite complex, is described in pseudocode below.  Basically what the algorithm does is this:
    /// <ol>
    /// <li/> For each function set and return type, determine the number of trees of each size which exist for that 
    /// function set and tree type.  Also determine all the permutations of tree sizes among children of a given node.  
    /// All this can be done with dynamic programming.  Do this just once offline, after the function sets are loaded. 
    /// <li/> Using these tables, construct distributions of choices of tree size, child tree size permutations, etc.
    /// <li/> When you need to create a tree, pick a size, then use the distriutions to recursively create the tree (top-down).
    /// </ol>
    /// <p/> <b>Dealing with Zero Distributions</b>
    /// <p/> Some domains have NO tree of a certain size.  For example, 
    /// Artificial Ant's function set can make NO trees of size 2.
    /// What happens when we're asked to make a tree of (invalid) size 2 in
    /// Artificial Ant then?  Uniform presently handles it as follows:
    /// <ol><li/> If the system specifically requests a given size that's invalid, Uniform will 
    /// look for the next larger size which is valid.  If it can't find any,
    /// it will then look for the next smaller size which is valid.
    /// <li/> If a random choice yields a given size that's invalid,
    /// Uniform will pick again.
    /// <li/> If there is *no* valid size for a given return type, which probably indicates
    /// an error, Uniform will halt and complain.
    /// </ol>
    /// <h3>Pseudocode:</h3>
    /// <pre>
    /// Func NumTreesOfType(type,size)
    /// If NUMTREESOFTYPE[type,size] not defined,       // memoize
    /// N[type] = all nodes compatible with type
    /// NUMTREESOFTYPE[type,size] = Sum(n in N[type], NumTreesRootedByNode(n,size))
    /// return NUMTREESOFTYPE[type,size]
    /// 
    /// Func NumTreesRootedByNode(node,size)
    /// If NUMTREESROOTEDBYNODE[node,size] not defined,   // memoize
    /// count = 0
    /// left = size - 1
    /// If node.Children.length = 0 and left = 0  // a valid terminal
    /// count = 1
    /// Else if node.Children.length &lt;= left  // a valid nonterminal
    /// For s is 1 to left inclusive  // yeah, that allows some illegal stuff, it gets set to 0
    /// count += NumChildPermutations(node,s,left,0)
    /// NUMTREESROOTEDBYNODE[node,size] = count
    /// return NUMTREESROOTEBYNODE[node,size]
    /// 
    /// 
    /// Func NumChildPermutations(parent,size,outof,pickchild)
    /// // parent is our parent node
    /// // size is the size of pickchild's tree that we're considering
    /// // pickchild is the child we're considering
    /// // outof is the total number of remaining nodes (including size) yet to fill
    /// If NUMCHILDPERMUTATIONS[parent,size,outof,pickchild] is not defined,        // memoize
    /// count = 0
    /// if pickchild = parent.Children.length - 1        and outof==size        // our last child, outof must be size
    /// count = NumTreesOfType(parent.Children[pickchild].Type,size)
    /// else if pickchild &lt; parent.Children.length - 1 and 
    /// outof-size >= (parent.Children.length - pickchild-1)    // maybe we can fill with terminals
    /// cval = NumTreesOfType(parent.Children[pickchild].Type,size)
    /// tot = 0
    /// For s is 1 to outof-size // some illegal stuff, it gets set to 0
    /// tot += NumChildPermutations(parent,s,outof-size,pickchild+1)
    /// count = cval * tot
    /// NUMCHILDPERMUTATIONS [parent,size,outof,pickchild] = count            
    /// return NUMCHILDPERMUTATIONS[parent,size,outof,pickchild]
    /// 
    /// 
    /// For each type type, size size
    /// ROOT_D[type,size] = probability distribution of nodes of type and size, derived from
    /// NUMTREESOFTYPE[type,size], our node list, and NUMTREESROOTEDBYNODE[node,size]
    /// 
    /// For each parent,outof,pickchild
    /// CHILD_D[parent,outof,pickchild] = probability distribution of tree sizes, derived from
    /// NUMCHILDPERMUTATIONS[parent,size,outof,pickchild]
    /// 
    /// Func FillNodeWithChildren(parent,pickchild,outof)
    /// If pickchild = parent.Children.length - 1               // last child
    /// Fill parent.Children[pickchild] with CreateTreeOfType(parent.Children[pickchild].Type,outof)
    /// Else choose size from CHILD_D[parent,outof,pickchild]
    /// Fill parent.pickchildren[pickchild] with CreateTreeOfType(parent.Children[pickchild].Type,size)
    /// FillNodeWithChildren(parent,pickchild+1,outof-size)
    /// return
    /// </pre>
    /// Func CreateTreeOfType(type,size)
    /// Choose node from ROOT_D[type,size]
    /// If size > 1
    /// FillNodeWithChildren(node,0,size-1)
    /// return node
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>true-dist</tt><br/>
    /// <font size="-1">bool= true or false (default)</font></td>
    /// <td valign="top">(should we use the true numbers of trees for each size as the distribution 
    /// for picking trees, as opposed to the user-specified distribution?)</td></tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.build.Uniform")]
    public class Uniform : GPNodeBuilder
    {
        #region Constants

        public const string P_UNIFORM = "uniform";
        public const string P_TRUEDISTRIBUTION = "true-dist";

        #endregion // Constants
        #region Static

        /// <summary>
        /// This "no-op" is a legacy from the original use of Java BigInteger types
        /// </summary>
        /// <param name="i">The decimal value to convert</param>
        /// <returns>a double that represents a probability</returns>
        private static double GetProb(BigInteger i)
        {
            if (i == BigInteger.Zero)
                return 0.0f;

            return (double)i;
        }

        #endregion // Static
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPBuildDefaults.ParamBase.Push(P_UNIFORM); }
        }

        /// <summary>
        /// Mapping of integers to function sets
        /// </summary>
        public GPFunctionSet[] FunctionSets { get; set; }

        /// <summary>
        /// Mapping of function sets to Integers
        /// </summary>
        public Hashtable FunctionSetsHash { get; set; }

        /// <summary>
        /// Mapping of GPNodes to Integers (thus to ints)
        /// </summary>
        public Hashtable FuncNodesHash { get; set; }

        /// <summary>
        /// Number of nodes
        /// </summary>
        public int NumFuncNodes { get; set; }

        /// <summary>
        /// Max arity of any node
        /// </summary>
        public int MaxArity { get; set; }

        /// <summary>
        /// Maximum size of nodes computed
        /// </summary>
        public int MaxTreeSize { get; set; }

        /// <summary>
        /// True size distributions
        /// </summary>
        public BigInteger[][][] TrueSizesBigInt { get; set; }

        public double[][][] TrueSizes { get; set; }

        /// <summary>
        /// Do we use the true distributions to pick tree sizes?
        /// </summary>
        public bool UseTrueDistribution { get; set; }

        // Sun in its infinite wisdom (what idiots) decided to make
        // BigInteger IMMUTABLE.  There is a MutableBigInteger, but it's not
        // public!  And Sun only caches the first 16 positive and 16 negative
        // integer constants, not exactly that useful for us.  As a result, we'll
        // be making a dang lot of BigIntegers here.  Garbage-collection hell.  :-(
        // ...well, it's not all that slow really.

        public BigInteger[/*FunctionSet*/][/*type*/][/*size*/] NUMTREESOFTYPE { get; set; }
        public BigInteger[/*FunctionSet*/][/*nodenum*/][/*size*/] NUMTREESROOTEDBYNODE { get; set; }
        public BigInteger[/*FunctionSet*/][/*parentnodenum*/][/*size*/][/*outof*/][/*pickchild*/] NUMCHILDPERMUTATIONS { get; set; }

        /// <summary>
        /// Tables derived from the previous ones through some massaging.
        /// </summary>
        public UniformGPNodeStorage[/*FunctionSet*/][/*type*/][/*size*/][/*the nodes*/] ROOT_D { get; set; }

        /// <summary>
        /// Is ROOT_D all zero for these values?
        /// </summary>
        public bool[/*FunctionSet*/][/*type*/][/*size*/] ROOT_D_ZERO { get; set; }

        public double[/*FunctionSet*/][/*type*/][/*outof*/][/*pickchild*/][/* the nodes*/] CHILD_D { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            
            var def = DefaultBase;
            
            // use true distributions? false is default
            UseTrueDistribution = state.Parameters.GetBoolean(paramBase.Push(P_TRUEDISTRIBUTION), def.Push(P_TRUEDISTRIBUTION), false);
            
            if (MinSize > 0)
            // we're using maxSize and minSize
                MaxTreeSize = MaxSize;
            else if (SizeDistribution != null)
                MaxTreeSize = SizeDistribution.Length;
            else
                state.Output.Fatal("Uniform is used for the GP node builder, but no distribution was specified." 
                    + "  You must specify either a min/max size, or a full size distribution.", 
                    paramBase.Push(P_MINSIZE), def.Push(P_MINSIZE));
            // preprocess offline
            Preprocess(state, MaxTreeSize);
        }

        #endregion // Setup
        #region Operations

        public virtual int PickSize(IEvolutionState state, int thread, int functionset, int type)
        {
            if (UseTrueDistribution)
                return RandomChoice.PickFromDistribution(TrueSizes[functionset][type], state.Random[thread].NextDouble());

            return base.PickSize(state, thread);
        }
        
        public virtual void  Preprocess(IEvolutionState state, int maxTreeSize)
        {
            state.Output.Message("Determining Tree Sizes");
            
            MaxTreeSize = maxTreeSize;
            
            var functionSetRepository = ((GPInitializer) state.Initializer).FunctionSetRepository;
            
            // Put each function set into the arrays
            FunctionSets = new GPFunctionSet[functionSetRepository.Count];
            FunctionSetsHash = Hashtable.Synchronized(new Hashtable());
            var e = functionSetRepository.Values.GetEnumerator();
            var count = 0;
            while (e.MoveNext())
            {
                var funcs = (GPFunctionSet) e.Current;
                FunctionSetsHash[funcs] = count;
                FunctionSets[count++] = funcs;
            }
            
            // For each function set, assign each GPNode to a unique integer
            // so we can keep track of it (ick, this will be inefficient!)
            FuncNodesHash = Hashtable.Synchronized(new Hashtable());
            var t_nodes = Hashtable.Synchronized(new Hashtable());
            count = 0;
            MaxArity = 0;
            for (var x = 0; x < FunctionSets.Length; x++)
            {
                GPNode n;
                // hash all the nodes so we can remove duplicates
                for (var typ = 0; typ < FunctionSets[x].Nodes.Length; typ++)
                    for (var nod = 0; nod < FunctionSets[x].Nodes[typ].Length; nod++)
                        t_nodes[n = FunctionSets[x].Nodes[typ][nod]] = n;

                // rehash with Integers, yuck
                e = t_nodes.Values.GetEnumerator();
                GPNode tmpn;
                while (e.MoveNext())
                {
                    tmpn = (GPNode) e.Current;
                    if (MaxArity < tmpn.Children.Length)
                        MaxArity = tmpn.Children.Length;
                    if (!FuncNodesHash.ContainsKey(tmpn))  // don't remap the node; it'd make holes
                        FuncNodesHash[tmpn] = count++;
                }
            }
            
            NumFuncNodes = FuncNodesHash.Count;
            
            var initializer = (GPInitializer) state.Initializer;
            var numAtomicTypes = initializer.NumAtomicTypes;
            var numSetTypes = initializer.NumSetTypes;
            var functionSetsLength = FunctionSets.Length;
            var atomicPlusSetTypes = numAtomicTypes + numSetTypes;
            var maxTreeSizePlusOne = MaxTreeSize + 1;
            
            // set up the arrays

            // NUMTREESOFTYPE
            NUMTREESOFTYPE = TensorFactory.Create<BigInteger>(functionSetsLength, atomicPlusSetTypes, maxTreeSizePlusOne);

            // NUMTREESROOTEDBYNODE
            NUMTREESROOTEDBYNODE = TensorFactory.Create<BigInteger>(functionSetsLength, NumFuncNodes, maxTreeSizePlusOne);

            // NUMCHILDPERMUTATIONS
            NUMCHILDPERMUTATIONS = TensorFactory.Create<BigInteger>(functionSetsLength, 
                                                                    NumFuncNodes, 
                                                                    maxTreeSizePlusOne, 
                                                                    maxTreeSizePlusOne, 
                                                                    MaxArity);

            // ROOT_D
            ROOT_D = TensorFactory.CreateOpenEnded<UniformGPNodeStorage>(functionSetsLength, 
                                                                         atomicPlusSetTypes, 
                                                                         maxTreeSizePlusOne); // 4D OpenEnded

            // ROOT_D_ZERO
            ROOT_D_ZERO = TensorFactory.Create<bool>(functionSetsLength, 
                                                     atomicPlusSetTypes, 
                                                     maxTreeSizePlusOne);
            
            // CHILD_D
            CHILD_D = TensorFactory.CreateOpenEnded<double>(functionSetsLength, 
                                                            NumFuncNodes, 
                                                            maxTreeSizePlusOne, 
                                                            maxTreeSizePlusOne); // 5D OpenEnded

            var types = ((GPInitializer) (state.Initializer)).Types;

            // _TrueSizesBigInt
            TrueSizesBigInt = TensorFactory.Create<BigInteger>(functionSetsLength, 
                                                          atomicPlusSetTypes, 
                                                          maxTreeSizePlusOne);

            // Go through each function set and determine numbers
            // (this will take quite a while!  Thankfully it's offline)

            for (var x = 0; x < FunctionSets.Length; x++)
                for (var y = 0; y < numAtomicTypes + numSetTypes; y++)
                    for (var z = 1; z <= MaxTreeSize; z++)
                        state.Output.Message("FunctionSet: " + FunctionSets[x].Name + ", Type: " + types[y].Name 
                            + ", Size: " + z + " num: " + (TrueSizesBigInt[x][y][z] = NumTreesOfType(initializer, x, y, z)));
            
            state.Output.Message("Compiling Distributions");
            
            TrueSizes = TensorFactory.Create<double>(functionSetsLength, 
                                                     atomicPlusSetTypes, 
                                                     maxTreeSizePlusOne);

            // convert to doubles and organize distribution
            for (var x = 0; x < FunctionSets.Length; x++)
                for (var y = 0; y < numAtomicTypes + numSetTypes; y++)
                {
                    for (var z = 1; z <= MaxTreeSize; z++)
                        TrueSizes[x][y][z] = (double)TrueSizesBigInt[x][y][z]; // BRS : DOES THIS TRUNCATE ANYTHING ???

                    // and if this is all zero (a possibility) we should be forgiving (hence the 'true') -- I *think*
                    RandomChoice.OrganizeDistribution(TrueSizes[x][y], true);
                }
            
            // compute our percentages
            ComputePercentages();
        }
        
        /// <summary>
        /// hopefully this will get inlined
        /// </summary>
        public int IntForNode(GPNode node)
        {
            return (int) FuncNodesHash[node];
        }
        
        public virtual BigInteger NumTreesOfType(GPInitializer initializer, int functionset, int type, int size)
        {
            if (NUMTREESOFTYPE[functionset][type][size] == BigInteger.Zero)
            {
                var nodes = FunctionSets[functionset].Nodes[type];
                var count = BigInteger.Zero;
                for (var x = 0; x < nodes.Length; x++)
                    count = BigInteger.Add(count, NumTreesRootedByNode(initializer, functionset, nodes[x], size));
                NUMTREESOFTYPE[functionset][type][size] = count;
            }
            return NUMTREESOFTYPE[functionset][type][size];
        }
        
        public virtual BigInteger NumTreesRootedByNode(GPInitializer initializer, int functionset, GPNode node, int size)
        {
            if (NUMTREESROOTEDBYNODE[functionset][IntForNode(node)][size] == BigInteger.Zero)
            {
                var one = BigInteger.One;
                var count = BigInteger.Zero;
                var outof = size - 1;

                if (node.Children.Length == 0 && outof == 0)
                // a valid terminal
                    count = one;

                else if (node.Children.Length <= outof)
                // a valid nonterminal
                    for (var s = 1; s <= outof; s++)
                        count = BigInteger.Add(count, NumChildPermutations(initializer, functionset, node, s, outof, 0));

                //System.out.PrintLn("Node: " + node + " Size: " + size + " Count: " +count);
                NUMTREESROOTEDBYNODE[functionset][IntForNode(node)][size] = count;
            }
            return NUMTREESROOTEDBYNODE[functionset][IntForNode(node)][size];
        }
        
        public virtual BigInteger NumChildPermutations(GPInitializer initializer, int functionset, GPNode parent, int size, int outof, int pickchild)
        {
            if (NUMCHILDPERMUTATIONS[functionset][IntForNode(parent)][size][outof][pickchild] == 0)
            {
                var count = BigInteger.Zero;
                if (pickchild == parent.Children.Length - 1 && size == outof)
                    count = NumTreesOfType(initializer, functionset, parent.Constraints(initializer).ChildTypes[pickchild].Type, size);

                else if (pickchild < parent.Children.Length - 1 && outof - size >= (parent.Children.Length - pickchild - 1))
                {
                    var cval = NumTreesOfType(initializer, functionset, parent.Constraints(initializer).ChildTypes[pickchild].Type, size);
                    var tot = BigInteger.Zero;
                    for (var s = 1; s <= outof - size; s++)
                        tot = BigInteger.Add(tot, NumChildPermutations(initializer, functionset, parent, s, outof - size, pickchild + 1));
                    count = BigInteger.Multiply(cval, tot);
                }
                // out.PrintLn("Parent: " + parent + " Size: " + size + " OutOf: " + outof + 
                //       " PickChild: " + pickchild + " Count: " +count);
                NUMCHILDPERMUTATIONS[functionset][IntForNode(parent)][size][outof][pickchild] = count;
            }
            return NUMCHILDPERMUTATIONS[functionset][IntForNode(parent)][size][outof][pickchild];
        }
        
        public virtual void ComputePercentages()
        {
            // load ROOT_D
            for (var f = 0; f < NUMTREESOFTYPE.Length; f++)
                for (var t = 0; t < NUMTREESOFTYPE[f].Length; t++)
                    for (var s = 0; s < NUMTREESOFTYPE[f][t].Length; s++)
                    {
                        ROOT_D[f][t][s] = new UniformGPNodeStorage[FunctionSets[f].Nodes[t].Length];
                        for (var x = 0; x < ROOT_D[f][t][s].Length; x++)
                        {
                            ROOT_D[f][t][s][x] = new UniformGPNodeStorage();
                            ROOT_D[f][t][s][x].Node = FunctionSets[f].Nodes[t][x];
                            ROOT_D[f][t][s][x].Prob = GetProb(NUMTREESROOTEDBYNODE[f][IntForNode(ROOT_D[f][t][s][x].Node)][s]);
                        }
                        // organize the distribution
                        //System.out.PrintLn("Organizing " + f + " " + t + " " + s);
                        // check to see if it's all zeros
                        for (var x = 0; x < ROOT_D[f][t][s].Length; x++)
                            if (ROOT_D[f][t][s][x].Prob != 0.0)
                            {
                                // don't need to check for negatives here I believe
                                RandomChoice.OrganizeDistribution(ROOT_D[f][t][s], ROOT_D[f][t][s][0]);
                                ROOT_D_ZERO[f][t][s] = false;
                                break;
                            }
                            else
                            {
                                ROOT_D_ZERO[f][t][s] = true;
                            }
                    }
            
            // load CHILD_D
            for (var f = 0; f < NUMCHILDPERMUTATIONS.Length; f++)
                for (var p = 0; p < NUMCHILDPERMUTATIONS[f].Length; p++)
                    for (var o = 0; o < MaxTreeSize + 1; o++)
                        for (var c = 0; c < MaxArity; c++)
                        {
                            CHILD_D[f][p][o][c] = new double[o + 1];
                            for (var s = 0; s < CHILD_D[f][p][o][c].Length; s++)
                                CHILD_D[f][p][o][c][s] = GetProb(NUMCHILDPERMUTATIONS[f][p][s][o][c]);
                            // organize the distribution
                            //System.out.PrintLn("Organizing " + f + " " + p + " " + o + " " + c);
                            // check to see if it's all zeros
                            for (var x = 0; x < CHILD_D[f][p][o][c].Length; x++)
                                if (CHILD_D[f][p][o][c][x] != 0.0)
                                {
                                    // don't need to check for negatives here I believe
                                    RandomChoice.OrganizeDistribution(CHILD_D[f][p][o][c]);
                                    break;
                                }
                        }
        }
        
        internal virtual GPNode CreateTreeOfType(IEvolutionState state, int thread, GPInitializer initializer,
                                                        int functionset, int type, int size, IMersenneTwister mt)
        {
            //System.out.PrintLn("" + functionset + " " + type + " " + size);
            var choice = RandomChoice.PickFromDistribution(ROOT_D[functionset][type][size], ROOT_D[functionset][type][size][0], mt.NextDouble());
            var node = ROOT_D[functionset][type][size][choice].Node.LightClone();
            node.ResetNode(state, thread); // give ERCs a chance to randomize
            //System.out.PrintLn("Size: " + size + "Rooted: " + node);
            if (node.Children.Length == 0 && size != 1)
            // uh oh
            {
                Console.Out.WriteLine("Size: " + size + " Node: " + node);
                for (var x = 0; x < ROOT_D[functionset][type][size].Length; x++)
                {
                    Console.Out.WriteLine("" + x + (ROOT_D[functionset][type][size][x].Node) + " " + ROOT_D[functionset][type][size][x].Prob);
                }
            }
            if (size > 1)
            // nonterminal
                FillNodeWithChildren(state, thread, initializer, functionset, node,  ROOT_D[functionset][type][size][choice].Node, 0, size - 1, mt);
            return node;
        }
        
        internal virtual void FillNodeWithChildren(IEvolutionState state, int thread, GPInitializer initializer, int functionset, 
                                                        GPNode parent, GPNode parentc, int pickchild, int outof, IMersenneTwister mt)
        {
            if (pickchild == parent.Children.Length - 1)
            {
                parent.Children[pickchild] = CreateTreeOfType(state, thread, initializer, functionset, 
                                    parent.Constraints(initializer).ChildTypes[pickchild].Type, outof, mt);
            }
            else
            {
                var size = RandomChoice.PickFromDistribution(CHILD_D[functionset][IntForNode(parentc)][outof][pickchild], mt.NextDouble());

                parent.Children[pickchild] = CreateTreeOfType(state, thread, initializer, functionset, 
                                    parent.Constraints(initializer).ChildTypes[pickchild].Type, size, mt);

                FillNodeWithChildren(state, thread, initializer, functionset, parent, parentc, pickchild + 1, outof - size, mt);
            }
            parent.Children[pickchild].Parent = parent;
            parent.Children[pickchild].ArgPosition = (sbyte) pickchild;
        }
        
        public override GPNode NewRootedTree(IEvolutionState state, GPType type, int thread, IGPNodeParent parent, 
                                                            GPFunctionSet funcs, int argPosition, int requestedSize)
        {
            var initializer = ((GPInitializer) state.Initializer);
            
            if (requestedSize == NOSIZEGIVEN)
            // pick from the distribution
            {
                var BOUNDARY = 20; // if we try 20 times and fail, check to see if it's possible to succeed
                var bound = 0;
                
                var fset = (int) FunctionSetsHash[funcs];
                var siz = PickSize(state, thread, fset, type.Type);
                var typ = type.Type;
                
                // this code is confusing.  The idea is:
                // if the number of trees of our arbitrarily-picked size is zero, we try BOUNDARY
                // number of times to find a tree which will work, picking new sizes each
                // time.  If we still haven't found anything, we will continue to search
                // for a working tree only if we know for sure that one exists in the distribution.
                
                var checkState = false; // BRS : Can't call this "checked" as in ECJ because of the compiler keyword
                while (ROOT_D_ZERO[fset][typ][siz])
                {
                    if (++bound == BOUNDARY)
                    {
                        if (!checkState)
                        {
                            checkState = true;
                            for (var x = 0; x < ROOT_D_ZERO[fset][typ].Length; x++)
                                if (!ROOT_D_ZERO[fset][typ][x])
                                {
                                    goto check_brk; // BRS : TODO : This is different from ECJ "break check;" Is it equivalent?
                                } // found a non-zero
                            // uh oh, we're all zeroes
                            state.Output.Fatal("ec.gp.build.Uniform was asked to build a tree with functionset " + funcs 
                                + " rooted with type " + type + ", but cannot because for some reason there are no trees"
                                + " of any valid size (within the specified size range) which exist for this function set and type.");
                        }
check_brk: ;					
                    }
                    siz = PickSize(state, thread, fset, typ);
                }
                
                // okay, now we have a valid size.
                var n = CreateTreeOfType(state, thread, initializer, fset, typ, siz, state.Random[thread]);
                n.Parent = parent;
                n.ArgPosition = (sbyte) argPosition;
                return n;
            }
            else if (requestedSize < 1)
            {
                state.Output.Fatal("ec.gp.build.Uniform requested to build a tree, but a requested size was given that is < 1.");
                return null; // never happens
            }
            else
            {
                var fset = (int) FunctionSetsHash[funcs];
                var typ = type.Type;
                var siz = requestedSize;
                
                // if the number of trees of the requested size is zero, we first march up until we
                // find a tree size with non-zero numbers of trees.  Failing that, we march down to
                // find one.  If that still fails, we issue an error.  Otherwise we use the size
                // we discovered.
                
                if (ROOT_D_ZERO[fset][typ][siz])
                {
                    // march up
                    for (var x = siz + 1; x < ROOT_D_ZERO[fset][typ].Length; x++)
                        if (ROOT_D_ZERO[fset][typ][siz])
                        {
                            siz = x;
                            goto determineSize_brk;
                        }
                    // march down
                    for (var x = siz - 1; x >= 0; x--)
                        if (ROOT_D_ZERO[fset][typ][siz])
                        {
                            siz = x;
                            goto determineSize_brk;
                        }
                    // issue an error
                    state.Output.Fatal("ec.gp.build.Uniform was asked to build a tree with functionset " + funcs + " rooted with type " 
                        + type + ", and of size " + requestedSize + ", but cannot because for some reason there are no trees of any"
                        + " valid size (within the specified size range) which exist for this function set and type.");
                }

determineSize_brk: ;
                
                
                var n = CreateTreeOfType(state, thread, initializer, fset, typ, siz, state.Random[thread]);
                n.Parent = parent;
                n.ArgPosition = (sbyte) argPosition;
                return n;
            }
        }

        #endregion // Operations
    }
}