using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using XnaFan.ImageComparison.Netcore;

namespace ProductionDataSvc.AcceptanceTests.Helpers
{
  public class ImageUtils
  {
    /// <summary>
    /// Compare the images and return the difference as a percentage.
    /// </summary>
    public static float CompareImagesAndGetDifferencePercent(byte[] expectedTileData, byte[] actualTileData, string expFileName, string actFileName)
    {
      var expectedImage = ConvertToImage(expectedTileData);
      var actualImage = ConvertToImage(actualTileData);
      SaveImageFile(expFileName, expectedImage);
      SaveImageFile(actFileName, actualImage);

      var len = actFileName.Length - 4;
      var diff = expectedImage.PercentageDifference(actualImage, 0);

      if (diff > 0.0)
      {
        var diffImage = expectedImage.GetDifferenceImage(actualImage);
        var diffFileName = actFileName.Substring(0, len) + "Differences.png";
        SaveImageFile(diffFileName, diffImage);        
      }

      return diff;
    }

    /// <summary>
    /// Convert a byte stream to image.
    /// </summary>
    private static Image<Rgba32> ConvertToImage(byte[] imageStream)
    {
      using (var ms = new MemoryStream(imageStream))
      {
        return Image.Load<Rgba32>(ms);
      }
    }

    private static void SaveImageFile(string fileName, Image<Rgba32> image)
    {
      try
      {
        image.Save(fileName);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }
  }
}
