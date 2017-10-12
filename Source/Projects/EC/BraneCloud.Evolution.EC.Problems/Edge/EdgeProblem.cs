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
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.Problems.Edge
{
    /// <summary>
    /// EdgeProblem implements the Symbolic Edge problem.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt><br/>
    /// <font size="-1">classname, inherits or == ec.app.edge.EdgeData</font></td>
    /// <td valign="top">(the class for the prototypical GPData object for the Edge problem)</td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>data</tt></td>
    /// <td>species (the GPData object)</td></tr>
    /// </table>
    /// </summary>
    [ECConfiguration("ec.problems.edge.EdgeProblem")]
    public class EdgeProblem : GPProblem, ISimpleProblem
    {
        public const string P_GENERALIZE = "generalize";
        public const string P_ALLPOS = "allpos";
        public const string P_ALLNEG = "allneg";
        public const string P_TESTPOS = "testpos";
        public const string P_TESTNEG = "testneg";
        public const string P_MAXTEST = "maxtest";

        public const int MIN_ARRAY_SIZE = 64;

        // reading states (BAD is initial state)
        public const int BAD = 0;
        public const int READING0 = 1;
        public const int READING1 = 2;
        public const int EPSILON = 3;

        // we'll need to deep clone this one though.
        public EdgeData Input;

        // building graph
        public bool[] Start;
        public bool[] Accept;
        public int NumNodes;
        public int[] From;
        public int[] To;
        public int[] Reading;
        public int NumEdges;

        // adjacency lists
        public int[][] Reading1;
        public int[] Reading1L;
        public int[][] Reading0;
        public int[] Reading0L;
        public int[][] Epsilon;
        public int[] EpsilonL;

        // positive test
        public bool[][] PosT;
        // negative test
        public bool[][] NegT;
        // positive all
        public bool[][] PosA;
        // negative all
        public bool[][] NegA;

        // testing
        public bool[] State1;
        public bool[] State2;

        // generalize?
        public bool Generalize;

        public override object Clone()
        {
            // we don't need to copy any of our arrays, they're null until
            // we actually start using them.

            var myobj = (EdgeProblem)(base.Clone());

            // we also don't need to clone the positive/negative
            // examples, since they don't change through the course
            // of our run (I hope!)  Otherwise we'd need to clone them
            // here.

            // clone our data object
            myobj.Input = (EdgeData)(Input.Clone());
            return myobj;
        }

        public static String Fill(int num, char c)
        {
            var buf = new char[num];
            for (var x = 0; x < num; x++) buf[x] = c;
            return new String(buf);
        }

        public const int J_LEFT = 0;
        public const int J_RIGHT = 1;
        public const int J_CENTER = 2;

        public static string Justify(string s, int len, int justification)
        {
            int size = len - s.Length;
            if (size < 0) size = 0;
            switch (justification)
            {
                case J_LEFT:
                    return s + Fill(size, ' ');
                case J_RIGHT:
                    return Fill(size, ' ') + s;
                default: // (J_CENTER)
                    return Fill(size / 2, ' ') + s + Fill(size - (size / 2), ' ');
            }
        }

        public string PrintCurrentNFA()
        {
            var strsize = NumNodes.ToString().Length;
            var str = "";
            for (var x = 0; x < NumNodes; x++)
            {
                str += Justify(x.ToString(), strsize, J_RIGHT) + " " +
                    (Start[x] ? "S" : " ") + (Accept[x] ? "A" : " ") +
                    " -> ";

                if (Reading0L[x] > 0)
                {
                    str += "(0:";
                    for (var y = 0; y < Reading0L[x]; y++)
                        str += ((y > 0 ? "," : "") + Reading0[x][y]);
                    str += ") ";
                }

                if (Reading1L[x] > 0)
                {
                    str += "(1:";
                    for (var y = 0; y < Reading1L[x]; y++)
                        str += ((y > 0 ? "," : "") + Reading1[x][y]);
                    str += ") ";
                }

                if (EpsilonL[x] > 0)
                {
                    str += "(e:";
                    for (var y = 0; y < EpsilonL[x]; y++)
                        str += ((y > 0 ? "," : "") + Epsilon[x][y]);
                    str += ")";
                }
                str += "\n";
            }
            return str;
        }

        public bool[][] RestrictToSize(int size, bool[][] cases, IEvolutionState state, int thread)
        {
            var csize = cases.Length;
            if (csize < size) return cases;

            var hash = new Hashtable();
            for (var x = 0; x < size; x++)
            {
                while (true)
                {
                    var b = cases[state.Random[thread].NextInt(csize)];
                    if (!hash.Contains(b)) { hash[b] = b; break; }
                }
            }

            var newcases = new bool[size][];
            var e = hash.Keys.GetEnumerator();
            for (var x = 0; x < size; x++)
            {
                e.MoveNext();
                newcases[x] = (bool[])(e.Current);
            }

            // sort the cases -- amazing, but hashtable doesn't always
            // return the same ordering, I guess that's because it does
            // pointer hashing.  Just want to guarantee replicability!

            // is this correct?
            Array.Sort(newcases, new MyComparer());
            return newcases;
        }

        private class MyComparer : IComparer
        {
            public int Compare(Object a, Object b)
            {
                var aa = (bool[])a;
                var bb = (bool[])b;

                for (var x = 0; x < Math.Min(aa.Length, bb.Length); x++)
                    if (!aa[x] && bb[x]) return -1;
                    else if (aa[x] && !bb[x]) return 1;
                if (aa.Length < bb.Length) return -1;
                if (aa.Length > bb.Length) return 1;
                return 0;
            }
        }

        public bool[][] Slurp(Stream f) // throws IOException
        {
            var r =
                new StreamReader(new GZipStream(f, CompressionMode.Decompress));
            string bits;

            var v = new ArrayList();
            while ((bits = r.ReadLine()) != null)
            {
                bits = bits.Trim();
                var len = bits.Length;
                if (len == 0) continue; // empty line
                if (bits[0] == '#') continue; // comment
                if (bits.ToLower().Equals("e"))
                    v.Add(new bool[0]);
                else
                {
                    var b = new bool[len];
                    for (var x = 0; x < len; x++)
                        b[x] = (bits[x] == '1');
                    v.Add(b);
                }
            }
            r.Close();
            var result = new bool[v.Count][];
            //v.CopyInto(result);
            for (var i = 0; i < v.Count; i++)
            {
                result[i] = (bool[])v[i];
            }
            return result;
        }

        public void PrintBits(IEvolutionState state, bool[][] bits)
        {
            StringBuilder s;
            for (var x = 0; x < bits.Length; x++)
            {
                s = new StringBuilder();
                for (var y = 0; y < bits[x].Length; y++)
                    if (bits[x][y]) s.Append('1');
                    else s.Append('0');
                if (s.Length == 0) state.Output.Message("(empty)");
                else state.Output.Message(s.ToString());
            }
        }

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // very important, remember this
            base.Setup(state, paramBase);

            // do we generalize?
            Generalize = state.Parameters.GetBoolean(paramBase.Push(P_GENERALIZE), null, false);

            // load the test examples here

            //FileInfo ap = null;
            //FileInfo an = null;
            //FileInfo tp = null;
            //FileInfo tn = null;
            //int restriction;

            //if (Generalize)
            //{
            //    ap = state.Parameters.GetFile(paramBase.Push(P_ALLPOS), null);
            //    an = state.Parameters.GetFile(paramBase.Push(P_ALLNEG), null);
            //}

            //tp = state.Parameters.GetFile(paramBase.Push(P_TESTPOS), null);
            //tn = state.Parameters.GetFile(paramBase.Push(P_TESTNEG), null);

            //if (Generalize)
            //{
            //    if (ap == null) state.Output.Error("File doesn't exist", paramBase.Push(P_ALLPOS));
            //    if (an == null) state.Output.Error("File doesn't exist", paramBase.Push(P_ALLNEG));
            //}

            //if (tp == null) state.Output.Error("File doesn't exist", paramBase.Push(P_TESTPOS));
            //if (tn == null) state.Output.Error("File doesn't exist", paramBase.Push(P_TESTNEG));
            //state.Output.ExitIfErrors();

            Stream ap = null;
            Stream an = null;
            Stream tp = null;
            Stream tn = null;
            int restriction;

            if (Generalize)
            {
                ap = state.Parameters.GetResource(paramBase.Push(P_ALLPOS), null);
                an = state.Parameters.GetResource(paramBase.Push(P_ALLNEG), null);
            }

            tp = state.Parameters.GetResource(paramBase.Push(P_TESTPOS), null);
            tn = state.Parameters.GetResource(paramBase.Push(P_TESTNEG), null);

            if (Generalize)
            {
                if (ap == null) state.Output.Error("File doesn't exist", paramBase.Push(P_ALLPOS));
                if (an == null) state.Output.Error("File doesn't exist", paramBase.Push(P_ALLNEG));
            }

            if (tp == null) state.Output.Error("File doesn't exist", paramBase.Push(P_TESTPOS));
            if (tn == null) state.Output.Error("File doesn't exist", paramBase.Push(P_TESTNEG)); state.Output.ExitIfErrors();

            if (Generalize)
            {
                state.Output.Message("Reading Positive Examples");
                try { PosA = Slurp(ap); }
                catch (IOException e)
                {
                    state.Output.Error(
                        "IOException reading file (here it is)\n" + e, paramBase.Push(P_ALLPOS));
                }
                state.Output.Message("Reading Negative Examples");
                try { NegA = Slurp(an); }
                catch (IOException e)
                {
                    state.Output.Error(
                        "IOException reading file (here it is)\n" + e, paramBase.Push(P_ALLNEG));
                }
            }

            state.Output.Message("Reading Positive Training Examples");
            try { PosT = Slurp(tp); }
            catch (IOException e)
            {
                state.Output.Error(
                    "IOException reading file (here it is)\n" + e, paramBase.Push(P_TESTPOS));
            }
            if ((restriction = state.Parameters.GetInt(
                        paramBase.Push(P_MAXTEST), null, 1)) > 0)
            {
                // Need to restrict
                state.Output.Message("Restricting to <= " + restriction + " Unique Examples");
                PosT = RestrictToSize(restriction, PosT, state, 0);
            }

            state.Output.Message("");
            PrintBits(state, PosT);
            state.Output.Message("");

            state.Output.Message("Reading Negative Training Examples");
            try { NegT = Slurp(tn); }
            catch (IOException e)
            {
                state.Output.Error(
                    "IOException reading file (here it is)\n" + e, paramBase.Push(P_TESTNEG));
            }
            if ((restriction = state.Parameters.GetInt(
                        paramBase.Push(P_MAXTEST), null, 1)) > 0)
            {
                // Need to restrict
                state.Output.Message("Restricting to <= " + restriction + " Unique Examples");
                NegT = RestrictToSize(restriction, NegT, state, 0);
            }

            state.Output.Message("");
            PrintBits(state, NegT);
            state.Output.Message("");

            state.Output.ExitIfErrors();


            // set up our input -- don't want to use the default base, it's unsafe
            Input = (EdgeData)state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_DATA), null, typeof(EdgeData));
            Input.Setup(state, paramBase.Push(P_DATA));
        }

        public bool Test(bool[] sample)
        {
            var STATE_1 = false;
            //        final boolean STATE_2 = true;
            var st = STATE_1;

            // set initial state
            for (var x = 0; x < NumNodes; x++)
                State1[x] = Start[x];

            // run
            for (var x = 0; x < sample.Length; x++)
            {
                if (st == STATE_1)
                {
                    for (var y = 0; y < NumNodes; y++)
                        State2[y] = false;
                    for (var y = 0; y < NumNodes; y++)  // yes, *start*.length
                        if (State1[y])  // i'm in this state
                        {
                            // advance edges
                            if (sample[x]) // reading a 1
                                for (var z = 0; z < Reading1L[y]; z++)
                                    State2[Reading1[y][z]] = true;
                            else  // reading a 0
                                for (var z = 0; z < Reading0L[y]; z++)
                                    State2[Reading0[y][z]] = true;
                        }


                    // advance along epsilon boundary
                    var moreEpsilons = true;
                    while (moreEpsilons)
                    {
                        moreEpsilons = false;
                        for (var y = 0; y < NumNodes; y++)
                            if (State2[y])
                                for (var z = 0; z < EpsilonL[y]; z++)
                                {
                                    if (!State2[Epsilon[y][z]]) moreEpsilons = true;
                                    State2[Epsilon[y][z]] = true;
                                }
                    }
                }


                else //if (st==STATE_2)
                {
                    for (var y = 0; y < NumNodes; y++)
                        State1[y] = false;
                    for (var y = 0; y < NumNodes; y++)  // yes, *start*.length
                        if (State2[y])  // i'm in this state
                        {
                            // advance edges
                            if (sample[x]) // reading a 1
                                for (var z = 0; z < Reading1L[y]; z++)
                                    State1[Reading1[y][z]] = true;
                            else  // reading a 0
                                for (var z = 0; z < Reading0L[y]; z++)
                                    State1[Reading0[y][z]] = true;
                        }

                    // advance along epsilon boundary
                    var moreEpsilons = true;
                    while (moreEpsilons)
                    {
                        moreEpsilons = false;
                        for (var y = 0; y < NumNodes; y++)
                            if (State1[y])
                                for (var z = 0; z < EpsilonL[y]; z++)
                                {
                                    if (!State1[Epsilon[y][z]]) moreEpsilons = true;
                                    State1[Epsilon[y][z]] = true;
                                }
                    }
                }

                st = !st;
            }

            // am I in an accepting state?
            if (st == STATE_1)  // just loaded the result into state 1 from state 2
            {
                for (var x = 0; x < NumNodes; x++)
                    if (Accept[x] && State1[x]) return true;
            }
            else // (st==STATE_2)
            {
                for (var x = 0; x < NumNodes; x++)
                    if (Accept[x] && State2[x]) return true;
            }
            return false;
        }

        int _totpos;
        int _totneg;

        /// <summary>
        /// Tests an individual, returning its successful positives in totpos and its successful negatives in totneg.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="ind"></param>
        /// <param name="threadnum"></param>
        /// <param name="pos"></param>
        /// <param name="neg"></param>
        public void FullTest(IEvolutionState state, Individual ind, int threadnum, bool[][] pos, bool[][] neg)
        {
            // reset the graph
            NumNodes = 2;
            NumEdges = 1; From[0] = 0; To[0] = 1;
            Start[0] = Start[1] = Accept[0] = Accept[1] = false;
            Input.edge = 0;

            // generate the graph
            ((GPIndividual)ind).Trees[0].Child.Eval(
                state, threadnum, Input, Stack, ((GPIndividual)ind), this);

            // produce the adjacency matrix
            if (Reading1.Length < NumNodes ||
                Reading1[0].Length < NumEdges)
            {
                Reading1 = TensorFactory.Create<int>(NumNodes * 2, NumEdges * 2); // new int[numNodes * 2][numEdges * 2];
                Reading0 = TensorFactory.Create<int>(NumNodes * 2, NumEdges * 2); // new int[numNodes*2][numEdges*2];
                Epsilon = TensorFactory.Create<int>(NumNodes * 2, NumEdges * 2); // new int[numNodes*2][numEdges*2];
                Reading1L = new int[NumNodes * 2];
                Reading0L = new int[NumNodes * 2];
                EpsilonL = new int[NumNodes * 2];
            }

            for (int y = 0; y < NumNodes; y++)
            {
                Reading1L[y] = 0;
                Reading0L[y] = 0;
                EpsilonL[y] = 0;
            }

            for (var y = 0; y < NumEdges; y++)
                switch (Reading[y])
                {
                    case READING0:
                        Reading0[From[y]][Reading0L[From[y]]++] = To[y];
                        break;
                    case READING1:
                        Reading1[From[y]][Reading1L[From[y]]++] = To[y];
                        break;
                    case EPSILON:
                        Epsilon[From[y]][EpsilonL[From[y]]++] = To[y];
                        break;
                }

            // create the states
            if (State1.Length < NumNodes)
            {
                State1 = new bool[NumNodes * 2];
                State2 = new bool[NumNodes * 2];
            }

            // test the graph on our data

            _totpos = 0;
            _totneg = 0;
            for (var y = 0; y < pos.Length; y++)
                if (Test(pos[y])) _totpos++;
            for (var y = 0; y < neg.Length; y++)
                if (!Test(neg[y])) _totneg++;
        }

        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (Start == null)
            {
                Start = new bool[MIN_ARRAY_SIZE];
                Accept = new bool[MIN_ARRAY_SIZE];
                Reading = new int[MIN_ARRAY_SIZE];
                From = new int[MIN_ARRAY_SIZE];
                To = new int[MIN_ARRAY_SIZE];
                State1 = new bool[MIN_ARRAY_SIZE];
                State2 = new bool[MIN_ARRAY_SIZE];
                Reading1 = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Reading0 = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Epsilon = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Reading1L = new int[MIN_ARRAY_SIZE];
                Reading0L = new int[MIN_ARRAY_SIZE];
                EpsilonL = new int[MIN_ARRAY_SIZE];
            }

            if (!ind.Evaluated)  // don't bother reevaluating
            {
                FullTest(state, ind, threadnum, PosT, NegT);
                // the fitness better be KozaFitness!
                var f = ((KozaFitness)ind.Fitness);

                // this is an awful fitness metric, but it's the standard
                // one used for these problems.  :-(

                f.SetStandardizedFitness(state, (float)
                        (1.0 - ((double)(_totpos + _totneg)) /
                        (PosT.Length + NegT.Length)));

                // here are two other more reasonable fitness metrics
                /*
                  f.setStandardizedFitness(state,(float)
                  (1.0 - Math.min(((double)totpos)/posT.length,
                  ((double)totneg)/negT.length)));

                  f.setStandardizedFitness(state,(float)
                  (1.0 - (((double)totpos)/posT.length +
                  ((double)totneg)/negT.length)/2.0));
                */

                f.Hits = _totpos + _totneg;
                ind.Evaluated = true;
            }
        }

        public override void Describe(IEvolutionState state, Individual ind, int subpopulation, int threadnum, int log)
        {
            if (Start == null)
            {
                Start = new bool[MIN_ARRAY_SIZE];
                Accept = new bool[MIN_ARRAY_SIZE];
                Reading = new int[MIN_ARRAY_SIZE];
                From = new int[MIN_ARRAY_SIZE];
                To = new int[MIN_ARRAY_SIZE];
                State1 = new bool[MIN_ARRAY_SIZE];
                State2 = new bool[MIN_ARRAY_SIZE];
                Reading1 = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Reading0 = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Epsilon = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Reading1L = new int[MIN_ARRAY_SIZE];
                Reading0L = new int[MIN_ARRAY_SIZE];
                EpsilonL = new int[MIN_ARRAY_SIZE];
            }

            if (Generalize)
                FullTest(state, ind, threadnum, PosA, NegA);
            else
                FullTest(state, ind, threadnum, PosT, NegT);

            if (Generalize)
                state.Output.PrintLn("\n\nBest Individual's Generalization Score...\n" +
                    "Pos: " + _totpos + "/" + PosA.Length +
                    " Neg: " + _totneg + "/" + NegA.Length +
                    "\n(pos+neg)/(allpos+allneg):     " +
                    (float)
                    (((double)(_totpos + _totneg)) / (PosA.Length + NegA.Length)) +
                    "\n((pos/allpos)+(neg/allneg))/2: " +
                    (float)
                    (((((double)_totpos) / PosA.Length) + (((double)_totneg) / NegA.Length)) / 2) +
                    "\nMin(pos/allpos,neg/allneg):    " +
                    (float)Math.Min((((double)_totpos) / PosA.Length), (((double)_totneg) / NegA.Length)),
                    log);

            state.Output.PrintLn("\nBest Individual's NFA\n=====================\n",
                log);

            state.Output.PrintLn(PrintCurrentNFA(), log);
        }

        public String DescribeShortGeneralized(Individual ind, IEvolutionState state, int subpopulation, int threadnum)
        {
            if (Start == null)
            {
                Start = new bool[MIN_ARRAY_SIZE];
                Accept = new bool[MIN_ARRAY_SIZE];
                Reading = new int[MIN_ARRAY_SIZE];
                From = new int[MIN_ARRAY_SIZE];
                To = new int[MIN_ARRAY_SIZE];
                State1 = new bool[MIN_ARRAY_SIZE];
                State2 = new bool[MIN_ARRAY_SIZE];
                Reading1 = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Reading0 = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Epsilon = TensorFactory.Create<int>(MIN_ARRAY_SIZE, MIN_ARRAY_SIZE); // new int[MIN_ARRAY_SIZE][MIN_ARRAY_SIZE];
                Reading1L = new int[MIN_ARRAY_SIZE];
                Reading0L = new int[MIN_ARRAY_SIZE];
                EpsilonL = new int[MIN_ARRAY_SIZE];
            }

            FullTest(state, ind, threadnum, PosA, NegA);

            return ": " +
                ((double)_totpos) / PosA.Length + " " +
                ((double)_totneg) / NegA.Length + " " +
                (((double)(_totpos + _totneg)) / (PosA.Length + NegA.Length)) + " " +
                (((((double)_totpos) / PosA.Length) + (((double)_totneg) / NegA.Length)) / 2) + " " +
                Math.Min((((double)_totpos) / PosA.Length), (((double)_totneg) / NegA.Length)) + " : ";
        }
    }
}