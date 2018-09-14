using System;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace XnaFan.ImageComparison.Netcore
{
    public static class ExtensionMethods
    {
        //the font to use for the DifferenceImages
        private static readonly Font DefaultFont = SystemFonts.CreateFont(SystemFonts.Collection.Families.First().Name, 6);

        //the brushes to use for the DifferenceImages
        private static Rgba32[] brushes = new Rgba32[256];

        //Create the brushes in varying intensities
        static ExtensionMethods()
        {
          for (int i = 0; i < 256; i++)
          {
            brushes[i] = new Rgba32(i, i / 3, i / 2, 255);
          }
        }

        /// <summary>
        /// Gets the difference between two images as a percentage
        /// </summary>
        /// <param name="img1">The first image</param>
        /// <param name="img2">The image to compare to</param>
        /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
        /// <returns>The difference between the two images as a percentage</returns>
        public static float PercentageDifference(this Image<Rgba32> img1, Image<Rgba32> img2, byte threshold = 3)
        {
            byte[,] differences = img1.GetDifferences(img2);

            int diffPixels = 0;

            foreach (byte b in differences)
            {
                if (b > threshold) { diffPixels++; }
            }

            return diffPixels / 256f;
        }

        /// <summary>
        /// The Bhattacharyya difference (the difference between normalized versions of the histograms of both images)
        /// This tells something about the differences in the brightness of the images as a whole, not so much about where they differ.
        /// </summary>
        /// <param name="img1">The first image to compare</param>
        /// <param name="img2">The second image to compare</param>
        /// <returns>The difference between the images' normalized histograms</returns>
        public static float BhattacharyyaDifference(this Image<Rgba32> img1, Image<Rgba32> img2)
        {
            byte[,] img1GrayscaleValues = img1.GetGrayScaleValues();
            byte[,] img2GrayscaleValues = img2.GetGrayScaleValues();

            var normalizedHistogram1 = new double[16, 16];
            var normalizedHistogram2 = new double[16, 16];

            double histSum1 = 0.0;
            double histSum2 = 0.0;

            foreach (var value in img1GrayscaleValues) { histSum1 += value; }
            foreach (var value in img2GrayscaleValues) { histSum2 += value; }


            for (int x = 0; x < img1GrayscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < img1GrayscaleValues.GetLength(1); y++)
                {
                    normalizedHistogram1[x, y] = (double)img1GrayscaleValues[x, y] / histSum1;
                }
            }
            for (int x = 0; x < img2GrayscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < img2GrayscaleValues.GetLength(1); y++)
                {
                    normalizedHistogram2[x, y] = (double)img2GrayscaleValues[x, y] / histSum2;
                }
            }

            double bCoefficient = 0.0;
            for (int x = 0; x < img2GrayscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < img2GrayscaleValues.GetLength(1); y++)
                {
                    double histSquared = normalizedHistogram1[x, y] * normalizedHistogram2[x, y];
                    bCoefficient += Math.Sqrt(histSquared);
                }
            }

            double dist1 = 1.0 - bCoefficient;
            dist1 = Math.Round(dist1, 8);
            double distance = Math.Sqrt(dist1);
            distance = Math.Round(distance, 8);
            return (float)distance;

        }


        /// <summary>
        /// Gets an image which displays the differences between two images
        /// </summary>
        /// <param name="img1">The first image</param>
        /// <param name="img2">The image to compare with</param>
        /// <param name="adjustColorSchemeToMaxDifferenceFound">Whether to adjust the color indicating maximum difference (usually 255) to the maximum difference found in this case.
        /// E.g. if the maximum difference found is 12, then a true value in adjustColorSchemeToMaxDifferenceFound would result in 0 being black, 6 being dark pink, and 12 being bright pink.
        /// A false value would still have differences of 255 as bright pink resulting in the 12 difference still being very dark.</param>
        /// <param name="percentages">Whether to write percentages in each of the 255 squares (true) or the absolute value (false)</param>
        /// <returns>an image which displays the differences between two images</returns>
        public static Image<Rgba32> GetDifferenceImage(this Image<Rgba32> img1, Image<Rgba32> img2, bool adjustColorSchemeToMaxDifferenceFound = false, bool absoluteText = false)
        {
            //create a 16x16 tiles image with information about how much the two images differ
            int cellsize = 16;  //each tile is 16 pixels wide and high
            Image<Rgba32> bmp = new Image<Rgba32>(16 * cellsize + 1, 16 * cellsize + 1); //16 blocks * 16 pixels + a borderpixel at left/bottom
 
            var rect = new RectangleF(0, 0, bmp.Width, bmp.Height);
            bmp.Mutate(ctx => ctx.Fill(Rgba32.Black, rect));
            byte[,] differences = img1.GetDifferences(img2);
            byte maxDifference = 255;

            //if wanted - adjust the color scheme, by finding the new maximum difference
            if (adjustColorSchemeToMaxDifferenceFound)
            {
                maxDifference = 0;
                foreach (byte b in differences)
                {
                    if (b > maxDifference)
                    {
                        maxDifference = b;
                    }
                }

                if (maxDifference == 0)
                {
                    maxDifference = 1;
                }
            }

            DrawDifferencesToBitmap(absoluteText, cellsize, bmp, differences, maxDifference);
            
            return bmp;
        }

        private static void DrawDifferencesToBitmap(bool absoluteText, int cellsize, Image<Rgba32> img, byte[,] differences, byte maxDifference)
        {
            for (int y = 0; y < differences.GetLength(1); y++)
            {
              for (int x = 0; x < differences.GetLength(0); x++)
              {
                byte cellValue = differences[x, y];
                string cellText = null;

                if (absoluteText)
                {
                  cellText = cellValue.ToString();
                }
                else
                {
                  cellText = string.Format("{0}%", (int) cellValue);
                }

                float percentageDifference = (float) differences[x, y] / maxDifference;
                int colorIndex = (int) (255 * percentageDifference);

                var rect = new RectangleF(x * cellsize, y * cellsize, cellsize, cellsize);
                img.Mutate(ctx => ctx.Fill(brushes[colorIndex], rect).Draw(Graphics.BluePen, rect));
                SizeF size = TextMeasurer.Measure(cellText, new RendererOptions(DefaultFont));
                PointF point = new PointF((int) (x * cellsize + cellsize / 2 - size.Width / 2 + 1),
                  (int) (y * cellsize + cellsize / 2 - size.Height / 2 + 1));
                //Sometimes the string is too long to fit and gives an exception. 
                //Font reduced from 8 to 6 to lessen chance of it happening but for safety we'll ignore.
                try
                {
                  img.Mutate(ctx => ctx.DrawText(cellText, DefaultFont, Rgba32.Black, point));
                }
                catch (ArgumentOutOfRangeException e)
                {
                }
                point = new PointF((int) (x * cellsize + cellsize / 2 - size.Width / 2),
                  (int) (y * cellsize + cellsize / 2 - size.Height / 2));
                try
                {
                  img.Mutate(ctx => ctx.DrawText(cellText, DefaultFont, Rgba32.White, point));
                }
                catch (ArgumentOutOfRangeException e)
                {
                }
              }
            }
        }


        /// <summary>
        /// Finds the differences between two images and returns them in a doublearray
        /// </summary>
        /// <param name="img1">The first image</param>
        /// <param name="img2">The image to compare with</param>
        /// <returns>the differences between the two images as a doublearray</returns>
        public static byte[,] GetDifferences(this Image<Rgba32> img1, Image<Rgba32> img2)
        {
            Image<Rgba32> thisOne = (Image<Rgba32>)img1.Resize(16, 16).GetGrayScaleVersion();
            Image<Rgba32> theOtherOne = (Image<Rgba32>)img2.Resize(16, 16).GetGrayScaleVersion();
            byte[,] differences = new byte[16, 16];
            byte[,] firstGray = thisOne.GetGrayScaleValues();
            byte[,] secondGray = theOtherOne.GetGrayScaleValues();

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    differences[x, y] = (byte)Math.Abs(firstGray[x, y] - secondGray[x, y]);
                }
            }
            thisOne.Dispose();
            theOtherOne.Dispose();
            return differences;
        }

        /// <summary>
        /// Gets the lightness of the image in 256 sections (16x16)
        /// </summary>
        /// <param name="img">The image to get the lightness for</param>
        /// <returns>A doublearray (16x16) containing the lightness of the 256 sections</returns>
        public static byte[,] GetGrayScaleValues(this Image<Rgba32> img)
        {
            using (Image<Rgba32> thisOne = (Image<Rgba32>)img.Resize(16, 16).GetGrayScaleVersion())
            {
                byte[,] grayScale = new byte[16, 16];

                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        grayScale[x, y] = (byte)Math.Abs(thisOne[x,y].R);
                    }
                }
                return grayScale;
            }
        }


        /// <summary>
        /// Converts an image to grayscale
        /// </summary>
        /// <param name="original">The image to grayscale</param>
        /// <returns>A grayscale version of the image</returns>
        public static Image<Rgba32> GetGrayScaleVersion(this Image<Rgba32> original)
        {
            Image<Rgba32> newBitmap = new Image<Rgba32>(original.Width, original.Height);
            newBitmap.Mutate(ctx => ctx.DrawImage(original, 1f));
            newBitmap.Mutate(ctx => ctx.Grayscale());
            return newBitmap;
        }

        /// <summary>
        /// Resizes an image
        /// </summary>
        /// <param name="originalImage">The image to resize</param>
        /// <param name="newWidth">The new width in pixels</param>
        /// <param name="newHeight">The new height in pixels</param>
        /// <returns>A resized version of the original image</returns>
        public static Image<Rgba32> Resize(this Image<Rgba32> originalImage, int newWidth, int newHeight)
        {
            Image<Rgba32> smallVersion = originalImage.Clone();
            smallVersion.Mutate(ctx => ctx.Resize(newWidth, newHeight));           
            return smallVersion;
        }

        /// <summary>
        /// Helpermethod to print a doublearray of 
        /// </summary>
        /// <typeparam name="T">The type of doublearray</typeparam>
        /// <param name="doubleArray">The doublearray to print</param>
        public static void ToConsole<T>(this T[,] doubleArray)
        {
            for (int y = 0; y < doubleArray.GetLength(0); y++)
            {
                Console.Write("[");
                for (int x = 0; x < doubleArray.GetLength(1); x++)
                {
                    Console.Write(string.Format("{0,3},", doubleArray[x, y]));
                }
                Console.WriteLine("]");
            }
        }

        /// <summary>
        /// Gets a bitmap with the RGB histograms of a bitmap
        /// </summary>
        /// <param name="bmp">The bitmap to get the histogram for</param>
        /// <returns>A bitmap with the histogram for R, G and B values</returns>
        public static Image<Rgba32> GetRgbHistogramBitmap(this Image<Rgba32> bmp)
        {
            return new Histogram(bmp).Visualize();
        }

        /// <summary>
        /// Get a histogram for a bitmap
        /// </summary>
        /// <param name="bmp">The bitmap to get the histogram for</param>
        /// <returns>A histogram for the bitmap</returns>
        public static Histogram GetRgbHistogram(this Image<Rgba32> bmp)
        {
            return new Histogram(bmp);
        }

    }
}
