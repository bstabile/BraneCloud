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
using System.Runtime.Serialization;

using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{	
    /// <summary> 
    /// GPFunctionSet is a IClique which represents a set of GPNode prototypes
    /// forming a standard function set for forming certain trees in individuals.
    /// GPFunctionSets instances have unique names with which they're referenced by
    /// GPTreeConstraints objects indicating that they're used for certain trees.
    /// GPFunctionSets store their GPNode IPrototypes in three hashtables,
    /// one for all nodes, one for nonterminals, and one for terminals.  Each
    /// hashed item is an array of GPNode objects,
    /// hashed by the return type of the GPNodes in the array.
    /// 
    /// GPFunctionSets also contain prototypical GPNode nodes which they
    /// clone to form their arrays.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>name</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of function set.  Must be different from other function set instances)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of functions in the function set)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>func.</tt><i>n</i><br/>
    /// <font size="-1">classname, inherits and != ec.gp.GPNode</font></td>
    /// <td valign="top">(class of function node <i>n</i> in the set)</td></tr>
    /// </table>
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>func.</tt><i>n</i></td>
    /// <td>function node <i>n</i></td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPFunctionSet")]
    public class GPFunctionSet : IClique // , ISerializable
    {
        #region Constants

        private const long SerialVersionUID = 1;

        public const string P_NAME = "name";
        public const string P_FUNC = "func";
        public const string P_SIZE = "size";

        #endregion // Constants
        #region Static

        /// <summary>
        /// Returns the function set for a given name.
        /// You must guarantee that after calling functionSetFor(...) one or
        /// several times, you call state.Output.ExitIfErrors() once. 
        /// </summary>		
        public static GPFunctionSet FunctionSetFor(string functionSetName, IEvolutionState state)
        {
            var funcs = (GPFunctionSet)(((GPInitializer)state.Initializer).FunctionSetRepository[functionSetName]);
            if (funcs == null)
                state.Output.Error("The GP function set \"" + functionSetName + "\" could not be found.");
            return funcs;
        }

        #endregion // Static
        #region Properties

        /// <summary>
        /// Name of the GPFunctionSet 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The nodes that our GPTree can use: arrays of nodes hashed by type. 
        /// </summary>
        public Hashtable NodesByType { get; set; }

        /// <summary>
        /// The nodes that our GPTree can use: nodes[type][thenodes]. 
        /// </summary>
        public GPNode[][] Nodes { get; set; }

        /// <summary>
        /// The nonterminals our GPTree can use: arrays of nonterminals hashed by type. 
        /// </summary>
        public Hashtable NonterminalsByType { get; set; }

        /// <summary>
        /// The nonterminals our GPTree can use: nonterminals[type][thenodes]. 
        /// </summary>
        public GPNode[][] Nonterminals { get; set; }

        /// <summary>
        /// The terminals our GPTree can use: arrays of terminals hashed by type. 
        /// </summary>
        public Hashtable TerminalsByType { get; set; }

        /// <summary>
        /// The terminals our GPTree can use: terminals[type][thenodes]. 
        /// </summary>
        public GPNode[][] Terminals { get; set; }


        // some convenience properties which speed up various kinds of mutation operators

        /// <summary>
        /// The nodes that our GPTree can use, hashed by name().
        /// </summary>
        public Hashtable NodesByName { get; set; }

        /// <summary>
        /// Nodes == a given arity, that is: nodesByArity[type][arity][thenodes] 
        /// </summary>
        public GPNode[][][] NodesByArity { get; set; }

        /// <summary>
        /// Nonterminals &lt;= a given arity, that is: nonterminalsUnderArity[type][arity][thenodes] --
        /// this will be O(n^2).  Obviously, the number of nonterminals at arity slot 0 is 0.
        /// </summary>
        public GPNode[][][] NonterminalsUnderArity { get; set; }

        /// <summary>
        /// Nonterminals >= a given arity, that is: nonterminalsOverArity[type][arity][thenodes] --
        /// this will be O(n^2).  Obviously, the number of nonterminals at arity slot 0 is all the 
        /// nonterminals of that type. 
        /// </summary>
        public GPNode[][][] NonterminalsOverArity { get; set; }

        #endregion // Properties
        #region Setup

        // BRS : TODO : Where does the following comment belong? (this is where ECJ leaves it dangling....
        /// <summary>
        /// Sets up all the GPFunctionSet, loading them from the parameter file.
        /// This must be called before anything is called which refers to a type by name. 
        /// </summary>

        /// <summary>
        /// Sets up the arrays based on the hashtables 
        /// </summary>		
        public virtual void PostProcessFunctionSet()
        {
            Nodes = new GPNode[NodesByType.Count][];
            Terminals = new GPNode[TerminalsByType.Count][];
            Nonterminals = new GPNode[NonterminalsByType.Count][];

            var e = NodesByType.Keys.GetEnumerator();
            while (e.MoveNext())
            {
                var gpt = (GPType)(e.Current);
                var gpfi = (GPNode[])(NodesByType[gpt]);
                Nodes[gpt.Type] = gpfi;
            }
            e = NonterminalsByType.Keys.GetEnumerator();
            while (e.MoveNext())
            {
                var gpt = (GPType)(e.Current);
                var gpfi = (GPNode[])(NonterminalsByType[gpt]);
                Nonterminals[gpt.Type] = gpfi;
            }
            e = TerminalsByType.Keys.GetEnumerator();
            while (e.MoveNext())
            {
                var gpt = (GPType)(e.Current);
                var gpfi = (GPNode[])(TerminalsByType[gpt]);
                Terminals[gpt.Type] = gpfi;
            }

            // set up arity-based arrays
            // first, determine the maximum arity
            var max_arity = 0;
            for (var x = 0; x < Nodes.Length; x++)
                for (var y = 0; y < Nodes[x].Length; y++)
                    if (max_arity < Nodes[x][y].Children.Length)
                        max_arity = Nodes[x][y].Children.Length;

            // next set up the == array
            NodesByArity = new GPNode[Nodes.Length][][];
            for (var i = 0; i < Nodes.Length; i++)
            {
                NodesByArity[i] = new GPNode[max_arity + 1][];
            }
            for (var x = 0; x < Nodes.Length; x++)
                for (var a = 0; a <= max_arity; a++)
                {
                    // how many nodes do we have?
                    var num_of_a = 0;
                    for (var y = 0; y < Nodes[x].Length; y++)
                        if (Nodes[x][y].Children.Length == a)
                            num_of_a++;
                    // allocate and fill
                    NodesByArity[x][a] = new GPNode[num_of_a];
                    var cur_a = 0;
                    for (var y = 0; y < Nodes[x].Length; y++)
                        if (Nodes[x][y].Children.Length == a)
                            NodesByArity[x][a][cur_a++] = Nodes[x][y];
                }

            // now set up the <= nonterminals array
            NonterminalsUnderArity = new GPNode[Nonterminals.Length][][];
            for (var i2 = 0; i2 < Nonterminals.Length; i2++)
            {
                NonterminalsUnderArity[i2] = new GPNode[max_arity + 1][];
            }
            for (var x = 0; x < Nonterminals.Length; x++)
                for (var a = 0; a <= max_arity; a++)
                {
                    // how many nonterminals do we have?
                    var num_of_a = 0;
                    for (var y = 0; y < Nonterminals[x].Length; y++)
                        if (Nonterminals[x][y].Children.Length <= a)
                            num_of_a++;
                    // allocate and fill
                    NonterminalsUnderArity[x][a] = new GPNode[num_of_a];
                    var cur_a = 0;
                    for (var y = 0; y < Nonterminals[x].Length; y++)
                        if (Nonterminals[x][y].Children.Length <= a)
                            NonterminalsUnderArity[x][a][cur_a++] = Nonterminals[x][y];
                }



            // now set up the >= nonterminals array
            NonterminalsOverArity = new GPNode[Nonterminals.Length][][];
            for (var i3 = 0; i3 < Nonterminals.Length; i3++)
            {
                NonterminalsOverArity[i3] = new GPNode[max_arity + 1][];
            }
            for (var x = 0; x < Nonterminals.Length; x++)
                for (var a = 0; a <= max_arity; a++)
                {
                    // how many nonterminals do we have?
                    var num_of_a = 0;
                    for (var y = 0; y < Nonterminals[x].Length; y++)
                        if (Nonterminals[x][y].Children.Length >= a)
                            num_of_a++;
                    // allocate and fill
                    NonterminalsOverArity[x][a] = new GPNode[num_of_a];
                    var cur_a = 0;
                    for (var y = 0; y < Nonterminals[x].Length; y++)
                        if (Nonterminals[x][y].Children.Length >= a)
                            NonterminalsOverArity[x][a][cur_a++] = Nonterminals[x][y];
                }
        }

        /// <summary>
        /// Must be done <i>after</i> GPType and GPNodeConstraints have been set up. 
        /// </summary>		
        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            // What's my name?
            Name = state.Parameters.GetString(paramBase.Push(P_NAME), null);
            if (Name == null)
                state.Output.Fatal("No name was given for this function set.", paramBase.Push(P_NAME));

            // Register me
            var tempObject = ((GPInitializer)state.Initializer).FunctionSetRepository[Name];
            ((GPInitializer)state.Initializer).FunctionSetRepository[Name] = this;
            var old_funcs = (GPFunctionSet)(tempObject);
            if (old_funcs != null)
                state.Output.Fatal("The GPFunctionSet \"" + Name + "\" has been defined multiple times.", paramBase.Push(P_NAME));

            // How many functions do I have?
            var numFuncs = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (numFuncs < 1)
                state.Output.Error("The GPFunctionSet \"" + Name + "\" has no functions.", paramBase.Push(P_SIZE));

            NodesByName = new Hashtable();

            var p = paramBase.Push(P_FUNC);
            var tmp = ArrayList.Synchronized(new ArrayList(10));
            for (var x = 0; x < numFuncs; x++)
            {
                // load
                var pp = p.Push("" + x);
                var gpfi = (GPNode)(state.Parameters.GetInstanceForParameter(pp, null, typeof(GPNode)));
                gpfi.Setup(state, pp);

                // add to my collection
                tmp.Add(gpfi);

                // Load into the nodesByName hashtable
                var nodes = (GPNode[])NodesByName[gpfi.Name];
                if (nodes == null)
                    NodesByName[gpfi.Name] = new[] { gpfi };
                else
                {
                    // O(n^2) but uncommon so what the heck.
                    var nodes2 = new GPNode[nodes.Length + 1];
                    Array.Copy(nodes, 0, nodes2, 0, nodes.Length);
                    nodes2[nodes2.Length - 1] = gpfi;
                    NodesByName[gpfi.Name] = nodes2;
                }
            }

            // Make my hash tables
            NodesByType = Hashtable.Synchronized(new Hashtable());
            TerminalsByType = Hashtable.Synchronized(new Hashtable());
            NonterminalsByType = Hashtable.Synchronized(new Hashtable());

            // Now set 'em up according to the types in GPType

            var e = ((GPInitializer)state.Initializer).TypeRepository.Values.GetEnumerator();
            var initializer = ((GPInitializer)state.Initializer);
            while (e.MoveNext())
            {
                var typ = (GPType)(e.Current);

                // make vectors for the type.
                var nodes_v = ArrayList.Synchronized(new ArrayList(10));
                var terminals_v = ArrayList.Synchronized(new ArrayList(10));
                var nonterminals_v = ArrayList.Synchronized(new ArrayList(10));

                // add GPNodes as appropriate to each vector
                var v = tmp.GetEnumerator();
                while (v.MoveNext())
                {
                    var i = (GPNode)(v.Current);
                    if (typ.CompatibleWith(initializer, i.Constraints(initializer).ReturnType))
                    {
                        nodes_v.Add(i);
                        if (i.Children.Length == 0)
                            terminals_v.Add(i);
                        else
                            nonterminals_v.Add(i);
                    }
                }

                // turn nodes_h' vectors into arrays
                var ii = new GPNode[nodes_v.Count];
                nodes_v.CopyTo(ii);
                NodesByType[typ] = ii;

                // turn terminals_h' vectors into arrays
                ii = new GPNode[terminals_v.Count];
                terminals_v.CopyTo(ii);
                TerminalsByType[typ] = ii;

                // turn nonterminals_h' vectors into arrays
                ii = new GPNode[nonterminals_v.Count];
                nonterminals_v.CopyTo(ii);
                NonterminalsByType[typ] = ii;
            }

            // I don't check to see if the generation mechanism will be valid here
            // -- I check that in GPTreeConstraints, where I can do the weaker check
            // of going top-down through functions rather than making sure that every
            // single function has a compatible argument function (an unneccessary check)

            state.Output.ExitIfErrors(); // because I promised when I called n.Setup(...)

            // postprocess the function set
            PostProcessFunctionSet();
        }

        #endregion // Setup
        #region ToString

        /// <summary>
        /// Returns the name. 
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #endregion // ToString
        #region ISerializable (Not currently used)

        //public virtual void  GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    // this wastes an hashtable pointer, but what the heck.			
        //    SupportClass.DefaultWriteObject(info, context, this);
        //}
        
        // BRS : TODO : If implementing ISerializable...
        //protected GPFunctionSet(SerializationInfo info, StreamingContext context)
        //{
        //    SupportClass.DefaultReadObject(info, context, this);
        //}
        //public GPFunctionSet()
        //{
        //}

        #endregion // ISerializable
    }
}