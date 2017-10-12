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
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;

namespace BraneCloud.Evolution.EC.Randomization
{
    /// <summary> 
    /// <h3>MersenneTwister and MersenneTwisterFast</h3>
    /// <p/><b>Version 18</b>, based on version _mt199937(99/10/29)
    /// of the Mersenne Twister algorithm found at 
    /// <a href="http://www.math.keio.ac.jp/matumoto/emt.html">
    /// The Mersenne Twister Home Page</a>, with the initialization
    /// improved using the new 2002/1/26 initialization algorithm
    /// By Sean Luke, October 2004.
    /// 
    /// <p/><b>MersenneTwister</b> is a drop-in subclass replacement
    /// for java.util.Random.  It is properly synchronized and
    /// can be used in a multithreaded environment.  On modern VMs such
    /// as HotSpot, it is approximately 1/3 slower than java.util.Random.
    /// 
    /// <p/><b>MersenneTwisterFast</b> is not a subclass of java.util.Random.  It has
    /// the same public methods as Random does, however, and it is
    /// algorithmically identical to MersenneTwister.  MersenneTwisterFast
    /// has hard-code inlined all of its methods directly, and made all of them
    /// final (well, the ones of consequence anyway).  Further, these
    /// methods are <i>not</i> synchronized, so the same MersenneTwisterFast
    /// instance cannot be shared by multiple threads.  But all this helps
    /// MersenneTwisterFast achieve well over twice the speed of MersenneTwister.
    /// java.util.Random is about 1/3 slower than MersenneTwisterFast.
    /// 
    /// <h3>About the Mersenne Twister</h3>
    /// <p/>This is a Java version of the C-program for _mt19937: Integer version.
    /// The _mt19937 algorithm was created by Makoto Matsumoto and Takuji Nishimura,
    /// who ask: "When you use this, send an email to: matumoto@math.keio.ac.jp
    /// with an appropriate reference to your work".  Indicate that this
    /// is a translation of their algorithm into Java.
    /// 
    /// <p/><b>Reference. </b>
    /// Makato Matsumoto and Takuji Nishimura,
    /// "Mersenne Twister: A 623-Dimensionally Equidistributed Uniform
    /// Pseudo-Random Number Generator",
    /// <i>ACM Transactions on Modeling and. Computer Simulation,</i>
    /// Vol. 8, No. 1, January 1998, pp 3--30.
    /// 
    /// <h3>About this Version</h3>
    /// 
    /// <p/><b>Changes since V17:</b> Removed vestigial references to &= 0xffffffff
    /// which stemmed from the original C code.  The C code could not guarantee that
    /// ints were 32 bit, hence the masks.  The vestigial references in the Java
    /// code were likely optimized out anyway.
    /// 
    /// <p/><b>Changes since V16:</b> Added nextDouble(includeZero, includeOne) and
    /// nextFloat(includeZero, includeOne) to allow for half-open, fully-closed, and
    /// fully-open intervals.
    ///     
    /// <p/><b>Changes Since V15:</b> Added serialVersionUID to quiet compiler warnings
    /// from Sun's overly verbose compilers as of JDK 1.5.
    /// 
    /// <p/><b>Changes Since V14:</b> made strictfp, with StrictMath.log and StrictMath.sqrt
    /// in nextGaussian instead of Math.log and Math.sqrt.  This is largely just to be safe,
    /// as it presently makes no difference in the speed, correctness, or results of the
    /// algorithm.
    /// 
    /// <p/><b>Changes Since V13:</b> clone() method CloneNotSupportedException removed.  
    /// 
    /// <p/><b>Changes Since V12:</b> clone() method added.  
    /// 
    /// <p/><b>Changes Since V11:</b> stateEquals(...) method added.  MersenneTwisterFast
    /// is equal to other MersenneTwisterFasts with identical state; likewise
    /// MersenneTwister is equal to other MersenneTwister with identical state.
    /// This isn't equals(...) because that requires a contract of immutability
    /// to compare by value.
    /// 
    /// <p/><b>Changes Since V10:</b> A documentation error suggested that
    /// setSeed(int[]) required an int[] array 624 long.  In fact, the array
    /// can be any non-zero length.  The new version also checks for this fact.
    /// 
    /// <p/><b>Changes Since V9:</b> readState(stream) and writeState(stream)
    /// provided.
    /// 
    /// <p/><b>Changes Since V8:</b> setSeed(int) was only using the first 28 bits
    /// of the seed; it should have been 32 bits.  For small-number seeds the
    /// behavior is identical.
    /// 
    /// <p/><b>Changes Since V7:</b> A documentation error in MersenneTwisterFast
    /// (but not MersenneTwister) stated that nextDouble selects uniformly from
    /// the full-open interval [0,1].  It does not.  nextDouble's contract is
    /// identical across MersenneTwisterFast, MersenneTwister, and java.util.Random,
    /// namely, selection in the half-open interval [0,1).  That is, 1.0 should
    /// not be returned.  A similar contract exists in nextFloat.
    /// 
    /// <p/><b>Changes Since V6:</b> License has changed from LGPL to BSD.
    /// New timing information to compare against
    /// java.util.Random.  Recent versions of HotSpot have helped Random increase
    /// in speed to the point where it is faster than MersenneTwister but slower
    /// than MersenneTwisterFast (which should be the case, as it's a less complex
    /// algorithm but is synchronized).
    /// 
    /// <p/><b>Changes Since V5:</b> New empty constructor made to work the same
    /// as java.util.Random -- namely, it seeds based on the current time in
    /// milliseconds.
    /// 
    /// <p/><b>Changes Since V4:</b> New initialization algorithms.  See
    /// (see <a href="http://www.math.keio.ac.jp/matumoto/MT2002/emt19937ar.html"</a>
    /// http://www.math.keio.ac.jp/matumoto/MT2002/emt19937ar.html</a>)
    /// 
    /// <p/>The MersenneTwister code is based on standard _mt19937 C/C++ 
    /// code by Takuji Nishimura,
    /// with suggestions from Topher Cooper and Marc Rieffel, July 1997.
    /// The code was originally translated into Java by Michael Lecuyer,
    /// January 1999, and the original code is Copyright (c) 1999 by Michael Lecuyer.
    /// 
    /// <h3>Java notes</h3>
    /// 
    /// <p/>This implementation implements the bug fixes made
    /// in Java 1.2's version of Random, which means it can be used with
    /// earlier versions of Java.  See 
    /// <a href="http://www.javasoft.com/products/jdk/1.2/docs/api/java/util/Random.html">
    /// the JDK 1.2 java.util.Random documentation</a> for further documentation
    /// on the random-number generation contracts made.  Additionally, there's
    /// an undocumented bug in the JDK java.util.Random.nextBytes() method,
    /// which this code fixes.
    /// 
    /// <p/> Just like java.util.Random, this
    /// generator accepts a long seed but doesn't use all of it.  java.util.Random
    /// uses 48 bits.  The Mersenne Twister instead uses 32 bits (int size).
    /// So it's best if your seed does not exceed the int range.
    /// 
    /// <p/>MersenneTwister can be used reliably 
    /// on JDK version 1.1.5 or above.  Earlier Java versions have serious bugs in
    /// java.util.Random; only MersenneTwisterFast (and not MersenneTwister nor
    /// java.util.Random) should be used with them.
    /// 
    /// <h3>License</h3>
    /// 
    /// Copyright (c) 2003 by Sean Luke. <br/>
    /// Portions copyright (c) 1993 by Michael Lecuyer. <br/>
    /// All rights reserved. <br/>
    /// 
    /// <p/>Redistribution and use in source and binary forms, with or without 
    /// modification, are permitted provided that the following conditions are met:
    /// <ul>
    /// <li/> Redistributions of source code must retain the above copyright notice, 
    /// this list of conditions and the following disclaimer.
    /// <li/> Redistributions in binary form must reproduce the above copyright notice, 
    /// this list of conditions and the following disclaimer in the documentation 
    /// and/or other materials provided with the distribution.
    /// <li/> Neither the name of the copyright owners, their employers, nor the 
    /// names of its contributors may be used to endorse or promote products 
    /// derived from this software without specific prior written permission.
    /// </ul>
    /// <p/>THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
    /// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
    /// DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNERS OR CONTRIBUTORS BE 
    /// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
    /// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
    /// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
    /// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
    /// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
    /// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
    /// POSSIBILITY OF SUCH DAMAGE.
    
