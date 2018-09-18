using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using XnaFan.ImageComparison.Netcore;

namespace WebApiTests.Utilities
{
  public class CommonUtils
  {
    /// <summary>
    /// Compare the images and return the differnce
    /// </summary>
    /// <param name="expectedTileData">byte array of expected tile data</param>
    /// <param name="actualTileData">byte array of actual tile data</param>
    /// <param name="expFileName">expected file name that is saved</param>
    /// <param name="actFileName">actual file name that is saved</param>
    /// <param name="threshold">threshold for comparison. Used to ignore minor differences</param>
    /// <returns>the differnce of the images as a percentage</returns>
    public static float CompareImagesAndGetDifferencePercent(byte[] expectedTileData, byte[] actualTileData,
      string expFileName, string actFileName, byte threshold = 0)
    {
      var expectedImage = ConvertToImage(expectedTileData);
      var actualImage = ConvertToImage(actualTileData);
      SaveImageFile(expFileName, expectedImage);
      SaveImageFile(actFileName, actualImage);

      var len = actFileName.Length - 4;
      var diff = expectedImage.PercentageDifference(actualImage, threshold);
      if (diff > 0.0)
      {
        var diffImage = expectedImage.GetDifferenceImage(actualImage);
        var diffFileName = actFileName.Substring(0, len) + "Differences.png";
        SaveImageFile(diffFileName, diffImage);        
      }
      return diff;
    }

    /// <summary>
    /// Convert an byte stream to image
    /// </summary>
    public static Image<Rgba32> ConvertToImage(byte[] imageStream)
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
