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
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// GPIndividual is an Individual used for GP evolution runs.
    /// GPIndividuals contain, at the very least, a nonempty array of GPTrees.
    /// You can use GPIndividual directly, or subclass it to extend it as
    /// you see fit.
    ///
    /// <p/>GPIndividuals have two clone methods: clone() and lightClone().  clone() is
    /// a deep clone method as usual.  lightClone() is a light clone which does not copy
    /// the trees.
    /// 
    /// <p/>In addition to serialization for checkpointing, 
    /// Individuals may read and write themselves to streams in three ways.
    /// <ul/>
    /// <li/><b>WriteIndividual(...,BinaryWriter)/ReadIndividual(...,BinaryReader)</b>&nbsp;&nbsp;&nbsp;
    /// This method transmits or receives an individual in binary.  It is the most efficient approach to sending
    /// individuals over networks, etc.  These methods write the evaluated flag and the fitness, then
    /// call <b>ReadGenotype/WriteGenotype</b>, which you must implement to write those parts of your 
    /// Individual special to your functions-- the default versions of ReadGenotype/WriteGenotype throw errors.
    /// You don't need to implement them if you don't plan on using Read/WriteIndividual.
    /// 
    /// <li/><b>PrintIndividual(...,StreamWriter) / ReadIndividual(...,StreamReader)</b>&nbsp;&nbsp;&nbsp;
    /// This approach transmits or receives an indivdual in text encoded such that the individual is largely readable
    /// by humans but can be read back in 100% by ECJ as well.  Because GPIndividuals are often very large,
    /// <b>GPIndividual has overridden these methods -- they work differently than in Individual (the base class).</b>  
    /// In specific: <b>ReadIndividual</b> by default reads in the fitness and the evaluation flag, 
    /// then calls <b>ParseGenotype</b> to read in the trees (via GPTree.ReadTree(...)).
    /// However <b>PrintIndividual</b> by default prints the fitness and evaluation flag, and prints all the trees
    /// by calling GPTree.PrintTree(...).  It does not call <b>GenotypeToString</b> at all.  
    /// This is because it's very wasteful to build up a large string holding the printed form of the GPIndividual 
    /// just to pump it out a stream once.
    /// 
    /// <li/><b>PrintIndividualForHumans(...,StreamWriter)</b>&nbsp;&nbsp;&nbsp;
    /// This approach prints an individual in a fashion intended for human consumption only. 
    /// Because GPIndividuals are often very large, <b>GPIndividual has overridden this methods 
    /// -- it works differently than in Individual (the base class).</b>  In specific:
    /// <b>PrintIndividual</b> by default prints the fitness and evaluation flag, and prints all the trees
    /// by calling GPTree.PrintTreeForHumans(...).  It does not call <b>GenotypeToStringForHumans</b> at all.  
    /// This is because it's very wasteful to build up a large string holding the printed form 
    /// of the GPIndividual just to pump it out a stream once.
    /// 
    /// <p/>In general, the various readers and writers do three things: they tell the Fitness to read/write itself,
    /// they read/write the evaluated flag, and they read/write the GPTree array (by having each GPTree read/write
    /// itself).  If you add instance variables to GPIndividual, you'll need to read/write those variables as well.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>numtrees</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of trees in the GPIndividual)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>tree.</tt><i>n</i><br/>
    /// <font size="-1">classname, inherits or = ec.gp.GPTree</font></td>
    /// <td valign="top">(class of tree <i>n</i> in the individual)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.individual
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>tree.</tt><i>n</i></td>
    /// <td>tree <i>n</i> in the individual</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPIndividual")]
    public class GPIndividual : Individual
    {
        #region Constants   ***************************************************************************

        private const long SerialVersionUID = 1;

        public const string P_NUMTREES = "numtrees";
        public const string P_TREE = "tree";

        #endregion // Constants
        #region Properties   **************************************************************************

        public override IParameter DefaultBase
        {
            get { return GPDefaults.ParamBase.Push(P_INDIVIDUAL); }
        }

        public GPTree[] Trees { get; set; }

        /// <summary>
        /// Returns the "size" of the individual, namely, the number of nodes in all of its subtrees.  
        /// </summary>
        public override long Size
        {
            get
            {
                return Trees.Aggregate<GPTree, long>(0, (current, t) => current + t.Child.NumNodes(GPNode.NODESEARCH_ALL));
            }
        }

        #endregion // Properties
        #region Setup   *******************************************************************************

        /// <summary>
        /// Sets up a prototypical GPIndividual with those features which it
        /// shares with other GPIndividuals in its species, and nothing more. 
        /// </summary>		
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase); // actually unnecessary (Individual.Setup() is empty)

            var def = DefaultBase;

            // set my evaluation to false
            Evaluated = false;

            // how many trees?
            var t = state.Parameters.GetInt(paramBase.Push(P_NUMTREES), def.Push(P_NUMTREES), 1); // at least 1 tree for GP!
            if (t <= 0)
                state.Output.Fatal("A GPIndividual must have at least one tree.", paramBase.Push(P_NUMTREES), def.Push(P_NUMTREES));

            // load the trees
            Trees = new GPTree[t];

            for (var x = 0; x < t; x++)
            {
                var p = paramBase.Push(P_TREE).Push("" + x);
                Trees[x] = (GPTree)(state.Parameters.GetInstanceForParameterEq(p, def.Push(P_TREE).Push("" + x), typeof(GPTree)));
                Trees[x].Owner = this;
                Trees[x].Setup(state, p);
            }

            // now that our function sets are all associated with trees,
            // give the nodes a chance to determine whether or not this is
            // going to work for them (especially the ADFs).

            var initializer = ((GPInitializer)state.Initializer);
            for (var x = 0; x < t; x++)
            {
                foreach (var gpfi in Trees[x].Constraints(initializer).FunctionSet.Nodes)
                {
                    foreach (var t1 in gpfi)
                        t1.CheckConstraints(state, x, this, paramBase);
                }
            }
            // because I promised with checkConstraints(...)
            state.Output.ExitIfErrors();
        }


        /// <summary>
        /// Verification of validity of the GPIndividual -- strictly for debugging purposes only 
        /// </summary>
        public void Verify(IEvolutionState state)
        {
            if (!(state.Initializer is GPInitializer))
            {
                state.Output.Error("Initializer is not a GPInitializer");
                return;
            }

            // BRS: Not sure why this is in the ECJ code. Commenting it out for now...
            //      Turns out they commented it out in v22! :)
            //var initializer = (GPInitializer)(state.Initializer);

            if (Trees == null)
            {
                state.Output.Error("Null trees in GPIndividual.");
                return;
            }
            for (var x = 0; x < Trees.Length; x++)
                if (Trees[x] == null)
                {
                    state.Output.Error("Null tree (#" + x + ") in GPIndividual.");
                    return;
                }
            foreach (var t in Trees)
                t.Verify(state);
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Comparison   **************************************************************************

        public override bool Equals(object ind)
        {
            if (ind == null) return false;
            if (!(GetType() == ind.GetType()))
                return false; // GPIndividuals are special.
            var i = (GPIndividual) ind;
            if (Trees.Length != i.Trees.Length)
                return false;

            // this default version works fine for most GPIndividuals.
            for (var x = 0; x < Trees.Length; x++)
                if (!(Trees[x].TreeEquals(i.Trees[x])))
                    return false;
            return true;
        }
        
        public override int GetHashCode()
        {
            // stolen from GPNode.  It's a decent algorithm.
            var hash = GetType().GetHashCode();

            return Trees.Aggregate(hash, (current, t) => (current << 1 | BitShifter.URShift(current, 31)) ^ t.TreeHashCode());
        }

        #endregion // Comparison
        #region Cloning   *****************************************************************************

        /// <summary>
        /// Deep-clones the GPIndividual.  Note that you should not deep-clone the prototypical GPIndividual
        /// stored in GPSpecies: they contain blank GPTrees with null roots, and this method,
        /// which calls GPTree.clone(), will produce a NullPointerException as a result. Instead, you probably
        /// want to use GPSpecies.NewIndividual(...) if you're thinking of playing with the prototypical
        /// GPIndividual. 
        /// </summary>
        public override object Clone()
        {
            // a deep clone

            var myobj = (GPIndividual)(base.Clone());

            // copy the tree array
            myobj.Trees = new GPTree[Trees.Length];
            for (var x = 0; x < Trees.Length; x++)
            {
                myobj.Trees[x] = (GPTree)(Trees[x].Clone()); // force a deep clone
                myobj.Trees[x].Owner = myobj; // reset owner away from me
            }
            return myobj;
        }

        /// <summary>
        /// Like clone(), but doesn't force the GPTrees to deep-clone themselves. 
        /// </summary>
        public virtual GPIndividual LightClone()
        {
            // a light clone
            var myobj = (GPIndividual)(base.Clone());

            // copy the tree array
            myobj.Trees = new GPTree[Trees.Length];
            for (var x = 0; x < Trees.Length; x++)
            {
                myobj.Trees[x] = Trees[x].LightClone(); // note light-cloned!
                myobj.Trees[x].Owner = myobj; // reset owner away from me
            }
            return myobj;
        }

        #endregion // Cloning
        #region IO   **********************************************************************************

        /// <summary>
        /// Prints just the trees of the GPIndividual.  
        /// Broken out like this to be used by GEIndividual to avoid
        /// re-printing the fitness and evaluated premables.
        /// </summary>
        public virtual void PrintTrees(IEvolutionState state, int log)
        {
            for (var x = 0; x < Trees.Length; x++)
            {
                state.Output.PrintLn("Tree " + x + ":", log);
                Trees[x].PrintTreeForHumans(state, log);
            }
        }

        public override void PrintIndividualForHumans(IEvolutionState state, int log)
        {
            state.Output.PrintLn(EVALUATED_PREAMBLE + (Evaluated ? "true" : "false"), log);
            Fitness.PrintFitnessForHumans(state, log);
            PrintTrees(state, log);
        }

        /// <summary>
        /// Overridden for the GPIndividual genotype, writing each tree in turn. 
        /// </summary>
        public override void PrintIndividual(IEvolutionState state, StreamWriter writer)
        {
            writer.WriteLine(EVALUATED_PREAMBLE + Code.Encode(Evaluated));
            Fitness.PrintFitness(state, writer);
            for (var x = 0; x < Trees.Length; x++)
            {
                writer.WriteLine("Tree " + x + ":");
                Trees[x].PrintTree(state, writer);
            }
        }

        protected override void ParseGenotype(IEvolutionState state, StreamReader reader)
        {
            // Read my trees
            foreach (var t in Trees)
            {
                reader.ReadLine(); // throw it away -- it's the tree indicator
                t.ReadTree(state, reader);
            }
        }

        /// <summary>
        /// Overridden for the GPIndividual genotype. 
        /// </summary>
        public override void WriteGenotype(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(Trees.Length);
            foreach (var t in Trees)
                t.WriteTree(state, writer);
        }

        /// <summary>
        /// Overridden for the GPIndividual genotype. 
        /// </summary>
        public override void ReadGenotype(IEvolutionState state, BinaryReader reader)
        {
            var treelength = reader.ReadInt32();
            if (Trees == null || treelength != Trees.Length)
            {
                // wrong size!
                state.Output.Fatal("Number of trees differ in GPIndividual when reading from ReadGenotype(EvolutionState, BinaryReader).");
                // Guard against Output being changed so that it doesn't kill the process on "Fatal".
                throw new InvalidOperationException("Number of trees differ in GPIndividual when reading from ReadGenotype(EvolutionState, BinaryReader).");
            }

            foreach (var t in Trees)
                t.ReadTree(state, reader);
        }

        #endregion // IO
    }
}