using System;
using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("Tiles.feature")]
  public class TilesSteps : FeaturePostRequestBase<JObject, TileResult>
  {
    [Then(@"the Raw PNG Tiles response should match ""(.*)"" result from the repository")]
    public void ThenTheRawPNGTilesResponseShouldMatchResultFromTheRepository(string responseName)
    {
      var allowedImageDifference = Convert.ToDouble(3) / 100;
      var expectedTileData = PostRequestHandler.ResponseRepo[responseName].TileData;
      var actualTileData = PostRequestHandler.ByteContent;
      var expFileName = string.Empty;
      var actFileName = string.Empty;

      var differencePercent = ImageUtils.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData);

      if (Math.Abs(differencePercent) >= allowedImageDifference)
      {
        Console.WriteLine($"Image difference in {responseName} data:");
        Console.WriteLine($"ACTUAL: {Convert.ToBase64String(actualTileData)}");
        Console.WriteLine($"EXPECTED: {Convert.ToBase64String(expectedTileData)}");

        expFileName = "Expected_" + responseName + ".png";
        actFileName = "Actual_" + responseName + ".png";

        ImageUtils.SaveImageFiles(expectedTileData, actualTileData, expFileName, actFileName);
      }

      Assert.True(Math.Abs(differencePercent) < allowedImageDifference, "Actual Difference:" + differencePercent * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");
    }
  }
}
