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
using System.Collections;
using System.Collections.Specialized;

namespace BraneCloud.Evolution.EC.Support
{
    /*******************************/
    /// <summary>
    /// Represents a collection ob objects that contains no duplicate elements.
    /// </summary>	
    public interface SetSupport : IList
    {
        /// <summary>
        /// Adds a new element to the Collection if it is not already present.
        /// </summary>
        /// <param name="obj">The object to add to the collection.</param>
        /// <returns>Returns true if the object was added to the collection, otherwise false.</returns>
        new bool Add(object obj);

        /// <summary>
        /// Adds all the elements of the specified collection to the Set.
        /// </summary>
        /// <param name="c">Collection of objects to add.</param>
        /// <returns>true</returns>
        bool AddAll(ICollection c);
    }

    /*******************************/
    /// <summary>
    /// This class provides functionality not found in .NET collection-related interfaces.
    /// </summary>
    public class ICollectionSupport
    {
        /// <summary>
        /// Adds a new element to the specified collection.
        /// </summary>
        /// <param name="c">Collection where the new element will be added.</param>
        /// <param name="obj">Object to add.</param>
        /// <returns>true</returns>
        public static bool Add(ICollection c, object obj)
        {
            var added = false;
            //Reflection. Invoke either the "add" or "Add" method.
            System.Reflection.MethodInfo method;
            try
            {
                //Get the "add" method for proprietary classes
                method = c.GetType().GetMethod("Add");
                if (method == null)
                    method = c.GetType().GetMethod("add");
                var index = (int)method.Invoke(c, new[] { obj });
                if (index >= 0)
                    added = true;
            }
            catch (Exception e)
            {
                throw e;
            }
            return added;
        }

