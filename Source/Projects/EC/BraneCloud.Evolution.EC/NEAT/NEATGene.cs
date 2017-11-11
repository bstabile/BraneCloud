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
using System.Text;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATGene is the combination of class Gene and class Link in original code. It
     * is used to represent a single connection between two nodes (NEATNode) of a
     * neural network, and extends the abstract Gene class to make use of its
     * read/write utilities.
     * 
     * @author Ermo Wei and David Freelan
     *
     */
    [ECConfiguration("ec.neat.NEATGene")]
    public class NEATGene : Gene
    {
        // BRS: Defined in Gene
        //public const string P_GENE = "gene";

        /** The weight of link this gene is represent. */
        public double Weight;

        /** The actual in node this gene connect to. */
        public NEATNode InNode;

        /** The actual out node this gene connect to. */
        public NEATNode OutNode;

        /**
         * The id of the in node, this is useful in reading a gene from file, we
         * will use this id to find the actual node after we finish reading the
         * genome file.
         */
        public int InNodeId;

        /**
         * The id of the in node, this is useful in reading a gene from file, we
         * will use this id to find the actual node after we finish reading the
         * genome file.
         */
        public int OutNodeId;

        /** Is the link this gene represent a recurrent link. */
        public bool IsRecurrent;

        /** Time delay of the link, used in network activation. */
        public bool TimeDelay;

        /** The innovation number of this link. */
        public int InnovationNumber;

        /**
         * The mutation number of this gene, Used to see how much mutation has
         * changed.
         */
        public double MutationNumber;

        /** Is the link this gene represent is enable in network activation. */
        public bool Enable;

        /**
         * Is this gene frozen, a frozen gene's weight cannot get mutated in
         * breeding procedure.
         */
        public bool Frozen;

        /**
         * The setup method initializes a "meaningless" gene that does not specify
         * any connection.
         */
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            Weight = 0.0;
            // node id 1-indexed
            InNodeId = 0;
            OutNodeId = 0;
            InNode = null;
            OutNode = null;
            IsRecurrent = false;
            InnovationNumber = 0;
            MutationNumber = 0.0;
            TimeDelay = false;
            Enable = true;
            Frozen = false;
        }

        public override IParameter DefaultBase => NEATDefaults.ParamBase.Push(P_GENE);

        public override void Reset(IEvolutionState state, int thread)
        {
            // frozen and timeDelay are not read from template genome, we set it
            // here
            Frozen = false;
            TimeDelay = false;
        }

        /** Reset the gene with given parameters. */
        public void Reset(double w, int iNodeId, int oNodeId, bool recur, int innov, double mutNum)
        {
            // Gene::Gene(double w, NNode *inode, NNode *onode, bool recur, double
            // innov, double mnum)
            Weight = w;
            InNodeId = iNodeId;
            OutNodeId = oNodeId;
            InNode = null;
            OutNode = null;
            IsRecurrent = recur;
            InnovationNumber = innov;
            MutationNumber = mutNum;
            TimeDelay = false;
            Enable = true;
            Frozen = false;
        }

        public override object Clone()
        {
            // Gene::Gene(Gene *g,Trait *tp,NNode *inode,NNode *onode)
            // we do not clone the inNode and outNode instance
            NEATGene myobj = (NEATGene) base.Clone();
            myobj.Weight = Weight;
            myobj.IsRecurrent = IsRecurrent;
            myobj.InNodeId = InNodeId;
            myobj.OutNodeId = OutNodeId;
            myobj.InnovationNumber = InnovationNumber;
            myobj.MutationNumber = MutationNumber;
            myobj.Enable = Enable;
            myobj.Frozen = Frozen;
            myobj.TimeDelay = TimeDelay;

            return myobj;
        }

        public override string PrintGeneToStringForHumans()
        {
            return PrintGeneToString();
        }

        /**
         * This method convert the gene in to human readable format. It can be
         * useful in debugging.
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            //string mask03 = " 0.00000000000000000;-0.00000000000000000";
            string mask03 = " 0.00000000000000000";

            string mask5 = " 0000";

            sb.Append("\n [Link (" + InNode.NodeId.ToString(mask5));
            sb.Append("," + OutNode.NodeId.ToString(mask5));
            sb.Append("]  innov (" + InnovationNumber.ToString(mask5));

            sb.Append(", mut=" + MutationNumber.ToString(mask03) + ")");
            sb.Append(" Weight " + Weight.ToString(mask03));



            if (!Enable)
                sb.Append(" -DISABLED-");

            if (IsRecurrent)
                sb.Append(" -RECUR-");

            return sb.ToString();
        }

        /**
         * This method is used to output a gene that is same as the format in start
         * genome file.
         */
        public override string PrintGeneToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Code.Encode(InNode.NodeId));
            sb.Append(Code.Encode(OutNode.NodeId));
            sb.Append(Code.Encode(Weight));
            sb.Append(Code.Encode(IsRecurrent));
            sb.Append(Code.Encode(InnovationNumber));
            sb.Append(Code.Encode(MutationNumber));
            sb.Append(Code.Encode(Enable));

            return sb.ToString();
        }

        /**
         * This method is used to read a gene in start genome from file. Example :
         * i1|i4|d0|0.0|Fi1|d0|0.0|T have these parts : i1 i4 d0|0.0 F i1 d0|0.0 T
         * which are: inNode outNode weight isRecurrent innovationNumber
         * mutationNumber enable
         */
        public override void ReadGeneFromString(string str, IEvolutionState state)
        {
            // Gene::Gene(const char *argline, std::vector<Trait*> &traits,
            // std::vector<NNode*> &nodes)
            DecodeReturn dr = new DecodeReturn(str);
            Code.Decode(dr);
            InNodeId = (int) dr.L;
            Code.Decode(dr);
            OutNodeId = (int) dr.L;
            Code.Decode(dr);
            Weight = dr.D;
            Code.Decode(dr);
            IsRecurrent = dr.L == 1;
            Code.Decode(dr);
            InnovationNumber = (int) dr.L;
            Code.Decode(dr);
            MutationNumber = dr.D;
            Code.Decode(dr);
            Enable = dr.L == 1;
        }

        /**
         * "Placeholder" method for generating a hashcode. The algorithm is stolen
         * from GPIndividual and modified a bit to use NEATGene's variables. It is
         * by no means "good" and is subject for improvement.
         */
        public override int GetHashCode()
        {
            int hash = InnovationNumber;
            hash = hash * 31 + 17 + InNodeId;
            hash = hash * 31 + 17 + OutNodeId;
            hash = hash * 31 + 17 + (int) BitConverter.DoubleToInt64Bits(Weight);
            hash = hash * 31 + 17 + (int) BitConverter.DoubleToInt64Bits(MutationNumber);
            if (Enable) hash = hash * 31 + 17;
            if (IsRecurrent) hash = hash * 31 + 13; // different value
            return hash;
        }

        public override bool Equals(object o)
        {
            NEATGene g = (NEATGene) o;
            if (InNodeId != g.InNodeId)
                return false;
            if (OutNodeId != g.OutNodeId)
                return false;
            if (!Weight.Equals(g.Weight))
                return false;
            if (IsRecurrent != g.IsRecurrent)
                return false;
            if (InnovationNumber != g.InnovationNumber)
                return false;
            if (!MutationNumber.Equals(g.MutationNumber))
                return false;
            return Enable == g.Enable;

        }

    }
}