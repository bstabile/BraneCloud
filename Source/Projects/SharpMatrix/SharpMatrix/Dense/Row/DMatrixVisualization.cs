using System;
using System.Drawing;
using SharpMatrix.Data;

namespace SharpMatrix.Dense.Row
{
    //package org.ejml.dense.row;

//import javax.swing.*;
//import java.awt.*;


/**
 * <p>
 * Functions for visualizing matrices in a GUI matrices.
 * </p>
 *
 * <p>
 * NOTE: In some embedded applications there is no GUI or AWT is not supported (like in Android) so excluding
 * this class is necessary.
 * </p>
 *
 * @author Peter Abeles
 */
    public class DMatrixVisualization
    {
        /**
         * Creates a window visually showing the matrix's state.  Block means an element is zero.
         * Red positive and blue negative.  More intense the color larger the element's absolute value
         * is.
         *
         * @param A A matrix.
         * @param title Name of the window.
         */
        public static void show(DMatrixD1 A, string title)
        {
            // TODO : BRS: Implement for .NET
            throw new NotImplementedException();
            /*
            JFrame frame = new JFrame(title);

            int width = 300;
            int height = 300;

            if (A.numRows > A.numCols)
            {
                width = width * A.numCols / A.numRows;
            }
            else
            {
                height = height * A.numRows / A.numCols;
            }

            DMatrixComponent panel = new DMatrixComponent(width, height);
            panel.setMatrix(A);

            frame.add(panel, BorderLayout.CENTER);

            frame.pack();
            frame.setVisible(true);
            */
        }
    }
}