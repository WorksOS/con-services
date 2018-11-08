using FluentAssertions;
using System.Net;
using WebApiTests.Models;
using WebApiTests.Utilities;
using Xunit;
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
    private Getter<MultipleThumbnailsResult> multiTileRequester;

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
        default: Assert.True(false, TEST_FAIL_MESSAGE); break;
      }
    }

    [Then(@"The resulting thumbnail should match ""(.*)"" from the response repository within ""(.*)"" percent")]
    public void ThenTheResultingThumbnailShouldMatchFromTheResponseRepositoryWithinPercent(string responseName, string tolerance)
    {
      byte[] expectedResponse = tileRequester.ResponseRepo[responseName];
      CompareExpectedAndActualTiles(responseName, tolerance, expectedResponse, currentResponse);
    }

    [When(@"I request multiple Report Tiles")]
    public void WhenIRequestMultipleReportTiles()
    {
      multiTileRequester = new Getter<MultipleThumbnailsResult>(uri, responseRepositoryFileName);
      switch (operation)
      {
        case "multiple":
          multiTileRequester.DoRequest(uri, HttpStatusCode.OK);
          break;
        default: Assert.True(false, TEST_FAIL_MESSAGE); break;
      }
    }

    [Then(@"The result should match ""(.*)"" from the response repository")]
    public void ThenTheResultShouldMatchFromTheResponseRepository(string responseName)
    {
      multiTileRequester.CurrentResponse.Should().Be(multiTileRequester.ResponseRepo[responseName]);
    }
  }
}
