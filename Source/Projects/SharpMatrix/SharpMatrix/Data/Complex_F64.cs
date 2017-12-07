using System;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    /**
     * <p>
     * Represents a complex number using 64bit floating point numbers.  A complex number is composed of
     * a real and imaginary components.
     * </p>
     */
    [Serializable]
    public class Complex_F64
    {
        public double real;
        public double imaginary;

        public Complex_F64(double real, double imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public Complex_F64()
        {
        }

        public double getReal()
        {
            return real;
        }

        public double getMagnitude()
        {
            return Math.Sqrt(real * real + imaginary * imaginary);
        }

        public double getMagnitude2()
        {
            return real * real + imaginary * imaginary;
        }

        public void setReal(double real)
        {
            this.real = real;
        }

        public double getImaginary()
        {
            return imaginary;
        }

        public void setImaginary(double imaginary)
        {
            this.imaginary = imaginary;
        }

        public void set(double real, double imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public void set(Complex_F64 a)
        {
            this.real = a.real;
            this.imaginary = a.imaginary;
        }

        public bool isReal()
        {
            return imaginary == 0.0;
        }

        public string toString()
        {
            if (imaginary == 0)
            {
                return "" + real;
            }
            else
            {
                return real + " " + imaginary + "i";
            }
        }

        public Complex_F64 plus(Complex_F64 a)
        {
            Complex_F64 ret = new Complex_F64();
            ComplexMath_F64.plus(this, a, ret);
            return ret;
        }

        public Complex_F64 minus(Complex_F64 a)
        {
            Complex_F64 ret = new Complex_F64();
            ComplexMath_F64.minus(this, a, ret);
            return ret;
        }

        public Complex_F64 times(Complex_F64 a)
        {
            Complex_F64 ret = new Complex_F64();
            ComplexMath_F64.multiply(this, a, ret);
            return ret;
        }

        public Complex_F64 divide(Complex_F64 a)
        {
            Complex_F64 ret = new Complex_F64();
            ComplexMath_F64.divide(this, a, ret);
            return ret;
        }
    }
}