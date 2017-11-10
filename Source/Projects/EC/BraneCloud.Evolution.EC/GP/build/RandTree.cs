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
using System.Collections.Generic;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.GP.Build
{
    [Serializable]
    [ECConfiguration("ec.gp.build.RandTree")]
    public class RandTree : GPNodeBuilder
    {
        #region Constants

        public const string P_RANDOMBRANCH = "randtree";

        #endregion // Constants
        #region Static

        public class ArityObject
        {
            public readonly int Arity;
            public ArityObject(int a)
            {
                Arity = a;
            }
        }

        /// <summary>
        /// Added method to enhance linked list functionality with ArityObject
        /// </summary>
        private static bool Contains(IList<ArityObject> initArities, int a)
        {
            var truth = false;
            var counter = 0;
            ArityObject b;

            if (initArities.Count != 0)
                while ((counter < initArities.Count) && (!truth))
                {
                    b = initArities[counter];
                    if (b.Arity == a)
                    {
                        truth = true;
                    }
                    counter++;
                }
            return truth;
        }

        private static void Remove(IList<ArityObject> initArities, int a)
        {
            var counter = 0;
            var removed = false;
            while ((counter < initArities.Count) && (!removed))
            {
                var b = initArities[counter];
                if (b.Arity == a)
                {
                    initArities.RemoveAt(counter);
                    removed = true;
                }
                counter++;
            }
        }

        private static int[] Select(IList<int[]> permutations, int size)
        {
            var total = 0;
            long denominator = 1;
            int[] current;
            var quantity = new int[permutations.Count];

            for (var counter1 = 0; counter1 < permutations.Count; counter1++)
            {
                current = permutations[counter1];
                long residue = size;
                // Quick internal calculations
                for (var counter2 = 0; counter2 < current.Length; counter2++)
                {
                    residue -= current[counter2];
                    denominator *= Fact(current[counter2]);
                }
                quantity[counter1] = (int)(Fact(size - 1) / (denominator * Fact(residue)));
                total += quantity[counter1];
            }

            var prob = new double[quantity.Length];
            // quantities found... now build array for probabilities
            for (var counter1 = 0; counter1 < quantity.Length; counter1++)
            {
                prob[counter1] = quantity[counter1] / (double)total;
                // I don't think we need to check for negative values here -- Sean
            }
            RandomChoice.OrganizeDistribution(prob);
            var selection = RandomChoice.PickFromDistribution(prob, 0.0, 7);

            return permutations[selection];
        }

        private static long Fact(long num)
        {
            if (num == 0)
            {
                return 1;
            }

            return num * Fact(num - 1);
        }

        /// <summary>
        /// This function parses the dyck word and puts random nodes into their slots.
        /// </summary>
        private static GPNode BuildTree(IEvolutionState state, int thread,
            IGPNodeParent parent, int argPosition, GPFunctionSet funcs, string dyckWord)
        {
            var s = new List<GPNode>();

            // Parsing dyck word from left to right and building tree
            for (var counter = 0; counter < dyckWord.Length; counter++)
            {
                char nextChar;
                if (counter < dyckWord.Length - 1)
                {
                    nextChar = dyckWord[counter + 1];
                }
                else
                {
                    nextChar = '*';
                }
                if ((nextChar == 'x') || (nextChar == '*'))
                /* terminal node */
                {
                    var nn = funcs.Terminals[0];
                    var n = nn[state.Random[thread].NextInt(nn.Length)].LightClone();
                    n.ResetNode(state, thread); // give ERCs a chance to randomize
                    s.Add(n);
                }
                else if (nextChar == 'y')
                /* non-terminal */
                {
                    // finding arity of connection
                    var ycount = 0; /* arity */
                    var nextCharY = (nextChar == 'y');
                    counter++;
                    while ((counter < dyckWord.Length) && (nextCharY))
                    {
                        if (dyckWord[counter] == 'y')
                        {
                            ycount++;
                        }
                        if (counter < dyckWord.Length - 1)
                        {
                            nextCharY = (dyckWord[counter + 1] == 'y');
                        }
                        counter++;
                    }

                    //Arity found.  Now just choose non terminal at random.
                    var nonTerms = funcs.NodesByArity[0][ycount];
                    var nT = nonTerms[state.Random[thread].NextInt(nonTerms.Length)].LightClone();
                    // Non terminal chosen, now attaching children
                    var childcount = ycount;
                    while (childcount > 0)
                    {
                        childcount--;
                        if (s.Count == 0)
                        {
                            state.Output.Fatal("Stack underflow when building tree.");
                        }
                        var child = Pop(s);
                        child.Parent = nT;
                        child.ArgPosition = (sbyte)childcount;
                        nT.Children[childcount] = child;
                    }
                    nT.ArgPosition = 0;
                    nT.Parent = null;
                    s.Add(nT);
                    if (counter != dyckWord.Length)
                        counter--;
                }
            }
            return Pop(s);
        }

        #endregion // Static
        #region Properties

        public override IParameter DefaultBase => GPBuildDefaults.ParamBase.Push(P_RANDOMBRANCH); 
        

        public int[] Arities { get; set; }
        public bool AritySetupDone { get; set; }

        public List<int[]> Permutations { get; set; }

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            // we use size distributions -- did the user specify any?
            if (!CanPick())
                state.Output.Fatal("RandTree requires some kind of size distribution set, either with "
                    + P_MINSIZE + "/" + P_MAXSIZE + ", or with " + P_NUMSIZES + ".", paramBase, DefaultBase);
        }

        #endregion // Setup
        #region Operations

        public virtual void SetupArities(IEvolutionState state, GPFunctionSet funcs)
        {
            var noOfArities = 0;
            var current = 0;
            var initArities = new List<ArityObject>();
            var initializer = (GPInitializer)state.Initializer;

            // count available arities and place on linked list
            while (current < funcs.Nodes[0].Length)
            {
                {
                    var a = funcs.Nodes[0][current].Constraints(initializer).ChildTypes.Length;
                    if ((!Contains(initArities, a)) && (a != 0))
                    {
                        initArities.Add(new ArityObject(a));
                        noOfArities++;
                    }
                }
                current++;
            }

            if (initArities.Count == 0)
            {
                state.Output.Fatal("Arity count failed... counted 0.");
            }

            // identify different available arities and place on array
            Arities = new int[noOfArities];
            current = 0;

            while (current < noOfArities)
            {
                // finds maximum arity on the list
                var marker = 0;
                foreach (var t in initArities)
                {
                    var max = t;
                    if (max.Arity > marker)
                    {
                        marker = max.Arity;
                    }
                }

                // Place maximum found on the array
                Arities[current] = marker;
                Remove(initArities, marker);
                current++;
            }

            AritySetupDone = true;
        }

        public override GPNode NewRootedTree(IEvolutionState state, GPType type, int thread, IGPNodeParent parent,
                                                        GPFunctionSet funcs, int argPosition, int requestedSize)
        {
            var valid = false;

            var treeSize = PickSize(state, thread);

            if (!AritySetupDone)
            {
                SetupArities(state, funcs);
            }

            var temp = new int[Arities.Length];
            Permutations = new List<int[]>();
            Permute(0, temp, treeSize - 1);
            if (Permutations.Count == 0)
            {
                state.Output.Fatal("Not able to build combination of nodes.");
            }
            var scheme = Select(Permutations, treeSize);
            var word = BuildDyckWord(treeSize, Arities, scheme, state, thread);
            var cycle = 0;
            while (!valid)
            {
                valid = CheckDyckWord(word);
                if (!valid)
                {
                    word = String.Concat(word.Substring(word.Length - 1, word.Length),
                                         word.Substring(0, word.Length - 1));
                    cycle++;
                    if (cycle >= (treeSize * 2) - 1)
                    {
                        state.Output.Fatal("Not able to find valid permutation for generated Dyck word: " + word);
                    }
                }
            }
            return BuildTree(state, thread, parent, argPosition, funcs, word);
        }

        /// <summary>
        /// Recursive function to work out all combinations and push them onto ArrayList
        /// </summary>
        private void Permute(int current, int[] sol, int size)
        {
            var counter = 0;
            var result = 0;
            // base case
            if (current == Arities.Length - 1)
            /* set last one to maximum allowable */
            {
                while (result <= size)
                {
                    counter++;
                    result = result + Arities[current];
                }
                result = result - Arities[current];
                counter--;
                if (result < 0)
                {
                    result = 0;
                    counter = 0;
                }
                sol[current] = counter;

                //Adding this solution to the list.
                Permutations.Add(sol);
            }
            // end of base case
            else
            {
                while (result <= size)
                {
                    if (result <= size)
                    {
                        sol[current] = counter;
                        Permute(current + 1, sol, size - result);
                    }
                    result = result + Arities[current];
                    counter++;
                }
            }
        }

        public virtual string BuildDyckWord(int requestedSize, int[] arities, int[] s, IEvolutionState state, int thread)
        {
            var checksum = 0;
            var size = 0;

            var dyck = "";
            var addStr = "";

            var scheme = s;
            int counter;
            for (counter = 0; counter < scheme.Length; counter++)
            {
                checksum += scheme[counter] * arities[counter];
            }

            size = checksum + 1;
            if (size != requestedSize)
            {
                state.Output.Message("A tree of the requested size could not be built.  Using smaller size.");
            }
            var choices = size;

            for (counter = 0; counter < size; counter++)
            {
                dyck = dyck + "x*";
            }

            // Find a non-0 arity to insert
            counter = 0;
            var arity = 0;

            while ((arity == 0) && (counter < scheme.Length))
            {
                if (scheme[counter] > 0)
                {
                    arity = arities[counter];
                    scheme[counter]--;
                }
                counter++;
            }

            while (arity != 0)
            {
                var choice = state.Random[thread].NextInt(choices--) + 1;
                var pos = -1;
                counter = 0;
                // find insertion position within the string
                while (counter != choice)
                {
                    pos++;
                    if (dyck[pos] == '*')
                    {
                        counter++;
                    }
                }
                // building no of y's in string
                addStr = "";
                while (addStr.Length < arity)
                {
                    addStr = addStr + "y";
                }

                // finally put the string together again
                dyck = dyck.Substring(0, pos) + addStr + dyck.Substring(pos + 1, dyck.Length);

                // Find another non-0 arity to insert
                counter = 0;
                arity = 0;
                while (arity == 0 && counter < scheme.Length)
                {
                    if (scheme[counter] > 0)
                    {
                        arity = arities[counter];
                        scheme[counter]--;
                    }
                    counter++;
                }
            }
            //Clean up leftover *'s
            for (counter = 0; counter < dyck.Length; counter++)
            {
                if (dyck[counter] == '*')
                {
                    dyck = dyck.Substring(0, counter) + dyck.Substring(counter + 1, dyck.Length);
                }
            }
            return dyck;
        }

        /// <summary>
        /// function to check validity of Dyck word
        /// </summary>
        public virtual bool CheckDyckWord(string dyck)
        {
            var counter = 0;
            var underflow = false;
            var stack = "";
            while ((counter < dyck.Length) && (!underflow))
            {
                switch (dyck[counter])
                {

                    case 'x':
                        {
                            stack = stack + "x";
                            break;
                        }
                    case 'y':
                        {
                            if (stack.Length <= 1)
                            {
                                underflow = true;
                                stack = "";
                            }
                            else
                            {
                                stack = stack.Substring(0, stack.Length - 1);
                            }
                            break;
                        }
                }
                counter++;
            }
            if (stack.Length != 1)
            {
                return false;
            }
            return true;
        }

        #endregion // Operations
        #region Support

        /// <summary>
        /// Removes the element at the top of the stack and returns it.
        /// </summary>
        /// <param name="stack">The stack where the element at the top will be returned and removed.</param>
        /// <returns>The element at the top of the stack.</returns>
        private static GPNode Pop(IList<GPNode> stack)
        {
            var obj = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            return obj;
        }

        #endregion // Support
    }
}