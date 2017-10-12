/*
 * BraneCloud
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using BraneCloud.Evolution.Archetype;

namespace BraneCloud.Evolution.Collections.Generic
{
    /// <summary>
    /// The purpose of this DictionaryTree is a little hard to explain.
    /// At the moment the BraneCloud.Evolution.EC configuration does not rely on this.
    /// Instead, you might be better served by taking a good look
    /// at "BraneCloud.Evolution.EC.Configuration.ParameterDatabase".
    /// You should also pay a visit to the documentation 
    /// for Sean Luke's ECJ project at GMU. 
    /// The ParameterDatabase is in "ec.util".
    /// TODO: Explain! ("Miles to go before I sleep...")
    /// </summary>
    public class DictionaryTree<TKey, TValue> : ISynchronizedDictionary<TKey, TValue>, ITrackDictionaryTreeUsage<TKey>, INamespace
    {
        public DictionaryTree()
        {
            Parents = new List<DictionaryTree<TKey, TValue>>();
        }

        #region ISerializable

        protected DictionaryTree(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
            FullName = info.GetString("FullName");
            _localEntries = (ISynchronizedDictionary<TKey, TValue>)info.GetValue("_localEntries", typeof(ISynchronizedDictionary<TKey, TValue>));
            _localEntries = (ISynchronizedDictionary<TKey, TValue>)info.GetValue("_localDefaults", typeof(ISynchronizedDictionary<TKey, TValue>));
            Parents = (List<DictionaryTree<TKey, TValue>>) info.GetValue("Parnets", typeof (IList<DictionaryTree<TKey, TValue>>));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("FullName", FullName);
            info.AddValue("_localEntries", _localEntries);
            info.AddValue("_localDefaults", _localDefaults);
            info.AddValue("Parents", Parents);
        }

        #endregion // ISerializable

        public string Name { get; protected set; }
        public string FullName { get; protected set; }

        private readonly ISynchronizedDictionary<TKey, TValue> _localEntries = new SynchronizedDictionary<TKey, TValue>();
        protected ISynchronizedDictionary<TKey, TValue> LocalEntries { get { return _localEntries; } }
        public ISynchronizedDictionary<TKey, TValue> AllEntries
        {
            get
            {
                var dict = new SynchronizedDictionary<TKey, TValue>(_localEntries);
                for (var x = Parents.Count - 1; x >= 0; x--)
                {
                    foreach (var entry in Parents[x].AllEntries.Where(entry => !dict.ContainsKey(entry.Key)))
                    {
                        dict.Add(entry.Key, entry.Value);
                    }
                }
                return dict;
            }
        }

        private readonly ISynchronizedDictionary<TKey, TValue> _localDefaults = new SynchronizedDictionary<TKey, TValue>();
        public ISynchronizedDictionary<TKey, TValue> LocalDefaults { get { return _localDefaults; } }
        public ISynchronizedDictionary<TKey, TValue> AllDefaults
        {
            get
            {
                var dict = new SynchronizedDictionary<TKey, TValue>(_localDefaults);
                for (var x = Parents.Count - 1; x >= 0; x--)
                {
                    foreach (var entry in Parents[x].AllDefaults.Where(entry => !dict.ContainsKey(entry.Key)))
                    {
                        dict.Add(entry.Key, entry.Value);
                    }
                }
                return dict;
            }
        }

        private readonly object _syncRoot = new object();
        public object SyncRoot { get { return _syncRoot; } }

        public IList<DictionaryTree<TKey, TValue>> Parents { get; private set; }

        #region Wrapped Dictionary   ********************************************************************

        #region ITrackDictionaryTreeUsage (inherits ITrackDictionaryUsage)

        public bool TrackAll
        {
            get
            {
                if (!_localEntries.TrackAll || !_localDefaults.TrackAll)
                    return false;
                return Parents.All(parent => parent.TrackAll);
            }
            set
            {
                _localEntries.TrackAll = value;
                _localDefaults.TrackAll = value;
                foreach (var parent in Parents)
                {
                    parent.TrackAll = value;
                }
            }

        }
        public bool TrackAccessed
        {
            get
            {
                if (!_localEntries.TrackAccessed || !_localDefaults.TrackAccessed)
                    return false;
                return Parents.All(parent => parent.TrackAccessed);
            }
            set
            { 
                _localEntries.TrackAccessed = value;
                _localDefaults.TrackAccessed = value;
                foreach (var parent in Parents)
                {
                    parent.TrackAccessed = value;
                }
            }
        }
        public bool TrackGotten
        {
            get
            {
                if (!_localEntries.TrackGotten || !_localDefaults.TrackGotten)
                    return false;
                return Parents.All(parent => parent.TrackGotten);
            }
            set
            {
                _localEntries.TrackGotten = value;
                _localDefaults.TrackGotten = value;
                foreach (var parent in Parents)
                {
                    parent.TrackGotten = value;
                }
            }
        }

        public ReadOnlyCollection<TKey> Accessed
        {
            get
            {
                if (!TrackAccessed) return null;
                // BRS : TODO : should probably code this as a unique union
                var list = new List<TKey>();
                var primary = AccessedPrimary;
                var defaults = AccessedDefaults;
                if (primary != null && primary.Count > 0)
                    list.AddRange(primary);
                if (list.Count == 0 && defaults.Count > 0)
                    list.AddRange(defaults);
                else
                {
                    foreach (var key in defaults.Where(key => !list.Contains(key)))
                    {
                        list.Add(key);
                    }
                }
                return new ReadOnlyCollection<TKey>(list);
            }
        }
        public ReadOnlyCollection<TKey> Gotten
        {
            get
            {
                if (!TrackGotten) return null;
                // BRS : TODO : should probably code this as a unique union
                var list = new List<TKey>();
                var primary = GottenPrimary;
                var defaults = GottenDefaults;
                if (primary != null && primary.Count > 0)
                    list.AddRange(primary);
                if (list.Count == 0 && defaults.Count > 0)
                    list.AddRange(defaults);
                else
                {
                    foreach (var key in defaults.Where(key => !list.Contains(key)))
                    {
                        list.Add(key);
                    }
                }
                return new ReadOnlyCollection<TKey>(list);
            }
        }

        public ReadOnlyCollection<TKey> AccessedPrimary
        {
            get
            {
                if (!TrackAccessed) return null;

                var list = new List<TKey>();
                if (_localEntries.Accessed != null && _localEntries.Accessed.Count > 0)
                    list.AddRange(_localEntries.Accessed);
                foreach (var key in Parents.SelectMany(parent => parent.AccessedPrimary.Where(key => !list.Contains(key))))
                {
                    list.Add(key);
                }
                return new ReadOnlyCollection<TKey>(list);
            }
        }
        public ReadOnlyCollection<TKey> GottenPrimary
        {
            get
            {
                if (!TrackGotten) return null;

                var list = new List<TKey>();
                if (_localEntries.Gotten != null && _localEntries.Gotten.Count > 0)
                    list.AddRange(_localEntries.Gotten);
                foreach (var key in Parents.SelectMany(parent => parent.GottenPrimary.Where(key => !list.Contains(key))))
                {
                    list.Add(key);
                }
                return new ReadOnlyCollection<TKey>(list);
            }
        }
        public ReadOnlyCollection<TKey> AccessedDefaults
        {
            get
            {
                if (!TrackAccessed) return null;

                var list = new List<TKey>();
                if (_localDefaults.Accessed != null && _localDefaults.Accessed.Count > 0)
                    list.AddRange(_localDefaults.Accessed);
                foreach (var key in Parents.SelectMany(parent => parent.AccessedDefaults.Where(key => !list.Contains(key))))
                {
                    list.Add(key);
                }
                return new ReadOnlyCollection<TKey>(list);
            }
        }
        public ReadOnlyCollection<TKey> GottenDefaults
        {
            get
            {
                if (!TrackGotten) return null;

                var list = new List<TKey>();
                if (_localDefaults.Gotten != null && _localDefaults.Gotten.Count > 0)
                    list.AddRange(_localDefaults.Gotten);
                foreach (var key in Parents.SelectMany(parent => parent.GottenDefaults.Where(key => !list.Contains(key))))
                {
                    list.Add(key);
                }
                return new ReadOnlyCollection<TKey>(list);
            }
        }

        public void ClearTracked()
        {
            _localEntries.ClearTracked();
            _localDefaults.ClearTracked();
            foreach (var parent in Parents)
                parent.ClearTracked();
        }
        public void ClearAccessed()
        {
            _localEntries.ClearAccessed();
            _localDefaults.ClearAccessed();
            foreach (var parent in Parents)
                parent.ClearAccessed();
        }
        public void ClearGotten()
        {
            _localEntries.ClearGotten();
            _localDefaults.ClearGotten();
            foreach (var parent in Parents)
                parent.ClearGotten();
        }

        public void ClearTrackedKey(TKey key)
        {
            _localEntries.ClearTrackedKey(key);
            _localDefaults.ClearTrackedKey(key);
            foreach (var parent in Parents)
                parent.ClearTrackedKey(key);
        }
        public void ClearAccessedKey(TKey key)
        {
            _localEntries.ClearAccessedKey(key);
            _localDefaults.ClearAccessedKey(key);
            foreach (var parent in Parents)
                parent.ClearAccessedKey(key);
        }
        public void ClearGottenKey(TKey key)
        {
            _localEntries.ClearGottenKey(key);
            _localDefaults.ClearGottenKey(key);
            foreach (var parent in Parents)
                parent.ClearGottenKey(key);
        }

        #endregion // ITrackDictionaryTreeUsage (inherits ITrackDictionaryUsage)

        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// This adds a key/value pair "shallowly". 
        /// That is, it only adds it to the top-level dictionary.
        /// The parents and defaults, if they exist, are unaffected.
        /// Note that this is just a convenience, since the method could also be called directly on the Primary dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                LocalEntries.Add(key, value);
            }
        }

        /// <summary>
        /// This adds a key/value pair "shallowly". 
        /// That is, it only adds it to the top-level dictionary.
        /// The parents and defaults, if they exist, are unaffected.
        /// Note that this is just a convenience, since the method could also be called directly on the Primary dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddDefault(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                LocalDefaults.Add(key, value);
            }
        }

        /// <summary>
        /// This removes a Primary entry matching the specified key "deeply".
        /// That is, it removes it from the top-level dictionary
        /// and from all ancestors, if they exist.
        /// </summary>
        /// <remarks>Traking info is also removed.</remarks>
        /// <param name="key">The key of the entry to be removed.</param>
        /// <returns>A boolean indicating if the specified entry was found and removed.</returns>
        public bool Remove(TKey key)
        {
            lock (_syncRoot)
            {
                var found = false;
                // Remove it from each of the parents (recursively)
                foreach (var parent in Parents.Where(parent => parent.Remove(key))) // Tracking info is also removed.
                {
                    found = true;
                }
                // finally remove it from the top-level
                if (LocalEntries.Remove(key)) // Tracking info is also removed.
                {
                    found = true;
                }
                return found;
            }
        }

        /// <summary>
        /// This removes any Primary entries matching a specified key "shallowly".
        /// That is, it removes it from the top-level Primary dictionary, 
        /// All ancestor Primary dictionaries remain unaffected.
        /// </summary>
        /// <remarks>Traking info is also removed.</remarks>
        /// <param name="key">The key of the entry to be removed.</param>
        /// <returns>A boolean indicating if the specified entry was found and removed.</returns>
        public bool RemoveLocal(TKey key)
        {
            lock (_syncRoot)
            {
                return LocalEntries.Remove(key); // Tracking info is also removed.
            }
        }

        /// <summary>
        /// This removes a default entry matching the specified key "deeply".
        /// That is, it removes it from the top-level Defaults dictionary, 
        /// and from all ancestor Default dictionaries in which it is found.
        /// </summary>
        /// <remarks>Traking info is also removed.</remarks>
        /// <param name="key">The key that specifies the default entry to be removed.</param>
        /// <returns>A boolean indicating if the specified entry was found and removed.</returns>
        public bool RemoveDefault(TKey key)
        {
            lock (_syncRoot)
            {
                var found = false;
                // First remove the default from each of the parents wherever it might be found.
                foreach (var parent in Parents.Where(parent => parent.RemoveDefault(key)))
                {
                    found = true;
                }
                // Now remove the local default if it exists.
                if (LocalDefaults.Remove(key))  // Tracking info is also removed.
                {
                    found = true;
                }
                return found;
            }
        }

        /// <summary>
        /// This removes a default entry matching the specified key "shallowly".
        /// That is, it only removes it from the top-level Defaults dictionary (parent defaults are unaffected).
        /// Note that this is just a convenience, since the method could be called directly on Defaults.
        /// </summary>
        /// <remarks>Traking info is also removed.</remarks>
        /// <param name="key">The key that uniquely identifies the default entry of interest.</param>
        /// <returns>A boolean indicating if any default entries matching the key were found and removed.</returns>
        public bool RemoveLocalDefault(TKey key)
        {
            lock (_syncRoot)
            {
                return LocalDefaults.Remove(key); // Tracking info is also removed.
            }
        }

        /// <summary>
        /// This performs a "deep" search for the specified key.
        /// That is, the Primary dictionary,
        /// and the Primary dictionaries of all ancestors will be searched.
        /// If a "shallow" search is required, call "Shallow().ContainsKey()" instead.
        /// </summary>
        /// <remarks>If the key is found, it is added to the Accessed list if TrackAccessed is enabled.</remarks>
        /// <param name="key">The key that will be searched.</param>
        /// <returns>A boolean indicating if the specified entry was found.</returns>
        public bool ContainsKey(TKey key)
        {
            lock (_syncRoot)
            {
                if (LocalEntries.ContainsKey(key)) return true;
                for (var x = Parents.Count - 1; x >= 0; x--)
                    if (Parents[x].ContainsKey(key)) return true;
                return false;
            }
        }

        /// <summary>
        /// This performs a "deep" search for the specified default key.
        /// That is, the Defaults dictionary,
        /// and the Defaults dictionaries of all ancestors will be searched.
        /// If a "shallow" search is required, call "Shallow().ContainsDefaultKey()" 
        /// or directly call Defaults.ContainsKey() instead.
        /// </summary>
        /// <param name="key">The key that will be searched.</param>
        /// <returns>A boolean indicating if the specified entry was found.</returns>
        public bool ContainsDefaultKey(TKey key)
        {
            lock (_syncRoot)
            {
                if (LocalDefaults.ContainsKey(key)) return true;
                for (var x = Parents.Count - 1; x >= 0; x--)
                    if (Parents[x].ContainsDefaultKey(key)) return true;
                return false;
            }
        }

        /// <summary>
        /// This returns all unique keys found in the "flattened" dictionary.
        /// That is, an aggregate of all top-level keys, plus all keys in parents,
        /// plus all keys found in the 'Defaults' dictionary, if it exists.
        /// If only top-level keys are required, call "Shallow().Keys" instead.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                lock (_syncRoot)
                {
                    var list = LocalEntries.Keys.ToList();
                    // enumerate parents backwards from newest to oldest
                    for (var x = Parents.Count - 1; x >= 0; x--)
                    {
                        foreach (var key in Parents[x].Keys.Where(key => !list.Contains(key)))
                        {
                            list.Add(key);
                        }
                    }
                    return list;
                }
            }
        }

        /// <summary>
        /// This returns all unique default keys found in the "flattened" dictionary.
        /// That is, an aggregate of all unique top-level default keys and all default keys in parents.
        /// If only top-level default keys are required, call "LocalDefaults.Keys" instead.
        /// </summary>
        public ICollection<TKey> DefaultKeys
        {
            get
            {
                lock (_syncRoot)
                {
                    var list = LocalDefaults.Keys.ToList();
                    // enumerate parents backwards from newest to oldest
                    for (var x = Parents.Count - 1; x >= 0; x--)
                    {
                        foreach (var key in Parents[x].DefaultKeys.Where(key => !list.Contains(key)))
                        {
                            list.Add(key);
                        }
                    }
                    return list;
                }
            }
        }

        /// <summary>
        /// This returns all unique keys found in the "flattened" dictionary.
        /// That is, an aggregate of all top-level keys, plus all keys in parents,
        /// plus all keys found in the 'Defaults' dictionary, if it exists.
        /// If only top-level keys are required, call "Shallow().Keys" instead.
        /// </summary>
        public ICollection<TKey> AnyKeys
        {
            get
            {
                lock (_syncRoot)
                {
                    // Start with primary keys
                    var list = Keys.ToList();

                    // Now get default keys
                    foreach (var key in DefaultKeys.Where(key => !list.Contains(key)))
                    {
                        list.Add(key);
                    }

                    return list;
                }
            }
        }

        /// <summary>
        /// This performs a deep search for the specified key.
        /// That is, it first looks in the top-level dictionary,
        /// then in each of the parents (from newest to oldest),
        /// Note that defaults are NOT searched!
        /// If you need to check for a default, use "TryGetDefault()" instead.
        /// If a "shallow" search is required, call "Shallow().TryGetValue()" instead.
        /// </summary>
        /// <remarks>If the search is successful, the key is added to both the Accessed and Gotten lists.</remarks>
        /// <param name="key">The key that will be searched.</param>
        /// <param name="value">The out value of the entry if one is found.</param>
        /// <returns>A boolean indicating if the specified key and associated value were found.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                // finding it at the top-level means that we are done
                if (_localEntries.TryGetValue(key, out value))
                    return true;
                // check each parent from newest to oldest
                for (var x = Parents.Count - 1; x >= 0; x--)
                    if (Parents[x].TryGetValue(key, out value))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// This performs a deep search for the specified key.
        /// That is, it first looks in the Primary dictionary,
        /// then in each of the parents (from newest to oldest),
        /// If "tryGetDefaultOnFailure" argument is true, then "TryGetDefault()"
        /// will be called if the primary search fails.
        /// If a "shallow" search is required, call "Shallow().TryGetValue()" instead.
        /// </summary>
        /// <remarks>If the search is successful, the key is added to both the Accessed and Gotten lists.</remarks>
        /// <param name="key">The key that will be searched.</param>
        /// <param name="value">The out value of the entry if one is found.</param>
        /// <param name="tryGetDefaultOnFailure">A boolean indicating whether or not defaults should also be searched.</param>
        /// <returns>A boolean indicating if the specified key and associated value were found.</returns>
        public bool TryGetValue(TKey key, out TValue value, bool tryGetDefaultOnFailure)
        {
            if (TryGetValue(key, out value))
                return true;
            return tryGetDefaultOnFailure && TryGetDefault(key, out value);
        }

        /// <summary>
        /// This performs a deep search for the specified default key.
        /// That is, it first looks in the LocalDefaults dictionary,
        /// then in each LocalDefaults dictionary of the parents (from newest to oldest).
        /// If a "shallow" search is required, call "LocalDefaults.TryGetValue()" instead.
        /// </summary>
        /// <remarks>If the search is successful, the key is added to both the Accessed and Gotten lists.</remarks>
        /// <param name="key">The key that will be searched.</param>
        /// <param name="value">The out value of the entry if one is found.</param>
        /// <returns>A boolean indicating if the specified key and associated value were found.</returns>
        public bool TryGetDefault(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                // finding it at the top-level means that we are done
                if (LocalDefaults.TryGetValue(key, out value))
                    return true;
                // check each parent from newest to oldest
                for (var x = Parents.Count - 1; x >= 0; x--)
                    if (Parents[x].TryGetDefault(key, out value))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// This performs a "width first" search for the specified key.
        /// That is, it first looks in the Primary dictionary,
        /// then in the defaults, then does the same in each of the parents (from newest to oldest),
        /// This should NOT be used without a clear understanding that a default may be
        /// returned before all ancestors have been searched for a primary setting!
        /// If a "shallow" search is required, call "Shallow().TryGetAny()" instead.
        /// </summary>
        /// <param name="key">The key that will be searched.</param>
        /// <param name="value">The out value of the entry if one is found.</param>
        /// <returns>A boolean indicating if the specified key and associated value were found.</returns>
        public bool TryGetAny(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                // finding it at the top-level means that we are done
                if (_localEntries.TryGetValue(key, out value))
                    return true;
                // if all esle fails, try to return a default if it exists
                // Note that here you may end up getting a default before each parent branch has been queried!
                if (LocalDefaults.TryGetValue(key, out value))
                    return true;
                // check each parent from newest to oldest
                // Note that here you may get a default before deeper ancestors are queried!
                for (var x = Parents.Count - 1; x >= 0; x--)
                    if (Parents[x].TryGetAny(key, out value))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// This returns a "flattened" collection of values.
        /// That is, each key will be matched with a value from the Primary dictionary (if found),
        /// or from a parent (searched from newest to oldest).
        /// Note that this effectively "hides" all values that are "shadowed" by Parents.
        /// If only top-level items are required, call "Shallow().Values" instead.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                lock (_syncRoot)
                {
                    var dict = new Dictionary<TKey, TValue>(_localEntries);
                    // enumerate parents backwards from newest to oldest
                    for (var x = Parents.Count - 1; x >= 0; x++)
                    {
                        foreach (var key in Parents[x].Keys.Where(key => !dict.ContainsKey(key)))
                        {
                            dict.Add(key, Parents[x][key]);
                        }
                    }
                    return dict.Values;
                }
            }
        }

        /// <summary>
        /// This returns a "flattened" collection of values.
        /// That is, each key will be matched with a value from the Primary dictionary (if found),
        /// or from a parent (searched from newest to oldest).
        /// Note that this effectively "hides" all values that are "shadowed" by Parents.
        /// If only top-level items are required, call "Shallow().Values" instead.
        /// </summary>
        public ICollection<TValue> DefaultValues
        {
            get
            {
                lock (_syncRoot)
                {
                    var dict = new Dictionary<TKey, TValue>(LocalDefaults);
                    // enumerate parents backwards from newest to oldest
                    for (var x = Parents.Count - 1; x >= 0; x++)
                    {
                        foreach (var key in Parents[x].DefaultKeys.Where(key => !dict.ContainsKey(key)))
                        {
                            // Search is "deep" when we do it this way. To look only in the top-level Defaults would not be adequate.
                            var def = default(TValue);
                            if (!Parents[x].TryGetDefault(key, out def))
                                throw new ArgumentOutOfRangeException("The value for the specified key could not be found. This should never happen!");
                            dict.Add(key, def);
                        }
                    }
                    return dict.Values;
                }
            }
        }

        /// <summary>
        /// This returns all entries found in the Primary dictionary, the Defaults, and all Parents.
        /// Defaults are entered first, then each parent (from oldest to newest) adds or overwrites entries,
        /// and finally the local Primary entries are enumerated to update whatever already exists, adding
        /// whatever has not yet been entered. This ensures that only the final values will appear for
        /// each unique key.
        /// </summary>
        /// <returns>A flattened set of entries that represents the final values for all unique keys.</returns>
        public DictionaryTree<TKey, TValue> Flatten()
        {
            return Flatten(null);
        }

        /// <summary>
        /// This returns all entries found in Defaults, Parents, and the Primary dictionary.
        /// Defaults are entered first, then each parent (from oldest to newest) adds or updates entries,
        /// and finally the Primary entries are enumerated to update whatever already exists, adding
        /// whatever has not yet been entered. This ensures that only the final values will appear for
        /// each unique key.
        /// </summary>
        public DictionaryTree<TKey, TValue> Flatten(DictionaryTree<TKey,TValue> overridingInstance)
        {
            lock (_syncRoot)
            {
                if (overridingInstance == null)
                    overridingInstance = new DictionaryTree<TKey, TValue>();
                overridingInstance = Underlay(overridingInstance);
                // add missing entries from each of the parents, newest to oldest, flattening each in turn
                for (var x = Parents.Count - 1; x >= 0; x--)
                {
                    // parents are only adding entries for keys that don't yet exist
                    overridingInstance = Parents[x].Flatten(overridingInstance);
                }

                return overridingInstance;
            }
        }

        private DictionaryTree<TKey, TValue> Underlay(DictionaryTree<TKey, TValue> overridingInstance)
        {
            lock (_syncRoot)
            {
                if (overridingInstance == null)
                    overridingInstance = new DictionaryTree<TKey, TValue>();
                foreach (var pair in _localEntries)
                {
                    if (!overridingInstance.LocalEntries.ContainsKey(pair.Key))
                        overridingInstance.LocalEntries.Add(pair.Key, pair.Value);
                }
                foreach (var pair in LocalDefaults)
                {
                    if (!overridingInstance.LocalDefaults.ContainsKey(pair.Key))
                        overridingInstance.LocalDefaults.Add(pair.Key, pair.Value);
                }
                return overridingInstance;
            }
        }

        /// <summary>
        /// The default indexer will try to return a Primary value from this instance.
        /// If the key isn't found, then it will look for it in parents, starting from 
        /// the most recently added, and then looking backwards through other parents added earlier.
        /// Note that the "setter" for this indexer has no effect on Parents.
        /// It will only set the value in the top-level Primary dictionary. 
        /// Parent and Default values, in other words, continue to "shadow" the keyed property just as before.
        /// If the property associated with a given key is shallowly removed, the shadow value comes back into view. 
        /// If it is deeply removed, then the property ceases to exist at any level.
        /// </summary>
        /// <param name="key">The key used to identify a particular property.</param>
        /// <returns>
        /// The value found for the specified key.
        /// </returns>
        public TValue this[TKey key]
        {
            get { return this[key, false]; }
            set
            {
                lock (_syncRoot)
                {
                    if (_localEntries.ContainsKey(key))
                        _localEntries[key] = value;
                    else
                    {
                        _localEntries.Add(key, value);
                    }
                }
            }
        }

        /// <summary>
        /// This indexer will try to return a Primary value from this instance, and optionally
        /// it will look for a default if the Primary value is not found and "tryGetDefaultOnFailure" is true.
        /// If the key isn't found in the Primary dictionary, then it will look for it in parents, starting from 
        /// the most recently added, and then looking backwards through "older" parents (i.e. those added earlier).
        /// If the key still isn't found, then it for a default (if "tryGetDefaultOnFailure" is true).
        /// Finally, if the key STILL isn't found, an <see cref="ArgumentOutOfRangeException" /> will be thrown.
        /// Note that the "setter" for this indexer has no effect on Parents or Defaults.
        /// It will only set the value in the top-level Primary dictionary. 
        /// Parent and Default values, in other words, continue to "shadow" the keyed property just as before.
        /// If the property associated with a given key is shallowly removed, then the shadow value comes back into view. 
        /// If the property is "deeply" removed, then the property ceases to exist at any level.
        /// </summary>
        /// <param name="key">The key used to identify a particular property.</param>
        /// <param name="tryGetDefaultOnFailure">
        /// A boolean indicating whether or not to look for a default value if no primary value can be found.
        /// </param>
        /// <returns>
        /// The value found for the specified key, or a default value if "tryGetDefaultOnFailure" is true.
        /// </returns>
        public TValue this[TKey key, bool tryGetDefaultOnFailure]
        {
            get
            {
                lock (SyncRoot)
                {
                    TValue v;
                    // If this has the property, then return it
                    if (TryGetValue(key, out v))
                        return v;
                    if (TryGetDefault(key, out v))
                        return v;
                    // when all else fails, throw!
                    throw new ArgumentOutOfRangeException("No value could be found for the specified key.");
                }
            }
            set { this[key] = value; }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// This adds a new KeyValuePair to the top-level dictionary.
        /// The Parents and Defaults are unaffected.
        /// </summary>
        /// <param name="item">The KeyValuePair that will be added to the top-level dictionary.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                _localEntries.Add(item);
            }
        }

        /// <summary>
        /// This clears the top-level dictionaries AND the parent references.
        /// If a deep clear is required, call "ClearDeeply()" instead.
        /// If only local entries need to be cleared 
        /// (to bring shadowed values into view, for example)
        /// then call LocalEntries.Clear() or LocalDefaults.Clear().
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                _localEntries.Clear();
                _localDefaults.Clear();
                Parents.Clear();
            }
        }

        /// <summary>
        /// This clear all items from the entire data structure.
        /// That is, it first clears all items in the top-level dictionary,
        /// then it clears each of the parents, and finally it clears the defaults, if they exist.
        /// </summary>
        public void ClearDeeply()
        {
            lock (_syncRoot)
            {
                _localEntries.Clear();
                foreach (var parent in Parents)
                    parent.ClearDeeply();
                if (LocalDefaults != null)
                    LocalDefaults.Clear();
            }
        }

        /// <summary>
        /// This checks "deeply" for the specified item.
        /// That is, it tried first to find it in the top-level dictionary,
        /// then it checks each of the parents, then it checks defaults if they exist.
        /// If a "shallow" check is required, call "Shallow().Contains()" instead.
        /// </summary>
        /// <param name="item">The KeyValuePair of interest.</param>
        /// <returns>A boolean value indicating it the item was found.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                // if the item exists at the top level, then we're done
                if (_localEntries.Contains(item))
                    return true;
                // it doesn't really matter in what order the parents are searched (we either find it or not)
                if (Parents.Any(parent => (parent as ICollection<KeyValuePair<TKey, TValue>>).Contains(item)))
                {
                    return true;
                }
                // lastly we need to check Defaults
                return LocalDefaults != null && (LocalDefaults as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
            }
        }

        /// <summary>
        /// This copies the "Flattened" dictionary to an array.
        /// That is, all unique keys are combined with the most relevant value that is found.
        /// We define priority in the following order: 
        /// Top-level dictionary, Parents from newest to oldest, Defaults.
        /// If an array of only top-level items is needed, the call "Shallow().CopyTo()" instead.
        /// </summary>
        /// <param name="array">The flattened array of KeyValuePairs for all unique keys.</param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_syncRoot)
            {
                var flat = Flatten();
                var list = new List<KeyValuePair<TKey, TValue>>(flat.LocalEntries);
                list.AddRange(flat.LocalDefaults.Where(pair => !flat.LocalEntries.ContainsKey(pair.Key)));

                list.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _localEntries.Count;
                }
            }
        }

        public int CountDeep
        {
            get
            {
                lock (_syncRoot)
                {
                    return Flatten().Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// This operation if inherently ambiguous in the context of nested properties.
        /// Therefore, to remain consistent, we define it at the top-level only.
        /// If the intent is to remove a KeyValuePair at ANY level, then
        /// the RemoveDeeply method is the one to use.
        /// </summary>
        /// <param name="item">The KeyValuePair to remove.</param>
        /// <returns>A boolean value indicating if the item was found and removed.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                return _localEntries.Remove(item);
            }
        }

        /// <summary>
        /// This operation is implemented to remain consistent with the IDictionary version.
        /// If the intent is only to remove a given KeyValuePair from the top-level collection,
        /// then Remove(..) should be used instead.
        /// </summary>
        /// <param name="item">The KeyValuePair to remove.</param>
        /// <returns>A boolean value indicating if the item was found and removed.</returns>
        public bool RemoveDeeply(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                // if we find it at the top-level, the we are done
                if (_localEntries.Remove(item))
                    return true;
                // try the parents
                if (Parents.Any(parent => parent.RemoveDeeply(item)))
                {
                    return true;
                }
                // try the defaults
                return LocalDefaults.Remove(item);
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// This enumerator operates only on the main dictionary, and not on the "flattened" view.
        /// If a "flattened" enumeration is required, call "Flatten().GetEnumerator()" instead.
        /// </summary>
        /// <returns>An enumerator that will enumerate all unique key/value properties found.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _localEntries.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _localEntries.GetEnumerator();
        }

        #endregion

        #endregion // Wrapped Dictionary   ********************************************************************
    }
}