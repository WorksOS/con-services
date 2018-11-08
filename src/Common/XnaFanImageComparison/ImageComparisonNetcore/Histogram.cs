using System;
using System.Linq;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

// Created in 2012 by Jakob Krarup (www.xnafan.net).
// Use, alter and redistribute this code freely,
// but please leave this comment :)

namespace XnaFan.ImageComparison.Netcore
{
    /// <summary>
    /// A class which facilitates working with RGB histograms
    /// It encapsulates a Bitmap and lets you get information about the Bitmap
    /// </summary>
    public class Histogram
    {
        /// <summary>
        /// The red values in an image
        /// </summary>
        public byte[] Red { get; private set; }

        /// <summary>
        /// The green values in an image
        /// </summary>
        public byte[] Green { get; private set; }

        /// <summary>
        /// The blue values in an image
        /// </summary>
        public byte[] Blue { get; private set; }

        /// <summary>
        /// The bitmap to get histogram info for
        /// </summary>
        public Image<Rgba32> Bitmap { get; private set; }

        public Histogram(Image<Rgba32> bitmap)
        {
            Bitmap = bitmap;
            Red = new byte[256];
            Green = new byte[256];
            Blue = new byte[256];
            CalculateHistogram();
        }

        /// <summary>
        /// Constructs a new Histogram from a file, given its path
        /// </summary>
        /// <param name="filePath">The path to the image to work with</param>
        public Histogram(string filePath) : this(Image.Load<Rgba32>(filePath)) { }


        /// <summary>
        /// Calculates the values in the histogram
        /// </summary>
        private void CalculateHistogram()
        {
            Image<Rgba32> newBmp = (Image<Rgba32>)this.Bitmap.Resize(16, 16);
            Rgba32 c;
            for (int x = 0; x < newBmp.Width; x++)
            {
                for (int y = 0; y < newBmp.Height; y++)
                {
                    c = newBmp[x,y];
                    Red[c.R]++;
                    Green[c.G]++;
                    Blue[c.B]++;
                }
            }
            newBmp.Dispose();
        }

      static readonly Pen<Rgba32>[] p = { Graphics.RedPen, Graphics.GreenPen, Graphics.BluePen };
      static readonly string[] penNames = { "Red", "Green", "Blue" };

      private static readonly Font smallCaptionFont =
        SystemFonts.CreateFont("Microsoft Sans Serif", 11f, FontStyle.Regular);


    /// <summary>
    /// Gets a bitmap with the RGB histograms
    /// </summary>
    /// <returns>Three histograms for R, G and B values in the Histogram</returns>
    public Image<Rgba32> Visualize()
        {
            int oneColorHeight = 100;
            int margin = 10;

            float[] maxValues = new float[] { Red.Max(), Green.Max(), Blue.Max() };
            byte[][] values = new byte[][] { Red, Green, Blue };


            Image<Rgba32> histogramBitmap = new Image<Rgba32>(276, oneColorHeight * 3 + margin * 4);
            ///Graphics g = Graphics.FromImage(histogramBitmap);
            var rect = new RectangleF(0, 0, histogramBitmap.Width, histogramBitmap.Height);
            histogramBitmap.Mutate(ctx => ctx.Fill(Rgba32.White, rect));
            int yOffset = margin + oneColorHeight;

            for (int i = 0; i < 256; i++)
            {
                for (int color = 0; color < 3; color++)
                {
                  PointF[] points =
                  {
                    new PointF(margin + i, yOffset * (color + 1)),
                    new PointF(margin + i, yOffset * (color + 1) - (values[color][i] / maxValues[color]) * oneColorHeight)
                  };
                  histogramBitmap.Mutate(ctx => ctx.DrawLines(p[color], points));
                }
            }

            for (int i = 0; i < 3; i++)
            {
              var point = new PointF(margin + 11, yOffset * i + margin + margin + 1);
              histogramBitmap.Mutate(ctx => ctx.DrawText(penNames[i] + ", max value: " + maxValues[i], smallCaptionFont, Rgba32.Silver, point));
              point = new PointF(margin + 10, yOffset * i + margin + margin);
              histogramBitmap.Mutate(ctx => ctx.DrawText(penNames[i] + ", max value: " + maxValues[i], smallCaptionFont, Rgba32.Black, point));
              rect = new RectangleF(margin, yOffset * i + margin, 256, oneColorHeight);
              histogramBitmap.Mutate(ctx => ctx.Draw(p[i], rect));
            }

            return histogramBitmap;
        }


        /// <summary>
        /// Gives a human-readable representation of the RGB values in the histogram
        /// </summary>
        /// <returns>a human-readable representation of the RGB values in the histogram</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 256; i++)
            {
                sb.Append(string.Format("RGB {0,3} : ", i) + string.Format("({0,3},{1,3},{2,3})", Red[i], Green[i], Blue[i]));
                sb.AppendLine();
            }

            return sb.ToString();
        }


        /// <summary>
        /// Gets the variance between two histograms (http://en.wikipedia.org/wiki/Variance) as a percentage of the maximum possible variance: 256 (for a white image compared to a black image)
        /// </summary>
        /// <param name="histogram">the histogram to compare this one to</param>
        /// <returns>A percentage which tells how different the two histograms are</returns>
        public float GetVariance(Histogram histogram)
        {
            //
            double diffRed = 0, diffGreen = 0, diffBlue = 0;
            for (int i = 0; i < 256; i++)
            {
                diffRed += Math.Pow(Red[i] - histogram.Red[i], 2);
                diffGreen += Math.Pow(Green[i] - histogram.Green[i], 2);
                diffBlue += Math.Pow(Blue[i] - histogram.Blue[i], 2);
            }

            diffRed /= 256;
            diffGreen /= 256;
            diffBlue /= 256;
            const double maxDiff = 512;
            return (float)(diffRed / maxDiff + diffGreen / maxDiff + diffBlue / maxDiff) / 3;
        }
    }
}