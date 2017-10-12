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
    /// GPType is a IClique which represents types in 
    /// Strongly-Typed Genetic Programming (STGP). 
    /// (David Montana, "Strongly-Typed Genetic Programming", 
    /// <i>Evolutionary Computation</i> 3(2), pp. 199-230).
    /// 
    /// <p/>In STGP, each function node has a <i>return-type</i>, and each of
    /// its arguments also have assigned types.  Furthermore, the tree
    /// itself has a predefined "tree type".  STGP permits crossover
    /// and mutation of trees only with the constraint that each node's
    /// return type "fits" with the corresponding argument type in the
    /// node's parent; further, the root node's return type must "fit" with
    /// the tree type.
    /// 
    /// <p/>The simplest definition of "fit" is that the types must be the same.
    /// Montana calls this "Basic" STGP (see section 2.1).  This is the form
    /// of STGP that most implementations do, and it's not very powerful.
    /// 
    /// <p/>Montana further defined generic functions (ones with polymorphic
    /// data types).  Such beasts "fit" only if the trees involved can be
    /// unified to make them fit, an expensive proceedure which ECJ does not
    /// support.  However, ECJ's does support a compromise between simple 
    /// "Basic" STGP and STGP with polymorphic types: providing both 
    /// <i>atomic types</i> (basic STGP) and a more powerful notion of 
    /// <i>set types</i>.
    /// 
    /// <p/>An atomic type is a basic GP type.  Atomic types "fit" only
    /// if they're the same object (this implementation uses == ).   A problem
    /// domain that only uses atomic types is essentially doing "Basic" STGP.
    /// 
    /// <p/>Set types are sets of atomic types.  A set type "fits" with an
    /// atomic type only if it contains the atomic type in its set.  A set type
    /// "fits" with another set type only if they share at least one atomic type
    /// in common (that is, the intersection of their sets is nonempty).
    /// 
    /// <p/>Set types allow for types which can fit in several different generic
    /// places -- an object can now say that it "fits" with types
    /// A or B or C, but not D or E.
    /// 
    /// <p/>GPTypes are a IClique, and they set themselves up as a group; in general
    /// you should not create any new GPTypes.   You should also not attempt to
    /// clone them, since type equivalence is determined partially by pointer
    /// equivalence.
    /// 
    /// <p/><b>What Set and Atomic Types Can Do. </b>
    /// Set and Atomic types can be used for most of the existing literature
    /// (major exceptions: Tina Yu's work, and also work on multiplying 
    /// matricies with GP).  For example,  
    /// I am fairly certain that atomic types and set types can be used to
    /// implement any mechanism devisable using type inheritance along the lines
    /// of (Thomas Haynes, Dale Schoenefeld, and Roger Wainwright, 
    /// "Type Inheritance in Strongly Typed Genetic Programming",
    /// <i>Advances in Genetic Progrmming 2</i>, pp. 359-376.  
    /// Let's say that you wanted to define some classes a-la Haynes <i>et al</i>
    /// with multiple inheritance,
    /// say, a Vehicle, a Red-Thing, a Car (which is a Vehicle), a Truck (which
    /// is a Vehicle), and a Fire-Truck (which is a Truck and a Red-Thing).  Now, you
    /// want to guarantee that children nodes fit with parents only if the return
    /// value of the children node is a subclass of the parents' argument slot.
    /// You can do this as follows: first, you create an atomic type for each
    /// of the classes above.  Then you create the set types: Vehicle-S = {Vehicle},
    /// Red-Thing-S = {Red-Thing}, Car-S = {Car,Vehicle}, Truck-S = {Truck,Vehicle},
    /// and Fire-Truck-S = {Fire-Truck,Truck,Red-Thing}.  Then you set up your function
    /// sets so that the return type of each node is an <i>atomic type</i>, and the
    /// argument types of nodes are <i>set types</i>.  For example, if the function
    /// FOO is supposed to take a Fire Truck and a Car and return another Car, then
    /// you set the return type to Car, the first argument type to Fire-Truck-S, and the
    /// second return type to Car-S.  The rest is left up to the reader as an excercise :-)
    /// 
    /// <p/>I also believe that set types and atomic types can handle most grammar-based
    /// mechanisms I've seen, which in general appear reducable to STGP anyway;
    /// for example, in Eric Jones and William Joines, "Genetic
    /// Design of Electronic Circuits".  <i>Late-Breaking Papers at the 1999 Genetic 
    /// and Evolutionary Computatiokn Conference</i>.  124-133.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>a.size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of atomic types)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>s.size</tt><br/>
    /// <font size="-1">int &gt;= 0</font></td>
    /// <td valign="top">(number of set types)</td></tr>
    /// <tr><td valign="top"><i>base</i><tt>.a.</tt><i>n</i><tt>.Name</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of atomic type <i>n</i>.  Must be different from other GPType names)</td></tr>
    /// <tr><td valign="top"><i>base</i><tt>.s.</tt><i>n</i><tt>.Name</tt><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of set type <i>n</i>.  Must be different from other GPType names)</td></tr>
    /// <tr><td valign="top"><i>base</i><tt>.s.</tt><i>n</i><tt>.size</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(number of atomic types in the set type <i>n</i>'s set)</td></tr>
    /// <tr><td valign="top"><i>base</i><tt>.s.</tt><i>n</i><tt>.member.</tt><i>m</i><br/>
    /// <font size="-1">String</font></td>
    /// <td valign="top">(name of atomic type member <i>m</i> in set type <i>n</i>)</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPType")]
    public abstract class GPType : IClique
    {
        #region Constants

        public const string P_NAME = "name";

        #endregion // Constants
        #region Static

        /// <summary>
        /// Returns a type for a given name.
        /// You must guarantee that after calling TypeFor(...) one or
        /// several times, you call state.Output.ExitIfErrors() once. 
        /// </summary>		
        public static GPType TypeFor(string typeName, IEvolutionState state)
        {
            var myType = (GPType)(((GPInitializer)state.Initializer).TypeRepository[typeName]);
            if (myType == null)
                state.Output.Error("The GP type \"" + typeName + "\" could not be found.");
            return myType;
        }

        #endregion // Static
        #region Properties

        /// <summary>
        /// The name of the type. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The preassigned integer value for the type. 
        /// </summary>
        public int Type { get; set; }

        #endregion // Properties
        #region Setup

        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            // What's my name?
            Name = state.Parameters.GetString(paramBase.Push(P_NAME), null);
            if (String.IsNullOrEmpty(Name))
            {
                state.Output.Fatal("No name was given for this GP type.", paramBase.Push(P_NAME));
                throw new InvalidOperationException("Name cannot be null or empty.");
            }

            // Register me
            var tempObject = ((GPInitializer)state.Initializer).TypeRepository[Name];
            ((GPInitializer)state.Initializer).TypeRepository[Name] = this;

            var oldType = (GPType)tempObject;
            if (oldType != null)
            {
                state.Output.Fatal("The GP type \"" + Name + "\" has been defined multiple times.",
                                   paramBase.Push(P_NAME));
                throw new InvalidOperationException("The GP type \"" + Name + "\" has been defined multiple times.");
            }
        }

        #endregion // Setup
        #region Comparison

        /// <summary>
        /// Am I compatible with ("fit" with) <i>t</i>?  For two atomic
        /// types, this is done by direct pointer equality.  For
        /// two set types, this is done by determining if the intersection
        /// is nonempty.  A set type is compatible with an atomic type
        /// if it contains the atomic type in its set. 
        /// </summary>
        public abstract bool CompatibleWith(GPInitializer initializer, GPType t);

        #endregion // Comparison
        #region ToString

        /// <summary>
        /// Returns the type's name. 
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #endregion // ToString
    }
}