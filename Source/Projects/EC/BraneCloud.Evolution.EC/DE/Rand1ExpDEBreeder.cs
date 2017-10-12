// ECJv20 : This has been removed from v20

using System;

using BraneCloud.Evolution.EC.Model.Vector;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Model.DE
{
	/// <summary> Rand1ExpDEBreeder implements the DE/rand/1/exp Differential Evolution Algorithm,
	/// explored recently in the "Differential Evolution: A Practical Approach to Global Optimization"
	/// book by Kenneth Price, Rainer Storn, and Jouni Lampinen.
	/// The code relies (with permission from the original authors) on the DE algorithms posted at
	/// http://www.icsi.berkeley.edu/~storn/code.html .  For more information on
	/// Differential Evolution, please refer to the aforementioned webpage and book.
	/// </summary>
	
	[Serializable]
	public class Rand1ExpDEBreeder : DEBreeder
	{
		// Cr is crossover constant.  A default value of 0.9 might be a good idea.
		public const string P_Cr = "Cr";
		public double Cr;
		
		// F is weighting factor.  A default value of 0.8 might be a good idea.
		public const string P_F = "F";
		public double F;
		
		public override void  Setup(IEvolutionState state, IParameter paramBase)
		{
			base.Setup(state, paramBase);
			
			Cr = state.Parameters.GetDouble(paramBase.Push(P_Cr), null, 0.0);
			if (Cr < 0.0 || Cr > 1.0)
				state.Output.Fatal("Parameter not found, or its value is outside of [0.0,1.0].", paramBase.Push(P_Cr), null);
			
			F = state.Parameters.GetDouble(paramBase.Push(P_F), null, 0.0);
			if (F < 0.0 || F > 1.0)
				state.Output.Fatal("Parameter not found, or its value is outside of [0.0,1.0].", paramBase.Push(P_F), null);
		}
		
		public override Individual CreateIndividual(IEvolutionState state, int subpop, Individual[] inds, int index, int thread)
		{
			// select three indexes different from one another and from that of the current parent
			int r0, r1, r2;
			do 
			{
				r0 = state.Random[thread].NextInt(inds.Length);
			}
			while (r0 == index);
			do 
			{
				r1 = state.Random[thread].NextInt(inds.Length);
			}
			while (r1 == r0 || r1 == index);
			do 
			{
				r2 = state.Random[thread].NextInt(inds.Length);
			}
			while (r2 == r1 || r2 == r0 || r2 == index);
			
			var v = (DoubleVectorIndividual) (inds[index].Clone());
			var g0 = (DoubleVectorIndividual) (inds[r0]);
			var g1 = (DoubleVectorIndividual) (inds[r1]);
			var g2 = (DoubleVectorIndividual) (inds[r2]);
			
			var dim = v.genome.Length;
			var localIndex = state.Random[thread].NextInt(dim);
			var counter = 0;
			
			// create the child
			do 
			{
				v.genome[localIndex] = g0.genome[localIndex] + F * (g1.genome[localIndex] - g2.genome[localIndex]);
				localIndex = ++localIndex % dim;
			}
			while ((state.Random[thread].NextDouble() < Cr) && (++counter < dim));
			
			return v;
		}
	}
}