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
using System.IO;
using System.Text;
using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.PSO
{
    /**
     * Particle is a DoubleVectorIndividual with additional statistical information
     * necessary to perform Particle Swarm Optimization.  Specifically, it has a 
     * VELOCITY, a NEIGHBORHOOD of indexes of individuals, a NEIGHBORHOOD BEST genome
     * and fitness, and a PERSONAL BEST genome and fitness.  These elements, plus the
     * GLOBAL BEST genome and fitness found in PSOBreeder, are used to collectively
     * update the particle's location in space.
     *
     * <p> Particle updates its location in two steps.  First, it gathers current
     * Neighborhood and personal best statistics via the update(...) method.  Then
     * it updates the particle's Velocity and location (genome) according to these
     * statistics in the tweak(...) method.  Notice that neither of these methods is
     * the defaultMutate(...) method used in DoubleVectorIndividual: this means that
     * in *theory* you could rig up Particles to also be mutated if you thought that
     * was a good reason.
     * 
     * <p> Many of the parameters passed into the tweak(...) method are based on
     * weights determined by the PSOBreeder.
     *
     * @author Khaled Ahsan Talukder
     */


    [ECConfiguration("ec.pso.Particle")]
    public class Particle : DoubleVectorIndividual
    {
        public const string AUXILLARY_PREAMBLE = "Auxillary: ";

        // my Velocity
        public double[] Velocity { get; set; }

        // the individuals in my Neighborhood
        public int[] Neighborhood { get; set; }

        // the best genome and fitness members of my Neighborhood ever achieved
        public double[] NeighborhoodBestGenome { get; set; }

        public IFitness NeighborhoodBestFitness { get; set; }

        // the best genome and fitness *I* personally ever achieved
        public double[] PersonalBestGenome { get; set; }

        public IFitness PersonalBestFitness { get; set; }


        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            Velocity = new double[genome.Length];
        }


        public override Object Clone()
        {
            Particle myobj = (Particle) base.Clone();
            // must clone the Velocity and Neighborhood pattern if they exist
            if (Velocity != null) Velocity = (double[]) Velocity.Clone();
            if (Neighborhood != null) Neighborhood = (int[]) Neighborhood.Clone();
            return myobj;
        }

        public void Update(IEvolutionState state, int subpop, int myindex, int thread)
        {
            // update personal best
            if (PersonalBestFitness == null || Fitness.BetterThan(PersonalBestFitness))
            {
                PersonalBestFitness = (Fitness) Fitness.Clone();
                PersonalBestGenome = (double[]) genome.Clone();
            }

            // initialize Neighborhood if it's not been created yet
            PSOBreeder psob = (PSOBreeder) state.Breeder;
            if (Neighborhood == null || psob.Neighborhood == PSOBreeder.C_NEIGHBORHOOD_RANDOM_EACH_TIME)
            {
                if (psob.Neighborhood == PSOBreeder.C_NEIGHBORHOOD_RANDOM
                ) // "random" scheme is the only thing that is available for now
                    Neighborhood = CreateRandomPattern(myindex, psob.IncludeSelf,
                        state.Population.Subpops[subpop].Individuals.Count, psob.NeighborhoodSize, state, thread);
                else if (psob.Neighborhood == PSOBreeder.C_NEIGHBORHOOD_TOROIDAL ||
                         psob.Neighborhood == PSOBreeder.C_NEIGHBORHOOD_RANDOM_EACH_TIME)
                    Neighborhood = CreateToroidalPattern(myindex, psob.IncludeSelf,
                        state.Population.Subpops[subpop].Individuals.Count, psob.NeighborhoodSize);
                else // huh?
                    state.Output.Fatal("internal error: invalid PSO Neighborhood style: " + psob.Neighborhood);
            }

            // identify Neighborhood best
            NeighborhoodBestFitness = Fitness; // initially me
            NeighborhoodBestGenome = genome;
            for (int i = 0; i < Neighborhood.Length; i++)
            {
                int ind = Neighborhood[i];
                if (state.Population.Subpops[subpop].Individuals[ind].Fitness.BetterThan(Fitness))
                {
                    NeighborhoodBestFitness = state.Population.Subpops[subpop].Individuals[ind].Fitness;
                    NeighborhoodBestGenome =
                        ((DoubleVectorIndividual) (state.Population.Subpops[subpop].Individuals[ind]))
                        .genome;
                }
            }

            // clone Neighborhood best
            NeighborhoodBestFitness = (Fitness) (NeighborhoodBestFitness.Clone());
            NeighborhoodBestGenome = (double[]) (NeighborhoodBestGenome.Clone());
        }

        // VelocityCoeff:       cognitive/confidence coefficient for the Velocity
        // personalCoeff:       cognitive/confidence coefficient for self
        // informantCoeff:      cognitive/confidence coefficient for informants/neighbours
        // globalCoeff:         cognitive/confidence coefficient for global best, this is not done in the standard PSO
        public void Tweak(
            IEvolutionState state, double[] globalBest,
            double velocityCoeff, double personalCoeff,
            double informantCoeff, double globalCoeff,
            int thread)
        {
            for (int x = 0; x < GenomeLength; x++)
            {
                double xCurrent = genome[x];
                double xPersonal = PersonalBestGenome[x];
                double xNeighbour = NeighborhoodBestGenome[x];
                double xGlobal = globalBest[x];
                double beta = state.Random[thread].NextDouble() * personalCoeff;
                double gamma = state.Random[thread].NextDouble() * informantCoeff;
                double delta = state.Random[thread].NextDouble() * globalCoeff;

                double newVelocity = (velocityCoeff * Velocity[x]) + (beta * (xPersonal - xCurrent)) +
                                     (gamma * (xNeighbour - xCurrent)) + (delta * (xGlobal - xCurrent));
                Velocity[x] = newVelocity;
                genome[x] += newVelocity;
            }

            Evaluated = false;
        }

        // Creates a toroidal Neighborhood pattern for the individual
        int[] CreateRandomPattern(int myIndex, bool includeSelf, int popsize, int neighborhoodSize,
            IEvolutionState state,
            int threadnum)
        {
            // TODO: BRS: We have to solve the problem of Java -> C# (hashtables)
            IMersenneTwister mtf = state.Random[threadnum];
            HashSet<int> already = new HashSet<int>();
            int[] neighbors;

            if (includeSelf)
            {
                neighbors = new int[neighborhoodSize + 1];
                neighbors[neighborhoodSize] = myIndex; // put me at the top
                already.Add(myIndex);
            }
            else
                neighbors = new int[neighborhoodSize];

            Int32 n;
            for (int i = 0; i < neighborhoodSize; i++)
            {
                do
                {
                    neighbors[i] = mtf.NextInt(popsize);
                    n = neighbors[i];
                } while (already.Contains(n));
                already.Add(n);
            }
            return neighbors;

        }

        // Creates a toroidal Neighborhood pattern for the individual indexed by 'myindex'
        int[] CreateToroidalPattern(int myindex, bool includeSelf, int popsize, int neighborhoodSize)
        {
            int[] neighbors;

            if (includeSelf)
            {
                neighbors = new int[neighborhoodSize + 1];
                neighbors[neighborhoodSize] = myindex; // put me at the top
            }
            else
                neighbors = new int[neighborhoodSize];

            int pos = 0;
            for (int i = myindex - neighborhoodSize / 2; i < myindex; i++)
            {
                neighbors[pos++] = ((i % popsize) + popsize) % popsize;
            }

            for (int i = myindex + 1; i < neighborhoodSize - (neighborhoodSize / 2) + 1; i++)
            {
                neighbors[pos++] = ((i % popsize) + popsize) % popsize;
            }

            return neighbors;
        }

        #region Comparison

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            // no need to change anything I think
            return hash;
        }


        public override bool Equals(object ind)
        {
            if (!base.Equals(ind)) return false;
            Particle i = (Particle) ind;

            if ((Velocity == null && i.Velocity != null) ||
                (Velocity != null && i.Velocity == null))
                return false;

            if (Velocity != null)
            {
                if (Velocity.Length != i.Velocity.Length)
                    return false;
                for (int j = 0; j < Velocity.Length; j++)
                    if (!Velocity[j].Equals(i.Velocity[j]))
                        return false; // Not Equal
            }

            if ((Neighborhood == null && i.Neighborhood != null) ||
                (Neighborhood != null && i.Neighborhood == null))
                return false;

            if (Neighborhood != null)
            {
                if (Neighborhood.Length != i.Neighborhood.Length)
                    return false;
                for (int j = 0; j < Neighborhood.Length; j++)
                    if (Neighborhood[j] != i.Neighborhood[j])
                        return false;
            }

            if ((NeighborhoodBestGenome == null && i.NeighborhoodBestGenome != null) ||
                (NeighborhoodBestGenome != null && i.NeighborhoodBestGenome == null))
                return false;

            if (NeighborhoodBestGenome != null)
            {
                if (NeighborhoodBestGenome.Length != i.NeighborhoodBestGenome.Length)
                    return false;
                for (int j = 0; j < NeighborhoodBestGenome.Length; j++)
                    if (!NeighborhoodBestGenome[j].Equals(i.NeighborhoodBestGenome[j]))
                        return false; // Not Equal
            }

            if ((NeighborhoodBestFitness == null && i.NeighborhoodBestFitness != null) ||
                (NeighborhoodBestFitness != null && i.NeighborhoodBestFitness == null))
                return false;

            if (NeighborhoodBestFitness != null)
            {
                if (!NeighborhoodBestFitness.Equals(i.NeighborhoodBestFitness))
                    return false;
            }

            if ((PersonalBestGenome == null && i.PersonalBestGenome != null) ||
                (PersonalBestGenome != null && i.PersonalBestGenome == null))
                return false;

            if (PersonalBestGenome != null)
            {
                if (PersonalBestGenome.Length != i.PersonalBestGenome.Length)
                    return false;
                for (int j = 0; j < PersonalBestGenome.Length; j++)
                    if (!PersonalBestGenome[j].Equals(i.PersonalBestGenome[j]))
                        return false; // Not Equal
            }

            if ((PersonalBestFitness == null && i.PersonalBestFitness != null) ||
                (PersonalBestFitness != null && i.PersonalBestFitness == null))
                return false;

            if (PersonalBestFitness != null)
            {
                if (!PersonalBestFitness.Equals(i.PersonalBestFitness))
                    return false;
            }

            return true;
        }


        #endregion

        /// The following methods handle modifying the auxillary data when the
        /// genome is messed around with.

        void ResetAuxillaryInformation()
        {
            Neighborhood = null;
            NeighborhoodBestGenome = null;
            NeighborhoodBestFitness = null;
            PersonalBestGenome = null;
            PersonalBestFitness = null;
            for (int i = 0; i < Velocity.Length; i++)
                Velocity[i] = 0.0;
        }

        public override void Reset(IEvolutionState state, int thread)
        {
            base.Reset(state, thread);
            if (genome.Length != Velocity.Length)
                Velocity = new double[genome.Length];
            ResetAuxillaryInformation();
        }

        // This would be exceptionally weird to use in a PSO context, but for
        // consistency's sake...
        public void SetGenomeLength(int len)
        {
            GenomeLength = len;

            // we always reset regardless of whether the length is the same
            if (genome.Length != Velocity.Length)
                Velocity = new double[genome.Length];
            ResetAuxillaryInformation();
        }

        // This would be exceptionally weird to use in a PSO context, but for
        // consistency's sake...
        public void SetGenome(object gen)
        {
            Genome = gen;

            // we always reset regardless of whether the length is the same
            if (genome.Length != Velocity.Length)
                Velocity = new double[genome.Length];
            ResetAuxillaryInformation();
        }

        // This would be exceptionally weird to use in a PSO context, but for
        // consistency's sake...
        public override void Join(object[] pieces)
        {
            base.Join(pieces);

            // we always reset regardless of whether the length is the same
            if (genome.Length != Velocity.Length)
                Velocity = new double[genome.Length];
            ResetAuxillaryInformation();
        }




        /// gunk for reading and writing, but trying to preserve some of the 
        /// auxillary information

        StringBuilder EncodeAuxillary()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(AUXILLARY_PREAMBLE);
            sb.Append(Code.Encode(true));
            sb.Append(Code.Encode(Neighborhood != null));
            sb.Append(Code.Encode(NeighborhoodBestGenome != null));
            sb.Append(Code.Encode(NeighborhoodBestFitness != null));
            sb.Append(Code.Encode(PersonalBestGenome != null));
            sb.Append(Code.Encode(PersonalBestFitness != null));
            sb.Append("\n");

            // Velocity
            sb.Append(Code.Encode(Velocity.Length));
            for (int i = 0; i < Velocity.Length; i++)
                sb.Append(Code.Encode(Velocity[i]));
            sb.Append("\n");

            // Neighborhood 
            if (Neighborhood != null)
            {
                sb.Append(Code.Encode(Neighborhood.Length));
                for (int i = 0; i < Neighborhood.Length; i++)
                    sb.Append(Code.Encode(Neighborhood[i]));
                sb.Append("\n");
            }

            // Neighborhood best
            if (NeighborhoodBestGenome != null)
            {
                sb.Append(Code.Encode(NeighborhoodBestGenome.Length));
                for (int i = 0; i < NeighborhoodBestGenome.Length; i++)
                    sb.Append(Code.Encode(NeighborhoodBestGenome[i]));
                sb.Append("\n");
            }

            if (NeighborhoodBestFitness != null)
                sb.Append(NeighborhoodBestFitness.FitnessToString());

            // personal     best
            if (PersonalBestGenome != null)
            {
                sb.Append(Code.Encode(PersonalBestGenome.Length));
                for (int i = 0; i < PersonalBestGenome.Length; i++)
                    sb.Append(Code.Encode(PersonalBestGenome[i]));
                sb.Append("\n");
            }

            if (PersonalBestFitness != null)
                sb.Append(PersonalBestFitness.FitnessToString());
            sb.Append("\n");

            return sb;
        }

        public override void PrintIndividual(IEvolutionState state, int log)
        {
            base.PrintIndividual(state, log);
            state.Output.PrintLn(EncodeAuxillary().ToString(), log);
        }

        public override void PrintIndividual(IEvolutionState state, StreamWriter writer)
        {
            base.PrintIndividual(state, writer);
            writer.Write(EncodeAuxillary().ToString());
        }

        public override void ReadIndividual(IEvolutionState state,
            StreamReader reader)
        {
            base.ReadIndividual(state, reader);

            // Next, read auxillary header.
            DecodeReturn d = new DecodeReturn(Code.ReadStringWithPreamble(AUXILLARY_PREAMBLE, state, reader));
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_BOOLEAN)
                state.Output.Fatal("Line " + d.LineNumber + " should have six boolean values but seems to have fewer.");
            bool v = d.L != 0;
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_BOOLEAN)
                state.Output.Fatal("Line " + d.LineNumber + " should have six boolean values but seems to have fewer.");
            bool n = d.L != 0;
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_BOOLEAN)
                state.Output.Fatal("Line " + d.LineNumber + " should have six boolean values but seems to have fewer.");
            bool nb = d.L != 0;
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_BOOLEAN)
                state.Output.Fatal("Line " + d.LineNumber + " should have six boolean values but seems to have fewer.");
            bool nbf = d.L != 0;
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_BOOLEAN)
                state.Output.Fatal("Line " + d.LineNumber + " should have six boolean values but seems to have fewer.");
            bool pb = d.L != 0;
            Code.Decode(d);
            if (d.Type != DecodeReturn.T_BOOLEAN)
                state.Output.Fatal("Line " + d.LineNumber + " should have six boolean values but seems to have fewer.");
            bool pbf = d.L != 0;

            // Next, read auxillary arrays.
            if (v)
            {
                String s = reader.ReadLine();
                d = new DecodeReturn(s);
                Code.Decode(d);
                if (d.Type != DecodeReturn.T_INT)
                    state.Output.Fatal("Velocity length missing.");
                Velocity = new double[(int) d.L];
                for (int i = 0; i < Velocity.Length; i++)
                {
                    Code.Decode(d);
                    if (d.Type != DecodeReturn.T_DOUBLE)
                        state.Output.Fatal("Velocity information not long enough");
                    Velocity[i] = d.D;
                }
            }
            else Velocity = new double[genome.Length];

            if (n)
            {
                String s = reader.ReadLine();
                d = new DecodeReturn(s);
                Code.Decode(d);
                if (d.Type != DecodeReturn.T_INT)
                    state.Output.Fatal("Neighborhood length missing.");
                Neighborhood = new int[(int) (d.L)];
                for (int i = 0; i < Neighborhood.Length; i++)
                {
                    Code.Decode(d);
                    if (d.Type != DecodeReturn.T_INT)
                        state.Output.Fatal("Neighborhood information not long enough");
                    Neighborhood[i] = (int) (d.L);
                }
            }
            else Neighborhood = null;

            if (nb)
            {
                String s = reader.ReadLine();
                d = new DecodeReturn(s);
                Code.Decode(d);
                if (d.Type != DecodeReturn.T_INT)
                    state.Output.Fatal("Neighborhood-Best length missing.");
                NeighborhoodBestGenome = new double[(int) (d.L)];
                for (int i = 0; i < NeighborhoodBestGenome.Length; i++)
                {
                    Code.Decode(d);
                    if (d.Type != DecodeReturn.T_DOUBLE)
                        state.Output.Fatal("Neighborhood-Best genome not long enough");
                    NeighborhoodBestGenome[i] = d.D;
                }
            }
            else NeighborhoodBestGenome = null;

            if (nbf)
            {
                // here we don't know what kind of fitness it is.  So we'll do our best and guess
                // that it's the same fitness as our own Particle 
                NeighborhoodBestFitness = (Fitness) (Fitness.Clone());
                NeighborhoodBestFitness.ReadFitness(state, reader);
            }

            if (pb)
            {
                String s = reader.ReadLine();
                d = new DecodeReturn(s);
                Code.Decode(d);
                if (d.Type != DecodeReturn.T_INT)
                    state.Output.Fatal("Personal-Best length missing.");
                PersonalBestGenome = new double[(int) (d.L)];
                for (int i = 0; i < PersonalBestGenome.Length; i++)
                {
                    Code.Decode(d);
                    if (d.Type != DecodeReturn.T_DOUBLE)
                        state.Output.Fatal("Personal-Best genome not long enough");
                    PersonalBestGenome[i] = d.D;
                }
            }
            else PersonalBestGenome = null;

            if (pbf)
            {
                // here we don't know what kind of fitness it is.  So we'll do our best and guess
                // that it's the same fitness as our own Particle 
                PersonalBestFitness = (Fitness) Fitness.Clone();
                PersonalBestFitness.ReadFitness(state, reader);
            }
        }

        public override void WriteIndividual(IEvolutionState state, BinaryWriter dataOutput)
        {
            base.WriteIndividual(state, dataOutput);

            if (Velocity != null) // it's always non-null
            {
                dataOutput.Write(true);
                dataOutput.Write(Velocity.Length);
                for (int i = 0; i < Velocity.Length; i++)
                    dataOutput.Write(Velocity[i]);
            }
            else dataOutput.Write(false); // this will never happen


            if (Neighborhood != null)
            {
                dataOutput.Write(true);
                dataOutput.Write(Neighborhood.Length);
                for (int i = 0; i < Neighborhood.Length; i++)
                    dataOutput.Write(Neighborhood[i]);
            }
            else dataOutput.Write(false);


            if (NeighborhoodBestGenome != null)
            {
                dataOutput.Write(true);
                dataOutput.Write(NeighborhoodBestGenome.Length);
                for (int i = 0; i < NeighborhoodBestGenome.Length; i++)
                    dataOutput.Write(NeighborhoodBestGenome[i]);
            }
            else dataOutput.Write(false);


            if (NeighborhoodBestFitness != null)
            {
                dataOutput.Write(true);
                NeighborhoodBestFitness.WriteFitness(state, dataOutput);
            }
            else dataOutput.Write(false);


            if (PersonalBestGenome != null) // it's always non-null
            {
                dataOutput.Write(true);
                dataOutput.Write(PersonalBestGenome.Length);
                for (int i = 0; i < PersonalBestGenome.Length; i++)
                    dataOutput.Write(PersonalBestGenome[i]);
            }
            else dataOutput.Write(false);


            if (PersonalBestFitness != null)
            {
                dataOutput.Write(true);
                PersonalBestFitness.WriteFitness(state, dataOutput);
            }
            else dataOutput.Write(false);
        }

        public override void ReadIndividual(IEvolutionState state, BinaryReader dataInput)
        {
            base.ReadIndividual(state, dataInput);

            // Next, read auxillary arrays.
            if (dataInput.ReadBoolean())
            {
                Velocity = new double[dataInput.ReadInt32()];
                for (int i = 0; i < Velocity.Length; i++)
                    Velocity[i] = dataInput.ReadDouble();
            }
            else Velocity = new double[genome.Length];

            if (dataInput.ReadBoolean())
            {
                Neighborhood = new int[dataInput.ReadInt32()];
                for (int i = 0; i < Neighborhood.Length; i++)
                    Neighborhood[i] = dataInput.ReadInt32();
            }
            else Neighborhood = null;

            if (dataInput.ReadBoolean())
            {
                NeighborhoodBestGenome = new double[dataInput.ReadInt32()];
                for (int i = 0; i < NeighborhoodBestGenome.Length; i++)
                    NeighborhoodBestGenome[i] = dataInput.ReadDouble();
            }
            else NeighborhoodBestGenome = null;

            if (dataInput.ReadBoolean())
            {
                NeighborhoodBestFitness = (Fitness) Fitness.Clone();
                NeighborhoodBestFitness.ReadFitness(state, dataInput);
            }

            if (dataInput.ReadBoolean())
            {
                PersonalBestGenome = new double[dataInput.ReadInt32()];
                for (int i = 0; i < PersonalBestGenome.Length; i++)
                    PersonalBestGenome[i] = dataInput.ReadDouble();
            }
            else PersonalBestGenome = null;

            if (dataInput.ReadBoolean())
            {
                PersonalBestFitness = (Fitness) Fitness.Clone();
                PersonalBestFitness.ReadFitness(state, dataInput);
            }
        }

    }
}