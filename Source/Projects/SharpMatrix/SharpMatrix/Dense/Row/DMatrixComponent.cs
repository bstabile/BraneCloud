using System;
using System.Drawing;
using BraneCloud.Evolution.EC.MatrixLib.Data;

namespace BraneCloud.Evolution.EC.MatrixLib.Dense.Row
{
    //package org.ejml.dense.row;

/**
 * Renders a matrix as an image.  Positive elements are shades of red, negative shades of blue, 0 is black.
 *
 * @author Peter Abeles
 */
    public class DMatrixComponent /* : JPanel */
    {
        Image image;

        public DMatrixComponent(int width, int height)
        {
            // TODO : BRS: Convert to .NET
            throw new NotImplementedException();
            /*
            image = new Image(width, height, BufferedImage.TYPE_INT_RGB);
            setPreferredSize(new Dimension(width, height));
            setMinimumSize(new Dimension(width, height));
            */
        }

        public /* synchronized */ void setMatrix(DMatrixD1 A)
        {
            // TODO : BRS: Convert to .NET
            throw new NotImplementedException();
            /*
            double maxValue = CommonOps_DDRM.elementMaxAbs(A);
            renderMatrix(A, image, maxValue);
            repaint();
            */
        }

        public static void renderMatrix(DMatrixD1 M, Image image, double maxValue)
        {
            // TODO : BRS: Convert to .NET
            throw new NotImplementedException();
            /*
            int w = image.getWidth();
            int h = image.getHeight();

            double widthStep = (double) M.numCols / image.getWidth();
            double heightStep = (double) M.numRows / image.getHeight();

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    double value = M.get((int) (i * heightStep), (int) (j * widthStep));

                    if (value == 0)
                    {
                        image.setRGB(j, i, 255 << 24);
                    }
                    else if (value > 0)
                    {
                        int p = 255 - (int) (255.0 * (value / maxValue));
                        int rgb = 255 << 24 | 255 << 16 | p << 8 | p;

                        image.setRGB(j, i, rgb);
                    }
                    else
                    {
                        int p = 255 + (int) (255.0 * (value / maxValue));
                        int rgb = 255 << 24 | p << 16 | p << 8 | 255;

                        image.setRGB(j, i, rgb);
                    }
                }
            }
            */

        }

        public /* override synchronized */ void paint(Graphics g)
        {
            // TODO : BRS: Convert to .NET
            throw new NotImplementedException();
            /*
            g.drawImage(image, 0, 0, this);
            */
        }

    }
}
