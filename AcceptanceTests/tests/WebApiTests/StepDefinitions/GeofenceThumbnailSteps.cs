using System.Net;
using WebApiTests.Utilities;
using Xunit.Gherkin.Quick;

namespace WebApiTests.StepDefinitions
{
  [FeatureFile("GeofenceThumbnail.feature")]//path relative to output folder
  public sealed class GeofenceThumbnailSteps : StepsBase
  {
    private string uri;
    private string responseRepositoryFileName;
    private Getter<byte[]> tileRequester;
    private byte[] currentResponse;

    [Given(@"The geofence thumbnail URI is ""(.*)""")]
    public void GivenTheGeofenceThumbnailURIIs(string uri)
    {
      this.uri = TileClientConfig.TileSvcBaseUri + uri;
    }

    [And(@"the expected response is in the ""(.*)"" respository")]
    public void GivenTheExpectedResponseIsInTheRespository(string fileName)
    {
      responseRepositoryFileName = fileName;
    }


    [When(@"I request a Report Tile for geofence UID ""(.*)""")]
    public void WhenIRequestAReportTileForGeofenceUID(string geofenceUid)
    {
      uri += $"?geofenceUid={geofenceUid}";
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
