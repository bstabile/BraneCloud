using System.Collections.Generic;
using System.Linq;

namespace BraneCloud.Evolution.EC.NEAT
{
    public static class NEATExtensions
    {
        /// <summary>
        /// Adjusted fitness ascending.
        /// </summary>
        private class AdjustedFitnessAscendingComparer : IComparer<NEATIndividual>
        {
            public int Compare(NEATIndividual ind1, NEATIndividual ind2)
            {
                if (ind1 == null) return -1; // nulls at the beginning
                if (ind2 == null) return 1; // nulls at the beginning
                if (ind1.AdjustedFitness < ind2.AdjustedFitness)
                    return -1;
                if (ind1.AdjustedFitness > ind2.AdjustedFitness)
                    return 1;
                return 0;
            }
        }

        /// <summary>
        /// Adjusted fitness descending.
        /// </summary>
        private class AdjustedFitnessDescendingComparer : IComparer<NEATIndividual>
        {
            public int Compare(NEATIndividual ind1, NEATIndividual ind2)
            {
                if (ind1 == null) return 1; // nulls at the end
                if (ind2 == null) return -1; // nulls at the end
                if (ind1.AdjustedFitness < ind2.AdjustedFitness)
                    return 1;
                if (ind1.AdjustedFitness > ind2.AdjustedFitness)
                    return -1;
                return 0;
            }
        }

        /// <summary>
        /// This assumes that the individuals for each species
        /// have already been sorted in the same order (ascending).
        /// That is, it compares the individuals at the highest index.
        /// </summary>
        private class NEATSubspeciesFitnessAscendingComparer : IComparer<NEATSubspecies>
        {
            public int Compare(NEATSubspecies o1, NEATSubspecies o2)
            {
                if (o1 == null) return -1;
                if (o2 == null) return 1;

                var idx1 = o1.Individuals.Count - 1;
                var idx2 = o2.Individuals.Count - 1;
                NEATIndividual ind1 = (NEATIndividual)o1.Individuals[idx1];
                NEATIndividual ind2 = (NEATIndividual)o2.Individuals[idx2];

                if (ind1 == null) return -1;
                if (ind2 == null) return 1;

                if (ind1.Fitness.Value < ind2.Fitness.Value)
                    return -1;
                if (ind1.Fitness.Value > ind2.Fitness.Value)
                    return 1;
                return 0;
            }
        }

        /// <summary>
        /// This assumes that the individuals for each species
        /// have already been sorted in the same order (descending).
        /// That is, it compares the individuals at the lowest index.
        /// </summary>
        private class NEATSubspeciesFitnessDescendingComparer : IComparer<NEATSubspecies>
        {
            public int Compare(NEATSubspecies o1, NEATSubspecies o2)
            {
                if (o1 == null) return 1;
                if (o2 == null) return -1;

                NEATIndividual ind1 = (NEATIndividual)o1.Individuals[0];
                NEATIndividual ind2 = (NEATIndividual)o2.Individuals[0];

                if (ind1.Fitness.Value < ind2.Fitness.Value)
                    return 1;
                if (ind1.Fitness.Value > ind2.Fitness.Value)
                    return -1;
                return 0;
            }
        }


        /// <summary>
        /// This assumes that the individuals for each species
        /// have already been sorted in the same order (ascending).
        /// That is, it compares the individuals at the highest index.
        /// </summary>
        public static void SortByFitnessAscending(this IList<NEATSubspecies> list)
        {
            if (list == null || list.Count < 2) return;
            var sorted = list.OrderBy(x => x, new NEATSubspeciesFitnessAscendingComparer()).ToList();
            list.Clear();
            ((List<NEATSubspecies>)list).AddRange(sorted);
        }

        /// <summary>
        /// This assumes that the individuals for each species
        /// have already been sorted in the same order (descending).
        /// That is, it compares the individuals at the lowest index.
        /// </summary>
        public static void SortByFitnessDescending(this IList<NEATSubspecies> list)
        {
            if (list == null || list.Count < 2) return;
            var sorted = list.OrderBy(x => x, new NEATSubspeciesFitnessDescendingComparer()).ToList();
            list.Clear();
            ((List<NEATSubspecies>)list).AddRange(sorted);
        }

        /// <summary>
        /// Adjusted fitness ascending.
        /// </summary>
        public static void SortByAdjustedFitnessAscending(this IList<NEATIndividual> list)
        {
            if (list == null || list.Count < 2) return;
            var sorted = list.OrderBy(x => x, new AdjustedFitnessAscendingComparer()).ToList();
            list.Clear();
            ((List<NEATIndividual>) list).AddRange(sorted);
        }

        /// <summary>
        /// Adjusted fitness descending.
        /// </summary>
        public static void SortByAdjustedFitnessDescending(this IList<NEATIndividual> list)
        {
            if (list == null || list.Count < 2) return;
            var sorted = list.OrderBy(x => x, new AdjustedFitnessDescendingComparer()).ToList();
            list.Clear();
            ((List<NEATIndividual>)list).AddRange(sorted);
        }
    }
}