    // Note: this class is hard-inlined in all of its methods.  This makes some of
    // the methods well-nigh unreadable in their complexity.  In fact, the Mersenne
    // Twister is fairly easy code to understand: if you're trying to get a handle
    // on the code, I strongly suggest looking at MersenneTwister.java first.
    // -- Sean
    
    [Serializable]
    public class MersenneTwisterFast : Random, IMersenneTwister
    {
        #region Constants

        // Serialization
        private const long serialVersionUID = - 8219700664442619525L; // locked as of Version 15
        
        // Period parameters
        private const int N = 624;
        private const int M = 397;
        private const int MATRIX_A = unchecked((int) 0x9908b0df); //    private static final * constant vector a
        private const int UPPER_MASK = unchecked((int) 0x80000000); // most significant w-r bits
        private const int LOWER_MASK = 0x7fffffff; // least significant r bits
        
        
        // Tempering parameters
        private const int TEMPERING_MASK_B = unchecked((int) 0x9d2c5680);
        private const int TEMPERING_MASK_C = unchecked((int) 0xefc60000);

        #endregion // Constants
        #region Static

        /// <summary>
        /// Creates a new positive random number 
        /// </summary>
        /// <param name="random">The last random obtained</param>
        /// <returns>Returns a new positive random number</returns>
        protected static long NextLong(Random random)
        {
            long temporaryLong = random.Next();
            temporaryLong = (temporaryLong << 32) + random.Next();
            if (random.Next(-1, 1) < 0)
                return -temporaryLong;

            return temporaryLong;
        }

