using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    /**
     * <p>
     * Represents a complex number using 64bit floating point numbers.  A complex number is composed of
     * a real and imaginary components.
     * </p>
     */
     [Serializable]
    public class Complex_F32
    {
        public float real;
        public float imaginary;

        public Complex_F32(float real, float imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public Complex_F32()
        {
        }

        public float getReal()
        {
            return real;
        }

        public float getMagnitude()
        {
            return (float) Math.Sqrt(real * real + imaginary * imaginary);
        }

        public float getMagnitude2()
        {
            return real * real + imaginary * imaginary;
        }

        public void setReal(float real)
        {
            this.real = real;
        }

        public float getImaginary()
        {
            return imaginary;
        }

        public void setImaginary(float imaginary)
        {
            this.imaginary = imaginary;
        }

        public void set(float real, float imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public void set(Complex_F32 a)
        {
            this.real = a.real;
            this.imaginary = a.imaginary;
        }

        public bool isReal()
        {
            return imaginary == 0.0f;
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

        public Complex_F32 plus(Complex_F32 a)
        {
            Complex_F32 ret = new Complex_F32();
            ComplexMath_F32.plus(this, a, ret);
            return ret;
        }

        public Complex_F32 minus(Complex_F32 a)
        {
            Complex_F32 ret = new Complex_F32();
            ComplexMath_F32.minus(this, a, ret);
            return ret;
        }

        public Complex_F32 times(Complex_F32 a)
        {
            Complex_F32 ret = new Complex_F32();
            ComplexMath_F32.multiply(this, a, ret);
            return ret;
        }

        public Complex_F32 divide(Complex_F32 a)
        {
            Complex_F32 ret = new Complex_F32();
            ComplexMath_F32.divide(this, a, ret);
            return ret;
        }
    }
}