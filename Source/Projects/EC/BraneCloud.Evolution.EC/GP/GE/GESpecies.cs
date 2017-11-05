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
using System.Collections.Generic;
using System.IO;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.GP.GE
{
    /// <summary>
    /// GESpecies generates GPIndividuals from GEIndividuals through the application of a grammar parse graph
    /// computed by the GrammarParser.
    /// 
    /// <p/>GESpecies uses a <b>GrammarParser</b> to do its dirty work.  This parser's job is to take a grammar (in the form of a BufferedReader) 
    /// and convert it to a tree of GrammarNodes which define the parse graph of the grammar. The GESpecies then interprets his parse graph
    /// according to the values in the GEIndividual to produce the equivalent GPIndividual, which is then evaluated.
    /// 
    /// <p/>To do this, GESpecies relies on a subsidiary GPSpecies which defines the GPIndividual and various GPFunctionSets from which to
    /// build the parser.  This is a grand hack -- the GPSpecies does not know it's being used this way, and so we must provide various dummy
    /// parameters to keep the GPSpecies happy even though they'll never be used.
    /// 
    /// <p/>If you are daring, you can replace the GrammarParser with one of your own to customize the parse structure and grammar.
    ///
    /// <p/><b>ECJ's Default GE Grammar</b>  GE traditionally can use any grammar, and builds parse graphs from that.  For simplicity, and in order to
    /// remain as compatable as possible with ECJ's existing GP facilities (and GP tradition), ECJ only uses a single Lisp-like grammar which
    /// generates standard ECJ trees.  This doesn't lose much in generality as the grammar is quite genral.
    /// 
    /// <p/>The grammar assumes that expansion points are enclosed in &lt;> and functions are enclosed in ().  For example:
    /// 
    /// <p/><tt>
    /// # This is a comment
    /// &lt;prog> ::= &lt;op><br/>
    /// &lt;op> ::= (if-food-ahead &lt;op> &lt;op>)<br/>
    /// &lt;op> ::=  (progn2 &lt;op> &lt;op>)<br/>
    /// &lt;op> ::= (progn3 &lt;op> &lt;op> &lt;op>)<br/>
    /// &lt;op> ::= (left) | (right) | (move)<br/>
    /// </tt>
    /// 
    /// <p/>alternatively the grammar could also be writen in the following format:</p>
    /// 
    /// <p/><tt>
    /// &lt;prog> ::= &lt;op><br/>
    /// &lt;op> ::= (if-food-ahead &lt;op> &lt;op>) | (progn2 &lt;op> &lt;op>) | (progn3 &lt;op> &lt;op> &lt;op>) | (left) | (right) | (move)<br/>
    /// </tt>
    /// 
    /// <p/>Note that you can use several lines to define the same grammar rule: for example, <tt>&lt;op></tt> was defined by several lines when
    /// it could have consisted of several elements separated by vertical pipes ( <tt>|</tt> ).  Either way is fine, or a combination of both.
    /// 
    /// <p/>GPNodes are included in the grammar by using their name.  This includes ERCs, ADFs, ADMs, and ADFArguments, which should all work just fine.
    /// For example, since most ERC GPNodes are simply named "ERC", if you have only one ERC GPNode in your function set, you can just use <tt>(ERC)</tt>
    /// in your grammar.
    /// 
    /// <p/>Once the gammar file has been created and Setup has been run trees can the be created using the genome (chromosome) of a GEIndividual.
    /// A genome of an individual is an array of random integers each of which are one int long.  These numbers are used when a decision point 
    /// (a rule having more that one choice) is reached within the grammar.  Once a particular gene (index) in the genome has been used it will 
    /// not be used again (this may change) when creating the tree.
    /// 
    /// <p/>For example:<br/>
    /// number of chromosomes used = 0<br/>
    /// genome = {23, 654, 86}<br/>
    /// the current rule we are considering is &lt;op>.<br/>
    /// %lt;op> can map into one of the following: (if-food-ahead &lt;op> &lt;op>) | (progn2 &lt;op> &lt;op>) | (progn3 &lt;op> &lt;op> &lt;op>) 
    /// | (left) | (right) | (move)<br/>
    /// Since the rule &lt;op> has more than one choice that it can map to, we must consult the genome to decide which choice to take.  In this case
    /// the number of chromosomes used is 0 so genome[0] is used and number of chromosomes used is incremented.  Since values in the genome can
    /// be negitive values they are offset by 128 (max negitive of a int) giving us a value from 0-255.  A modulus is performed on this resulting 
    /// number by the number of choices present for the given rule.  In the above example since we are using genome[0] the resulting operation would 
    /// look like: 23+128=151, number of choices for &lt;op> = 6, 151%6=1 so we use choices[1] which is: (progn2 &lt;op> &lt;op>).  If all the genes
    /// in a genome are used and the tree is still incompete an invalid tree error is returned.
    /// 
    /// <p/>Each node in the tree is a GPNode and trees are constructed depth first.
    /// 
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.</i><tt>file</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(the file is where the rules of the gammar are stored)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base.</i><tt>gp-species</tt><br/>
    /// <font size="-1">classname, inherits and != ec.gp.GPSpecies</font></td>
    /// <td valign="top">(the GPSpecies subservient to the GESpecies)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base.</i><tt>parser</tt><br/>
    /// <font size="-1">classname, inherits and != ge.GrammarParser</font></td>
    /// <td valign="top">(the GrammarParser used by the GESpecies)</td></tr>
    /// </table>
     /// <p/><b>Default Base</b><br/>
    /// ge.GESpecies
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ge.GESpecies")]
    public class GESpecies : IntegerVectorSpecies
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_GESPECIES = "species";
        public const string P_FILE = "file";
        public const string P_GPSPECIES = "gp-species";
        public const string P_PARSER = "parser";
        public const string P_PASSES = "passes";
        public const string P_INITSCHEME = "init-scheme" ;

        /// <summary>
        /// Return value which denotes that the tree has grown too large.
        /// </summary>
        public const int BIG_TREE_ERROR = -1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase => GEDefaults.ParamBase.Push(P_GESPECIES);

        /// <summary>
        /// The GPSpecies subsidiary to GESpecies.
        /// </summary>
        public GPSpecies GPSpecies { get; set; }

        /// <summary>
        /// All the ERCs created so far, the ERCs are mapped as,
        /// "key --> list of ERC nodes", where the key = (genome[i] - minGene[i]);
        /// The ERCBank is "static", beacause we need one identical copy
        /// for all the individuals; Moreover, this copy may be sent to
        /// other sub-populations as well.
        /// </summary>
        public Hashtable ERCBank { get; set; }

        /// <summary>
        /// The parsed grammars.
        /// </summary>
        public GrammarRuleNode[] Grammar { get; set; }

        /** The number of passes permitted through the genome if we're wrapping.   Must be >= 1. */
        public int Passes { get; set; }

        public string InitScheme { get; set; } = "default";

        /// <summary>
        /// The prototypical parser used to parse the grammars.
        /// </summary>
        public GrammarParser ParserPrototype { get; set; }

        /// <summary>
        /// Parser for each grammar -- khaled.
        /// </summary>
        public GrammarParser[] GrammarParser { get; set; } = null;

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            var def = DefaultBase;

            var p = paramBase.Push(P_GPSPECIES);
            GPSpecies = (GPSpecies)(state.Parameters.GetInstanceForParameterEq(p, def.Push(P_GPSPECIES), typeof(GPSpecies)));
            GPSpecies.Setup(state, p);

            // check to make sure that our individual prototype is a GPIndividual
            if (!(I_Prototype is IntegerVectorIndividual))
            {
                state.Output.Fatal("The Individual class for the Species " + GetType().Name + " must be a subclass of ge.GEIndividual.", paramBase);
            }

            ERCBank = Hashtable.Synchronized(new Hashtable());

            // load the grammars, one per ADF tree
            var gpi = (GPIndividual)(GPSpecies.I_Prototype);
            var trees = gpi.Trees;
            var numGrammars = trees.Length;

            ParserPrototype = (GrammarParser)state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_PARSER), 
                def.Push(P_PARSER), 
                typeof(GrammarParser));

            Grammar = new GrammarRuleNode[numGrammars];
            for (var i = 0; i < numGrammars; i++)
            {
                p = paramBase.Push(P_FILE);
                def = DefaultBase;

                //var grammarFile = state.Parameters.GetFile(p, def.Push(P_FILE).Push("" + i));
                Stream grammarFile = state.Parameters.GetResource(p, def.Push(P_FILE).Push("" + i));

                if (grammarFile == null)
                {
                    state.Output.Fatal("Error retrieving grammar file(s): " + def + "." + P_FILE + "." + i + " is undefined.");
                }

                GPFunctionSet gpfs = trees[i].Constraints((GPInitializer)state.Initializer).FunctionSet;
                // now we need different parser object for each of the grammars,
                // why? see GrammarParser.java for details -- khaled
                GrammarParser[i] = (GrammarParser)ParserPrototype.Clone();
                StreamReader reader = new StreamReader(grammarFile);
                Grammar[i] = GrammarParser[i].ParseRules(state, reader, gpfs);

                // Enumerate the grammar tree -- khaled
                GrammarParser[i].EnumerateGrammarTree(Grammar[i]);
                // Generate the predictive parse table -- khaled
                GrammarParser[i].PopulatePredictiveParseTable(Grammar[i]);

                try
                {
                    reader.Close();
                }
                catch (IOException e)
                {
                    // do nothing
                }
            }
            // get the initialization scheme -- khaled
            InitScheme = state.Parameters.GetString(paramBase.Push(P_INITSCHEME), def.Push(P_INITSCHEME));
            if (InitScheme != null && InitScheme.Equals("sensible"))
                state.Output.WarnOnce("Using a \"hacked\" version of \"sensible initialization\"");
            else
                state.Output.WarnOnce("Using default GE initialization scheme");

            // setup the "passes" parameters
            int MAXIMUM_PASSES = 1024;

            Passes = state.Parameters.GetInt(paramBase.Push(P_PASSES), def.Push(P_PASSES), 1);
            if (Passes < 1 || Passes > MAXIMUM_PASSES)
                state.Output.Fatal("Number of allowed passes must be >= 1 and <="
                                   + MAXIMUM_PASSES + ", likely small, such as <= 16.",
                    paramBase.Push(P_PASSES), def.Push(P_PASSES));
            int oldpasses = Passes;
            Passes = NextPowerOfTwo(Passes);
            if (oldpasses != Passes)
                state.Output.Warning("Number of allowed passes must be a power of 2.  Bumping from "
                                     + oldpasses + " to " + Passes, paramBase.Push(P_PASSES), def.Push(P_PASSES));
        }

        private int NextPowerOfTwo(int v)
        {
            // if negative or 0, couldn't bump.
            // See http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        /**
         * This is an ugly hack to simulate the "Sensible Initialization",
         * First we create a GPIndividual, then reverse-map it to GEIndividuals,
         * We do not need to call IntegerVectorSpecies.newIndividual() since it is overriden
         * by the GPSpecies.newIndividual();
         *
         * Moreover, as in the case for non-identical representations (i,e, GP-GE island
         * models etc,), the grammar rules, tree constraints, ERC's etc, are supposed to be
         * identical across all islands, so we are using the same "gpspecies" inside this class.
         *
         * However, the identicality of the GPTree particulars like grammar, constraints, ADFs,
         * ERC's may not be universally true.
         */
        public Individual NewIndividual(IEvolutionState state, int thread)
        {
            GEIndividual gei = null;
            if (InitScheme != null && InitScheme.Equals("sensible"))
            {
                GPIndividual gpi = (GPIndividual)GPSpecies.NewIndividual(state, thread);
                gei = ReverseMap(state, gpi, thread);
            }
            else
            {
                gei = (GEIndividual)base.NewIndividual(state, thread);
                gei.Species = this;
            }
            return gei;
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Creates all of an individual's trees
        /// </summary>
        /// <param name="state">Evolution state</param>
        /// <param name="ind">ind the GEIndividual</param>
        /// <param name="trees">array of trees for the individual</param>
        /// <param name="threadnum">thread number</param>
        /// <param name="ercMapsForFancyPrint"></param>
        /// <returns>Number of chromosomes consumed</returns>
        public int MakeTrees(IEvolutionState state, GEIndividual ind, GPTree[] trees, int threadnum, IDictionary<int, GPNode> ercMapsForFancyPrint)
        {
            int[] genome = ind.genome;
            var position = 0;

            // We start with one pass, then repeatedly double the genome length and
            // try again until it's big enough. This is simple but very costly in terms of
            // memory so our maximum pass size is MAXIMUM_PASSES, which should be small enough
            // to allow for even pretty long genomes.
            for (int i = 1; i <= Passes; i *= 2)  // note i starts at 1
            {
                position = MakeTrees(state, genome, trees, threadnum, ercMapsForFancyPrint);
                if (position < 0 && i < Passes)  // gotta try again
                {
                    // this is a total hack
                    int[] old = genome;
                    genome = new int[old.Length * 2];
                    Array.Copy(old, 0, genome, 0, old.Length);
                    Array.Copy(old, 0, genome, old.Length, old.Length);  // duplicate
                }
            }
            return (Math.Min(position, ind.genome.Length));
        }

        // called by the above
        public int MakeTrees(IEvolutionState state, int[] genome, GPTree[] trees,
            int threadnum, IDictionary<int, GPNode> ercMapsForFancyPrint)
        {
            int position = 0;

            for (int i = 0; i < trees.Length; i++)
            {
                // cannot complete one of the trees with the given chromosome
                if (position < 0)
                    return BIG_TREE_ERROR;
                position = MakeTree(state, genome, trees[i], position, i, threadnum, ercMapsForFancyPrint);
            }
            return position;
        }

        /// <summary>
        /// MakeTree, edits the tree that its given by adding a root (and all subtrees attached)
        /// </summary>
        /// <returns>The number of chromosomes used, or an BIG_TREE_ERROR sentinel value.</returns>
        public int MakeTree(IEvolutionState state, int[] genome, GPTree tree, int position, int treeNum, int threadnum, IDictionary<int, GPNode> ercMapsForFancyPrint)
        {
            // hack, use an array to pass an extra value
            int[] countNumberOfChromosomesUsed = { position };  // hack, use an array to pass an extra value

            var gpfs = tree.Constraints((GPInitializer)state.Initializer).FunctionSet;
            GPNode root;

            try // get the tree, or return an error.
            {
                root = MakeSubtree(countNumberOfChromosomesUsed, genome, state, gpfs, Grammar[treeNum], treeNum, threadnum, ercMapsForFancyPrint, tree, (byte)0);
            }
            catch (BigTreeException)
            {
                return BIG_TREE_ERROR;
            }

            if (root == null)
            {
                state.Output.Fatal("Invalid tree: tree #" + treeNum);
            }

            root.Parent = tree;
            tree.Child = root;
            return countNumberOfChromosomesUsed[0];
        }

        /// <summary>
        /// Thrown by makeSubtree when chromosome is not large enough for the generated tree.
        /// </summary>
        class BigTreeException : InvalidOperationException { const long SerialVersionUID = 1L; }

        GPNode MakeSubtree(IList<int> index, IList<int> genome, 
            IEvolutionState es, GPFunctionSet gpfs, GrammarRuleNode rule, 
            int treeNum, int threadNum, IDictionary<int, GPNode> ercMapsForFancyPrint, IGPNodeParent parent, byte argPosition)
        {
            //have we exceeded the length of the genome?  No point in going further.
            if (index[0] >= genome.Count)
            {
                throw new BigTreeException();
            }

            //expand the rule with the chromosome to get a body element
            int i;

            //non existant rule got passed in
            if (rule == null)
            {
                es.Output.Fatal("An undefined rule exists within the grammar.");
            }

            //more than one rule to consider, pick one based off the genome, and consume the current gene
            // avoid mod operation as much as possible
            if (rule.GetNumChoices() > 1)
            {
                i = (genome[index[0]] - (int)GetMinGene(index[0])) % rule.GetNumChoices();
            }
            else
            {
                i = 0;
            }
            index[0]++;
            GrammarNode choice = rule.GetChoice(i);

            // if body is another rule head
            //look up rule
            if (choice is GrammarRuleNode)
            {
                var nextrule = (GrammarRuleNode)choice;
                return MakeSubtree(index, genome, es, gpfs, nextrule, 
                    treeNum, threadNum, ercMapsForFancyPrint, parent, argPosition);
            }
            else //handle functions
            {
                GrammarFunctionNode funcgrammarnode = (GrammarFunctionNode)choice;

                GPNode validNode = funcgrammarnode.GetGPNodePrototype();

                int numChildren = validNode.Children.Length;
                //index 0 is the node itself
                int numChildrenInGrammar = funcgrammarnode.GetNumArguments();

                //does the grammar contain the correct amount of children that the GPNode requires
                if (numChildren != numChildrenInGrammar)
                {
                    es.Output.Fatal("GPNode " + validNode.ToStringForHumans() + " requires " 
                        + numChildren + " children.  "
                        + numChildrenInGrammar 
                        + " children found in the grammar.");
                }

                //check to see if it is an ERC node
                if (validNode is ERC)
                {
                    // have we exceeded the length of the genome?  No point in going further.
                    if (index[0] >= genome.Count)
                    {
                        throw new BigTreeException();
                    }

                    // ** do we actually need to maintain two vlaues ? key and originalVal ?
                    // ** there is no problem if we use the originalVal for both ERCBank and
                    // ** ercMapsForFancyPrint, moreover, this will also make the reverse-mapping case
                    // ** easier -- khaled

                    // these below two lines are from the original code --
                    // key for ERC hashtable look ups is the current index within the genome.  Consume it.
                    //int key = genome[index[0]] - (int)GetMinGene(index[0]);
                    //int originalVal = genome[index[0]];

                    // this single line is khaled's mod --
                    int genomeVal = genome[index[0]];
                    index[0]++;

                    validNode = ObtainERC(es, genomeVal, threadNum, validNode, ercMapsForFancyPrint);
                }
                //non ERC node
                else
                {
                    validNode = validNode.LightClone();
                }

                //get the rest.
                for (int j = 0, childNumber = 0; j < funcgrammarnode.GetNumArguments(); j++)
                {
                    //get and link children to the current GPNode
                    validNode.Children[childNumber] = MakeSubtree(index, genome, es, gpfs,
                        (GrammarRuleNode)funcgrammarnode.GetArgument(j), treeNum, threadNum, 
                        ercMapsForFancyPrint, validNode, (byte)childNumber);

                    if (validNode.Children[childNumber] == null)
                    {
                        return null;
                    }
                    childNumber++;
                }
                validNode.ArgPosition = argPosition;
                validNode.Parent = parent;
                return validNode;
            }
        }

        /// <summary>
        /// Loads an ERC from the ERCBank given the value in the genome.  
        /// If there is no such ERC, then one is created and randomized, then added to the bank.
        /// The point of this mechanism is to enable ERCs to appear in multiple places in a GPTree. 
        /// </summary>
        public GPNode ObtainERC(IEvolutionState state, int genomeVal, int threadnum, 
            GPNode node, IDictionary<int, GPNode> ercMapsForFancyPrint)
        {
            // TODO: BRS: Questionable key here because of Java -> C# conversion (hash codes)
            var ercList = (IList<GPNode>)ERCBank[genomeVal];

            if (ercList == null)
            {
                ercList = new List<GPNode>();
                ERCBank[genomeVal] = ercList;
            }

            GPNode dummy;

            // search array list for an ERC of the same type we want
            for (var i = 0; i < ercList.Count; i++)
            {
                dummy = ercList[i];

                // ERC was found inside the list
                if (dummy.NodeEquivalentTo(node))
                {
                    if (ercMapsForFancyPrint != null) ercMapsForFancyPrint[genomeVal] = dummy;
                    return dummy.LightClone();
                }
            }

            // erc was not found in the array list lets make one
            node = node.LightClone();
            node.ResetNode(state, threadnum);
            ercList.Add(node);
            if (ercMapsForFancyPrint != null) ercMapsForFancyPrint[genomeVal] = node;
            return node;
        }

        public override object Clone()
        {
            var other = (GESpecies)(base.Clone());
            other.GPSpecies = (GPSpecies)(GPSpecies.Clone());
            // ERCBank isn't cloned
            // ** I think we need to clone it -- khaled
            return other;
        }

        /// <summary>
        /// Returns the number of elements consumed from the GEIndividual array to produce
        /// the tree, else returns -1 if an error occurs, specifically if all elements were
        /// consumed and the tree had still not been completed. 
        /// If you pass in a non-null HashMap for ERCmappings, then ERCmappings will be loaded with key->ERCvalue
        /// pairs of ERC mappings used in this map.        
        /// </summary>
        public int Consumed(IEvolutionState state, GEIndividual ind, int threadnum)
        {
            // create a dummy individual
            var newind = ((GPIndividual)GPSpecies.I_Prototype).LightClone();

            // do the mapping and return the number consumed
            return MakeTrees(state, ind, newind.Trees, threadnum, null);
        }

        /// <summary>
        /// Returns a dummy GPIndividual with a single tree which was built by mapping
        /// over the elements of the given GEIndividual.  Null is returned if an error occurs,
        /// specifically, if all elements were consumed and the tree had still not been completed.
        /// </summary>
        public GPIndividual Map(IEvolutionState state, GEIndividual ind, int threadnum, IDictionary<int, GPNode> ercMapsForFancyPrint)
        {
            // create a dummy individual
            var newind = ((GPIndividual) GPSpecies.I_Prototype).LightClone();

            // Do NOT initialize its trees

            // Set the fitness to the ByteVectorIndividual's fitness
            newind.Fitness = ind.Fitness;
            newind.Evaluated = false;

            // Set the species to me
            newind.Species = GPSpecies;

            // do the mapping
            if (MakeTrees(state, ind, newind.Trees, threadnum, ercMapsForFancyPrint) < 0)  // error
                return null;

            return newind;
        }

        /** Flattens an S-expression */
        public IList FlattenSexp(IEvolutionState state, int threadnum, GPTree tree)
        {
            IList nodeList = GatherNodeString(state, threadnum, tree.Child, 0);
            return nodeList;
        }

        /** Used by the above function */
        public IList GatherNodeString(IEvolutionState state, int threadnum, GPNode node, int index)
        {
            ArrayList list = new ArrayList();
            if (node is ERC)
            {
                // Now, get the "key" from the "node", NOTE: the "node" is inside an ArrayList,
                // since the ERCBank is mapped as key --> ArrayList of GPNodes.
                // The "key" is the corresponding int value for the ERC.
                list.Add(node.Name.Trim()); // add "ERC"
                                              // then add the ERC key (original genome value)
                list.Add(GetKeyFromNode(state, threadnum, node, index).Trim());
            }
        else
            list.Add(node.ToString().Trim());
            if (node.Children.Length > 0)
            {
                for (int i = 0; i < node.Children.Length; i++)
                {
                    index++;
                    IList sublist = GatherNodeString(state, threadnum, node.Children[i], index);
                    list.AddRange(sublist);
                }
            }
            return list;
        }

        public string GetKeyFromNode(IEvolutionState state, int threadnum, GPNode node, int index)
        {
            throw new NotImplementedException();
            /*
            string str = null;
            // ERCBank has some contents at least.
            if (ERCBank != null && ERCBank.Count != 0)
            {
                Iterator iter = ERCBank.EntrySet().iterator();
                while (iter.hasNext())
                {
                    Map.Entry pairs = (Map.Entry)iter.next();
                    ArrayList nodeList = (ArrayList)pairs.getValue();
                    if (Collections.binarySearch(
                            nodeList,
                            node,
                            new Comparator()
                            {
                                    public int compare(Object o1, Object o2)
                                        {
                                            if (o1 is GPNode && o2 is GPNode)
                                                return ((GPNode)o1).ToString().
                                                    CompareTo(((GPNode)o2).ToString());
                                            return 0;
                                        }
                }) >= 0 )
                            {
                    // a match found, save the key, break loop.
                    str = ((Int32)pairs.getKey()).ToString();
                    break;
                }
            }
        }

            // If a suitable match is not found in the above loop,
            // Add the node in a new list and add it to the ERCBank
            // with a new random value as a key.
            if (str == null)
            {
                // if the hash-map is not created yet
                if (ERCBank == null) ERCBank = new Hashtable();
                // if the index is still in the range of minGene.Length, use it.
                // otherwise use the minGene[0] value.
                int minIndex = 0;
                if (index < MinGenes.Length) minIndex = index;
                // now generate a new key
                Int32 key = Integer.valueOf((int) MinGenes[minIndex]
                                            + state.Random[threadnum]
                                                .NextInt((int) (MaxGenes[minIndex] - MinGenes[minIndex] + 1)));
                ArrayList list = new ArrayList();
                list.Add(node.LightClone());
                ERCBank.Put(key, list);
                str = key.ToString();
            }
            return str;
            */
        }

        /**
         * The LL(1) parsing algorithm to parse the lisp tree, the lisp tree is actually
         * fed as a flattened list, the parsing code uses the "exact" (and as-is) procedure 
         * described in the dragon book.
         **/
    public int[] ParseSexp(ArrayList flatSexp, GrammarParser gp)
{
            throw new NotImplementedException();
            /*
    // We can't use array here, because we don't know how we are going to traverse
    // the grammar tree, so the length is not known beforehand.
    ArrayList intList = new ArrayList();
    Queue input = new Queue((ArrayList)flatSexp.Clone());
    Stack stack = new Stack();
    stack.Push(((GrammarNode)gp.ProductionRuleList.get(0)).GetHead());
    int index = 0;
    while (input.Count != 0)
    {
        String token = (String)input.Remove();
        while (true)
        {
            if (stack.Peek().Equals(token))
            {
                // if found a match, pop it from the stack
                stack.Pop();
                // if the stack top is an ERC, read the next token
                if (token.Equals("ERC"))
                {
                    token = (String)input.Remove();
                    intList.Add(Integer.valueOf(token));
                }
                break;
            }
            else
            {
                int rIndex = ((Integer)gp.RuleHeadToIndex.get(stack.peek())).intValue();
                int fIndex = ((Integer)gp.FunctionHeadToIndex.get(token)).intValue();
                Integer ruleIndex = new Integer(gp.PredictiveParseTable[rIndex][fIndex]);
                // get the action (rule) to expand
                GrammarNode action = (GrammarNode)gp.IndexToRule.get(ruleIndex);
                // if the index is still in the range of minGene.Length, use it.
                // otherwise use the minGene[0] value.
                int minIndex = 0; if (index < MinGenes.Length) minIndex = index;
                // now add
                intList.Add(new Integer(((Integer)gp.AbsIndexToRelIndex.get(ruleIndex)).intValue() + (int)MinGenes[minIndex]));
                index++;
                stack.Pop();
                action = action.Children[0];
                if (action is GrammarFunctionNode)
                        {
                    // push the rule (action) arguments in reverse way
                    for (int i = ((GrammarFunctionNode)action).GetNumArguments() - 1
                            ; i >= 0; i--)
                        stack.Push(((GrammarFunctionNode)action).GetArgument(i).GetHead());
                    // the rule (action) head should be on the top
                    stack.Push(action.GetHead());
                }
                    else if (action is GrammarRuleNode) // push as usual
                        stack.Push(((GrammarRuleNode)action).GetHead());
            }
        }
    }
    // now convert the list into an array
    int[] genomeVals = new int[intList.Count];
    for (int i = 0; i < intList.Count; i++) { genomeVals[i] = ((Int32)intList[i]).intValue(); }
    return genomeVals;
    */
}

/**
   Reverse of the original map() function, takes a GPIndividual and returns
   a corresponding GEIndividual; The GPIndividual may contain more than one trees,
   and such cases are handled accordingly, see the 3rd bullet below --

   NOTE:
   * This reverse mapping is only valid for S-expression trees ;

   * This procedure supports ERC for the current population (not for population
   /subpopulation from other islands); However, that could be done by merging
   all ERCBanks from all the sub-populations but that is not done yet ;

   * Support for the ADF's are done as follows -- suppose in one GPIndividual,
   there are N trees -- T1, T2, ,,, Tn and each of them follows n different
   grammars G1, G2, ,,, Gn respectively; now if they are reverse-mapped to
   int arrays, there will be n int arrays A1[], A2[], ,,, An[]; and suppose
   the i-th tree Ti is reverse mapped to int array Ai[] and morevoer Ai[] is 
   the longest among all the arrays (Bj[]s); so Bi[] is sufficient to build 
   all ADF trees Tjs.
*/
public GEIndividual ReverseMap(IEvolutionState state, GPIndividual ind, int threadnum)
{
    // create a dummy individual
    GEIndividual newind = (GEIndividual)I_Prototype.Clone();

    // The longest int will be able to contain all ADF trees.
    int longestIntLength = -1;
    int[] longestInt = null;
    // Now go through all the ADF trees.
    for (int treeIndex = 0; treeIndex < ind.Trees.Length; treeIndex++)
    {
        // Flatten the Lisp tree
        ArrayList flatSexp = (ArrayList)FlattenSexp(state, threadnum,
            ind.Trees[treeIndex]);
        // Now convert the flatten list into an array of ints
        // no. of trees == no. of grammars
        int[] genomeVals = ParseSexp(flatSexp, GrammarParser[treeIndex]);
        // store the longest int array
        if (genomeVals.Length >= longestIntLength)
        {
            longestIntLength = genomeVals.Length;
            longestInt = new int[genomeVals.Length];
            Array.Copy(genomeVals, 0, longestInt, 0, genomeVals.Length);
        }
        genomeVals = null;
    }
    // assign the longest int to the individual's genome
    newind.genome = longestInt;

    // update the GPIndividual's fitness information
    newind.Fitness = ind.Fitness;
    newind.Evaluated = false;

    // Set the species to me ? not sure.
    newind.Species = this;

    // return it
    return newind;
}
        #endregion // Operations
    }
}