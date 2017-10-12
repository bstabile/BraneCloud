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
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// GPInitializer is a SimpleInitializer which sets up all the ICliques,
    /// (the initial [tree/node] constraints, types, and function sets) for the GP system.
    /// 
    /// <p/>Note that the ICliques must be set up in a very particular order:
    /// <ol><li/>GPType<li/>GPNodeConstraints<li/>GPFunctionSets<li/>GPTreeConstraints</ol>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>gp.Type</tt></td>
    /// <td>GPTypes</td></tr>
    /// <tr><td valign="top"><tt>gp.nc</tt></td>
    /// <td>GPNodeConstraints</td></tr>
    /// <tr><td valign="top"><tt>gp.tc</tt></td>
    /// <td>GPTreeConstraints</td></tr>
    /// <tr><td valign="top"><tt>gp.fs</tt></td>
    /// <td>GPFunctionSets</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPInitializer")]
    public class GPInitializer : SimpleInitializer
    {
        #region Constants

        // used just here, so far as I know :-)
        public const int SIZE_OF_BYTE = 256;
        public const string P_TYPE = "type";
        public const string P_NODECONSTRAINTS = "nc";
        public const string P_TREECONSTRAINTS = "tc";
        public const string P_FUNCTIONSETS = "fs";
        public const string P_SIZE = "size";
        public const string P_ATOMIC = "a";
        public const string P_SET = "s";

        #endregion // Constants
        #region Properties

        /// <summary> 
        /// TODO Comment these members.
        /// TODO Make clients of these members more efficient by reducing unnecessary casting.
        /// </summary>
        public Hashtable TypeRepository { get; set; }

        public GPType[] Types { get; set; }
        public int NumAtomicTypes { get; set; }
        public int NumSetTypes { get; set; }

        // BRS: TODO : Should NodeConstraints be List<GPNodeConstraint> with int as number allowed?
        public Hashtable NodeConstraintRepository { get; set; }
        public List<GPNodeConstraints> NodeConstraints { get; set; }
        public int NumNodeConstraints { get; set; }

        public Hashtable FunctionSetRepository { get; set; }

        // BRS: TODO : Should TreeConstraints be List<GPTreeConstraint> with int as number allowed?
        public Hashtable TreeConstraintRepository { get; set; }
        public List<GPTreeConstraints> TreeConstraints { get; set; }
        public int NumTreeConstraints { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // TODO Move Setup methods to the corresponding GP type.

            // This is a good place to set up the types.  We use our own base off the
            // default GP base.  This MUST be done before loading constraints.
            SetupTypes(state, GPDefaults.ParamBase.Push(P_TYPE));
            
            // Now let's load our constraints and function sets also.
            // This is done in a very specific order, don't change it or things
            // will break.
            SetupNodeConstraints(state, GPDefaults.ParamBase.Push(P_NODECONSTRAINTS));
            SetupFunctionSets(state, GPDefaults.ParamBase.Push(P_FUNCTIONSETS));
            SetupTreeConstraints(state, GPDefaults.ParamBase.Push(P_TREECONSTRAINTS));
        }
        
        /// <summary>
        /// Sets up all the types, loading them from the parameter file.  This
        /// must be called before anything is called which refers to a type by name. 
        /// </summary>		
        public virtual void  SetupTypes(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing GP Types");
            
            TypeRepository = Hashtable.Synchronized(new Hashtable());
            NumAtomicTypes = NumSetTypes = 0;
            
            // How many atomic types do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_ATOMIC).Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of GP atomic types must be at least 1.", paramBase.Push(P_ATOMIC).Push(P_SIZE));
            
            // Load our atomic types
            
            for (var y = 0; y < x; y++)
                new GPAtomicType().Setup(state, paramBase.Push(P_ATOMIC).Push("" + y));
            
            // How many set types do we have?
            if (state.Parameters.ParameterExists(paramBase.Push(P_SET).Push(P_SIZE), null))
            {
                x = state.Parameters.GetInt(paramBase.Push(P_SET).Push(P_SIZE), null, 1);
                if (x < 0)
                    state.Output.Fatal("The number of GP set types must be at least 0.", paramBase.Push(P_SET).Push(P_SIZE));
            }
            // no set types
            else
                x = 0;
            
            // Load our set types
            
            for (var y = 0; y < x; y++)
                new GPSetType().Setup(state, paramBase.Push(P_SET).Push("" + y));
            
            // Postprocess the types
            PostProcessTypes();
        }
        
        /// <summary>
        /// Assigns unique integers to each atomic type, and sets up compatibility
        /// arrays for set types.  If you add new types (heaven forbid), you
        /// should call this method again to get all the types set up properly. 
        /// However, you will have to set up the function sets again as well,
        /// as their arrays are based on these type numbers. 
        /// </summary>
        public virtual void  PostProcessTypes()
        {
            // assign positive integers and 0 to atomic types
            var x = 0;
            var e = TypeRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var t = (GPType) (e.Current);
                if (t is GPAtomicType)
                {
                    t.Type = x; x++;
                }
            }
            
            // at this point, x holds the number of atomic types.
            NumAtomicTypes = x;
            
            // assign additional positive integers to set types
            // and set up arrays for the set types
            e = TypeRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var t = (GPType) (e.Current);
                if (t is GPSetType)
                {
                    ((GPSetType) t).PostProcessSetType(NumAtomicTypes);
                    t.Type = x; x++;
                }
            }
            
            // at this point, x holds the number of set types + atomic types
            NumSetTypes = x - NumAtomicTypes;
            
            // make an array for convenience.  Presently rarely used.
            Types = new GPType[NumSetTypes + NumAtomicTypes];
            e = TypeRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var t = (GPType)(e.Current);
                Types[t.Type] = t;
            }
        }
        
        /// <summary>
        /// Sets up all the GPNodeConstraints, loading them from the parameter file.  
        /// This must be called before anything is called which refers to a type by name. 
        /// </summary>	
        public virtual void SetupNodeConstraints(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing GP Node Constraints");
            
            NodeConstraintRepository = Hashtable.Synchronized(new Hashtable());
            NodeConstraints = new List<GPNodeConstraints>();
            NumNodeConstraints = 0;
            
            // How many GPNodeConstraints do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of GP node constraints must be at least 1.", paramBase.Push(P_SIZE));
            
            // Load our constraints
            for (var y = 0; y < x; y++)
            {
                GPNodeConstraints c;
                // Figure the constraint class
                if (state.Parameters.ParameterExists(paramBase.Push("" + y), null))
                    c = (GPNodeConstraints) (state.Parameters.GetInstanceForParameterEq(paramBase.Push("" + y), null, typeof(GPNodeConstraints)));
                else
                {
                    state.Output.Message("No GP Node Constraints specified, assuming the default class: ec.gp.GPNodeConstraints for " + paramBase.Push("" + y));
                    c = new GPNodeConstraints();
                }
                c.Setup(state, paramBase.Push("" + y));
            }
            
            // set our constraints array up
            var e = NodeConstraintRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var c = (GPNodeConstraints) (e.Current);
                c.ConstraintIndex = NodeConstraints.Count;
                NodeConstraints.Add(c);
                NumNodeConstraints = NodeConstraints.Count;
            }
        }
                
        public virtual void SetupFunctionSets(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing GP Function Sets");
            
            FunctionSetRepository = Hashtable.Synchronized(new Hashtable());
            // How many GPFunctionSets do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of GPFunctionSets must be at least 1.", paramBase.Push(P_SIZE));
            
            // Load our FunctionSet
            for (var y = 0; y < x; y++)
            {
                GPFunctionSet c;
                // Figure the GPFunctionSet class
                if (state.Parameters.ParameterExists(paramBase.Push("" + y), null))
                    c = (GPFunctionSet) (state.Parameters.GetInstanceForParameterEq(paramBase.Push("" + y), null, typeof(GPFunctionSet)));
                else
                {
                    state.Output.Message("No GPFunctionSet specified, assuming the default class: ec.gp.GPFunctionSet for " + paramBase.Push("" + y));
                    c = new GPFunctionSet();
                }
                c.Setup(state, paramBase.Push("" + y));
            }
        }
            
        /// <summary>
        /// Sets up all the GPTreeConstraints, loading them from the parameter file.  
        /// This must be called before anything is called which refers to a type by name. 
        /// </summary>		
        public virtual void  SetupTreeConstraints(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing GP Tree Constraints");
            
            TreeConstraintRepository = Hashtable.Synchronized(new Hashtable());
            TreeConstraints = new List<GPTreeConstraints>();
            NumTreeConstraints = 0;
            // How many GPTreeConstraints do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of GP tree constraints must be at least 1.", paramBase.Push(P_SIZE));
            
            // Load our constraints
            for (var y = 0; y < x; y++)
            {
                GPTreeConstraints c;
                // Figure the constraint class
                if (state.Parameters.ParameterExists(paramBase.Push("" + y), null))
                    c = (GPTreeConstraints) (state.Parameters.GetInstanceForParameterEq(paramBase.Push("" + y), null, typeof(GPTreeConstraints)));
                else
                {
                    state.Output.Message("No GP Tree Constraints specified, assuming the default class: ec.gp.GPTreeConstraints for " + paramBase.Push("" + y));
                    c = new GPTreeConstraints();
                }
                c.Setup(state, paramBase.Push("" + y));
            }
            
            // set our constraints array up
            var e = TreeConstraintRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var c = (GPTreeConstraints) (e.Current);
                c.ConstraintsIndex = NumTreeConstraints;
                TreeConstraints.Add(c);
                NumTreeConstraints = TreeConstraints.Count;
            }
        }

        #endregion // Setup
        #region Operations

        #endregion // Operations
    }
}

