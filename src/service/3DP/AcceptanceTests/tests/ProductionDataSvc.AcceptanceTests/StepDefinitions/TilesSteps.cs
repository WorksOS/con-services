using System;
using ProductionDataSvc.AcceptanceTests.Helpers;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("Tiles.feature")]
  public class TilesSteps : FeaturePostRequestBase<TileRequest, TileResult>
  {
    [Then(@"the Raw PNG Tiles response should match ""(.*)"" result from the repository")]
    public void ThenTheRawPNGTilesResponseShouldMatchResultFromTheRepository(string responseName)
    {
      var imageDifference = Convert.ToDouble(3) / 100;
      var expectedTileData = PostRequestHandler.ResponseRepo[responseName].TileData;
      var actualTileData = PostRequestHandler.ByteContent;
      var expFileName = "Expected_" + responseName + ".png";
      var actFileName = "Actual_" + responseName + ".png";

      var differencePercent = ImageUtils.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);

      Assert.True(Math.Abs(differencePercent) < imageDifference, "Actual Difference:" + differencePercent * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");
    }
  }
}
