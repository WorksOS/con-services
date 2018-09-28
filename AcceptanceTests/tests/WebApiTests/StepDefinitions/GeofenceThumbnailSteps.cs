using System.Net;
using WebApiTests.Models;
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
    private string operation;

    [Given(@"The geofence thumbnail URI is ""(.*)"" for operation ""(.*)""")]
    public void GivenTheGeofenceThumbnailURIIs(string uri, string operation)
    {
      this.uri = TileClientConfig.TileSvcBaseUri + uri;
      this.operation = operation;
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
      switch (operation)
      {
        case "png":
          currentResponse = tileRequester.DoRequestWithStreamResponse(uri);
          break;
        case "base64":
          tileRequester.DoValidRequest(HttpStatusCode.OK);
          currentResponse = tileRequester.CurrentResponse;
          break;
      }
    }

    [Then(@"The resulting thumbnail should match ""(.*)"" from the response repository within ""(.*)"" percent")]
    public void ThenTheResultingThumbnailShouldMatchFromTheResponseRepositoryWithinPercent(string responseName, string tolerance)
    {
      byte[] expectedResponse = tileRequester.ResponseRepo[responseName];
      CompareExpectedAndActualTiles(responseName, tolerance, expectedResponse, currentResponse);
    }
  }
}
