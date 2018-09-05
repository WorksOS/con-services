using System.IO;
using Draw = System.Drawing;
using System.Drawing.Imaging;

namespace VSS.TRex.Common
{
  public static class BitmapExtensions
  {
    /// <summary>
    /// Converts a bitmap to an array of bytes representing the image
    /// </summary>
    /// <param name="bitmap">The bitmap to convert</param>
    /// <returns>An array of bytes</returns>
    public static byte[] BitmapToByteArray(this Draw.Bitmap bitmap)
    {
      byte[] data;
      using (var bitmapStream = new MemoryStream())
      {
        bitmap.Save(bitmapStream, ImageFormat.Png);
        data = bitmapStream.ToArray();
        bitmapStream.Close();
      }

      return data;
    }
  }

}
