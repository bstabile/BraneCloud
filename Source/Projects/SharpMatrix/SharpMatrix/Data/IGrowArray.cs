using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib
{
    /**
     * An integer array which can have its size changed
     *
     * @author Peter Abeles
     */
    public class IGrowArray
    {
        public int[] data;
        public int Length;

        public IGrowArray(int length)
        {
            this.data = new int[length];
            this.Length = length;
        }

        public IGrowArray()
            : this(0)
        {
        }

        //public int length()
        //{
        //    return length;
        //}

        public void reshape(int length)
        {
            if (data.Length < length)
            {
                data = new int[length];
            }
            this.Length = length;
        }

        public int get(int index)
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
            data = new int[0];
            Length = 0;
        }
    }
}