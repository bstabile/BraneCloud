/*
 * BraneCloud
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 * 
 * Forgot where I got jump-started on this simple implementation. I think it was here:
 * http://www.tech.windowsapplication1.com/content/the-synchronized-dictionarytkey-tvalue
 * The change-tracking was added by me for a particular purpose that is too convoluted to go into here.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace BraneCloud.Evolution.Collections.Generic
{
    /// <summary>
    /// This wrapper for an <see cref="IDictionary{TKey, TValue}"/> is intended
    /// for use in a multithreaded environment. Due to the heavy locking it will
    /// probably not deliver high performance. It is mainly designed for managing
    /// a relatively small number of entries. It implements <see cref="ITrackDictionaryUsage{TKey}"/>
    /// to keep track of which keys have been "Accessed", and which values have been "Gotten".
    /// It does NOT, however, keep track of the number of times a given key is accessed or gotten. 
    /// </summary>
    /// <remarks>
    /// Note that the usage tracking only detects what is done through <see cref="ContainsKey"/> and the indexer for retrieving values.
    /// It does not attempt to restrict access to the Keys and Values collections, and any access performed on those
    /// collections will not be monitored. This was done to allow enumeration when particular keys and values are
    /// not being searched. It is up to the client to account for any "short-circuiting" of tracking that might occur.
    /// One way to handle this is to use the AddAccessedKey and AddGottenKey to update the tracking lists. These methods
    /// will throw an <see cref="InvalidOperationException"/> if the key does not exist in the internal dictionary.
    /// </remarks>
    /// <typeparam name="TKey">The type of the key for the dictionary entries.</typeparam>
    /// <typeparam name="TValue">The type of the value for the dictionary entries.</typeparam>
    public class SynchronizedDictionary<TKey, TValue> : ISynchronizedDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> _innerDict;

        [NonSerialized]
        private readonly Object _syncRoot = new object();

        public object SyncRoot { get { return _syncRoot; } }

        #region ISerializable

        protected SynchronizedDictionary(SerializationInfo info, StreamingContext context)
        {
            _innerDict = (IDictionary<TKey, TValue>)
                info.GetValue("_innerDict", typeof (IDictionary<TKey, TValue>));
            _accessed = (IList<TKey>) info.GetValue("_accessed", typeof (IList<TKey>));
            _gotten = (IList<TKey>) info.GetValue("_gotten", typeof (IList<TKey>));
            _trackAccessed = (bool) info.GetBoolean("_trackAccessed");
            _trackGotten = info.GetBoolean("_trackGotten");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_innerDict", _innerDict);
            info.AddValue("_accessed", _accessed);
            info.AddValue("_gotten", _gotten);
            info.AddValue("_trackAccessed", _trackAccessed);
            info.AddValue("_trackGotten", _trackGotten);
        }

        #endregion // ISerializable

        #region ITrackDictionaryUsage

        private IList<TKey> _accessed;
        private IList<TKey> _gotten;
        public ReadOnlyCollection<TKey> Accessed 
        { 
            get 
            {
                lock (_syncRoot)
                {
                    return _accessed == null ? null : new ReadOnlyCollection<TKey>(_accessed);
                }
            } 
        }
        public ReadOnlyCollection<TKey> Gotten
        {
            get
            {
                lock (_syncRoot)
                {
                    return _gotten == null ? null : new ReadOnlyCollection<TKey>(_gotten);
                }
            }
        }

        public bool TrackAll
        {
            get { return (_trackAccessed && _trackGotten); }
            set
            {
                TrackAccessed = value;
                TrackGotten = value;
            }
        }

        private bool _trackAccessed;
        public bool TrackAccessed
        {
            get { return _trackAccessed; } 
            set
            {
                lock (_syncRoot)
                {
                    if (_trackAccessed == value) return; // Nothing to do
                    _trackAccessed = value;
                    _accessed = _trackAccessed ? new List<TKey>() : null;
                }
            }
        }

        private bool _trackGotten;
        public bool TrackGotten
        {
            get { return _trackGotten; }
            set
            {
                lock (_syncRoot)
                {
                    if (_trackGotten == value) return; // Nothing to do
                    _trackGotten = value;
                    _gotten = _trackGotten ? new List<TKey>() : null;
                }
            }
        }

        public void AddAccessedKey(TKey key)
        {
            lock (_syncRoot)
            {
                if (!_innerDict.ContainsKey(key))
                    throw new InvalidOperationException("The specified key does not exist. It cannot be added to the 'Accessed' collection.");
                if (_trackAccessed && !_accessed.Contains(key))
                    _accessed.Add(key);
            }
        }
        public void AddGottenKey(TKey key)
        {
            lock (_syncRoot)
            {
                if (!_innerDict.ContainsKey(key))
                    throw new InvalidOperationException("The specified key does not exist. It cannot be added to the 'Gotten' collection.");
                if (_trackGotten && !_gotten.Contains(key))
                    _gotten.Add(key);
            }
        }

        public void ClearTracked()
        {
            lock (_syncRoot)
            {
                _accessed.Clear();
                _gotten.Clear();
            }
        }
        public void ClearAccessed()
        {
            lock (_syncRoot)
            {
                _accessed.Clear();
            }
        }
        public void ClearGotten()
        {
            lock (_syncRoot)
            {
                _gotten.Clear();
            }
        }

        public void ClearTrackedKey(TKey key)
        {
            lock (_syncRoot)
            {
                ClearAccessedKey(key);
                ClearGottenKey(key);
            }
        }
        public void ClearAccessedKey(TKey key)
        {
            lock (_syncRoot)
            {
                if (_trackAccessed && _accessed.Contains(key))
                    _accessed.Remove(key);
            }
        }
        public void ClearGottenKey(TKey key)
        {
            lock (_syncRoot)
            {
                if (_trackGotten && _gotten.Contains(key))
                    _gotten.Remove(key);
            }
        }

        #endregion // ITrackDictionaryUsage

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                _innerDict.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_syncRoot)
            {
                if (_innerDict.ContainsKey(key))
                {
                    if (_trackAccessed && !_accessed.Contains(key))
                        _accessed.Add(key);
                    return true;
                }
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_syncRoot)
                {
                    return _innerDict.Keys;
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (_syncRoot)
            {
                ClearTrackedKey(key);
                return _innerDict.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                var found = _innerDict.TryGetValue(key, out value);
                if (found)
                {
                    AddAccessedKey(key);
                    AddGottenKey(key);
                }
                return found;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (_syncRoot)
                {
                    return _innerDict.Values;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_syncRoot)
                {
                    var val = _innerDict[key];
                    // if we get here then indexing succeeded so...
                    if (_trackAccessed && !_accessed.Contains(key))
                        _accessed.Add(key);
                    if (_trackGotten && !_gotten.Contains(key))
                        _gotten.Add(key);
                    return val;
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    // usage tracking doesn't include setting entries, so...
                    _innerDict[key] = value;
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                (_innerDict as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                ClearTracked();
                _innerDict.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                var found = (_innerDict as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
                if (found)
                    AddAccessedKey(item.Key);
                return found;
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_syncRoot)
            {
                (_innerDict as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _innerDict.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                var found = (_innerDict as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
                if (found) ClearTrackedKey(item.Key);
                return found;
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>) _innerDict).GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_syncRoot)
            {
                return ((IEnumerable) _innerDict).GetEnumerator();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// This constructor creates a new empty instance 
        /// with tracking of 'Accessed' keys and 'Gotten' values
        /// initially disabled by default.
        /// </summary>
        public SynchronizedDictionary()
        {
            lock (_syncRoot) // just in case there are static operations
            {
                _innerDict = new Dictionary<TKey, TValue>();
            }
        }

        /// <summary>
        /// This constructor creates a new instance with tracking of 'Accessed' keys
        /// and 'Gotten' values initially enabled or disabled according to the value
        /// of the 'trackAll' argument.
        /// </summary>
        /// <param name="trackAll">A boolean value indicating whether or not to track 'Accessed' keys and 'Gotten' values.</param>
        public SynchronizedDictionary(bool trackAll)
            : this()
        {
            lock (_syncRoot) // just in case there are static operations
            {
                TrackAll = trackAll;
            }
        }

        /// <summary>
        /// This constructor creates a new instance with tracking of 'Accessed' keys
        /// and 'Gotten' values initially enabled or disabled according to the value
        /// of the 'trackAccessed' and 'trackGotten' arguments.
        /// </summary>
        /// <param name="trackAccessed">A boolean value indicating whether or not to track 'Accessed' keys.</param>
        /// <param name="trackGotten">A boolean value indicating whether or not to track 'Gotten' values.</param>
        public SynchronizedDictionary(bool trackAccessed, bool trackGotten) : this()
        {
            lock (_syncRoot) // just in case there are static operations
            {
                TrackAccessed = trackAccessed;
                TrackGotten = trackGotten;
            }
        }

        /// <summary>
        /// This constructor will copy the entries 
        /// from an existing <see cref="IDictionary{TKey, TValue}"/>.
        /// Tracking of 'Accessed' keys and 'Gotten' values is initially disabled.
        /// </summary>
        /// <param name="other">The dictionary that will be copied to create this new instance.</param>
        public SynchronizedDictionary(IDictionary<TKey, TValue> other)
        {
            if (other == null)
                throw new ArgumentNullException("other", "The argument must not be null.");

            lock (_syncRoot) // just in case there are static operations
            {
                _innerDict = new Dictionary<TKey, TValue>(other);
            }
        }

        /// <summary>
        /// This constructor will copy the entries 
        /// from an existing <see cref="IDictionary{TKey, TValue}"/>.
        /// Tracking will be initially enabled or disabled according to the value 
        /// of the 'trackAccessed' and 'trackGotten' arguments.
        /// </summary>
        /// <param name="other">The dictionary that will be copied to create this new instance.</param>
        /// <param name="trackAll">A boolean value indicating whether or not to track 'Accessed' keys and 'Gotten' values.</param>
        public SynchronizedDictionary(IDictionary<TKey, TValue> other, bool trackAll)
        {
            if (other == null)
                throw new ArgumentNullException("other", "The argument must not be null.");

            lock (_syncRoot) // just in case there are static operations
            {
                _innerDict = new Dictionary<TKey, TValue>(other);
                TrackAll = trackAll;
            }
        }

        /// <summary>
        /// This constructor will copy the entries 
        /// from an existing <see cref="IDictionary{TKey, TValue}"/>.
        /// Tracking will be initially enabled or disabled according to the value 
        /// of the 'trackAccessed' and 'trackGotten' arguments.
        /// </summary>
        /// <param name="other">The dictionary that will be copied to create this new instance.</param>
        /// <param name="trackAccessed">A boolean value indicating whether or not to track 'Accessed' keys.</param>
        /// <param name="trackGotten">A boolean value indicating whether or not to track 'Gotten' values.</param>
        public SynchronizedDictionary(IDictionary<TKey, TValue> other, bool trackAccessed, bool trackGotten)
        {
            if (other == null)
                throw new ArgumentNullException("other", "The argument must not be null.");

            lock (_syncRoot) // just in case there are static operations
            {
                _innerDict = new Dictionary<TKey, TValue>(other);
                TrackAccessed = trackAccessed;
                TrackGotten = trackGotten;
            }
        }

        /// <summary>
        /// This constructor will copy the entries from another instance. 
        /// All tracking information (Accessed and Gotten) will also be copied.
        /// </summary>
        /// <param name="other">The instance that will be copied to create this new instance.</param>
        public SynchronizedDictionary(ISynchronizedDictionary<TKey, TValue> other)
            : this(other, true, true)
        {
        }

        /// <summary>
        /// This constructor will copy the entries from another instance.
        /// Tracking information (Accessed and Gotten) will also be copied 
        /// depending on the value of the 'copyTracking' argument.
        /// </summary>
        /// <param name="other">>The instance that will be copied to create this new instance.</param>
        /// <param name="copyTracking">A boolean indicating whether or not to copy all tracking information.</param>
        public SynchronizedDictionary(ISynchronizedDictionary<TKey, TValue> other, bool copyTracking)
            : this(copyTracking, copyTracking)
        {
        }

        /// <summary>
        /// This constructor will copy the entries from another instance. 
        /// Tracking information will also be copied depending on the values
        /// of the 'copyAccessedTracking' and 'copyGottenTracking' arguments.
        /// </summary>
        /// <param name="other">The instance that will be copied to create this new instance.</param>
        /// <param name="copyAccessedTracking">A boolean indicating whether or not to copy 'Accessed' tracking information.</param>
        /// <param name="copyGottenTracking">A boolean indicating whether or not to copy 'Gotten' tracking information.</param>
        public SynchronizedDictionary(ISynchronizedDictionary<TKey, TValue> other, bool copyAccessedTracking, bool copyGottenTracking)
            : this()
        {
            if (other == null)
                throw new ArgumentNullException("other", "The argument must not be null.");

            lock (_syncRoot) // just in case there are static operations
            {
                _innerDict = new Dictionary<TKey, TValue>(other);
                if (copyAccessedTracking)
                {
                    _trackAccessed = other.TrackAccessed;
                    _accessed = other.Accessed == null ? null : new List<TKey>(other.Accessed);
                }
                if (copyGottenTracking)
                {
                    _trackGotten = other.TrackGotten;
                    _gotten = other.Gotten == null ? null : new List<TKey>(other.Gotten);
                }
            }
        }

        #endregion // Constructors
    }
}