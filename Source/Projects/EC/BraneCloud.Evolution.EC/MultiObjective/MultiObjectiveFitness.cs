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

using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.MultiObjective
{
    /// <summary> 
    /// MultiObjectiveFitness is a subclass of Fitness which implements basic
    /// multi-objective mechanisms suitable for being used with a variety of
    /// multi-objective selection mechanisms, including ones using pareto-optimality.
    /// 
    /// <p/>The object contains two items: an array of floating point values
    /// representing the various multiple fitnesses (ranging from 0.0 (worst)
    /// to 1.0 inclusive).  By default, isIdealFitness() always returns false;
    /// you'll probably want to override that [if appropriate to your problem].
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>numobjectives</tt><br/>
    /// (else)<tt>multi.numobjectives</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(the number of fitnesses in the MultiFitness array)</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>criterion-is-and</tt><br/>
    /// <font size="-1"> bool = <tt>true</tt> (default) or <tt>false</tt></font></td>
    /// <td valign="top">(is the ideal individual one whose fitness values are <i>all</i> 1.0 
    /// (as opposed to one which contains <i>at least</i> one fitness value of 1.0))</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// multi.Fitness
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.multiobjective.MultiObjectiveFitness")]
    public class MultiObjectiveFitness : Fitness
    {
        #region Constants

        /// <summary>
        /// Parameter for size of objectives 
        /// </summary>
        public const string P_NUMOBJECTIVES = "num-objectives";

        /// <summary>
        /// Parameter for max fitness values
        /// </summary>
        public const string P_MAXOBJECTIVES = "max";

        /// <summary>
        /// Parameter for min fitness values
        /// </summary>
        public const string P_MINOBJECTIVES = "min";

        /// <summary>
        /// Is higher better?
        /// </summary>
        public const string P_MAXIMIZE = "maximize";

        public const string MULTI_FITNESS_POSTAMBLE = "[";
        public const string FITNESS_POSTAMBLE = "]";

        #endregion // Constants
        #region Static

        /// <summary>
        /// Remove an individual from the List, shifting the topmost individual in his place.
        /// </summary>
        static void Yank(int index, IList<Individual> list)
        {
            var size = list.Count;
            list[index] = list[size - 1];
            list.RemoveAt(size - 1);
        }

        /// <summary>
        /// Divides an array of Individuals into the Pareto front and the "nonFront" (everyone else). 
        /// The Pareto front is returned.  You may provide Lists for the front and a nonFront.
        /// If you provide null for the front, a List will be created for you.  If you provide
        /// null for the nonFront, non-front individuals will not be added to it.  This algorithm is O(n^2).
        /// </summary>
        public static IList<Individual> PartitionIntoParetoFront(Individual[] inds, IList<Individual> front, IList<Individual> nonFront)
        {
            if (front == null)
                front = new List<Individual>();

            // put the first guy in the front
            front.Add(inds[0]);

            // iterate over all the remaining individuals
            for (var i = 1; i < inds.Length; i++)
            {
                var ind = inds[i];

                var noOneWasBetter = true;
                var frontSize = front.Count;

                // iterate over the entire front
                for (var j = 0; j < frontSize; j++)
                {
                    var frontmember = front[j];

                    // if the front member is better than the individual, dump the individual and go to the next one
                    if (((MultiObjectiveFitness)(frontmember.Fitness)).ParetoDominates((MultiObjectiveFitness)(ind.Fitness)))
                    {
                        if (nonFront != null) nonFront.Add(ind);
                        noOneWasBetter = false;
                        break;  // failed.  He's not in the front
                    }
                    // if the individual was better than the front member, dump the front member.  But look over the
                    // other front members (don't break) because others might be dominated by the individual as well.
                    if (((MultiObjectiveFitness)(ind.Fitness)).ParetoDominates((MultiObjectiveFitness)(frontmember.Fitness)))
                    {
                        Yank(j, front);
                        // a front member is dominated by the new individual.  Replace him
                        frontSize--; // member got removed
                        j--;  // because there's another guy we now need to consider in his place
                        if (nonFront != null) nonFront.Add(frontmember);
                    }
                }
                if (noOneWasBetter)
                    front.Add(ind);
            }
            return front;
        }

        /// <summary>
        /// Divides inds into pareto front ranks (each a List), 
        /// and returns them, in order, stored in a List.
        /// </summary>
        public static IList<IList<Individual>> PartitionIntoRanks(Individual[] inds)
        {
            var frontsByRank = new List<IList<Individual>>();

            while (inds.Length > 0)
            {
                var front = new List<Individual>();
                var nonFront = new List<Individual>();
                PartitionIntoParetoFront(inds, front, nonFront);

                // build inds out of remainder
                inds = nonFront.ToArray();
                frontsByRank.Add(front);
            }
            return frontsByRank;
        }

        #endregion // Static
        #region Properties

        public override IParameter DefaultBase
        {
            get { return MultiObjectiveDefaults.ParamBase.Push(P_FITNESS); }
        }

        /// <summary>
        /// Returns true if this fitness is the "ideal" fitness. 
        /// Default always returns false.  
        /// You may want to override this. 
        /// </summary>
        public override bool IsIdeal { get { return false; } }

        /// <summary>
        /// Desired maximum fitness values. By default these are 1.0. Shared.
        /// </summary>
        public float[] MaxObjective { get { return _maxObjective; } set { _maxObjective = value; } }
        private float[] _maxObjective = new float[0]; // initialize to zero length array (in case anyone tries to access this)

        /// <summary>
        /// Desired minimum fitness values. By default these are 0.0. Shared.
        /// </summary>
        public float[] MinObjective { get { return _minObjective; } set { _minObjective = value; } }
        private float[] _minObjective = new float[0]; // initialize to zero length array (in case anyone tries to access this)

        /// <summary>
        /// The various fitnesses (values range from 0 (worst) to 1 INCLUSIVE).
        /// </summary>
        public float[] Objectives
        {
            get { return _objectives; }
            set { _objectives = value; }
        }

        private float[] _objectives = new float[0];

        public bool Maximize
        {
            get { return _maximize; }
            set { _maximize = value; }
        }
        private bool _maximize = true;

        public bool IsMaximizing
        {
            get { return _maximize; }
        }

        public int NumObjectives
        {
            get { return _objectives.Length; }
        }

        /// <summary>
        /// Returns the Max() of MultiFitnesses, which adheres to the <see cref="Fitness"/> 
        /// protocol for this method. Though you should not rely on a selection
        /// or statistics method which requires this.  
        /// </summary>
        public override float Value
        {
            get
            {
                var fit = _objectives[0];
                for (var x = 1; x < _objectives.Length; x++)
                    if (fit < _objectives[x])
                        fit = _objectives[x];
                return fit;
            }
        }

        // BRS : Use "Value" property instead. 
        // It is too confusing to have so many different names associated with a Fitness type's "Fitness".
        ///// <summary>
        ///// Returns the Max() of objectives, which adheres to the <see cref="Fitness"/> protocol
        ///// for this method. Though you should not rely on a selection or statistics
        ///// method which requires this.
        ///// </summary>
        ///// <returns></returns>
        //public virtual float Fitness
        //{
        //    get
        //    {
        //        var fit = Objectives[0];
        //        for (var x = 1; x < Objectives.Length; x++)
        //            if (fit < Objectives[x])
        //                fit = Objectives[x];
        //        return fit;
        //    }
        //}

        #endregion // Properties
        #region Setup

        public MultiObjectiveFitness()
        {
        }

        /// <summary>
        /// This constructor is useful in testing when we do not have 
        /// a parameter database available to set up the instance properly.
        /// This allows us to get the MinObjective, MaxObjective, and Objective
        /// arrays synchronized to a valid dimension (the default is zero-length).
        /// </summary>
        /// <param name="numObjectives">The number of objective values required.</param>
        public MultiObjectiveFitness(int numObjectives)
        {
            _minObjective = new float[numObjectives];
            _maxObjective = new float[numObjectives];
            _objectives = new float[numObjectives];
        }

        /// <summary>
        /// Sets up.  This must be called at least once in the prototype before instantiating any
        /// fitnesses that will actually be used in evolution. 
        /// </summary>
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase); // unnecessary really

            var def = DefaultBase;

            var numFitnesses = state.Parameters.GetInt(paramBase.Push(P_NUMOBJECTIVES), def.Push(P_NUMOBJECTIVES), 0);
            if (numFitnesses <= 0)
                state.Output.Fatal("The number of objectives must be an integer >= 1.", paramBase.Push(P_NUMOBJECTIVES),
                                   def.Push(P_NUMOBJECTIVES));

            _maximize = state.Parameters.GetBoolean(paramBase.Push(P_MAXIMIZE), def.Push(P_MAXIMIZE), true);

            _objectives = new float[numFitnesses];
            MaxObjective = new float[numFitnesses];
            MinObjective = new float[numFitnesses];

            for (var i = 0; i < numFitnesses; i++)
            {
                // load default globals
                MinObjective[i] = state.Parameters.GetFloatWithDefault(paramBase.Push(P_MINOBJECTIVES),
                                                                       def.Push(P_MINOBJECTIVES), 0.0f);
                MaxObjective[i] = state.Parameters.GetFloatWithDefault(paramBase.Push(P_MAXOBJECTIVES),
                                                                       def.Push(P_MAXOBJECTIVES), 1.0f);

                // load specifics if any
                MinObjective[i] = state.Parameters.GetFloatWithDefault(paramBase.Push(P_MINOBJECTIVES).Push("" + i),
                                                                       def.Push(P_MINOBJECTIVES).Push("" + i), MinObjective[i]);
                MaxObjective[i] = state.Parameters.GetFloatWithDefault(paramBase.Push(P_MAXOBJECTIVES).Push("" + i),
                                                                       def.Push(P_MAXOBJECTIVES).Push("" + i), MaxObjective[i]);

                // test for validity
                if (MinObjective[i] >= MaxObjective[i])
                    state.Output.Error("For objective " + i + "the min fitness must be strictly less than the max fitness.");
            }
            state.Output.ExitIfErrors();
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Returns auxilliary fitness value names to be printed by the statistics object.
        /// By default, an empty array is returned, but various algorithms may override this to provide additional columns.
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetAuxilliaryFitnessNames() { return new string[] { }; }

        /// <summary>
        /// Returns auxilliary fitness values to be printed by the statistics object.
        /// By default, an empty array is returned, but various algorithms may override this to provide additional columns.
        /// </summary>
        /// <returns></returns>
        public virtual double[] GetAuxilliaryFitnessValues() { return new double[] { }; }

        /// <summary>
        /// Returns the objectives as an array. Note that this is the *actual array*.
        /// Though you could set values in this array, you should NOT do this --
        /// rather, set them using SetObjectives().
        /// </summary>
        /// <returns></returns>
        public float[] GetObjectives()
        {
            return _objectives;
        }

        public float GetObjective(int i)
        {
            return _objectives[i];
        }

        public void SetObjectives(IEvolutionState state, float[] newObjectives)
        {
            if (newObjectives == null)
            {
                state.Output.Fatal("Null objective array provided to MultiObjectiveFitness.");
                throw new ArgumentNullException("newObjectives");
            }
            if (newObjectives.Length != _objectives.Length)
            {
                state.Output.Fatal("New objective array length does not match current length.");
            }
            for (var i = 0; i < newObjectives.Length; i++)
            {
                var f = newObjectives[i];
                if (f == Single.PositiveInfinity || f == Single.NegativeInfinity || Single.IsNaN(f))
                {
                    state.Output.Warning("Bad objective #" + i + ": " + f + ", setting to worst value for that objective.");
                    if (_maximize)
                        newObjectives[i] = MinObjective[i];
                    else
                        newObjectives[i] = MaxObjective[i];
                }
            }
            _objectives = newObjectives;
        }

        /// <summary>
        /// Returns true if I'm better than _fitness. The rule I'm using is this: if
        /// I am better in one or more criteria, and we are equal in the others, then
        /// betterThan is true, else it is false.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool ParetoDominates(MultiObjectiveFitness other)
        {
            var abeatsb = false;
            if (_maximize != other._maximize)
                throw new ApplicationException(
                    "Attempt made to compare two multiobjective fitnesses; but one expects higher values to be better and the other expectes lower values to be better.");
            if (_objectives.Length != other._objectives.Length)
                throw new ApplicationException("Attempt made to compare two multiobjective fitnesses; but they have different numbers of objectives.");
            if (_maximize)
            {
                for (var x = 0; x < _objectives.Length; x++)
                {
                    if (_objectives[x] > other._objectives[x])
                        abeatsb = true;
                    else if (_objectives[x] < other._objectives[x])
                        return false;
                }
            }
            else
            {
                for (var x = 0; x < _objectives.Length; x++)
                {
                    if (_objectives[x] < other._objectives[x])
                        abeatsb = true;
                    else if (_objectives[x] > other._objectives[x])
                        return false;
                }
            }
            return abeatsb;
        }

        /* Returns the Pareto rank for each individual.  Rank 0 is the best rank, then rank 1, and so on.  This is O(n) but it has a high constant overhead because it
            allocates a hashmap and does some autoboxing. */
        public static int[] GetRankings(Individual[] inds)
        {
            var r = new int[inds.Length];
            var ranks = PartitionIntoRanks(inds);  // get all the ranks

            // build a mapping of Individual -> index in inds array
            var m = new Dictionary<Individual, int>();
            for (var i = 0; i < inds.Length; i++)
                m.Add(inds[i], i);

            var numRanks = ranks.Count;
            for (var rank = 0; rank < numRanks; rank++)  // for each rank...
            {
                var front = ranks[rank];
                var numInds = front.Count;
                for (var ind = 0; ind < numInds; ind++)  // for each individual in that rank ...
                {
                    // get the index of the individual in the inds array
                    var i = m[front[ind]];
                    r[i] = rank;  // set the rank in the corresponding ranks array
                }
            }
            return r;
        }

        /// <summary>
        /// Returns the sum of the squared difference between two Fitnesses in Objective space.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double SumSquaredObjectiveDistance(MultiObjectiveFitness other)
        {
            double s = 0;
            for (var i = 0; i < _objectives.Length; i++)
            {
                double a = (_objectives[i] - other._objectives[i]);
                s += a * a;
            }
            return s;
        }

        /// <summary>
        /// Returns the Manhattan difference between two Fitnesses in Objective space.
        /// </summary>
        public double ManhattanObjectiveDistance(MultiObjectiveFitness other)
        {
            double s = 0;
            for (var i = 0; i < _objectives.Length; i++)
            {
                s += Math.Abs(_objectives[i] - other._objectives[i]);
            }
            return s;
        }

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// Returns true if I'm equivalent in fitness (neither better nor worse) 
        /// to _fitness. The rule I'm using is this:
        /// If one of us is better in one or more criteria, and we are equal in
        /// the others, then equivalentTo is false.  If each of us is better in
        /// one or more criteria each, or we are equal in all criteria, then 
        /// equivalentTo is true.
        /// </summary>
        public override bool EquivalentTo(IFitness fitness)
        {
            var other = (MultiObjectiveFitness)fitness;
            var abeatsb = false;
            var bbeatsa = false;

            if (_maximize != other._maximize)
                throw new ApplicationException(
                    "Attempt made to compare two multiobjective fitnesses; but one expects higher values to be better and the other expectes lower values to be better.");
            if (_objectives.Length != other._objectives.Length)
                throw new ApplicationException("Attempt made to compare two multiobjective fitnesses; but they have different numbers of objectives.");
            if (_maximize)
            {
                for (var x = 0; x < _objectives.Length; x++)
                {
                    if (_objectives[x] > other._objectives[x])
                        abeatsb = true;
                    if (_objectives[x] < other._objectives[x])
                        bbeatsa = true;
                    if (abeatsb && bbeatsa)
                        return true;
                }
            }
            else
            // lower is better
            {
                for (var x = 0; x < _objectives.Length; x++)
                {
                    if (_objectives[x] < other._objectives[x])
                        abeatsb = true;
                    if (_objectives[x] > other._objectives[x])
                        bbeatsa = true;
                    if (abeatsb && bbeatsa)
                        return true;
                }
            }
            if (abeatsb || bbeatsa)
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if I'm better than _fitness. The DEFAULT rule I'm using is this: if
        /// I am better in one or more criteria, and we are equal in the others, then
        /// betterThan is true, else it is false. Multiobjective optimization algorithms may
        /// choose to override this to do something else.
        /// </summary>
        /// <param name="fitness"></param>
        /// <returns></returns>
        public override bool BetterThan(IFitness fitness)
        {
            return ParetoDominates((MultiObjectiveFitness)fitness);
        }

        #endregion // Comparison
        #region Cloning

        public override object Clone()
        {
            var f = (MultiObjectiveFitness)base.Clone();
            f._objectives = (float[])_objectives.Clone(); // cloning an array
            // note that we do NOT clone max and min fitness -- they're shared
            return f;
        }

        #endregion // Cloning
        #region ToString

        /// <summary>
        /// <tt> Fitness: [</tt><i>fitness values encoded with ec.util.Code, separated by spaces</i><tt>]</tt>
        /// </summary>
        public override string FitnessToString()
        {
            var s = FITNESS_PREAMBLE + MULTI_FITNESS_POSTAMBLE;
            for (var x = 0; x < _objectives.Length; x++)
            {
                if (x > 0)
                    s = s + " ";
                s = s + Code.Encode(_objectives[x]);
            }
            s = s + " ";
            s = s + Code.Encode(_maximize);
            return s + FITNESS_POSTAMBLE;
        }

        /// <summary>
        /// <tt> Fitness: [</tt><i>fitness values encoded with ec.util.Code, separated by spaces</i><tt>]</tt>
        /// </summary>
        public override string FitnessToStringForHumans()
        {
            var s = FITNESS_PREAMBLE + MULTI_FITNESS_POSTAMBLE;
            for (var x = 0; x < _objectives.Length; x++)
            {
                if (x > 0)
                    s = s + " ";
                s = s + _objectives[x];
            }
            s = s + " ";
            s = s + (_maximize ? "max" : "min");
            return s + FITNESS_POSTAMBLE;
        }

        #endregion // ToString
        #region IO

        public override void ReadFitness(IEvolutionState state, StreamReader reader)
        {
            var d = Code.CheckPreamble(FITNESS_PREAMBLE + MULTI_FITNESS_POSTAMBLE, state, reader);
            for (var x = 0; x < _objectives.Length; x++)
            {
                Code.Decode(d);
                if (d.Type != DecodeReturn.T_FLOAT)
                    state.Output.Fatal("Reading Line " + d.LineNumber + ": " + "Bad Fitness (objectives value #" + x + ").");
                _objectives[x] = (float)d.D;
            }
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_BOOLEAN || !d.B.HasValue)
                state.Output.Fatal("Reading Line " + d.LineNumber + ": " + "Information missing about whether higher is better");

            _maximize = !d.B.HasValue || d.B.Value; // Default is true (but if HasValue == false we will be aborting anyway!)
        }

        public override void WriteFitness(IEvolutionState state, BinaryWriter writer)
        {
            writer.Write(_objectives.Length);
            foreach (var t in _objectives)
                writer.Write(t);
            writer.Write(_maximize);
            WriteTrials(state, writer);
        }

        public override void ReadFitness(IEvolutionState state, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            if (_objectives == null || _objectives.Length != len)
                _objectives = new float[len];
            for (var x = 0; x < _objectives.Length; x++)
                _objectives[x] = reader.ReadSingle();
            _maximize = reader.ReadBoolean();
            ReadTrials(state, reader);
        }

        #endregion // IO
    }
}