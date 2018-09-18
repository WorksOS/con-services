using System;
using Newtonsoft.Json;
using WebApiTests.Utilities;
using Xunit;
using Xunit.Gherkin.Quick;

namespace WebApiTests.StepDefinitions
{
  public class StepsBase : Feature
  {
    public void CompareExpectedAndActualTiles(string resultName, string difference, byte[] expectedTileData, byte[] actualTileData)
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
      Assert.True(Math.Abs(diff) < imageDifference, "Actual Difference:" + diff * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");

    }
  }
}