        /// <summary>
        /// Adds all of the elements of the "c" collection to the "target" collection.
        /// </summary>
        /// <param name="target">Collection where the new elements will be added.</param>
        /// <param name="c">Collection whose elements will be added.</param>
        /// <returns>Returns true if at least one element was added, false otherwise.</returns>
        public static bool AddAll(ICollection target, ICollection c)
        {
            var e = new ArrayList(c).GetEnumerator();
            var added = false;

            //Reflection. Invoke "addAll" method for proprietary classes
            System.Reflection.MethodInfo method;
            try
            {
                method = target.GetType().GetMethod("addAll");

                if (method != null)
                    added = (bool)method.Invoke(target, new object[] { c });
                else
                {
                    method = target.GetType().GetMethod("Add");
                    while (e.MoveNext())
                    {
                        bool tempBAdded = (int)method.Invoke(target, new[] { e.Current }) >= 0;
                        added = added ? added : tempBAdded;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return added;
        }

        /// <summary>
        /// Removes all the elements from the collection.
        /// </summary>
        /// <param name="c">The collection to remove elements.</param>
        public static void Clear(ICollection c)
        {
            //Reflection. Invoke "Clear" method or "clear" method for proprietary classes
            System.Reflection.MethodInfo method;
            try
            {
                method = c.GetType().GetMethod("Clear");

                if (method == null)
                    method = c.GetType().GetMethod("clear");

                method.Invoke(c, new object[] { });
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Determines whether the collection contains the specified element.
        /// </summary>
        /// <param name="c">The collection to check.</param>
        /// <param name="obj">The object to locate in the collection.</param>
        /// <returns>true if the element is in the collection.</returns>
        public static bool Contains(ICollection c, object obj)
        {
            var contains = false;

            //Reflection. Invoke "contains" method for proprietary classes
            System.Reflection.MethodInfo method;
            try
            {
                method = c.GetType().GetMethod("Contains");

                if (method == null)
                    method = c.GetType().GetMethod("contains");

                contains = (bool)method.Invoke(c, new object[] { obj });
            }
            catch (Exception e)
            {
                throw e;
            }

            return contains;
        }

        /// <summary>
        /// Determines whether the collection contains all the elements in the specified collection.
        /// </summary>
        /// <param name="target">The collection to check.</param>
        /// <param name="c">Collection whose elements would be checked for containment.</param>
        /// <returns>true id the target collection contains all the elements of the specified collection.</returns>
        public static bool ContainsAll(ICollection target, ICollection c)
        {
            var e = c.GetEnumerator();

            var contains = false;

            //Reflection. Invoke "containsAll" method for proprietary classes or "Contains" method for each element in the collection
            System.Reflection.MethodInfo method;
            try
            {
                method = target.GetType().GetMethod("containsAll");

                if (method != null)
                    contains = (bool)method.Invoke(target, new Object[] { c });
                else
                {
                    method = target.GetType().GetMethod("Contains");
                    while (e.MoveNext() == true)
                    {
                        if ((contains = (bool)method.Invoke(target, new[] { e.Current })) == false)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return contains;
        }

        /// <summary>
        /// Removes the specified element from the collection.
        /// </summary>
        /// <param name="c">The collection where the element will be removed.</param>
        /// <param name="obj">The element to remove from the collection.</param>
        public static bool Remove(ICollection c, object obj)
        {
            var changed = false;

            //Reflection. Invoke "remove" method for proprietary classes or "Remove" method
            System.Reflection.MethodInfo method;
            try
            {
                method = c.GetType().GetMethod("remove");

                if (method != null)
                    method.Invoke(c, new[] { obj });
                else
                {
                    method = c.GetType().GetMethod("Contains");
                    changed = (bool)method.Invoke(c, new[] { obj });
                    method = c.GetType().GetMethod("Remove");
                    method.Invoke(c, new[] { obj });
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return changed;
        }

        /// <summary>
        /// Removes all the elements from the specified collection that are contained in the target collection.
        /// </summary>
        /// <param name="target">Collection where the elements will be removed.</param>
        /// <param name="c">Elements to remove from the target collection.</param>
        /// <returns>true</returns>
        public static bool RemoveAll(ICollection target, ICollection c)
        {
            var al = ToArrayList(c);
            var e = al.GetEnumerator();

            //Reflection. Invoke "removeAll" method for proprietary classes or "Remove" for each element in the collection
            System.Reflection.MethodInfo method;
            try
            {
                method = target.GetType().GetMethod("removeAll");

                if (method != null)
                    method.Invoke(target, new object[] { al });
                else
                {
                    method = target.GetType().GetMethod("Remove");
                    System.Reflection.MethodInfo methodContains = target.GetType().GetMethod("Contains");

                    while (e.MoveNext())
                    {
                        while ((bool)methodContains.Invoke(target, new[] { e.Current }))
                            method.Invoke(target, new[] { e.Current });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        /// <summary>
        /// Retains the elements in the target collection that are contained in the specified collection
        /// </summary>
        /// <param name="target">Collection where the elements will be removed.</param>
        /// <param name="c">Elements to be retained in the target collection.</param>
        /// <returns>true</returns>
        public static bool RetainAll(ICollection target, ICollection c)
        {
            var e = new ArrayList(target).GetEnumerator();
            var al = new ArrayList(c);

            //Reflection. Invoke "retainAll" method for proprietary classes or "Remove" for each element in the collection
            System.Reflection.MethodInfo method;
            try
            {
                method = c.GetType().GetMethod("retainAll");

                if (method != null)
                    method.Invoke(target, new object[] { c });
                else
                {
                    method = c.GetType().GetMethod("Remove");

                    while (e.MoveNext() == true)
                    {
                        if (al.Contains(e.Current) == false)
                            method.Invoke(target, new[] { e.Current });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// Returns an array containing all the elements of the collection.
        /// </summary>
        /// <returns>The array containing all the elements of the collection.</returns>
        public static object[] ToArray(ICollection c)
        {
            var index = 0;
            var objects = new Object[c.Count];
            var e = c.GetEnumerator();

            while (e.MoveNext())
                objects[index++] = e.Current;

            return objects;
        }

        /// <summary>
        /// Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <param name="objects">The array into which the elements of the collection will be stored.</param>
        /// <returns>The array containing all the elements of the collection.</returns>
        public static object[] ToArray(ICollection c, object[] objects)
        {
            var index = 0;

            var type = objects.GetType().GetElementType();
            var objs = (object[])Array.CreateInstance(type, c.Count);

            var e = c.GetEnumerator();

            while (e.MoveNext())
                objs[index++] = e.Current;

            //If objects is smaller than c then do not return the new array in the parameter
            if (objects.Length >= c.Count)
                objs.CopyTo(objects, 0);

            return objs;
        }

        /// <summary>
        /// Converts an ICollection instance to an ArrayList instance.
        /// </summary>
        /// <param name="c">The ICollection instance to be converted.</param>
        /// <returns>An ArrayList instance in which its elements are the elements of the ICollection instance.</returns>
        public static ArrayList ToArrayList(ICollection c)
        {
            var tempArrayList = new ArrayList();
            var tempEnumerator = c.GetEnumerator();
            while (tempEnumerator.MoveNext())
                tempArrayList.Add(tempEnumerator.Current);
            return tempArrayList;
        }
    }

    /*******************************/
    /// <summary>
    /// This class has static methods to manage collections.
    /// </summary>
    public class CollectionsSupport
    {
        /// <summary>
        /// Copies the IList to other IList.
        /// </summary>
        /// <param name="SourceList">IList source.</param>
        /// <param name="TargetList">IList target.</param>
        public static void Copy(IList SourceList, IList TargetList)
        {
            for (var i = 0; i < SourceList.Count; i++)
                TargetList[i] = SourceList[i];
        }

        /// <summary>
        /// Replaces the elements of the specified list with the specified element.
        /// </summary>
        /// <param name="List">The list to be filled with the specified element.</param>
        /// <param name="Element">The element with which to fill the specified list.</param>
        public static void Fill(IList List, object Element)
        {
            for (var i = 0; i < List.Count; i++)
                List[i] = Element;
        }

        /// <summary>
        /// This class implements IComparer and is used for Comparing two String objects by evaluating 
        /// the numeric values of the corresponding Char objects in each string.
        /// </summary>
        class CompareCharValues : IComparer
        {
            public int Compare(object x, object y)
            {
                return String.CompareOrdinal((String)x, (String)y);
            }
        }

        /// <summary>
        /// Obtain the maximum element of the given collection with the specified comparator.
        /// </summary>
        /// <param name="Collection">Collection from which the maximum value will be obtained.</param>
        /// <param name="Comparator">The comparator with which to determine the maximum element.</param>
        /// <returns></returns>
        public static object Max(ICollection Collection, IComparer Comparator)
        {
            ArrayList tempArrayList;

            if (((ArrayList)Collection).IsReadOnly)
                throw new NotSupportedException();

            if ((Comparator == null) || (Comparator is Comparer))
            {
                try
                {
                    tempArrayList = new ArrayList(Collection);
                    tempArrayList.Sort();
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidCastException(e.Message);
                }
                return tempArrayList[Collection.Count - 1];
            }
            else
            {
                try
                {
                    tempArrayList = new ArrayList(Collection);
                    tempArrayList.Sort(Comparator);
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidCastException(e.Message);
                }
                return tempArrayList[Collection.Count - 1];
            }
        }

        /// <summary>
        /// Obtain the minimum element of the given collection with the specified comparator.
        /// </summary>
        /// <param name="Collection">Collection from which the minimum value will be obtained.</param>
        /// <param name="Comparator">The comparator with which to determine the minimum element.</param>
        /// <returns></returns>
        public static object Min(ICollection Collection, IComparer Comparator)
        {
            ArrayList tempArrayList;

            if (((ArrayList)Collection).IsReadOnly)
                throw new NotSupportedException();

            if ((Comparator == null) || (Comparator is Comparer))
            {
                try
                {
                    tempArrayList = new ArrayList(Collection);
                    tempArrayList.Sort();
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidCastException(e.Message);
                }
                return tempArrayList[0];
            }
            else
            {
                try
                {
                    tempArrayList = new ArrayList(Collection);
                    tempArrayList.Sort(Comparator);
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidCastException(e.Message);
                }
                return tempArrayList[0];
            }
        }


        /// <summary>
        /// Sorts an IList collections
        /// </summary>
        /// <param name="list">The IList instance that will be sorted</param>
        /// <param name="Comparator">The Comparator criteria, null to use natural comparator.</param>
        public static void Sort(IList list, IComparer Comparator)
        {
            if (((ArrayList)list).IsReadOnly)
                throw new NotSupportedException();

            if ((Comparator == null) || (Comparator is Comparer))
            {
                try
                {
                    ((ArrayList)list).Sort();
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidCastException(e.Message);
                }
            }
            else
            {
                try
                {
                    ((ArrayList)list).Sort(Comparator);
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidCastException(e.Message);
                }
            }
        }

        /// <summary>
        /// Shuffles the list randomly.
        /// </summary>
        /// <param name="List">The list to be shuffled.</param>
        public static void Shuffle(IList List)
        {
            var RandomList = new Random(unchecked((int)DateTime.Now.Ticks));
            Shuffle(List, RandomList);
        }

        /// <summary>
        /// Shuffles the list randomly.
        /// </summary>
        /// <param name="List">The list to be shuffled.</param>
        /// <param name="RandomList">The random to use to shuffle the list.</param>
        public static void Shuffle(IList List, Random RandomList)
        {
            object source;
            var target = 0;

            for (var i = 0; i < List.Count; i++)
            {
                target = RandomList.Next(List.Count);
                source = List[i];
                List[i] = List[target];
                List[target] = source;
            }
        }

        /*******************************/
        /// <summary>
        /// Converts the specified collection to its string representation.
        /// </summary>
        /// <param name="c">The collection to convert to string.</param>
        /// <returns>A string representation of the specified collection.</returns>
        public static string CollectionToString(ICollection c)
        {
            var s = new System.Text.StringBuilder();

            if (c != null)
            {

                var l = new ArrayList(c);

                var isDictionary = (c is BitArray || c is Hashtable || c is IDictionary || c is NameValueCollection || (l.Count > 0 && l[0] is DictionaryEntry));
                for (var index = 0; index < l.Count; index++)
                {
                    if (l[index] == null)
                        s.Append("null");
                    else if (!isDictionary)
                        s.Append(l[index]);
                    else
                    {
                        isDictionary = true;
                        if (c is NameValueCollection)
                            s.Append(((NameValueCollection)c).GetKey(index));
                        else
                            s.Append(((DictionaryEntry)l[index]).Key);
                        s.Append("=");
                        if (c is NameValueCollection)
                            s.Append(((NameValueCollection)c).GetValues(index)[0]);
                        else
                            s.Append(((DictionaryEntry)l[index]).Value);

                    }
                    if (index < l.Count - 1)
                        s.Append(", ");
                }

                if (isDictionary)
                {
                    if (c is ArrayList)
                        isDictionary = false;
                }
                if (isDictionary)
                {
                    s.Insert(0, "{");
                    s.Append("}");
                }
                else
                {
                    s.Insert(0, "[");
                    s.Append("]");
                }
            }
            else
                s.Insert(0, "null");
            return s.ToString();
        }

        /// <summary>
        /// Tests if the specified object is a collection and converts it to its string representation.
        /// </summary>
        /// <param name="obj">The object to convert to string</param>
        /// <returns>A string representation of the specified object.</returns>
        public static string CollectionToString(object obj)
        {
            var result = "";

            if (obj != null)
            {
                if (obj is ICollection)
                    result = CollectionToString((ICollection)obj);
                else
                    result = obj.ToString();
            }
            else
                result = "null";

            return result;
        }

    }

    /*******************************/
    /// <summary>
    /// SupportClass for the HashSet class.
    /// </summary>
    [Serializable]
    public class HashSetSupport : ArrayList, SetSupport
    {
        public HashSetSupport()
        {
        }

        public HashSetSupport(ICollection c)
        {
            AddAll(c);
        }

        public HashSetSupport(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Adds a new element to the ArrayList if it is not already present.
        /// </summary>		
        /// <param name="obj">Element to insert to the ArrayList.</param>
        /// <returns>Returns true if the new element was inserted, false otherwise.</returns>
        new public virtual bool Add(object obj)
        {
            bool inserted;

            if ((inserted = this.Contains(obj)) == false)
            {
                base.Add(obj);
            }

            return !inserted;
        }

        /// <summary>
        /// Adds all the elements of the specified collection that are not present to the list.
        /// </summary>
        /// <param name="c">Collection where the new elements will be added</param>
        /// <returns>Returns true if at least one element was added, false otherwise.</returns>
        public bool AddAll(ICollection c)
        {
            var e = new ArrayList(c).GetEnumerator();
            var added = false;

            while (e.MoveNext())
            {
                if (Add(e.Current))
                    added = true;
            }

            return added;
        }

        /// <summary>
        /// Returns a copy of the HashSet instance.
        /// </summary>		
        /// <returns>Returns a shallow copy of the current HashSet.</returns>
        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}