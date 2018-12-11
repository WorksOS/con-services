using WebApiTests.Utilities;
using Xunit;
using Xunit.Gherkin.Quick;

namespace WebApiTests.StepDefinitions
{
  public class StepsBase : Feature
  {
    protected string TEST_FAIL_MESSAGE = "Unsupported test operation";

    public void CompareExpectedAndActualTiles(string resultName, string difference, byte[] expectedTileData, byte[] actualTileData)
    {
      int actualDiff;
      Assert.True(CommonUtils.TilesMatch(resultName, difference, expectedTileData, actualTileData, out actualDiff), 
        $"Actual Difference:{actualDiff}% Expected tile doesn't match actual tile for {resultName}");
    }
  }
}
