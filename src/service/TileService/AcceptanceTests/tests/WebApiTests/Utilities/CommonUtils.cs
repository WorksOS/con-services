using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using XnaFan.ImageComparison.Netcore;
using Xunit;

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
        var diffImage = expectedImage.GetDifferenceImage(actualImage, false, true);
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

    public static bool TilesMatch(string resultName, string difference, byte[] expectedTileData, byte[] actualTileData)
    {
      double imageDifference = 0;
      if (!string.IsNullOrEmpty(difference))
      {
        imageDifference = Convert.ToDouble(difference) / 100;
      }
      //These 2 lines are for debugging so we can paste into an online image converter
      //var expectedTileDataString = JsonConvert.SerializeObject(expectedTileData);
      //var actualTileDataString = JsonConvert.SerializeObject(actualTileData);

      var expFileName = "Expected_" + /*ScenarioContext.Current.ScenarioInfo.Title +*/ resultName + ".png";
      var actFileName = "Actual_" + /*ScenarioContext.Current.ScenarioInfo.Title +*/ resultName + ".png";
      var diff = CommonUtils.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);
      Console.WriteLine("Actual Difference % = " + diff * 100);
      Console.WriteLine("Actual filename = " + actFileName);
      Console.WriteLine(actualTileData);
      return Math.Abs(diff) < imageDifference;
    }
  }
}
