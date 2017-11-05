using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.Mona
{
    /** Picture contains two images: an ORIGINAL image which is loaded from a file, and
    writable IMAGE, which you scribble on and try to make as similar to the ORIGINAL.

        BRS: We can use the Serializable attribute instead of implementing ISerializable
             because System.Drawing.Image is serializable. The Graphics class is not
             so that will be created when the instance is "thawed".
    */


    [ECConfiguration("ec.app.mona.Picture")]
	[Serializable]
    public class Picture : ICloneable
    {

        //public BufferedImage original;
        //public BufferedImage image;
        public Image Original { get; set; }
        public Image Image { get; set; }

        //public Graphics graphics;
        [NonSerialized]
        public Graphics Graphics;

        int[] XPoints = new int[0];
        int[] YPoints = new int[0];

        /** Loads the original and creates a new blank image to scribble on, and a new graphics object. */
        public void Load(FileInfo file)
        {
            try
            {
                Original = Image.FromFile(file.FullName);
                Image = (Image)Original.Clone();
                Graphics = System.Drawing.Graphics.FromImage(Image);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot load image file " + file + " because of error:\n\n" + e);
            }
        }

        public void DisposeGraphics()
        {
            Graphics?.Dispose();
            Graphics = null;
        }

        /** Adds a polygon with the given colors. ALL double values passed in must be 0.0 ... 1.0.
            The values are taken starting at vals[offset].  The first four values are colors and alpha.
            The remaining values are the x and y values of the polygon vertices. 
            You must call graphics.dispose() after you're done with all your polygon-drawing.  */
        public void AddPolygon(double[] vals, int offset, int numVertices)
        {

            // RGB
            double c1 = vals[offset];
            double c2 = vals[offset + 1];
            double c3 = vals[offset + 2];
            double c4 = vals[offset + 3];
            int r = Discretize(c1, 255);
            int b = Discretize(c2, 255);
            int g = Discretize(c3, 255);
            int a = Discretize(c4, 255);
            Color color = Color.FromArgb(a, r, g, b);


            /*
            // HSB (or HSV)
            double c1 = (vals[offset]);
            double c2 = (vals[offset+1]);
            double c3 = (vals[offset+2]);
            double c4 = (vals[offset+3]);
            int r = discretize(c1, 255);
            int b = discretize(c2, 255);
            int g = discretize(c3, 255);
            int a = discretize(c4, 255);
            int rgb = Color.HSBtoRGB((float)c1, (float)c2, (float)c3);
            Color color = new Color((rgb) & 0xFF, (rgb >> 8) & 0xFF, (rgb >> 16) & 0xFF, a);
            */


            if (XPoints.Length != numVertices)
            {
                XPoints = new int[numVertices];
                YPoints = new int[numVertices];
            }

            var points = new Point[numVertices];

            int width = Image.Width;
            int height = Image.Height;
            for (int i = 0; i < numVertices; i++)
            {
                XPoints[i] = Discretize(Extend(vals[offset + i * 2 + 4]), width - 1);
                YPoints[i] = Discretize(Extend(vals[offset + i * 2 + 5]), height - 1);
                points[i] = new Point
                {
                    X = XPoints[i],
                    Y = YPoints[i],
                };
            }

            if (Graphics == null) Graphics = Graphics.FromImage(Image);
            Graphics.FillPolygon(new SolidBrush(color), points);
        }

        // This allows genes from 0...1 to go to -0.025 ... +1.025.
        // which in turn makes it easy for polygons to have points off-screen
        double Extend(double value)
        {
            return (value * 1.05) - 0.025;
        }

        // this is small enough to be inlined
        int Discretize(double value, int max)
        {
            // This weird bit of magic uniformly spreads doubles over the 0...max space properly
            int v = (int)(value * (max + 1));
            if (v > max) v = max;
            return v;
        }

        /** Erases the image. */
        public void Clear()
        {
            if (Graphics == null)
                Graphics = System.Drawing.Graphics.FromImage(Image);
            Graphics.Clear(Color.Black);
        }

        /** The maximum possible error between the two images.  Will be a value >= 0.
            By the way, the min error -- what you're shooting for -- is 0 */
        double MaxError()
        {
            // we disregard alpha
            // width x height pixels, each with 3 color channels (rgb), each with an error of up to 255
            return Math.Sqrt(Image.Width * Image.Height * (255.0 * 255.0) * 3);
        }

        /** Computes the sum squared error between the image and the original.  This is defined as the sum, for all pixels,
            and for all three colors in the pixel, of the squared difference between the images with regard
            to that color on that pixel.  Error goes from 0 to 1.*/
        public double Error()
        {
            int[] originalData = GetDataElements(Original);
            int[] imageData = GetDataElements(Image);


            int len = originalData.Length;
            double error = 0;
            for (int i = 0; i < len; i++) // go through every pixel (which is stored as an int)
            {
                int a = originalData[i];
                int b = imageData[i];

                // since it's *ARGB*, the alpha is in the high byte, we ignore that.
                int error1 = (a & 0xff) - (b & 0xff);
                int error2 = ((a >> 8) & 0xff) - ((b >> 8) & 0xff);
                int error3 = ((a >> 16) & 0xff) - ((b >> 16) & 0xff);

                // do sum squared of color errors
                error += error1 * error1 + error2 * error2 + error3 * error3;
            }
            return Math.Sqrt(error) / MaxError();
        }

        int[] GetDataElements(Image img)
        {
            using (var bm = new Bitmap(img))
            {
                var arr = new int[bm.Width * bm.Height];
                for (var x = 0; x < bm.Width; x++)
                {
                    for (var y = 0; y < bm.Height; y++)
                    {
                        arr[x * bm.Width + y] = bm.GetPixel(x, y).ToArgb();
                    }
                }
                return arr;
            }
        }

        ///** For debugging only.  */
        //static JFrame f = new JFrame();
        //static bool first = true;
        //static JLabel left = new JLabel();
        //static JLabel right = new JLabel();

        public void Display(String title)
        {
            //left.setIcon(new ImageIcon(copyImage(Original)));
            //right.setIcon(new ImageIcon(copyImage(Image)));
            //if (first)
            //{
            //    first = false;
            //    f.getContentPane().setLayout(new GridLayout(1, 2));
            //    f.getContentPane().add(left);
            //    f.getContentPane().add(right);
            //    f.pack();
            //    f.setVisible(true);
            //}
            //f.setTitle(title);
            //f.repaint();
        }


        /** Saves the image (not the original) out to a PNG file so you can compare. */
        public void Save(FileInfo file)
        {
            try
            {
                Image.Save(file.FullName, ImageFormat.Png);
            }
            catch (Exception e)
            {
            }
        }

        // for cloneable, 'cause (grrr) BufferedImage isn't cloneable for some reason
        public object Clone()
        {
            var p = new Picture
            {
                Original = (Image) Original.Clone(),
                Image = (Image) Image.Clone(),
                XPoints = new int[0],
                YPoints = new int[0]
            };
            return p;
        }

    }
}