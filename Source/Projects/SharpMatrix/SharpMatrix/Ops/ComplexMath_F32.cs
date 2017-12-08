using System;

namespace SharpMatrix.Data
{
    /**
     * Basic math operations on complex numbers.
     *
     * @author Peter Abeles
     */
    public class ComplexMath_F32
    {

        /**
         * Complex conjugate
         * @param input Input complex number
         * @param conj Complex conjugate of the input number
         */
        public static void conj(Complex_F32 input, Complex_F32 conj)
        {
            conj.real = input.real;
            conj.imaginary = -input.imaginary;
        }

        /**
         * <p>
         * Addition: result = a + b
         * </p>
         *
         * @param a Complex number. Not modified.
         * @param b Complex number. Not modified.
         * @param result Storage for output
         */
        public static void plus(Complex_F32 a, Complex_F32 b, Complex_F32 result)
        {
            result.real = a.real + b.real;
            result.imaginary = a.imaginary + b.imaginary;
        }

        /**
         * <p>
         * Subtraction: result = a - b
         * </p>
         *
         * @param a Complex number. Not modified.
         * @param b Complex number. Not modified.
         * @param result Storage for output
         */
        public static void minus(Complex_F32 a, Complex_F32 b, Complex_F32 result)
        {
            result.real = a.real - b.real;
            result.imaginary = a.imaginary - b.imaginary;
        }

        /**
         * <p>
         * Multiplication: result = a * b
         * </p>
         *
         * @param a Complex number. Not modified.
         * @param b Complex number. Not modified.
         * @param result Storage for output
         */
        public static void multiply(Complex_F32 a, Complex_F32 b, Complex_F32 result)
        {
            result.real = a.real * b.real - a.imaginary * b.imaginary;
            result.imaginary = a.real * b.imaginary + a.imaginary * b.real;
        }

        /**
         * <p>
         * Division: result = a / b
         * </p>
         *
         * @param a Complex number. Not modified.
         * @param b Complex number. Not modified.
         * @param result Storage for output
         */
        public static void divide(Complex_F32 a, Complex_F32 b, Complex_F32 result)
        {
            float norm = b.getMagnitude2();
            result.real = (a.real * b.real + a.imaginary * b.imaginary) / norm;
            result.imaginary = (a.imaginary * b.real - a.real * b.imaginary) / norm;
        }

        /**
         * <p>
         * Converts a complex number into polar notation.
         * </p>
         *
         * @param input Standard notation
         * @param output Polar notation
         */
        public static void convert(Complex_F32 input, ComplexPolar_F32 output)
        {
            output.r = input.getMagnitude();
            output.theta = (float) Math.Atan2(input.imaginary, input.real);
        }

        /**
         * <p>
         * Converts a complex number in polar notation into standard notation.
         * </p>
         *
         * @param input Standard notation
         * @param output Polar notation
         */
        public static void convert(ComplexPolar_F32 input, Complex_F32 output)
        {
            output.real = input.r * (float) Math.Cos(input.theta);
            output.imaginary = input.r * (float) Math.Sin(input.theta);
        }

        /**
         * Division in polar notation.
         *
         * @param a Complex number in polar notation. Not modified.
         * @param b Complex number in polar notation. Not modified.
         * @param result Storage for output.
         */
        public static void multiply(ComplexPolar_F32 a, ComplexPolar_F32 b, ComplexPolar_F32 result)
        {
            result.r = a.r * b.r;
            result.theta = a.theta + b.theta;
        }

        /**
         * Division in polar notation.
         *
         * @param a Complex number in polar notation. Not modified.
         * @param b Complex number in polar notation. Not modified.
         * @param result Storage for output.
         */
        public static void divide(ComplexPolar_F32 a, ComplexPolar_F32 b, ComplexPolar_F32 result)
        {
            result.r = a.r / b.r;
            result.theta = a.theta - b.theta;
        }

        /**
         * Computes the power of a complex number in polar notation
         *
         * @param a Complex number
         * @param N Power it is to be multiplied by
         * @param result Result
         */
        public static void pow(ComplexPolar_F32 a, int N, ComplexPolar_F32 result)
        {
            result.r = (float) Math.Pow(a.r, N);
            result.theta = N * a.theta;
        }

        /**
         * Computes the N<sup>th</sup> root of a complex number in polar notation.  There are
         * N distinct N<sup>th</sup> roots.
         *
         * @param a Complex number
         * @param N The root's magnitude
         * @param k Specifies which root.  0 &le; k &lt; N
         * @param result Computed root
         */
        public static void root(ComplexPolar_F32 a, int N, int k, ComplexPolar_F32 result)
        {
            result.r = (float) Math.Pow(a.r, 1.0f / N);
            result.theta = (a.theta + 2.0f * k * UtilEjml.F_PI) / N;
        }

        /**
         * Computes the N<sup>th</sup> root of a complex number.  There are
         * N distinct N<sup>th</sup> roots.
         *
         * @param a Complex number
         * @param N The root's magnitude
         * @param k Specifies which root.  0 &le; k &lt; N
         * @param result Computed root
         */
        public static void root(Complex_F32 a, int N, int k, Complex_F32 result)
        {
            float r = a.getMagnitude();
            float theta = (float) Math.Atan2(a.imaginary, a.real);

            r = (float) Math.Pow(r, 1.0f / N);
            theta = (theta + 2.0f * k * UtilEjml.F_PI) / N;

            result.real = r * (float) Math.Cos(theta);
            result.imaginary = r * (float) Math.Sin(theta);
        }

        /**
         * Computes the square root of the complex number.
         *
         * @param input Input complex number.
         * @param root Output. The square root of the input
         */
        public static void sqrt(Complex_F32 input, Complex_F32 root)
        {
            float r = input.getMagnitude();
            float a = input.real;

            root.real = (float) Math.Sqrt((r + a) / 2.0f);
            root.imaginary = (float) Math.Sqrt((r - a) / 2.0f);
            if (input.imaginary < 0)
                root.imaginary = -root.imaginary;
        }
    }
}