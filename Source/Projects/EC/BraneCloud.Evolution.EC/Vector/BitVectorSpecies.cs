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
using System.Text;

using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Vector
{
    /**
     * BitVectorSpecies is a subclass of VectorSpecies with special
     * constraints for boolean vectors, namely BitVectorIndividual.
     *
     * <p>BitVectorSpecies can specify some parameters globally, per-segment, and per-gene.
     * See <a href="VectorSpecies.html">VectorSpecies</a> for information on how to this works.
     *
     * <p>
     * BitVectorSpecies provides support for two ways of mutating a gene.
     * <ul>
     * <li><b>reset</b> Replacing the gene's value with a value uniformly drawn from the gene's
     * range [true, false].</li>
     * <li><b>flip</b>Flipping the bit of the gene value.  This is the default.
     * </ul>
     *
     * <p>
     * BitVectorSpecies provides support for two ways of initializing a gene.  The initialization procedure
     * is determined by the choice of mutation procedure as described above.  If the mutation is floating-point
     * (<tt>reset, gauss, polynomial</tt>), then initialization will be done by resetting the gene
     * to uniformly chosen floating-point value between the minimum and maximum legal gene values, inclusive.
     * If the mutation is integer (<tt>integer-reset, integer-random-walk</tt>), then initialization will be done
     * by performing the same kind of reset, but restricting values to integers only.
     * 
     * 
     * <p>
     * <b>Parameters</b><br>
     * <table>
     <tr><td>&nbsp;
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-type</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>segment</tt>.<i>segment-number</i>.<tt>mutation-type</tt>&nbsp;&nbsp;&nbsp;<i>or</i><br>
     <tr><td valign=top style="white-space: nowrap"><i>base</i>.<tt>mutation-prob</tt>.<i>gene-number</i><br>
     * <font size=-1><tt>reset</tt>, <tt>flip</tt>, (default=<tt>flip</tt>)</font></td>
     * <td valign=top>(the mutation type)</td>
     * </tr>
     * 
     * </table>
     */

    [ECConfiguration("ec.vector.BitVectorSpecies")]
    public class BitVectorSpecies : VectorSpecies
    {
        public const string P_MUTATIONTYPE = "mutation-type";
        public const string V_RESET_MUTATION = "reset";
        public const string V_FLIP_MUTATION = "flip";

        public const int C_RESET_MUTATION = 0;
        public const int C_FLIP_MUTATION = 1;

        /** Mutation type, per gene.
            This array is one longer than the standard genome length.
            The top element in the array represents the parameters for genes in
            genomes which have extended beyond the genome length.  */
        private int[] _mutationType;

        public int MutationType(int gene)
        {
            int[] m = _mutationType;
            if (m.Length <= gene)
                gene = m.Length - 1;
            return m[gene];
        }


        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            IParameter def = DefaultBase;

            SetupGenome(state, paramBase);

            // CREATE THE ARRAYS

            _mutationType = Fill(new int[GenomeSize + 1], -1);

            // MUTATION

            String mtype = state.Parameters.GetStringWithDefault(paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE), null);
            int mutType = C_FLIP_MUTATION;
            if (mtype == null)
                state.Output.Warning("No global mutation type given for BitVectorSpecies, assuming 'flip' mutation",
                    paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE));
            else if (mtype.Equals(V_RESET_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = C_RESET_MUTATION; // redundant
            else if (mtype.Equals(V_FLIP_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = C_FLIP_MUTATION;
            else
                state.Output.Fatal("BitVectorSpecies given a bad mutation type: "
                    + mtype, paramBase.Push(P_MUTATIONTYPE), def.Push(P_MUTATIONTYPE));
            Fill(_mutationType, mutType);


            // CALLING SUPER

            // This will cause the remaining parameters to get set up, and
            // all per-gene and per-segment parameters to get set up as well.
            // We need to do this at this point because the global params need
            // to get set up first, and also prior to the prototypical individual
            // getting setup at the end of super.setup(...).

            base.Setup(state, paramBase);
        }




        /** Called when VectorSpecies is setting up per-gene and per-segment parameters.  The index
            is the current gene whose parameter is getting set up.  The Parameters in question are the
            bases for the gene.  The postfix should be appended to the end of any parameter looked up
            (it often contains a number indicating the gene in question), such as
            state.Parameters.exists(paramBase.Push(P_PARAM).Push(postfix), def.Push(P_PARAM).Push(postfix)
                            
            <p>If you override this method, be sure to call super(...) at some point, ideally first.
        */
        protected override void LoadParametersForGene(IEvolutionState state, int index, IParameter paramBase, IParameter def, string postfix)
        {
            base.LoadParametersForGene(state, index, paramBase, def, postfix);

            String mtype = state.Parameters.GetStringWithDefault(paramBase.Push(P_MUTATIONTYPE).Push(postfix), def.Push(P_MUTATIONTYPE).Push(postfix), null);
            int mutType = -1;
            if (mtype == null) { }  // we're cool
            else if (mtype.Equals(V_RESET_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = _mutationType[index] = C_RESET_MUTATION;
            else if (mtype.Equals(V_FLIP_MUTATION, StringComparison.InvariantCultureIgnoreCase))
                mutType = _mutationType[index] = C_FLIP_MUTATION;
            else
                state.Output.Fatal("BitVectorSpecies given a bad mutation type: " + mtype,
                    paramBase.Push(P_MUTATIONTYPE).Push(postfix), def.Push(P_MUTATIONTYPE).Push(postfix));
        }
    }
}