        #region BitShifting

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        protected static int URShift(int number, int bits)
        {
            if (number >= 0)
                return number >> bits;

            return (number >> bits) + (2 << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        protected static int URShift(int number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        protected static long URShift(long number, int bits)
        {
            if (number >= 0)
                return number >> bits;

            return (number >> bits) + (2L << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        protected static long URShift(long number, long bits)
        {
            return URShift(number, (int)bits);
        }

        #endregion // BitShifting
        #region ToByteArray

        /// <summary>
        /// Converts an array of sbytes to an array of bytes
        /// </summary>
        /// <param name="sbyteArray">The array of sbytes to be converted</param>
        /// <returns>The new array of bytes</returns>
        protected static byte[] ToByteArray(sbyte[] sbyteArray)
        {
            byte[] byteArray = null;

            if (sbyteArray != null)
            {
                byteArray = new byte[sbyteArray.Length];
                for (var index = 0; index < sbyteArray.Length; index++)
                    byteArray[index] = (byte)sbyteArray[index];
            }
            return byteArray;
        }

        /// <summary>
        /// Converts a string to an array of bytes
        /// </summary>
        /// <param name="sourceString">The string to be converted</param>
        /// <returns>The new array of bytes</returns>
        protected static byte[] ToByteArray(string sourceString)
        {
            return System.Text.UTF8Encoding.UTF8.GetBytes(sourceString);
        }

        /// <summary>
        /// Converts a array of object-type instances to a byte-type array.
        /// </summary>
        /// <param name="tempObjectArray">Array to convert.</param>
        /// <returns>An array of byte type elements.</returns>
        protected static byte[] ToByteArray(object[] tempObjectArray)
        {
            byte[] byteArray = null;
            if (tempObjectArray != null)
            {
                byteArray = new byte[tempObjectArray.Length];
                for (var index = 0; index < tempObjectArray.Length; index++)
                    byteArray[index] = (byte)tempObjectArray[index];
            }
            return byteArray;
        }

        #endregion // ToByteArray

        #endregion // Static
        #region Fields

        private int[] _mt; // the array for the state vector
        private int _mti; // _mti==N+1 means _mt[N] is not initialized
        private int[] _mag01;

        // a good initial seed (of int size, though stored in a long)
        //private static final long GOOD_SEED = 4357;

        private double __nextNextGaussian;
        private bool __haveNextNextGaussian;

        #endregion // Fields
        #region Setup

        /// <summary> 
        /// Constructor using the default seed.
        /// </summary>
        public MersenneTwisterFast()
            : this((DateTime.Now.Ticks - 621355968000000000) / 10000)
        {
        }

        /// <summary> 
        /// Constructor using a given seed.  Though you pass this seed in
        /// as a long, it's best to make sure it's actually an integer.
        /// </summary>
        public MersenneTwisterFast(long seed)
        {
            SetSeed(seed);
        }

        /// <summary> 
        /// Constructor using an array of integers as seed.
        /// Your array must have a non-zero length.  
        /// Only the first 624 integers in the array are used; 
        /// if the array is shorter than this then
        /// integers are repeatedly used in a wrap-around fashion.
        /// </summary>
        public MersenneTwisterFast(int[] array)
        {
            SetSeed(array);
        }

        #region Set Seed

        /// <summary> 
        /// Initalize the pseudo random number generator.  
        /// Don't pass in a long that's bigger than an int 
        /// (Mersenne Twister only uses the first 32 bits for its seed).   
        /// </summary>		
        public virtual void SetSeed(long seed)
        {
            lock (this)
            {
                // Due to a bug in java.util.Random clear up to 1.2, we're
                // doing our own Gaussian variable.
                __haveNextNextGaussian = false;

                _mt = new int[N];

                _mag01 = new int[2];
                _mag01[0] = 0x0;
                _mag01[1] = MATRIX_A;

                _mt[0] = (int)(seed & unchecked((int)0xffffffff));
                for (_mti = 1; _mti < N; _mti++)
                {
                    _mt[_mti] = (1812433253 * (_mt[_mti - 1] ^ URShift(_mt[_mti - 1], 30)) + _mti);
                    /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                    /* In the previous versions, MSBs of the seed affect   */
                    /* only MSBs of the array _mt[].                        */
                    /* 2002/01/09 modified by Makoto Matsumoto             */
                    //_mt[_mti] &= unchecked((int)0xffffffff);
                    /* for >32 bit machines */
                }
            }
        }

        /// <summary> 
        /// Sets the seed of the MersenneTwister using an array of integers.
        /// Your array must have a non-zero length.  Only the first 624 integers
        /// in the array are used; if the array is shorter than this then
        /// integers are repeatedly used in a wrap-around fashion.
        /// </summary>		
        public virtual void SetSeed(int[] array)
        {
            lock (this)
            {
                if (array.Length == 0)
                    throw new ArgumentException("Array length must be greater than zero");
                SetSeed(19650218);
                var i = 1;
                var j = 0;
                var k = (N > array.Length ? N : array.Length);
                for (; k != 0; k--)
                {
                    _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ URShift(_mt[i - 1], 30)) * 1664525)) + array[j] + j; /* non linear */
                    //_mt[i] &= unchecked((int)0xffffffff); /* for WORDSIZE > 32 machines */
                    i++;
                    j++;
                    if (i >= N)
                    {
                        _mt[0] = _mt[N - 1]; i = 1;
                    }
                    if (j >= array.Length)
                        j = 0;
                }
                for (k = N - 1; k != 0; k--)
                {
                    _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ URShift(_mt[i - 1], 30)) * 1566083941)) - i; /* non linear */
                    //_mt[i] &= unchecked((int)0xffffffff); /* for WORDSIZE > 32 machines */
                    i++;
                    if (i >= N)
                    {
                        _mt[0] = _mt[N - 1]; i = 1;
                    }
                }
                _mt[0] = unchecked((int)0x80000000); /* MSB is 1; assuring non-zero initial array */
            }
        }

