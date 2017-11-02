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
using System.Linq;
using System.Text;
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
    /// A genome of an individual is an array of random integers each of which are one byte long.  These numbers are used when a decision point 
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
    /// be negitive values they are offset by 128 (max negitive of a byte) giving us a value from 0-255.  A modulus is performed on this resulting 
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
    /// <font size="-1">classname, inherits and != ec.gp.ge.GrammarParser</font></td>
    /// <td valign="top">(the GrammarParser used by the GESpecies)</td></tr>
    /// </table>
     /// <p/><b>Default Base</b><br/>
    /// ec.gp.ge.GESpecies
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

        /// <summary>
        /// Return value which denotes that the tree has grown too large.
        /// </summary>
        public const int BIG_TREE_ERROR = -1;

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GEDefaults.ParamBase.Push(P_GESPECIES); }
        }

        /// <summary>
        /// The GPSpecies subsidiary to GESpecies.
        /// </summary>
        public GPSpecies GPSpecies { get; set; }

        /// <summary>
        /// All the ERCs created so far.
        /// </summary>
        public Hashtable ERCBank { get; set; }

        /// <summary>
        /// The parsed grammars.
        /// </summary>
        public GrammarRuleNode[] Grammar { get; set; }

        /// <summary>
        /// The prototypical parser used to parse the grammars.
        /// </summary>
        public GrammarParser ParserPrototype { get; set; }

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
            if (!(I_Prototype is ByteVectorIndividual))
            {
                state.Output.Fatal("The Individual class for the Species " + GetType().Name + " is must be a subclass of ec.gp.ge.GEIndividual.", paramBase);
            }

            ERCBank = Hashtable.Synchronized(new Hashtable());

            // load the grammars, one per ADF tree
            var gpi = (GPIndividual)(GPSpecies.I_Prototype);
            var trees = gpi.Trees;
            var numGrammars = trees.Length;

            ParserPrototype = (GrammarParser)(state.Parameters.GetInstanceForParameterEq(paramBase.Push(P_PARSER), def.Push(P_PARSER), typeof(GrammarParser)));

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

                //try
                //{
                    var gpfs = trees[i].Constraints((GPInitializer)state.Initializer).FunctionSet;
                    var grammarparser = (GrammarParser)(ParserPrototype.Clone());
                    //Grammar[i] = grammarparser.ParseRules(state, new StreamReader(grammarFile.FullName), gpfs);
                    Grammar[i] = grammarparser.ParseRules(state, new StreamReader(grammarFile), gpfs);
                //}
                //catch (FileNotFoundException)
                //{
                //    state.Output.Fatal("Error retrieving grammar file(s): " + def + "." + P_FILE + "." + i + " does not exist or cannot be opened.");
                //}
            }
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
        /// <param name="ercMappings"></param>
        /// <returns>Number of chromosomes consumed</returns>
        public int MakeTrees(IEvolutionState state, GEIndividual ind, GPTree[] trees, int threadnum, IDictionary<int, GPNode> ercMappings)
        {
            var position = 0;

            for (var i = 0; i < trees.Length; i++)
            {
                //cannot complete one of the trees with the given chromosome
                if (position < 0)
                    return BIG_TREE_ERROR;

                position = MakeTree(state, ind, trees[i], position, i, threadnum, ercMappings);
            }

            return position;
        }

        /// <summary>
        /// MakeTree, edits the tree that its given by adding a root (and all subtrees attached)
        /// </summary>
        /// <returns>The number of chromosomes used, or an BIG_TREE_ERROR sentinel value.</returns>
        public int MakeTree(IEvolutionState state, GEIndividual ind, GPTree tree, int position, int treeNum, int threadnum, IDictionary<int, GPNode> ercMappings)
        {
            int[] countNumberOfChromosomesUsed = { position };  // hack, use an array to pass an extra value
            var genome = ind.genome;
            var gpfs = tree.Constraints((GPInitializer)state.Initializer).FunctionSet;
            GPNode root;

            try // get the tree, or return an error.
            {
                root = MakeSubtree(countNumberOfChromosomesUsed, genome, state, gpfs, Grammar[treeNum], treeNum, threadnum, ercMappings);
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
        class BigTreeException : Exception { /* ECJ has: static long serialVersionUID = 1L; */ }

        GPNode MakeSubtree(IList<int> index, IList<byte> genome, IEvolutionState es, GPFunctionSet gpfs, GrammarRuleNode rule, int treeNum, int threadnum, IDictionary<int, GPNode> ercMappings)
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
            if (rule.GetNumChoices() > 1)
            {
                //casting to an int should be ok since the biggest these genes can be is a byte
                i = (genome[index[0]] - (int)GetMinGene(index[0])) % rule.GetNumChoices();
                index[0]++;
            }
            //only 1 rule to consider
            else
            {
                i = 0;
            }
            var choice = rule.GetChoice(i);

            // if body is another rule head
            //look up rule
            if (choice is GrammarRuleNode)
            {
                var nextrule = (GrammarRuleNode)choice;
                return MakeSubtree(index, genome, es, gpfs, nextrule, treeNum, threadnum, ercMappings);
            }
            else //handle functions
            {
                var funcgrammarnode = (GrammarFunctionNode)choice;

                var validNode = funcgrammarnode.GetGPNodePrototype();

                var numChildren = validNode.Children.Length;
                //index 0 is the node itself
                var numChildrenInGrammar = funcgrammarnode.GetNumArguments();

                //does the grammar contain the correct amount of children that the GPNode requires
                if (numChildren != numChildrenInGrammar)
                {
                    es.Output.Fatal("GPNode " + validNode.ToStringForHumans() + " requires " + numChildren + " children.  "
                        + numChildrenInGrammar + " children found in the grammar.");
                }

                //check to see if it is an ERC node
                if (validNode is ERC)
                {
                    // have we exceeded the length of the genome?  No point in going further.
                    if (index[0] >= genome.Count)
                    {
                        throw new BigTreeException();
                    }

                    // key for ERC hashtable look ups is the current index within the genome.  Consume it.
                    var key = genome[index[0]] - (int)GetMinGene(index[0]);
                    int originalVal = genome[index[0]];
                    index[0]++;

                    validNode = ObtainERC(es, key, originalVal, threadnum, validNode, ercMappings);
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
                        (GrammarRuleNode)funcgrammarnode.GetArgument(j), treeNum, threadnum, ercMappings);

                    if (validNode.Children[childNumber] == null)
                    {
                        return null;
                    }
                    childNumber++;
                }
                return validNode;
            }
        }

        /// <summary>
        /// Loads an ERC from the ERCBank given the value in the genome.  
        /// If there is no such ERC, then one is created and randomized, then added to the bank.
        /// The point of this mechanism is to enable ERCs to appear in multiple places in a GPTree. 
        /// </summary>
        public GPNode ObtainERC(IEvolutionState state, int key, int genomeVal, int threadnum, GPNode node, IDictionary<int, GPNode> ercMappings)
        {
            var ercList = (List<GPNode>)(ERCBank[key]);

            if (ercList == null)
            {
                ercList = new List<GPNode>();
                ERCBank[key] = ercList;
            }

            GPNode dummy;

            // search array list for an ERC of the same type we want
            for (var i = 0; i < ercList.Count; i++)
            {
                dummy = ercList[i];

                // ERC was found inside the list
                if (dummy.NodeEquivalentTo(node))
                {
                    if (ercMappings != null) ercMappings[genomeVal] = dummy;
                    return dummy.LightClone();
                }
            }

            // erc was not found in the array list lets make one
            node = node.LightClone();
            node.ResetNode(state, threadnum);
            ercList.Add(node);
            if (ercMappings != null) ercMappings[genomeVal] = node;
            return node;
        }

        public override object Clone()
        {
            var other = (GESpecies)(base.Clone());
            other.GPSpecies = (GPSpecies)(GPSpecies.Clone());
            // ERCBank isn't cloned
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
            var newind = ((GPIndividual)(GPSpecies.I_Prototype)).LightClone();

            // do the mapping and return the number consumed
            return MakeTrees(state, ind, newind.Trees, threadnum, null);
        }

        /// <summary>
        /// Returns a dummy GPIndividual with a single tree which was built by mapping
        /// over the elements of the given GEIndividual.  Null is returned if an error occurs,
        /// specifically, if all elements were consumed and the tree had still not been completed.
        /// </summary>
        public GPIndividual Map(IEvolutionState state, GEIndividual ind, int threadnum, IDictionary<int, GPNode> ercMappings)
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
            if (MakeTrees(state, ind, newind.Trees, threadnum, ercMappings) < 0)  // error
                return null;

            return newind;
        }

        #endregion // Operations
    }
}