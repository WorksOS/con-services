using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VSS.Productivity3D.Models.Extensions
{
  public static class BitmapExtensions
  {
    private static object lockObj = new object();

    /// <summary>
    /// Converts a bitmap to an array of bytes representing the image
    /// </summary>
    /// <param name="bitmap">The bitmap to convert</param>
    /// <returns>An array of bytes</returns>
    public static byte[] BitmapToByteArray(this Bitmap bitmap)
    {
      using var bitmapStream = new MemoryStream();

      lock (lockObj) {
        bitmap.Save(bitmapStream, ImageFormat.Png);
      }

      return bitmapStream.ToArray();
    }
  }
}
