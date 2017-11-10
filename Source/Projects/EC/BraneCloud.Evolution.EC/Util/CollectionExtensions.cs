using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BraneCloud.Evolution.EC.Util
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// The following private class does NOT have an equivalent in ECJ!
        /// </summary>
        private class FitnessAscendingComparer : IComparer<Individual>
        {
            public int Compare(Individual a, Individual b)
            {
                if (a == null) return -1; // nulls at the beginning
                if (b == null) return 1;
                // return 1 if should appear after object b in the array.
                // This is the case if a has WORSE fitness.
                if (a.Fitness.BetterThan(b.Fitness))
                    return 1;
                // return -1 if a should appear before object b in the array.
                // This is the case if b has WORSE fitness.
                if (b.Fitness.BetterThan(a.Fitness))
                    return -1;
                // else return 0
                return 0;
            }
        }

        /// <summary>
        /// The following private class does NOT have an equivalent in ECJ!
        /// </summary>
        private class FitnessDescendingComparer : IComparer<Individual>
        {
            public int Compare(Individual a, Individual b)
            {
                if (a == null) return 1; // nulls at the end
                if (b == null) return -1; // nulls at the end

                // return 1 if should appear after object b in the array.
                // This is the case if a has WORSE fitness.
                if (b.Fitness.BetterThan(a.Fitness))
                    return 1;
                // return -1 if a should appear before object b in the array.
                // This is the case if b has WORSE fitness.
                if (a.Fitness.BetterThan(b.Fitness))
                    return -1;
                // else return 0
                return 0;
            }
        }
        
        /// <summary>
        /// Sort individuals in ascending order.
        /// </summary>
        public static void SortByFitnessAscending(this IList<Individual> inds)
        {
            if (inds == null || inds.Count < 2) return;
            var sorted = inds.OrderBy(x => x, new FitnessAscendingComparer());
            inds.Clear();
            ((List<Individual>)inds).AddRange(sorted);
        }

        /// <summary>
        /// Sort individuals in descending order.
        /// </summary>
        public static void SortByFitnessDescending(this IList<Individual> inds)
        {
            if (inds == null || inds.Count < 2) return;
            var sorted = inds.OrderBy(x => x, new FitnessDescendingComparer());
            inds.Clear();
            ((List<Individual>) inds).AddRange(sorted);
        }

        /// <summary>
        /// This removes and returns one item from the top of a list.
        /// The existing capacity of the original list is not changed.
        /// If the list is empty then null is returned.
        /// </summary>
        /// <remarks>
        /// This only applies to lists containing reference types because
        /// when the list becomes empty this method will return default(T).
        /// For value types there would be no indication that the item is invalid. 
        /// </remarks>
        public static T RemoveFromTopDesctructively<T>(this IList<T> list)
            where T : class
        {
            if (list.Count == 0)
                return null;
            var i = list.Count - 1;
            var t = list[i];
            list.RemoveAt(i);
            return t;
        }

        /// <summary>
        /// TakeTop(count) removes and returns a range of items from 
        /// the top of a list in the original relative order.
        /// If count is greater than the size of the input then fewer 
        /// items will be returned.
        /// If the list is empty then a new empty list will be returned.
        /// The existing capacity of the original list is not changed.
        /// </summary>
        /// <remarks>
        /// This only applies to lists containing reference types because
        /// when the list becomes empty this method will return null.
        /// This method would not be appropriate for use with value types
        /// because "default(T)" would return 0 instead of null when the list 
        /// is empty and thus there would be no indication whether or not 0 
        /// is a valid entry.
        /// </remarks>
        public static IList<T> RemoveFromTopDesctructively<T>(this IList<T> list, int count)
            where T : class
        {
            var offset = list.Count - count;
            if (offset <= 0 && count > 0) return list.ToList();

            var newList = list.Skip(offset).Take(count).ToList();

            for (var i = 0; i < newList.Count; i++)
            {
                list.RemoveAt(list.Count - 1);
            }

            return newList;
        }

        #region HashSet<T>

        public static HashSet<T> Clone<T>(this HashSet<T> hashSet)
        {
            var newSet = new HashSet<T>();
            foreach (T t in hashSet)
            {
                newSet.Add(t);
            }
            return newSet;
        }
        public static HashSet<T> DeepClone<T>(this HashSet<T> hashSet)
            where T : ICloneable
        {
            var newSet = new HashSet<T>();
            foreach (T t in hashSet)
            {
                newSet.Add((T)t.Clone());
            }
            return newSet;
        }

        #endregion
    }
}
