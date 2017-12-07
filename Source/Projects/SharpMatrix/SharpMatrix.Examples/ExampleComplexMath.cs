using System;
using BraneCloud.Evolution.EC.MatrixLib.Data;
using BraneCloud.Evolution.EC.MatrixLib.Ops;

namespace BraneCloud.Evolution.EC.MatrixLib.Examples
{
    //package org.ejml.example;

/**
 * Demonstration of different operations that can be performed on complex numbers.
 *
 * @author Peter Abeles
 */
    public class ExampleComplexMath
    {

        public static void main(string[]args)
        {
            Complex_F64 a = new Complex_F64(1, 2);
            Complex_F64 b = new Complex_F64(-1, -0.6);
            Complex_F64 c = new Complex_F64();
            ComplexPolar_F64 polarC = new ComplexPolar_F64();

            Console.WriteLine("a = " + a);
            Console.WriteLine("b = " + b);
            Console.WriteLine("------------------");

            ComplexMath_F64.plus(a, b, c);
            Console.WriteLine("a + b = " + c);
            ComplexMath_F64.minus(a, b, c);
            Console.WriteLine("a - b = " + c);
            ComplexMath_F64.multiply(a, b, c);
            Console.WriteLine("a * b = " + c);
            ComplexMath_F64.divide(a, b, c);
            Console.WriteLine("a / b = " + c);

            Console.WriteLine("------------------");
            ComplexPolar_F64 polarA = new ComplexPolar_F64();
            ComplexMath_F64.convert(a, polarA);
            Console.WriteLine("polar notation of a = " + polarA);
            ComplexMath_F64.pow(polarA, 3, polarC);
            Console.WriteLine("a ** 3 = " + polarC);
            ComplexMath_F64.convert(polarC, c);
            Console.WriteLine("a ** 3 = " + c);
        }
    }
}