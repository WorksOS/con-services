using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VSS.Productivity3D.Models.Extensions
{
  public static class BitmapExtensions
  {
    /// <summary>
    /// All public clients of these methods should perform these operations under this global locl=k
    /// SkiaSharp could be an alternative here
    /// </summary>
    public static object LockObj = new object();

    /// <summary>
    /// All public clients of these methods should perform these operations under this global locl=k
    /// SkiaSharp could be an alternative here
    /// </summary>
    public static object LockObj = new object();

    /// <summary>
    /// Converts a bitmap to an array of bytes representing the image
    /// </summary>
    /// <param name="bitmap">The bitmap to convert</param>
    /// <returns>An array of bytes</returns>
    public static byte[] BitmapToByteArray(this Bitmap bitmap)
    {
      using var bitmapStream = new MemoryStream();
      bitmap.Save(bitmapStream, ImageFormat.Png);
      return bitmapStream.ToArray();
    }
  }
}
