using System.Net;
using System.Text;
using WebApiTests.Models;
using WebApiTests.Utilities;
using Xunit;
using Xunit.Gherkin.Quick;

namespace WebApiTests.StepDefinitions
{
  [FeatureFile("LineworkTile.feature")]//path relative to output folder
  public sealed class LineworkTileSteps : StepsBase
  {
    private string url;
    private string projectUid;
    private string fileType;
    private int zoomLevel;
    private int yTile;
    private int xTile;
    private int width = 256;
    private int height = 256;
    private Getter<byte[]> tileRequesterGood;
    private Getter<RequestResult> tileRequesterBad;

    [Given(@"the Linework Tile 3D service URI ""(.*)""")]
    public void GivenTheLineworkTile3DServiceURI(string url)
    {
      this.url = RestClient.TileServiceBaseUrl + url;
      this.url = "http://localhost:5000" + url;
    }

    [And(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [And(@"a zoomLevel ""(.*)""")]
    public void GivenAZoomLevel(int zoomLevel)
    {
      this.zoomLevel = zoomLevel;
    }

    [And(@"a yTile ""(.*)""")]
    public void GivenAYTile(int yTile)
    {
      this.yTile = yTile;
    }

    [And(@"a xTile ""(.*)""")]
    public void GivenAXTile(int xTile)
    {
      this.xTile = xTile;
    }

    [And(@"a fileType ""(.*)""")]
    public void GivenAFileType(string fileType)
    {
      this.fileType = fileType;
    }

    [When(@"I request a Linework Tile and the result file ""(.*)""")]
    public void WhenIRequestALineworkTileAndTheResultFile(string resultFile)
    {
      var uri = MakeUrl();
      tileRequesterGood = new Getter<byte[]>(uri, resultFile);
      _ = tileRequesterGood.SendRequest(uri, acceptHeader: MediaTypes.PNG);
    }

    [Then(@"the result tile should match the ""(.*)"" from the repository within ""(.*)"" percent")]
    public void ThenTheResultTileShouldMatchTheFromTheRepositoryWithinPercent(string resultName, string difference)
    {
      CompareExpectedAndActualTiles(resultName, difference, tileRequesterGood.ResponseRepo[resultName], tileRequesterGood.ByteContent);
    }

    [When(@"I request a Linework Tile Expecting BadRequest")]
    public void WhenIRequestALineworkTileExpectingBadRequest()
    {
      tileRequesterBad = new Getter<RequestResult>(MakeUrl());
      tileRequesterBad.DoInvalidRequest(HttpStatusCode.BadRequest);
    }

    [Then(@"I should get error code (.*) and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.Equal(errorCode, tileRequesterBad.CurrentResponse?.Code);
      Assert.Equal(message, tileRequesterBad.CurrentResponse?.Message);
    }

    private string MakeUrl()
    {
      var sb = new StringBuilder();
      sb.Append($"{url}/{zoomLevel}/{yTile}/{xTile}.png?projectUid={projectUid}&width={width}&height={height}");
    
      if (!string.IsNullOrEmpty(fileType))
      {
        sb.Append($"&fileType={fileType}");
      }
      return sb.ToString();
    }
  }
}
