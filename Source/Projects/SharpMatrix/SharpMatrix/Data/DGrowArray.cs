using System;

namespace SharpMatrix.Data
{
    //package org.ejml.data;

/**
 * A double array which can have its size changed
 *
 * @author Peter Abeles
 */
    public class DGrowArray
    {
        public double[] data;
        public int Length;

        public DGrowArray(int length)
        {
            this.data = new double[length];
            this.Length = length;
        }

        public DGrowArray()
            : this(0)
        {
        }

        public int length()
        {
            return Length;
        }

        public void reshape(int length)
        {
            if (data.Length < length)
            {
                data = new double[length];
            }
            this.Length = length;
        }

        public double get(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentException("Out of bounds");
            return data[index];
        }

        public void set(int index, int value)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentException("Out of bounds");
            data[index] = value;
        }

        public void free()
        {
            data = new double[0];
            Length = 0;
        }
    }
}