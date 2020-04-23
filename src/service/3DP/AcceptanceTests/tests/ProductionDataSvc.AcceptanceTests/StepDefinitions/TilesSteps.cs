using System;
using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using XnaFan.ImageComparison.Netcore.Common;
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

      CommonUtils.CompareImages(responseName, allowedImageDifference, expectedTileData, actualTileData, out var differencePercent);

      Assert.True(Math.Abs(differencePercent) < allowedImageDifference, "Actual Difference:" + differencePercent * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");
    }
  }
}
