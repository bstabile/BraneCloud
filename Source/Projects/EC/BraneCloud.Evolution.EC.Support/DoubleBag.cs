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
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Support
{
    /** Maintains a simple array (objs) of doubles and the number of doubles (numObjs) in the array
        (the array can be bigger than this number).  You are encouraged to access the doubles directly;
        they are stored in positions [0 ... numObjs-1].  If you wish to extend the array, you should call
        the resize method.
        
        <p>DoubleBag is approximately to double what Bag is to Object.  However, for obvious reasons, DoubleBag is not
        a java.util.Collection subclass and is purposely simple (it doesn't have an Iterator for example).
        
        <p>DoubleBag is not synchronized, and so should not be accessed from different threads without locking on it
        or some appropriate lock double first.  DoubleBag also has an unusual, fast method for removing doubles
        called remove(...), which removes the double simply by swapping the topmost double into its
        place.  This means that after remove(...) is called, the DoubleBag may no longer have the same order
        (hence the reason it's called a "DoubleBag" rather than some variant on "Vector" or "Array" or "List").  You can
        guarantee order by calling removeNondestructively(...) instead if you wish, but this is O(n) in the worst case.
*/

    [Serializable]
    public class DoubleBag : ICloneable, Indexed
    {
        private const long SerialVersionUID = 1;

        public double[] Objs { get; set; }
        public int NumObjs { get; set; }

        /** Creates a DoubleBag with a given initial capacity. */
        public DoubleBag(int capacity)
        {
            NumObjs = 0;
            Objs = new double[capacity];
        }

        public DoubleBag()
        {
            NumObjs = 0;
            Objs = new double[1];
        }

        /** Adds the doubles from the other DoubleBag without copying them.  The size of the
            new DoubleBag is the minimum necessary size to hold the doubles.  If the Other DoubleBag is
            null, a new empty DoubleBag is created. */
        public DoubleBag(DoubleBag other)
        {
            if (other == null)
            {
                NumObjs = 0;
                Objs = new double[1];
            }
            else
            {
                NumObjs = other.NumObjs;
                Objs = new double[NumObjs];
                Array.Copy(other.Objs, 0, Objs, 0, NumObjs);
            }
        }

        /** Creates a DoubleBag with the given elements. If the Other array is
            null, a new empty DoubleBag is created. */
        public DoubleBag(double[] other)
            : this()
        {
            if (other != null) AddAll(other);
        }

        public int Size => NumObjs;


        public bool IsEmpty => NumObjs <= 0;

        public bool AddAll(double[] other)
        {
            return AddAll(NumObjs, other);
        }

        public bool AddAll(int index, double[] other)
        {
            // throws NullPointerException if other == null,
            // ArrayArrayIndexOutOfBoundsException if index < 0,
            // and ArrayIndexOutOfBoundsException if index > numObjs
            if (index > NumObjs)
                throw new ArgumentOutOfRangeException(index.ToString());
            // { throwArrayIndexOutOfBoundsException(index); }
            if (other.Length == 0) return false;
            // make DoubleBag big enough
            if (NumObjs + other.Length > Objs.Length)
                Resize(NumObjs + other.Length);
            if (index != NumObjs) // scoot over elements if we're inserting in the middle
                Array.Copy(Objs, index, Objs, index + other.Length, NumObjs - index);
            Array.Copy(other, 0, Objs, index, other.Length);
            NumObjs += other.Length;
            return true;
        }

        public bool AddAll(DoubleBag other)
        {
            return AddAll(NumObjs, other);
        }

        public bool AddAll(int index, DoubleBag other)
        {
            // throws NullPointerException if other == null,
            // ArrayArrayIndexOutOfBoundsException if index < 0,
            // and ArrayIndexOutOfBoundsException if index > numObjs
            if (index > NumObjs)
                throw new ArgumentOutOfRangeException(index.ToString());
            //{ throwArrayIndexOutOfBoundsException(index); }
            if (other.NumObjs <= 0) return false;
            // make DoubleBag big enough
            if (NumObjs + other.NumObjs > Objs.Length)
                Resize(NumObjs + other.NumObjs);
            if (index != NumObjs) // scoot over elements if we're inserting in the middle
                Array.Copy(Objs, index, Objs, index + other.Size, NumObjs - index);
            Array.Copy(other.Objs, 0, Objs, index, other.NumObjs);
            NumObjs += other.NumObjs;
            return true;
        }

        public virtual object Clone()
        {
            var b = (DoubleBag) MemberwiseClone();
            b.Objs = (double[]) Objs.Clone();
            return b;
        }

        public void Resize(int toAtLeast)
        {
            if (Objs.Length >= toAtLeast) // already at least as big as requested
                return;

            if (Objs.Length * 2 > toAtLeast) // worth doubling
                toAtLeast = Objs.Length * 2;

            // now resize
            double[] newobjs = new double[toAtLeast];
            Array.Copy(Objs, 0, newobjs, 0, NumObjs);
            Objs = newobjs;
        }

        /** Resizes the objs array to max(numObjs, desiredLength), unless that value is greater than or equal to objs.Length,
            in which case no resizing is done (this operation only shrinks -- use resize() instead).
            This is an O(n) operation, so use it sparingly. */
        public void Shrink(int desiredLength)
        {
            if (desiredLength < NumObjs) desiredLength = NumObjs;
            if (desiredLength >= Objs.Length) return; // no reason to bother
            double[] newobjs = new double[desiredLength];
            Array.Copy(Objs, 0, newobjs, 0, NumObjs);
            Objs = newobjs;
        }


        /** Returns 0 if the DoubleBag is empty, else returns the topmost double. */
        public double Top()
        {
            if (NumObjs <= 0) return 0;
            else return Objs[NumObjs - 1];
        }

        /** Returns 0 if the DoubleBag is empty, else removes and returns the topmost double. */
        public double Pop()
        {
            // this curious arrangement makes me small enough to be inlined (35 bytes; right at the limit)
            int numObjs = NumObjs;
            if (numObjs <= 0) return 0;
            double ret = Objs[--numObjs];
            NumObjs = numObjs;
            return ret;
        }

        /** Synonym for add(obj) -- try to use add instead unless you
            want to think of the DoubleBag as a stack. */
        public bool Push(double obj)
        {
            if (NumObjs >= Objs.Length) DoubleCapacityPlusOne();
            Objs[NumObjs++] = obj;
            return true;
        }

        public bool Add(double obj)
        {
            if (NumObjs >= Objs.Length) DoubleCapacityPlusOne();
            Objs[NumObjs++] = obj;
            return true;
        }

        // private function used by add and push in order to get them below
        // 35 bytes -- always doubles the capacity and adds one
        void DoubleCapacityPlusOne()
        {
            double[] newobjs = new double[NumObjs * 2 + 1];
            Array.Copy(Objs, 0, newobjs, 0, NumObjs);
            Objs = newobjs;
        }

        public bool Contains(double o)
        {
            int numObjs = NumObjs;
            double[] objs = Objs;
            for (int x = 0; x < numObjs; x++)
                if (o.Equals(objs[x])) return true;
            return false;
        }

        public double Get(int index)
        {
            if (index >= NumObjs) //  || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index.ToString());
            return Objs[index];
        }

        public object GetValue(int index)
        {
            return Get(index);
        }

        public double Set(int index, double element)
        {
            if (index >= NumObjs) // || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index.ToString());
            double returnval = Objs[index];
            Objs[index] = element;
            return returnval;
        }

        public object SetValue(int index, object value)
        {
            double old = Get(index);
            double? newval;
            try
            {
                newval = (double) value;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("Expected a Double");
            }
            Set(index, newval.Value);
            return old;
        }

        /** Removes the double at the given index, shifting the other doubles down. */
        public double RemoveNondestructively(int index)
        {
            if (index >= NumObjs) // || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index.ToString());
            double ret = Objs[index];
            if (index < NumObjs - 1) // it's not the topmost double, must swap down
                Array.Copy(Objs, index + 1, Objs, index, NumObjs - index - 1);
            NumObjs--;
            return ret;
        }

        /** Removes the double at the given index, moving the topmost double into its position. */
        public double Remove(int index)
        {
            int numObjs = NumObjs;
            if (index >= numObjs) // || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index.ToString());
            double[] objs = Objs;
            double ret = objs[index];
            objs[index] = objs[numObjs - 1];
            NumObjs--;
            return ret;
        }

        /** Sorts the doubles into ascending numerical order. */
        public void Sort()
        {
            Array.Sort(Objs, 0, NumObjs);
        }


        /** Replaces all elements in the bag with the provided object. */
        public void Fill(double o)
        {
            // teeny bit faster
            double[] objs = Objs;
            int numObjs = NumObjs;

            for (int x = 0; x < numObjs; x++)
                objs[x] = o;
        }

        /** Shuffles (randomizes the order of) the DoubleBag */
        public void Shuffle(Random random)
        {
            // teeny bit faster
            double[] objs = Objs;
            int numObjs = NumObjs;
            double obj;
            int rand;

            for (int x = numObjs - 1; x >= 1; x--)
            {
                rand = random.Next(x + 1);
                obj = objs[x];
                objs[x] = objs[rand];
                objs[rand] = obj;
            }
        }

        /** Shuffles (randomizes the order of) the DoubleBag */
        public void Shuffle(IMersenneTwister random)
        {
            // teeny bit faster
            double[] objs = Objs;
            int numObjs = NumObjs;
            double obj;
            int rand;

            for (int x = numObjs - 1; x >= 1; x--)
            {
                rand = random.NextInt(x + 1);
                obj = objs[x];
                objs[x] = objs[rand];
                objs[rand] = obj;
            }
        }

        /** Reverses order of the elements in the DoubleBag */
        public void Reverse()
        {
            // teeny bit faster
            double[] objs = Objs;
            int numObjs = NumObjs;
            int l = numObjs / 2;
            for (int x = 0; x < l; x++)
            {
                var obj = objs[x];
                objs[x] = objs[numObjs - x - 1];
                objs[numObjs - x - 1] = obj;
            }
        }

        /** Removes all numbers in the DoubleBag.  This is done by clearing the internal array but 
            not replacing it with a new, smaller one. */
        public void Clear()
        {
            NumObjs = 0;
        }

        /**    
               Copies 'len' elements from the Bag into the provided array.
               The 'len' elements start at index 'fromStart' in the Bag, and
               are copied into the provided array starting at 'toStat'.
        */
        public void CopyIntoArray(int fromStart, double[] to, int toStart, int len)
        {
            Array.Copy(Objs, fromStart, to, toStart, len);
        }

        public double[] ToArray()
        {
            double[] o = new double[NumObjs];
            Array.Copy(Objs, 0, o, 0, NumObjs);
            return o;
        }

        public double[] ToDoubleArray()
        {
            double[] o = new double[NumObjs];
            for (int i = 0; i < NumObjs; i++)
                o[i] = Objs[i];
            return o;
        }

        public Type ComponentType => typeof(double);
    }
}