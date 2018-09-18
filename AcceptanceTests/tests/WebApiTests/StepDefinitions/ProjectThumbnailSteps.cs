using System.Net;
using WebApiTests.Utilities;
using Xunit.Gherkin.Quick;

namespace WebApiTests.StepDefinitions
{
  [FeatureFile("ProjectThumbnails.feature")]//path relative to output folder
  public sealed class ProjectThumbnailSteps : StepsBase
  {
    private string uri;
    private string responseRepositoryFileName;
    private Getter<byte[]> tileRequester;
    private byte[] currentResponse;

    [Given(@"The project thumbnail URI is ""(.*)""")]
    public void GivenTheProjectThumbnailURIIs(string uri)
    {
      this.uri = TileClientConfig.TileSvcBaseUri + uri;
    }

    [And(@"the expected response is in the ""(.*)"" respository")]
    public void GivenTheExpectedResponseIsInTheRespository(string fileName)
    {
      responseRepositoryFileName = fileName;
    }

    
    [When(@"I request a Report Tile for project UID ""(.*)""")]
    public void WhenIRequestAReportTileForProjectUID(string projectUid)
    {
      uri += $"?projectUid={projectUid}";
      tileRequester = new Getter<byte[]>(uri, responseRepositoryFileName);
      currentResponse = tileRequester.DoRequestWithStreamResponse(uri);
    }

    [Then(@"The resulting thumbnail should match ""(.*)"" from the response repository within ""(.*)"" percent")]
    public void ThenTheResultingThumbnailShouldMatchFromTheResponseRepositoryWithinPercent(string responseName, string tolerance)
    {
      CompareExpectedAndActualTiles(responseName, tolerance, tileRequester.ResponseRepo[responseName], currentResponse);
    }
  }
}
