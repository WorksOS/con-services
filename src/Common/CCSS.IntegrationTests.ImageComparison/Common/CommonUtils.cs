using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using XnaFan.ImageComparison.Netcore.XnaFan.ImageComparison;

namespace XnaFan.ImageComparison.Netcore.Common
{
  public static class CommonUtils
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
        var diffImage = expectedImage.GetDifferenceImage(actualImage, false, true);
        var diffFileName = actFileName.Substring(0, len) + "Differences.png";
        SaveImageFile(diffFileName, diffImage);
      }

      return diff;
    }

    public static Image<Rgba32> ConvertToImage(byte[] imageStream)
    {
      using var ms = new MemoryStream(imageStream);
      return Image.Load<Rgba32>(ms);
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

    /// <summary>
    /// Test whether two lists are equivalent.
    /// </summary>
    public static bool ListsAreEqual<T>(List<T> listA, List<T> listB)
    {
      if (listA == null && listB == null)
        return true;
      if (listA == null || listB == null)
        return false;
      if (listA.Count != listB.Count)
        return false;

      for (int i = 0; i < listA.Count; ++i)
      {
        if (!listB.Exists(item => item.Equals(listA[i])))
          return false;
      }

      return true;
    }

    public static bool CompareImages(string resultName, double difference, byte[] expectedTileData, byte[] actualTileData, out int actualDiff)
    {
      var expFileName = "Expected_" + resultName + ".png";
      var actFileName = "Actual_" + resultName + ".png";
      var diff = CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);

      Console.WriteLine("Actual Difference % = " + diff * 100);
      Console.WriteLine("Actual filename = " + actFileName);

      actualDiff = (int)Math.Round(diff * 100);
      return Math.Abs(diff) < difference;
    }
  }
}