/* CODE BEFORE CHANGING CONSTRAINTS TO USE GENERIC LISTS (TREE AND NODE CONSTRAINTS)!!!!
    /// <summary> 
    /// GPInitializer is a SimpleInitializer which sets up all the ICliques,
    /// (the initial [tree/node] constraints, types, and function sets) for the GP system.
    /// 
    /// <p/>Note that the ICliques must be set up in a very particular order:
    /// <ol><li/>GPType<li/>GPNodeConstraints<li/>GPFunctionSets<li/>GPTreeConstraints</ol>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt>gp.Type</tt></td>
    /// <td>GPTypes</td></tr>
    /// <tr><td valign="top"><tt>gp.nc</tt></td>
    /// <td>GPNodeConstraints</td></tr>
    /// <tr><td valign="top"><tt>gp.tc</tt></td>
    /// <td>GPTreeConstraints</td></tr>
    /// <tr><td valign="top"><tt>gp.fs</tt></td>
    /// <td>GPFunctionSets</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.gp.GPInitializer")]
    public class GPInitializer : SimpleInitializer
    {
        #region Constants

        // used just here, so far as I know :-)
        public const int SIZE_OF_BYTE = 256;
        public const string P_TYPE = "type";
        public const string P_NODECONSTRAINTS = "nc";
        public const string P_TREECONSTRAINTS = "tc";
        public const string P_FUNCTIONSETS = "fs";
        public const string P_SIZE = "size";
        public const string P_ATOMIC = "a";
        public const string P_SET = "s";

        #endregion // Constants
        #region Properties

        /// <summary> 
        /// TODO Comment these members.
        /// TODO Make clients of these members more efficient by reducing unnecessary casting.
        /// </summary>
        public Hashtable TypeRepository { get; set; }

        public GPType[] Types { get; set; }
        public int NumAtomicTypes { get; set; }
        public int NumSetTypes { get; set; }

        // BRS: TODO : Should NodeConstraints be List<GPNodeConstraint> with int as number allowed?
        public Hashtable NodeConstraintRepository { get; set; }
        public GPNodeConstraints[] NodeConstraints { get; set; }
        public sbyte NumNodeConstraints { get; set; }

        public Hashtable FunctionSetRepository { get; set; }

        // BRS: TODO : Should TreeConstraints be List<GPTreeConstraint> with int as number allowed?
        public Hashtable TreeConstraintRepository { get; set; }
        public GPTreeConstraints[] TreeConstraints { get; set; }
        public sbyte NumTreeConstraints { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // TODO Move Setup methods to the corresponding GP type.

            // This is a good place to set up the types.  We use our own base off the
            // default GP base.  This MUST be done before loading constraints.
            SetupTypes(state, GPDefaults.ParamBase.Push(P_TYPE));
            
            // Now let's load our constraints and function sets also.
            // This is done in a very specific order, don't change it or things
            // will break.
            SetupNodeConstraints(state, GPDefaults.ParamBase.Push(P_NODECONSTRAINTS));
            SetupFunctionSets(state, GPDefaults.ParamBase.Push(P_FUNCTIONSETS));
            SetupTreeConstraints(state, GPDefaults.ParamBase.Push(P_TREECONSTRAINTS));
        }
        
        /// <summary>
        /// Sets up all the types, loading them from the parameter file.  This
        /// must be called before anything is called which refers to a type by name. 
        /// </summary>		
        public virtual void  SetupTypes(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing GP Types");
            
            TypeRepository = Hashtable.Synchronized(new Hashtable());
            NumAtomicTypes = NumSetTypes = 0;
            
            // How many atomic types do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_ATOMIC).Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of GP atomic types must be at least 1.", paramBase.Push(P_ATOMIC).Push(P_SIZE));
            
            // Load our atomic types
            
            for (var y = 0; y < x; y++)
                new GPAtomicType().Setup(state, paramBase.Push(P_ATOMIC).Push("" + y));
            
            // How many set types do we have?
            if (state.Parameters.ParameterExists(paramBase.Push(P_SET).Push(P_SIZE), null))
            {
                x = state.Parameters.GetInt(paramBase.Push(P_SET).Push(P_SIZE), null, 1);
                if (x < 0)
                    state.Output.Fatal("The number of GP set types must be at least 0.", paramBase.Push(P_SET).Push(P_SIZE));
            }
            // no set types
            else
                x = 0;
            
            // Load our set types
            
            for (var y = 0; y < x; y++)
                new GPSetType().Setup(state, paramBase.Push(P_SET).Push("" + y));
            
            // Postprocess the types
            PostProcessTypes();
        }
        
        /// <summary>
        /// Assigns unique integers to each atomic type, and sets up compatibility
        /// arrays for set types.  If you add new types (heaven forbid), you
        /// should call this method again to get all the types set up properly. 
        /// However, you will have to set up the function sets again as well,
        /// as their arrays are based on these type numbers. 
        /// </summary>
        public virtual void  PostProcessTypes()
        {
            // assign positive integers and 0 to atomic types
            var x = 0;
            var e = TypeRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var t = (GPType) (e.Current);
                if (t is GPAtomicType)
                {
                    t.Type = x; x++;
                }
            }
            
            // at this point, x holds the number of atomic types.
            NumAtomicTypes = x;
            
            // assign additional positive integers to set types
            // and set up arrays for the set types
            e = TypeRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var t = (GPType) (e.Current);
                if (t is GPSetType)
                {
                    ((GPSetType) t).PostProcessSetType(NumAtomicTypes);
                    t.Type = x; x++;
                }
            }
            
            // at this point, x holds the number of set types + atomic types
            NumSetTypes = x - NumAtomicTypes;
            
            // make an array for convenience.  Presently rarely used.
            Types = new GPType[NumSetTypes + NumAtomicTypes];
            e = TypeRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var t = (GPType)(e.Current);
                Types[t.Type] = t;
            }
        }
        
        /// <summary>
        /// Sets up all the GPNodeConstraints, loading them from the parameter file.  
        /// This must be called before anything is called which refers to a type by name. 
        /// </summary>	
        public virtual void SetupNodeConstraints(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing GP Node Constraints");
            
            NodeConstraintRepository = Hashtable.Synchronized(new Hashtable());
            NodeConstraints = new GPNodeConstraints[SIZE_OF_BYTE];
            NumNodeConstraints = 0;
            
            // How many GPNodeConstraints do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of GP node constraints must be at least 1.", paramBase.Push(P_SIZE));
            
            // Load our constraints
            for (var y = 0; y < x; y++)
            {
                GPNodeConstraints c;
                // Figure the constraint class
                if (state.Parameters.ParameterExists(paramBase.Push("" + y), null))
                    c = (GPNodeConstraints) (state.Parameters.GetInstanceForParameterEq(paramBase.Push("" + y), null, typeof(GPNodeConstraints)));
                else
                {
                    state.Output.Message("No GP Node Constraints specified, assuming the default class: ec.gp.GPNodeConstraints for " + paramBase.Push("" + y));
                    c = new GPNodeConstraints();
                }
                c.Setup(state, paramBase.Push("" + y));
            }
            
            // set our constraints array up
            var e = NodeConstraintRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var c = (GPNodeConstraints) (e.Current);
                c.ConstraintCount = NumNodeConstraints;
                NodeConstraints[NumNodeConstraints] = c;
                NumNodeConstraints++;
            }
        }
                
        public virtual void SetupFunctionSets(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing GP Function Sets");
            
            FunctionSetRepository = Hashtable.Synchronized(new Hashtable());
            // How many GPFunctionSets do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of GPFunctionSets must be at least 1.", paramBase.Push(P_SIZE));
            
            // Load our FunctionSet
            for (var y = 0; y < x; y++)
            {
                GPFunctionSet c;
                // Figure the GPFunctionSet class
                if (state.Parameters.ParameterExists(paramBase.Push("" + y), null))
                    c = (GPFunctionSet) (state.Parameters.GetInstanceForParameterEq(paramBase.Push("" + y), null, typeof(GPFunctionSet)));
                else
                {
                    state.Output.Message("No GPFunctionSet specified, assuming the default class: ec.gp.GPFunctionSet for " + paramBase.Push("" + y));
                    c = new GPFunctionSet();
                }
                c.Setup(state, paramBase.Push("" + y));
            }
        }
            
        /// <summary>
        /// Sets up all the GPTreeConstraints, loading them from the parameter file.  
        /// This must be called before anything is called which refers to a type by name. 
        /// </summary>		
        public virtual void  SetupTreeConstraints(IEvolutionState state, IParameter paramBase)
        {
            state.Output.Message("Processing GP Tree Constraints");
            
            TreeConstraintRepository = Hashtable.Synchronized(new Hashtable());
            TreeConstraints = new GPTreeConstraints[SIZE_OF_BYTE];
            NumTreeConstraints = 0;
            // How many GPTreeConstraints do we have?
            var x = state.Parameters.GetInt(paramBase.Push(P_SIZE), null, 1);
            if (x <= 0)
                state.Output.Fatal("The number of GP tree constraints must be at least 1.", paramBase.Push(P_SIZE));
            
            // Load our constraints
            for (var y = 0; y < x; y++)
            {
                GPTreeConstraints c;
                // Figure the constraint class
                if (state.Parameters.ParameterExists(paramBase.Push("" + y), null))
                    c = (GPTreeConstraints) (state.Parameters.GetInstanceForParameterEq(paramBase.Push("" + y), null, typeof(GPTreeConstraints)));
                else
                {
                    state.Output.Message("No GP Tree Constraints specified, assuming the default class: ec.gp.GPTreeConstraints for " + paramBase.Push("" + y));
                    c = new GPTreeConstraints();
                }
                c.Setup(state, paramBase.Push("" + y));
            }
            
            // set our constraints array up
            var e = TreeConstraintRepository.Values.GetEnumerator();
            while (e.MoveNext())
            {
                var c = (GPTreeConstraints) (e.Current);
                c.ConstraintsIndex = NumTreeConstraints;
                TreeConstraints[NumTreeConstraints] = c;
                NumTreeConstraints++;
            }
        }

        #endregion // Setup
        #region Operations

        #endregion // Operations
    }

*/