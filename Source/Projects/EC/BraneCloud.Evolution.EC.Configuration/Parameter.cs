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
using System.Runtime.Serialization;

namespace BraneCloud.Evolution.EC.Configuration
{
    /// <summary> 
    /// A Parameter is an object which the ParameterDatabase class
    /// uses as a key to associate with strings, forming a key-value pair.
    /// Parameters are designed to be hierarchical in nature, consisting
    /// of "path items" separated by a path separator.
    /// Parameters are created either from a single path item, from an array
    /// of path items, or both.  For example, a parameter with the path
    /// foo.bar.baz might be created from 
    /// <tt>new Parameter(new String[] {"foo","bar","baz"})</tt>
    /// 
    /// <p/>Parameters are not mutable -- but once a parameter is created, path 
    /// items may be pushed an popped from it, forming a new parameter.
    /// For example, if a parameter p consists of the path foo.bar.baz,
    /// p.pop() results in a new parameter whose path is foo.bar
    /// This pushing and popping isn't cheap, so be sparing.
    /// 
    /// <p/>Because this system internally uses "." as its path separator, you should
    /// not use that character in parts of the path that you provide; however
    /// if you need some other path separator, you can change the delimiter in
    /// the code trivially.
    /// In fact, you can create a new Parameter with a path foo.bar.baz simply
    /// by calling <tt>new Parameter("foo.bar.baz")</tt> but you'd better know
    /// what you're doing.
    /// 
    /// <p/>Additionally, parameters must not contain "#", "=", non-ascii values,
    /// or whitespace.  Yes, a parameter path item may be empty.
    /// </summary>	
    [Serializable] // Be carefull to include appropriate ISerializable logic in derived classes!
    public class Parameter : IParameter
    {
        public const char Delimiter = '.';

        /// <summary>
        /// Creates a new parameter by joining the path items in s into a single path. 
        /// </summary>
        public Parameter(string[] s)
        {
            if (s.Length == 0) 
                throw new BadParameterException("Parameter created with length 0");
            for (int x = 0; x < s.Length; x++)
            {
                if (s[x] == null)
                    throw new BadParameterException("Parameter created with null string");
                if (x == 0)
                    Param = s[x];
                else
                    Param += (Delimiter + s[x]);
            }
        }
                
        /// <summary>
        /// Creates a new parameter from the single path item in s. 
        /// </summary>
        public Parameter(string s)
        {
            if (s == null)
                throw new BadParameterException("Parameter created with null string");
            Param = s;
        }
                
        /// <summary>
        /// Creates a new parameter from the path item in s, plus the path items in s2.  
        /// s2 may be null or empty, but not s 
        /// </summary>
        public Parameter(string s, string[] s2)
        {
            if (s == null)
                throw new BadParameterException("Parameter created with null string");
            Param = s;
            for (var x = 0; x < s2.Length; x++)
            {
                if (s2[x] == null)
                    throw new BadParameterException("Parameter created with null string");
                Param += (Delimiter + s2[x]);
            }
        }

        public string Param
        {
            get { return _param; }
            set { _param = value; }
        }
        protected string _param;		
        
        /// <summary>
        /// Returns a new parameter with s added to the end of the current path items. 
        /// </summary>
        public virtual IParameter Push(string s)
        {
            if (s == null)
                throw new BadParameterException("Parameter pushed with null string");
            return new Parameter(Param + Delimiter + s);
        }
                
        /// <summary>
        /// Returns a new parameter with the path items in s added to the end of the current path items. 
        /// </summary>
        public virtual IParameter Push(string[] s)
        {
            return new Parameter(Param, s);
        }
                
        /// <summary>
        /// Returns a new parameter with one path item popped off the end.  
        /// If this would result in a parameter with an empty collection of path items, null is returned. 
        /// </summary>
        /// <remarks>
        /// BRS: Not sure this is the desireable behavior. Should it return an empty parameter no matter
        /// how many times Pop is called? The problem is that if you pop all path items off and then
        /// attempt to test for a null Param property, an exception is thrown. On the other hand,
        /// you can set the param value at any time with a new value, and that value can even be null!
        /// </remarks>
        public virtual IParameter Pop()
        {
            var x = Param.LastIndexOf(Delimiter);
            if (x == -1)
            // there's nothing left.
                return null;
            return new Parameter(Param.Substring(0, (x) - (0)));
        }
        
        /// <summary>
        /// Returns a new parameter with n path items popped off the end.  
        /// If this would result in a parameter with an empty collection of path items, null is returned. 
        /// </summary>
        public virtual IParameter PopN(int n)
        {
            var s = Param;
            
            for (var y = 0; y < n; y++)
            {
                var x = s.LastIndexOf(Delimiter);
                if (x == -1)
                // there's nothing left
                    return null;

                s = Param.Substring(0, x);
            }
            return new Parameter(s);
        }
        
        
        /// <summary>
        /// Returns the path item at the far end of the parameter. 
        /// </summary>
        public virtual string Top()
        {
            var x = Param.LastIndexOf(Delimiter);
            if (x == -1)
                return Param;

            return Param.Substring(x + 1);
        }
        
        public override string ToString()
        {
            return Param;
        }

        #region ISerializable ------------------------------------------------------------>

        protected Parameter(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
                throw new ArgumentNullException("info", "SerializationInfo cannot be null");
            _param = info.GetString("__param");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
                throw new ArgumentNullException("info", "SerializationInfo cannot be null");
            info.AddValue("__param", _param);
        }

        #endregion <---------------------------------------------------------- ISerializable
    }
}