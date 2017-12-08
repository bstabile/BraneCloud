using System;
using System.Drawing;
using SharpMatrix.Data;

namespace SharpMatrix.Dense.Row
{
    //package org.ejml.dense.row;

//import javax.swing.*;
//import java.awt.*;
//import java.awt.image.BufferedImage;


/**
 * Renders a matrix as an image.  Positive elements are shades of red, negative shades of blue, 0 is black.
 *
 * @author Peter Abeles
 */
    public class FMatrixComponent /* : JPanel */
    {
        Image image;

        public FMatrixComponent(int width, int height)
        {
            // TODO : BRS: Implement for .NET
            throw new NotImplementedException();
            /*
            image = new Image(width, height, BufferedImage.TYPE_INT_RGB);
            setPreferredSize(new Dimension(width, height));
            setMinimumSize(new Dimension(width, height));
            */
        }

        public /* synchronized */ void setMatrix(FMatrixD1 A)
        {
            // TODO : BRS: Implement for .NET
            throw new NotImplementedException();
            /*
            float maxValue = CommonOps_FDRM.elementMaxAbs(A);
            renderMatrix(A, image, maxValue);
            repaint();
            */
        }

        public static void renderMatrix(FMatrixD1 M, Image image, float maxValue)
        {
            // TODO : BRS: Implement for .NET
            throw new NotImplementedException();
            /*
            int w = image.getWidth();
            int h = image.getHeight();

            float widthStep = (float) M.numCols / image.getWidth();
            float heightStep = (float) M.numRows / image.getHeight();

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    float value = M.get((int) (i * heightStep), (int) (j * widthStep));

                    if (value == 0)
                    {
                        image.setRGB(j, i, 255 << 24);
                    }
                    else if (value > 0)
                    {
                        int p = 255 - (int) (255.0f * (value / maxValue));
                        int rgb = 255 << 24 | 255 << 16 | p << 8 | p;

                        image.setRGB(j, i, rgb);
                    }
                    else
                    {
                        int p = 255 + (int) (255.0f * (value / maxValue));
                        int rgb = 255 << 24 | p << 16 | p << 8 | 255;

                        image.setRGB(j, i, rgb);
                    }
                }
                */
            }


        public /* override synchronized */ void paint(Graphics g)
        {
            // TODO : BRS: Implement for .NET
            throw new NotImplementedException();
            /*
            g.drawImage(image, 0, 0, this);
            */
        }
    }

}