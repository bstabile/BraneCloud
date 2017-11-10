using System;
using System.Linq;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.Support
{
    /*
      Copyright 2006 by Sean Luke and George Mason University
      Licensed under the Academic Free License version 3.0
      See the file "LICENSE" for more information
    */

    /** Maintains a simple array (objs) of ints and the number of ints (numObjs) in the array
        (the array can be bigger than this number).  You are encouraged to access the ints directly;
        they are stored in positions [0 ... numObjs-1].  If you wish to extend the array, you should call
        the Resize method.
        
        <p>IntBag is approximately to int what Bag is to Object.  However, for obvious reasons, IntBag is not
        a java.util.Collection subclass and is purposely simple (it doesn't have an Iterator for example).
        
        <p>IntBag is not synchronized, and so should not be accessed from different threads without locking on it
        or some appropriate lock int first.  IntBag also has an unusual, fast method for removing ints
        called remove(...), which removes the int simply by swapping the topmost int into its
        place.  This means that after remove(...) is called, the IntBag may no longer have the same order
        (hence the reason it's called a "IntBag" rather than some variant on "Vector" or "Array" or "List").  You can
        guarantee order by calling removeNondestructively(...) instead if you wish, but this is O(n) in the worst case.
*/
    [Serializable]
    public class IntBag : ICloneable, Indexed
    {
        public int[] Objs { get; set; }
        public int NumObjs { get; set; }

        /** Creates an IntBag with a given initial capacity. */
        public IntBag(int capacity)
        {
            NumObjs = 0;
            Objs = new int[capacity];
        }

        public IntBag()
        {
            NumObjs = 0;
            Objs = new int[1];
        }

        /** Adds the ints from the other IntBag without copying them.  The size of the
            new IntBag is the minimum necessary size to hold the ints. */
        public IntBag(IntBag other)
        {
            if (other == null)
            {
                NumObjs = 0;
                Objs = new int[1];
            }
            NumObjs = other.NumObjs;
            Objs = new int[NumObjs];
            Array.Copy(other.Objs, 0, Objs, 0, NumObjs);
        }

        public int Size => NumObjs;

        public bool IsEmpty => NumObjs <= 0;

        public bool AddAll(int index, int[] other)
        {
            // throws NullPointerException if other == null,
            // ArrayIndexOutOfBoundsException if index < 0,
            // and IndexOutOfBoundsException if index > numObjs
            if (index > NumObjs)
            {
                ThrowIndexOutOfBoundsException(index);
            }
            if (other.Length == 0) return false;
            // make IntBag big enough
            if (NumObjs + other.Length > Objs.Length)
                Resize(NumObjs + other.Length);
            if (index != NumObjs) // make room
                Array.Copy(Objs, index, Objs, index + other.Length, other.Length);
            Array.Copy(other, 0, Objs, index, other.Length);
            NumObjs += other.Length;
            return true;
        }

        public bool AddAll(IntBag other)
        {
            return AddAll(NumObjs, other);
        }

        public bool AddAll(int index, IntBag other)
        {
            // throws NullPointerException if other == null,
            // ArrayIndexOutOfBoundsException if index < 0,
            // and IndexOutOfBoundsException if index > numObjs
            if (index > NumObjs)
            {
                ThrowIndexOutOfBoundsException(index);
            }
            if (other.NumObjs <= 0) return false;
            // make IntBag big enough
            if (NumObjs + other.NumObjs > Objs.Length)
                Resize(NumObjs + other.NumObjs);
            if (index != NumObjs) // make room
                Array.Copy(Objs, index, Objs, index + other.NumObjs, other.NumObjs);
            Array.Copy(other.Objs, 0, Objs, index, other.NumObjs);
            NumObjs += other.NumObjs;
            return true;
        }

        /// <summary>
        /// This is a deep clone because it would be a mess to have 
        /// two clients working against a shallow copy (MemberwiseClone).
        /// </summary>
        public virtual object Clone()
        {
            return new IntBag {Objs = this.Objs.ToArray(), NumObjs = this.NumObjs};
        }

        public void Resize(int toAtLeast)
        {
            if (Objs.Length >= toAtLeast) // already at least as big as requested
                return;

            if (Objs.Length * 2 > toAtLeast) // worth doubling
                toAtLeast = Objs.Length * 2;

            // now resize
            int[] newobjs = new int[toAtLeast];
            Array.Copy(Objs, 0, newobjs, 0, NumObjs);
            Objs = newobjs;
        }

        /** Resizes the objs array to max(numObjs, desiredLength), unless that value is greater than or equal to objs.Length,
            in which case no resizing is done (this operation only shrinks -- use Resize() instead).
            This is an O(n) operation, so use it sparingly. */
        public void Shrink(int desiredLength)
        {
            if (desiredLength < NumObjs) desiredLength = NumObjs;
            if (desiredLength >= Objs.Length) return; // no reason to bother
            int[] newobjs = new int[desiredLength];
            Array.Copy(Objs, 0, newobjs, 0, NumObjs);
            Objs = newobjs;
        }

        /** Returns 0 if the IntBag is empty, else returns the topmost int. */
        public int Top()
        {
            if (NumObjs <= 0) return 0;
            else return Objs[NumObjs - 1];
        }

        /** Returns 0 if the IntBag is empty, else removes and returns the topmost int. */
        public int Pop()
        {
            // this curious arrangement makes me small enough to be inlined (28 bytes)
            int n = this.NumObjs;
            if (n <= 0) return 0;
            int ret = Objs[--n];
            this.NumObjs = n;
            return ret;
        }

        /** Synonym for add(obj) -- try to use add instead unless you
            want to think of the IntBag as a stack. */
        public bool Push(int obj)
        {
            // this curious arrangement makes me small enough to be inlined (35 bytes)
            int n = this.NumObjs;
            if (n >= Objs.Length) DoubleCapacityPlusOne();
            Objs[n] = obj;
            this.NumObjs = n + 1;
            return true;
        }

        public bool Add(int obj)
        {
            // this curious arrangement makes me small enough to be inlined (35 bytes)
            int n = this.NumObjs;
            if (n >= Objs.Length) DoubleCapacityPlusOne();
            Objs[n] = obj;
            this.NumObjs = n + 1;
            return true;
        }

        // private function used by add and push in order to get them below
        // 35 bytes -- always doubles the capacity and adds one
        void DoubleCapacityPlusOne()
        {
            int[] newobjs = new int[NumObjs * 2 + 1];
            Array.Copy(Objs, 0, newobjs, 0, NumObjs);
            Objs = newobjs;
        }

        public bool Contains(int o)
        {
            int numObjs = this.NumObjs;
            int[] objs = this.Objs;
            for (int x = 0; x < numObjs; x++)
                if (o == objs[x]) return true;
            return false;
        }

        public int Get(int index)
        {
            if (index >= NumObjs) // || index < 0)
                ThrowIndexOutOfBoundsException(index);
            return Objs[index];
        }

        public Object GetValue(int index)
        {
            return new Integer(Get(index));
        }

        public int Set(int index, int element)
        {
            if (index >= NumObjs) // || index < 0)
                ThrowIndexOutOfBoundsException(index);
            int returnval = Objs[index];
            Objs[index] = element;
            return returnval;
        }

        public Object SetValue(int index, Object value)
        {
            Integer old = new Integer(Get(index));
            Integer newval = null;
            try
            {
                newval = (Integer) value;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("Expected an Integer");
            }
            Set(index, (int) newval.Value);
            return old;
        }

        /** Removes the int at the given index, shifting the other ints down. */
        public int RemoveNondestructively(int index)
        {
            if (index >= NumObjs) // || index < 0)
                ThrowIndexOutOfBoundsException(index);
            int ret = Objs[index];
            if (index < NumObjs - 1) // it's not the topmost int, must swap down
                Array.Copy(Objs, index + 1, Objs, index, NumObjs - index - 1);
            NumObjs--;
            return ret;
        }

        /** Removes the int at the given index, moving the topmost int into its position. */
        public int Remove(int index)
        {
            int _numObjs = NumObjs;
            if (index >= _numObjs) // || index < 0)
                ThrowIndexOutOfBoundsException(index);
            int[] _objs = this.Objs;
            int ret = _objs[index];
            _objs[index] = _objs[_numObjs - 1];
            NumObjs--;
            return ret;
        }

        /** Sorts the ints into ascending numerical order. */
        public void Sort()
        {
            Array.Sort(Objs, 0, NumObjs);
        }

        /** Replaces all elements in the bag with the provided int. */
        public void Fill(int o)
        {
            // teeny bit faster
            int[] arr = this.Objs;
            int n = this.NumObjs;

            for (int x = 0; x < n; x++)
                arr[x] = o;
        }

        /** Shuffles (randomizes the order of) the IntBag */
        public void Shuffle(Random random)
        {
            // teeny bit faster
            int[] arr = this.Objs;
            int n = this.NumObjs;
            int obj;
            int rand;

            for (int x = n - 1; x >= 1; x--)
            {
                rand = random.Next(x + 1);
                obj = arr[x];
                arr[x] = arr[rand];
                arr[rand] = obj;
            }
        }

        /** Shuffles (randomizes the order of) the IntBag */
        public void Shuffle(IMersenneTwister random)
        {
            // teeny bit faster
            int[] arr = this.Objs;
            int n = this.NumObjs;
            int obj;
            int rand;

            for (int x = n - 1; x >= 1; x--)
            {
                rand = random.NextInt(x + 1);
                obj = arr[x];
                arr[x] = arr[rand];
                arr[rand] = obj;
            }
        }

        /** Reverses order of the elements in the IntBag */
        public void Reverse()
        {
            // teeny bit faster
            int[] objs = this.Objs;
            int numObjs = this.NumObjs;
            int l = numObjs / 2;
            int obj;
            for (int x = 0; x < l; x++)
            {
                obj = objs[x];
                objs[x] = objs[numObjs - x - 1];
                objs[numObjs - x - 1] = obj;
            }
        }

        protected void ThrowIndexOutOfBoundsException(int index)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index.ToString());
        }

        /** Removes all numbers in the IntBag.  This is done by clearing the internal array but 
            not replacing it with a new, smaller one. */
        public void Clear()
        {
            NumObjs = 0;
        }

        public int[] ToArray()
        {
            int[] arr = new int[NumObjs];
            Array.Copy(Objs, 0, arr, 0, NumObjs);
            return arr;
        }

        public Integer[] ToIntegerArray()
        {
            Integer[] o = new Integer[NumObjs];
            for (int i = 0; i < NumObjs; i++)
                o[i] = new Integer(Objs[i]);
            return o;
        }

        public long[] ToLongArray()
        {
            long[] arr = new long[NumObjs];
            for (int i = 0; i < NumObjs; i++)
                arr[i] = Objs[i];
            return arr;
        }

        public Double[] ToDoubleArray()
        {
            Double[] arr = new Double[NumObjs];
            for (int i = 0; i < NumObjs; i++)
                arr[i] = Objs[i];
            return arr;
        }

        public Type ComponentType => typeof(Integer);
    }

    public class Integer
    {
        public Integer() { }

        public Integer(int key)
        {
            Key = key;
            Value = key;
        }
        public int Key { get; set; }
        public object Value { get; set; }
    }
}
