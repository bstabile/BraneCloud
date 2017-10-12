// ECJv20 : This is removed in v20

using System;

namespace BraneCloud.Evolution.EC.Model.DE
{
	/// <summary> DEStatistics provides a straightforward solution to one problem
	/// many existing ECJ statistics classes have when used in conjunction
	/// with Differential Evolution (DE), namely reporting the fitness of individuals
	/// after they have been evaluated.  The problem stems from the fact that all
	/// individuals create children (there is no selection pressure).  Rather, the child
	/// competes immediately with its parent, and only the best of the two survives.  As
	/// a result, all other statistics classes would report the fitness of the child, as
	/// opposed to the fitness of the better of the child and the parent.  In many cases,
	/// that fitness might provide misleading information (for example, it might appear
	/// that the average fitness of the population is too random, and that there is no
	/// evident progress).  To fix this, the DEStatistics class performs the competition
	/// between the child and the parent right before other statistics classes might be
	/// invoked.  Make sure DEStatistics is set as the main statistics class, and the other
	/// are set as its children.
	/// </summary>
	
	[Serializable]
	public class DEStatistics : Statistics
	{
		
		public override void PostEvaluationStatistics(IEvolutionState state)
		{
			// keep the better of the previous population and the current one, if there is a previous population
			if (state.Breeder is DEBreeder)
			{
				var prevPop = ((DEBreeder) (state.Breeder)).PreviousPopulation; // for faster access
				if (prevPop != null)
				{
					if (prevPop.SubPops.Length != state.Population.SubPops.Length)
						state.Output.Fatal("The current population should have the same number of subpops as the previous population.");

					for (var i = 0; i < prevPop.SubPops.Length; i++)
					{
						if (state.Population.SubPops[i].Individuals.Length != prevPop.SubPops[i].Individuals.Length)
							state.Output.Fatal("SubPopulation " + i + " should have the same number of individuals in all generations.");

						for (var j = 0; j < state.Population.SubPops[i].Individuals.Length; j++)
							if (prevPop.SubPops[i].Individuals[j].Fitness.BetterThan(state.Population.SubPops[i].Individuals[j].Fitness))
								state.Population.SubPops[i].Individuals[j] = prevPop.SubPops[i].Individuals[j];
					}
					// remove the previous population from the DEBreeder, it is no longer needed
					((DEBreeder) (state.Breeder)).PreviousPopulation = null;
				}
			}
			
			// call other statistics
			base.PostEvaluationStatistics(state);
		}
	}
}