        #endregion // Set Seed

        #endregion // Setup
        #region Operations

        #region Integers

        public short NextShort()
        {
            int y;

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            return (short)URShift(y, 16);
        }

        public override int Next(int n) { return NextInt(n); }

        public int NextInt()
        {
            int y;

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;
                var mag01 = _mag01; // locals are slightly faster 

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            return y;
        }

        /// <summary>
        /// Returns an integer drawn uniformly from 0 to n-1.  Suffice it to say,
        /// n must be > 0, or an IllegalArgumentException is raised. 
        /// </summary>
        public int NextInt(int n)
        {
            if (n <= 0)
                throw new ArgumentException("n must be positive");

            if ((n & -n) == n)
            // i.e., n is a power of 2
            {
                int y;

                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                    _mti = 0;
                }

                y = _mt[_mti++];
                y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
                y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
                y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
                y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

                return (int)((n * (long)URShift(y, 1)) >> 31);
            }

            int bits, val;
            do
            {
                int y;

                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                    _mti = 0;
                }

                y = _mt[_mti++];
                y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
                y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
                y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
                y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

                bits = URShift(y, 1);
                val = bits % n;
            }
            while (bits - val + (n - 1) < 0);
            return val;
        }

        public long NextLong()
        {
            int y;
            int z;

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                }
                z = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(z, 1) ^ _mag01[z & 0x1];

                _mti = 0;
            }

            z = _mt[_mti++];
            z ^= URShift(z, 11); // TEMPERING_SHIFT_U(z)
            z ^= (z << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(z)
            z ^= (z << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(z)
            z ^= URShift(z, 18); // TEMPERING_SHIFT_L(z)

            return (((long)y) << 32) + (long)z;
        }

        /// <summary>
        /// Returns a long drawn uniformly from 0 to n-1.  Suffice it to say,
        /// n must be > 0, or an IllegalArgumentException is raised. 
        /// </summary>
        public long NextLong(long n)
        {
            if (n <= 0)
                throw new ArgumentException("n must be positive");

            long bits, val;
            do
            {
                int y;
                int z;

                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                    _mti = 0;
                }

                y = _mt[_mti++];
                y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
                y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
                y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
                y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                    }
                    z = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(z, 1) ^ _mag01[z & 0x1];

                    _mti = 0;
                }

                z = _mt[_mti++];
                z ^= URShift(z, 11); // TEMPERING_SHIFT_U(z)
                z ^= (z << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(z)
                z ^= (z << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(z)
                z ^= URShift(z, 18); // TEMPERING_SHIFT_L(z)

                bits = URShift(((((long)y) << 32) + (long)z), 1);
                val = bits % n;
            }
            while (bits - val + (n - 1) < 0);
            return val;
        }

        /// <summary>
        /// BRS: NOTE: This requires further work!!!
        /// </summary>
        public T NextIntegral<T>() where T : struct, IConvertible, IComparable
        {
            if (typeof(T) == typeof(byte))
                return (T) Convert.ChangeType(NextByte(), typeof (T));
            if (typeof(T) == typeof(sbyte))
                return (T)Convert.ChangeType(NextByte(), typeof(T));
            if (typeof(T) == typeof(short))
                return (T)Convert.ChangeType(NextShort(), typeof(T));
            if (typeof(T) == typeof(int))
                return (T)Convert.ChangeType(NextInt(), typeof(T));
            if (typeof(T) == typeof(long))
                return (T) Convert.ChangeType(NextLong(), typeof(T));
            throw new InvalidOperationException(String.Format("No conversion for type '{0}'", typeof(T)));
        }

        #endregion // Integers
        #region Floating Point

        /// <summary>
        /// Returns a random float in the half-open range from [0.0f,1.0f).  
        /// Thus 0.0f is a valid result but 1.0f is not. 
        /// </summary>
        public float NextFloat()
        {
            int y;

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
            return URShift(y, 8) / ((float)(1 << 24));
        }

        /// <summary>
        /// Returns a float in the range from 0.0f to 1.0f, possibly inclusive of 0.0f and 1.0f themselves.  
        /// Thus:
        /// <p/><table border="0">
        /// <th><td>Expression</td><td>Interval</td></th>
        /// <tr><td>NextFloat(false, false)</td><td>(0.0f, 1.0f)</td></tr>
        /// <tr><td>NextFloat(true, false)</td><td>[0.0f, 1.0f)</td></tr>
        /// <tr><td>NextFloat(false, true)</td><td>(0.0f, 1.0f]</td></tr>
        /// <tr><td>NextFloat(true, true)</td><td>[0.0f, 1.0f]</td></tr>
        /// </table>
        /// <p/>This version preserves all possible random values in the float range.
        /// </summary>
        public double NextFloat(bool includeZero, bool includeOne)
        {
            var d = 0.0f;
            do
            {
                d = NextFloat();                            // grab a value, initially from half-open [0.0f, 1.0f)
                if (includeOne && NextBoolean()) d += 1.0f; // if includeOne, with 1/2 probability, push to [1.0f, 2.0f)
            }
            while ((d > 1.0f) ||                           // everything above 1.0f is always invalid
                (!includeZero && d == 0.0f));           // if we're not including zero, 0.0f is invalid
            return d;
        }


        
        /// <summary>
        /// Returns a random double in the half-open range from [0.0,1.0).  
        /// Thus 0.0 is a valid result but 1.0 is not. 
        /// </summary>
        public override double NextDouble()
        {
            int y;
            int z;

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                }
                z = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(z, 1) ^ _mag01[z & 0x1];

                _mti = 0;
            }

            z = _mt[_mti++];
            z ^= URShift(z, 11); // TEMPERING_SHIFT_U(z)
            z ^= (z << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(z)
            z ^= (z << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(z)
            z ^= URShift(z, 18); // TEMPERING_SHIFT_L(z)

            /* derived from nextDouble documentation in jdk 1.2 docs, see top */
            //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
            return ((((long)URShift(y, 6)) << 27) + URShift(z, 5)) / (double)(1L << 53);
        }

        /// <summary>
        /// Returns a double in the range from 0.0 to 1.0, possibly inclusive of 0.0 and 1.0 themselves.  
        /// Thus:
        /// <p/><table border="0">
        /// <th><td>Expression</td><td>Interval</td></th>
        /// <tr><td>NextDouble(false, false)</td><td>(0.0, 1.0)</td></tr>
        /// <tr><td>NextDouble(true, false)</td><td>[0.0, 1.0)</td></tr>
        /// <tr><td>NextDouble(false, true)</td><td>(0.0, 1.0]</td></tr>
        /// <tr><td>NextDouble(true, true)</td><td>[0.0, 1.0]</td></tr>
        /// </table>
        /// <p/>This version preserves all possible random values in the double range.
        /// </summary>
        public double NextDouble(bool includeZero, bool includeOne)
        {
            var d = 0.0;
            do
            {
                d = NextDouble();                           // grab a value, initially from half-open [0.0, 1.0)
                if (includeOne && NextBoolean()) d += 1.0;  // if includeOne, with 1/2 probability, push to [1.0, 2.0)
            }
            while ((d > 1.0) ||                            // everything above 1.0 is always invalid
                (!includeZero && d == 0.0));            // if we're not including zero, 0.0 is invalid
            return d;
        }

        public double NextGaussian()
        {
            if (__haveNextNextGaussian)
            {
                __haveNextNextGaussian = false;
                return __nextNextGaussian;
            }

            double v1, v2, s;
            do
            {
                int y;
                int z;
                int a;
                int b;

                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                    _mti = 0;
                }

                y = _mt[_mti++];
                y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
                y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
                y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
                y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                    }
                    z = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(z, 1) ^ _mag01[z & 0x1];

                    _mti = 0;
                }

                z = _mt[_mti++];
                z ^= URShift(z, 11); // TEMPERING_SHIFT_U(z)
                z ^= (z << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(z)
                z ^= (z << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(z)
                z ^= URShift(z, 18); // TEMPERING_SHIFT_L(z)

                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        a = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(a, 1) ^ _mag01[a & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        a = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(a, 1) ^ _mag01[a & 0x1];
                    }
                    a = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(a, 1) ^ _mag01[a & 0x1];

                    _mti = 0;
                }

                a = _mt[_mti++];
                a ^= URShift(a, 11); // TEMPERING_SHIFT_U(a)
                a ^= (a << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(a)
                a ^= (a << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(a)
                a ^= URShift(a, 18); // TEMPERING_SHIFT_L(a)

                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        b = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(b, 1) ^ _mag01[b & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        b = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(b, 1) ^ _mag01[b & 0x1];
                    }
                    b = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(b, 1) ^ _mag01[b & 0x1];

                    _mti = 0;
                }

                b = _mt[_mti++];
                b ^= URShift(b, 11); // TEMPERING_SHIFT_U(b)
                b ^= (b << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(b)
                b ^= (b << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(b)
                b ^= URShift(b, 18); // TEMPERING_SHIFT_L(b)

                /* derived from nextDouble documentation in jdk 1.2 docs, see top */
                //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
                v1 = 2 * (((((long)URShift(y, 6)) << 27) + URShift(z, 5)) / (double)(1L << 53)) - 1;
                //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
                v2 = 2 * (((((long)URShift(a, 6)) << 27) + URShift(b, 5)) / (double)(1L << 53)) - 1;
                s = v1 * v1 + v2 * v2;
            }
            while (s >= 1 || s == 0);
            var multiplier = Math.Sqrt((-2) * Math.Log(s) / s);
            __nextNextGaussian = v2 * multiplier;
            __haveNextNextGaussian = true;
            return v1 * multiplier;
        }

        #endregion // Floating Point
        #region Char

        public char NextChar()
        {
            int y;

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            return (char)URShift(y, 16);
        }

        #endregion // Char
        #region Boolean

        public bool NextBoolean()
        {
            int y;

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            return (bool)(URShift(y, 31) != 0);
        }

        /// <summary>
        /// This generates a coin flip with a probability <tt>probability</tt>
        /// of returning true, else returning false.  <tt>probability</tt> must
        /// be between 0.0 and 1.0, inclusive.   Not as precise a random real
        /// event as nextBoolean(double), but twice as fast. To explicitly
        /// use this, remember you may need to cast to float first. 
        /// </summary>		
        public bool NextBoolean(float probability)
        {
            int y;

            if (probability < 0.0f || probability > 1.0f)
                throw new ArgumentException("probability must be between 0.0 and 1.0 inclusive.");
            if (probability == 0.0f)
                return false;
            // fix half-open issues
            else if (probability == 1.0f)
                return true; // fix half-open issues
            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
            return URShift(y, 8) / ((float)(1 << 24)) < probability;
        }

        /// <summary>
        /// This generates a coin flip with a probability <tt>probability</tt>
        /// of returning true, else returning false.  <tt>probability</tt> must
        /// be between 0.0 and 1.0, inclusive. 
        /// </summary>		
        public bool NextBoolean(double probability)
        {
            int y;
            int z;

            if (probability < 0.0 || probability > 1.0)
                throw new ArgumentException("probability must be between 0.0 and 1.0 inclusive.");
            if (probability == 0.0)
                return false;
            // fix half-open issues
            if (probability == 1.0)
                return true; // fix half-open issues
            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    z = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(z, 1) ^ _mag01[z & 0x1];
                }
                z = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(z, 1) ^ _mag01[z & 0x1];

                _mti = 0;
            }

            z = _mt[_mti++];
            z ^= URShift(z, 11); // TEMPERING_SHIFT_U(z)
            z ^= (z << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(z)
            z ^= (z << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(z)
            z ^= URShift(z, 18); // TEMPERING_SHIFT_L(z)

            /* derived from nextDouble documentation in jdk 1.2 docs, see top */
            //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
            return ((((long)URShift(y, 6)) << 27) + URShift(z, 5)) / (double)(1L << 53) < probability;
        }

        #endregion // Boolean
        #region Byte

        public sbyte NextByte()
        {
            int y;

            if (_mti >= N)
            // generate N words at one time
            {
                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                    _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
            y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
            y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
            y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

            return (sbyte)URShift(y, 24);
        }

        public override void NextBytes(byte[] bytes)
        {
            for (var x = 0; x < bytes.Length; x++)
                bytes[x] = (byte)NextInt(256);
        }

        public void NextBytes(sbyte[] bytes)
        {
            for (var x = 0; x < bytes.Length; x++)
            {
                int y;
                if (_mti >= N)
                // generate N words at one time
                {
                    int kk;

                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + M] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                        _mt[kk] = _mt[kk + (M - N)] ^ URShift(y, 1) ^ _mag01[y & 0x1];
                    }
                    y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
                    _mt[N - 1] = _mt[M - 1] ^ URShift(y, 1) ^ _mag01[y & 0x1];

                    _mti = 0;
                }

                y = _mt[_mti++];
                y ^= URShift(y, 11); // TEMPERING_SHIFT_U(y)
                y ^= (y << 7) & TEMPERING_MASK_B; // TEMPERING_SHIFT_S(y)
                y ^= (y << 15) & TEMPERING_MASK_C; // TEMPERING_SHIFT_T(y)
                y ^= URShift(y, 18); // TEMPERING_SHIFT_L(y)

                bytes[x] = (sbyte)URShift(y, 24);
            }
        }

        #endregion // Byte

        #endregion // Operations
        #region Cloning

        /* We're overriding all internal data, to my knowledge, so this should be okay */
        public virtual object Clone()
        {
            try
            {
                var f = (MersenneTwisterFast)MemberwiseClone();
                f._mt = (int[])(_mt.Clone());
                f._mag01 = (int[])(_mag01.Clone());
                return f;
            }
            catch (Exception)
            {
                throw new ApplicationException("Clone Not Supported");
            } // should never happen
        }

        #endregion // Cloning
        #region IO

        /// <summary>Reads the entire state of the MersenneTwister RNG from the stream </summary>
        public virtual void ReadState(BinaryReader reader)
        {
            var len = _mt.Length;
            for (var x = 0; x < len; x++)
                _mt[x] = reader.ReadInt32();

            len = _mag01.Length;
            for (var x = 0; x < len; x++)
                _mag01[x] = reader.ReadInt32();

            _mti = reader.ReadInt32();
            __nextNextGaussian = reader.ReadDouble();
            __haveNextNextGaussian = reader.ReadBoolean();
        }

        /// <summary>Writes the entire state of the MersenneTwister RNG to the stream </summary>
        public virtual void WriteState(BinaryWriter writer)
        {
            var len = _mt.Length;
            for (var x = 0; x < len; x++)
                writer.Write(_mt[x]);

            len = _mag01.Length;
            for (var x = 0; x < len; x++)
                writer.Write(_mag01[x]);

            writer.Write(_mti);
            writer.Write(__nextNextGaussian);
            writer.Write(__haveNextNextGaussian);
        }

        #endregion // IO
        #region Test

        /// <summary> 
        /// Tests the code.
        /// </summary>
        [STAThread]
        public static void Main(String[] args)
        {
            int j;

            // CORRECTNESS TEST
            // COMPARE WITH http://www.math.keio.ac.jp/matumoto/CODES/MT2002/mt19937ar.out

            var r = new MersenneTwisterFast(new int[] { 0x123, 0x234, 0x345, 0x456 });
            Console.Out.WriteLine("Output of MersenneTwisterFast with new (2002/1/26) seeding mechanism");
            for (j = 0; j < 1000; j++)
            {
                // first, convert the int from signed to "unsigned"
                var l = (long)r.NextInt();
                if (l < 0)
                    l += 4294967296L; // max int value
                var s = Convert.ToString(l);
                while (s.Length < 10)
                    s = " " + s; // buffer
                Console.Out.Write(s + " ");
                if (j % 5 == 4)
                    Console.Out.WriteLine();
            }

            // SPEED TEST

            const long SEED = 4357;

            Console.Out.WriteLine("\nTime to test grabbing 100000000 ints");

            var rr = new Random((Int32)SEED);
            var xx = 0;
            var ms = (DateTime.Now.Ticks - 621355968000000000) / 10000;
            for (j = 0; j < 100000000; j++)
            {
                xx += rr.Next();
            }
            Console.Out.WriteLine("java.util.Random: " + ((DateTime.Now.Ticks - 621355968000000000) / 10000 - ms) + "          Ignore this: " + xx);

            r = new MersenneTwisterFast(SEED);
            ms = (DateTime.Now.Ticks - 621355968000000000) / 10000;
            xx = 0;
            for (j = 0; j < 100000000; j++)
                xx += r.NextInt();
            Console.Out.WriteLine("Mersenne Twister Fast: " + ((DateTime.Now.Ticks - 621355968000000000) / 10000 - ms) + "          Ignore this: " + xx);

            // TEST TO COMPARE TYPE CONVERSION BETWEEN
            // MersenneTwisterFast.java AND MersenneTwister.java

            Console.Out.WriteLine("\nGrab the first 1000 booleans");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextBoolean() + " ");
                if (j % 8 == 7)
                    Console.Out.WriteLine();
            }
            if (j % 8 != 7)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab 1000 booleans of increasing probability using nextBoolean(double)");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextBoolean(j / 999.0) + " ");
                if (j % 8 == 7)
                    Console.Out.WriteLine();
            }
            if (j % 8 != 7)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab 1000 booleans of increasing probability using nextBoolean(float)");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextBoolean(j / 999.0f) + " ");
                if (j % 8 == 7)
                    Console.Out.WriteLine();
            }
            if (j % 8 != 7)
                Console.Out.WriteLine();

            var bytes = new sbyte[1000];
            Console.Out.WriteLine("\nGrab the first 1000 bytes using nextBytes");
            r = new MersenneTwisterFast(SEED);
            r.NextBytes(bytes);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(bytes[j] + " ");
                if (j % 16 == 15)
                    Console.Out.WriteLine();
            }
            if (j % 16 != 15)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 bytes -- must be same as nextBytes");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                sbyte b;
                Console.Out.Write((b = r.NextByte()) + " ");
                if (b != bytes[j])
                    Console.Out.Write("BAD ");
                if (j % 16 == 15)
                    Console.Out.WriteLine();
            }
            if (j % 16 != 15)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 shorts");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextShort() + " ");
                if (j % 8 == 7)
                    Console.Out.WriteLine();
            }
            if (j % 8 != 7)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 ints");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextInt() + " ");
                if (j % 4 == 3)
                    Console.Out.WriteLine();
            }
            if (j % 4 != 3)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 ints of different sizes");
            r = new MersenneTwisterFast(SEED);
            var max = 1;
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextInt(max) + " ");
                max *= 2;
                if (max <= 0)
                    max = 1;
                if (j % 4 == 3)
                    Console.Out.WriteLine();
            }
            if (j % 4 != 3)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 longs");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextLong() + " ");
                if (j % 3 == 2)
                    Console.Out.WriteLine();
            }
            if (j % 3 != 2)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 longs of different sizes");
            r = new MersenneTwisterFast(SEED);
            long max2 = 1;
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextLong(max2) + " ");
                max2 *= 2;
                if (max2 <= 0)
                    max2 = 1;
                if (j % 4 == 3)
                    Console.Out.WriteLine();
            }
            if (j % 4 != 3)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 floats");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextFloat() + " ");
                if (j % 4 == 3)
                    Console.Out.WriteLine();
            }
            if (j % 4 != 3)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 doubles");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextDouble() + " ");
                if (j % 3 == 2)
                    Console.Out.WriteLine();
            }
            if (j % 3 != 2)
                Console.Out.WriteLine();

            Console.Out.WriteLine("\nGrab the first 1000 gaussian doubles");
            r = new MersenneTwisterFast(SEED);
            for (j = 0; j < 1000; j++)
            {
                Console.Out.Write(r.NextGaussian() + " ");
                if (j % 3 == 2)
                    Console.Out.WriteLine();
            }
            if (j % 3 != 2)
                Console.Out.WriteLine();
        }

        #endregion // Test
        #region Serialization (Not Currently Used)

        public virtual bool StateEquals(object o)
        {
            if (o == this)
                return true;
            if (o == null || !(o is MersenneTwisterFast))
                return false;
            var other = (MersenneTwisterFast)o;
            if (_mti != other._mti)
                return false;
            for (var x = 0; x < _mag01.Length; x++)
                if (_mag01[x] != other._mag01[x])
                    return false;
            for (var x = 0; x < _mt.Length; x++)
                if (_mt[x] != other._mt[x])
                    return false;
            return true;
        }

        /// <summary>
        /// Writes the serializable fields to the SerializationInfo object, which stores all the data needed to serialize the specified object object.
        /// </summary>
        /// <param name="info">SerializationInfo parameter from the GetObjectData method.</param>
        /// <param name="context">StreamingContext parameter from the GetObjectData method.</param>
        /// <param name="instance">Object to serialize.</param>
        protected static void DefaultWriteObject(SerializationInfo info, StreamingContext context, object instance)
        {
            var thisType = instance.GetType();
            var mi = FormatterServices.GetSerializableMembers(thisType, context);
            foreach (var t in mi)
            {
                info.AddValue(t.Name, ((System.Reflection.FieldInfo)t).GetValue(instance));
            }
        }

        /// <summary>
        /// Reads the serialized fields written by the DefaultWriteObject method.
        /// </summary>
        /// <param name="info">SerializationInfo parameter from the special deserialization constructor.</param>
        /// <param name="context">StreamingContext parameter from the special deserialization constructor</param>
        /// <param name="instance">Object to deserialize.</param>
        protected static void DefaultReadObject(SerializationInfo info, StreamingContext context, object instance)
        {
            var thisType = instance.GetType();
            var mi = FormatterServices.GetSerializableMembers(thisType, context);
            foreach (var t in mi)
            {
                var fi = (FieldInfo)t;
                fi.SetValue(instance, info.GetValue(fi.Name, fi.FieldType));
            }
        }

        public virtual void GetObjectData(SerializationInfo outputInfo, StreamingContext context)
        {
            lock (this)
            {
                // just so we're synchronized.
                DefaultWriteObject(outputInfo, context, this);
            }
        }
        #endregion // Serialization (Not Currently Used)
    }
}