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

namespace BraneCloud.Evolution.EC.NEAT
{
    /**
     * NEATInnovation is a class for recording the innovation information during the
     * evolution of neat. This information is critical to determine if two
     * individuals have same origin. There a basic two types of innovation we want
     * to keep track of, adding a node or adding a gene (link) to the individual.
     * Different innovation require record different information.
     * 
     * @author Ermo Wei and David Freelan
     *
     */

    public class NEATInnovation : IPrototype
    {
        public const string P_INNOVATION = "innovation";

        /** Either NEWNODE (0) or NEWLINK (1). */
        public int InnovationType { get; set; }

        /**
         * Two nodes specify where the link innovation took place : this is the
         * input node.
         */
        public int InNodeId { get; set; }

        /**
         * Two nodes specify where the link innovation took place : this is the
         * output node.
         */
        public int OutNodeId { get; set; }

        /** The number assigned to the innovation. */
        public int InnovationNum1 { get; set; }

        /**
         * If this is a new node innovation,then there are 2 innovations (links)
         * added for the new node.
         */
        public int InnovationNum2 { get; set; }

        /** If a link is added, this is its weight. */
        public double NewWeight { get; set; }

        /** If a new node was created, this is its node id. */
        public int NewNodeId { get; set; }

        /**
         * If a new node was created, this is the innovation number of the gene's
         * link it is being stuck inside.
         */
        public int OldInnovationNum { get; set; }

        /** Is the link innovation a recurrent link. */
        public bool RecurFlag { get; set; }


        public virtual void Setup(IEvolutionState state, IParameter paramBase)
        {
            InnovationType = 0;
            InNodeId = 0;
            OutNodeId = 0;
            InnovationNum1 = 0;
            InnovationNum2 = 0;
            NewNodeId = 0;
            OldInnovationNum = 0;
            NewWeight = 0;
            RecurFlag = false;
        }


        public virtual IParameter DefaultBase => NEATDefaults.ParamBase.Push(P_INNOVATION);

        /**
         * When we have a new innovation, we clone an existing NEATInnovation
         * instance, and change its information with this reset
         * method.
         */
        public void Reset(int inNode, int outNode, int innovNum1, int innovNum2, int newId, int oldInnov)
        {
            InnovationType = 0;
            InNodeId = inNode;
            OutNodeId = outNode;
            InnovationNum1 = innovNum1;
            InnovationNum2 = innovNum2;
            NewNodeId = newId;
            OldInnovationNum = oldInnov;

            // unused parameters set to zero
            NewWeight = 0;
            RecurFlag = false;
        }

        /**
         * When we have a new innovation, we clone an existing NEATInnovation
         * instance, and change its information with this reset
         * method.
         */
        public void Reset(int inNode, int outNode, int oldInnov)
        {
            InnovationType = 0;
            InNodeId = inNode;
            OutNodeId = outNode;
            OldInnovationNum = oldInnov;

            // unused parameters set to zero
            InnovationNum1 = 0;
            InnovationNum2 = 0;
            NewNodeId = 0;
            NewWeight = 0;
            RecurFlag = false;
        }

        /**
         * When we have a new innovation, we clone an existing NEATInnovation
         * instance, and change its information with this reset
         * method.
         */
        public void Reset(int inNode, int outNode, int innovNum, double weight, bool recur)
        {
            InnovationType = 1;
            InNodeId = inNode;
            OutNodeId = outNode;
            InnovationNum1 = innovNum;
            NewWeight = weight;
            RecurFlag = recur;

            // unused parameters set to zero
            InnovationNum2 = 0;
            OldInnovationNum = 0;
            NewNodeId = 0;
        }

        /**
         * When we have a new innovation, we clone an existing NEATInnovation
         * instance, and change its information with this reset
         * method.
         */
        public void Reset(int inNode, int outNode, bool recur)
        {
            InnovationType = 1;
            InNodeId = inNode;
            OutNodeId = outNode;
            RecurFlag = recur;

            // unused parameters set to zero
            InnovationNum1 = 0;
            NewWeight = 0;
            InnovationNum2 = 0;
            OldInnovationNum = 0;
            NewNodeId = 0;

        }

        public virtual object Clone()
        {
            NEATInnovation myobj = null;
            try
            {
                myobj = (NEATInnovation) MemberwiseClone();
                myobj.InnovationType = InnovationType;
                myobj.InNodeId = InNodeId;
                myobj.OutNodeId = OutNodeId;
                myobj.InnovationNum1 = InnovationNum1;
                myobj.InnovationNum2 = InnovationNum2;
                myobj.NewWeight = NewWeight;
                myobj.NewNodeId = NewNodeId;
                myobj.OldInnovationNum = OldInnovationNum;
                myobj.RecurFlag = RecurFlag;
            }
            catch (CloneNotSupportedException e) // never happens
            {
                throw new InvalidOperationException();
            }
            return myobj;
        }

        public override int GetHashCode()
        {
            int result = InnovationType;
            result = result * 31 + 17 + InNodeId;
            result = result * 31 + 17 + OutNodeId;
            result = result * 31 + 17 + OldInnovationNum;
            if (RecurFlag)
                result = result + 13;

            return result;
        }

        public override bool Equals(object obj)
        {
            NEATInnovation inno = (NEATInnovation) obj;
            if (InnovationType != inno?.InnovationType)
                return false;
            if (InNodeId != inno.InNodeId)
                return false;
            if (OutNodeId != inno.OutNodeId)
                return false;
            if (OldInnovationNum != inno.OldInnovationNum)
                return false;
            return RecurFlag == inno.RecurFlag;
        }

    }
}