using System;
using SharpMatrix.Data;
using SharpMatrix.Dense.Fixed;
using SharpMatrix.Ops;
using SharpMatrix.Simple;

namespace SharpMatrix.Examples
{
    //package org.ejml.example;

/**
 * In some applications a small fixed sized matrix can speed things up a lot, e.g. 8 times faster.  One application
 * which uses small matrices is graphics and rigid body motion, which extensively uses 3x3 and 4x4 matrices.  This
 * example is to show some examples of how you can use a fixed sized matrix.
 *
 * @author Peter Abeles
 */
    public class ExampleFixedSizedMatrix
    {

        public static void main(string[] args)
        {
            // declare the matrix
            DMatrix3x3 a = new DMatrix3x3();
            DMatrix3x3 b = new DMatrix3x3();

            // Can assign values the usual way
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    a.set(i, j, i + j + 1);
                }
            }

            // Direct manipulation of each value is the fastest way to assign/read values
            a.a11 = 12;
            a.a23 = 64;

            // can print the usual way too
            a.print();

            // most of the standard operations are support
            CommonOps_DDF3.transpose(a, b);
            b.print();

            Console.WriteLine("Determinant = " + CommonOps_DDF3.det(a));

            // matrix-vector operations are also supported
            // Constructors for vectors and matrices can be used to initialize its value
            DMatrix3 v = new DMatrix3(1, 2, 3);
            DMatrix3 result = new DMatrix3();

            CommonOps_DDF3.mult(a, v, result);

            // Conversion into DMatrixRMaj can also be done
            DMatrixRMaj dm = ConvertDMatrixStruct.convert(a, null);

            dm.print();

            // This can be useful if you need do more advanced operations
            SimpleMatrix<DMatrixRMaj> sv = SimpleMatrix<DMatrixRMaj>.wrap(dm).svd().getV() as SimpleMatrix<DMatrixRMaj>;

            // can then convert it back into a fixed matrix
            DMatrix3x3 fv = ConvertDMatrixStruct.convert(sv.getDDRM(), (DMatrix3x3) null);

            Console.WriteLine("Original simple matrix and converted fixed matrix");
            sv.print();
            fv.print();
        }
    }
}