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

using System.Collections.Generic;
using System.Windows.Forms;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Util;

namespace BraneCloud.Evolution.EC.EDA.DOvS
{
    /**
     * CornerMap can help us to quickly identify the possible individuals that is
     * able to form a hyperbox around best individual. It has multiple key-value
     * pairs. Each key can have multiple values. The elements in CornerMap is sorted
     * based on the key of the elements. If two elements have the same key value,
     * it's order is determined by their insertion time.
     * 
     * <p>
     * It stores the map between one of the coordinate of the individual to the
     * individual. For example, we have a individual "ind" with 5 dimension (12, 3,
     * 4, 2, 8), we should create a array "corners" with 5 CornerMap. For each of
     * the CornerMap, we should insert the coordinate of the individual as key, and
     * the individual itself as the value, like (12, ind), (3, ind) .... into their
     * corresponding CornerMap.
     *
     * <p>
     * CornerMap is essentially a mimic of multimap in C++ where keys are in sorted,
     * but in the ArrayList for each key, the order is determined by their insertion
     * order. Here we simplify it with only useful function such as lowerBound and
     * upperBound.
     * 
     * @author Ermo Wei and David Freelan
     */

    [ECConfiguration("ec.eda.dovs.CornerMap")]
    public class CornerMap
    {

        /**
         * Simple structure store the key and value from this CornerMap. This is
         * userd for retrieving data from CornerMap
         * 
         * @author Ermo Wei
         *
         */
        public class Pair
        {
            public int Key { get; set; }
            public Individual Value { get; set; }

            //public int GetKey()
            //{
            //    return (int) Key;
            //}

            //public Individual GetValue()
            //{
            //    return Value;
            //}
        }

        /** major data structure used for this CornerMap, it is order by key */

        // BRS: Still not clear if the Integer type is used just to force boxing?
        //      TreeMap is just defined as an alias for OrderedDictionary<TKey, TValue>.
        //      But since this uses a struct, KeyValuePair<TKey,TValue>, instead of a class
        //      we have to return a nullable value from the HigherEntry and LowerEntry extension methods
        //      (in case we are already at the top or bottom of the list of individuals).
        // NOTE: I'm not sure if there is any reason to use the Integer reference type.
        //       So I'm just going to cross my fingers and use the int key type to see what happens.
        //       If nothing blows up then all of this could probably be simplified.

        //TreeMap<Integer, IList<Individual>> map = new TreeMap<Integer, IList<Individual>>();
        readonly TreeMap<int, IList<Individual>> _map = new TreeMap<int, IList<Individual>>();

        /** Insert a key and value pair into CornerMap */
        public void Insert(int coordindate, Individual ind)
        {
            if (!_map.ContainsKey(coordindate))
                _map.Add(coordindate, new List<Individual>());
            _map[coordindate].Add(ind);
        }

        /// <summary>
        /// This returns the smallest element whose key is equal to or bigger than the argument "key".
        /// </summary>
        public Pair LowerBound(int key)
        {
            Pair entry = new Pair();
            if (_map[key].Count == 0)
                return null;

            entry.Key = key;
            entry.Value = _map[key][0];
            return entry;
        }

        /// <summary>
        /// This method returns the smallest element whose key is bigger than (excluding equal to) "key".
        /// </summary>
        public Pair UpperBound(int key)
        {
            var entry = _map.HigherEntry(key);
            if (entry.HasValue)
            {
                if (entry.Value.Value.Count == 0)
                    return null;
                Pair pair = new Pair
                {
                    Key = entry.Value.Key,
                    Value = entry.Value.Value[0]
                };
                return pair;
            }
            else
                return null;
        }

        /// <summary>
        /// Test if we have another key value pair before parameter pair.
        /// </summary>
        public bool HasSmaller(Pair pair)
        {
            // First search this individual in the list
            IList<Individual> currentList = _map[pair.Key];
            for (int i = currentList.Count - 1; i >= 0; i--)
            {
                // We want to compare EXACT SAME OBJECT
                if (currentList[i] == pair.Value)
                {
                    // find, can we just return true?
                    if (i == 0)
                    {
                        // if this is already the first element in current list,
                        // find previous list
                        var entry = _map.LowerEntry(pair.Key);
                        if (entry.HasValue)
                        {
                            if (entry.Value.Value.Count == 0)
                                return false;

                            return true;
                        }
                        return false;
                    }
                    return true;
                }
            }
            // we didn't find it in the list, which should not happen
            return false;
        }

        /** Test if we have another key value pair after parameter pair */
        public bool HasLarger(Pair pair)
        {
            // First search this individual in the list
            IList<Individual> currentList = _map[pair.Key];
            for (int i = 0; i < currentList.Count; ++i)
            {
                // We want to compare EXACT SAME OBJECT
                if (currentList[i] == pair.Value)
                {
                    // find, can we just return true?
                    if (i == currentList.Count - 1)
                    {
                        // if this is already the last element in current list,
                        // find next list
                        var entry = _map.HigherEntry(pair.Key);
                        if (entry != null)
                        {
                            if (entry.Value.Value.Count == 0)
                                return false;
                            return true;
                        }
                        return false;
                    }
                    return true;
                }
            }
            // we didn't find it in the list, which should not happen
            return false;
        }

        /// <summary>
        /// Get a greatest key value pair from this CornerMap who is the immediate
        /// previous element of pair
        /// </summary>
        public Pair Smaller(Pair pair)
        {
            Pair newPair = new Pair();
            // First search this individual in the list
            IList<Individual> currentList = _map[pair.Key];
            for (int i = currentList.Count - 1; i >= 0; i--)
            {
                // We want to compare EXACT SAME OBJECT
                if (currentList[i] == pair.Value)
                {
                    // find, can we just return true?
                    if (i == 0)
                    {
                        // if this is already the first element in current list,
                        // find previous list
                        var entry = _map.LowerEntry(pair.Key);
                        if (entry != null)
                        {
                            if (entry.Value.Value.Count == 0)
                                return null;

                            newPair.Key = entry.Value.Key;
                            newPair.Value = entry.Value.Value[entry.Value.Value.Count - 1];
                            return newPair;
                        }
                        return null;
                    }
                    newPair.Key = pair.Key;
                    newPair.Value = currentList[i - 1];
                    return newPair;
                }
            }
            // we didn't find it in the list, which should not happen
            return null;
        }
    }
}