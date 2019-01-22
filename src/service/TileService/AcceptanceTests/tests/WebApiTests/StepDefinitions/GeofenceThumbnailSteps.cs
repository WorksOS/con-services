using System;
using FluentAssertions;
using System.Net;
using WebApiTests.Models;
using WebApiTests.Utilities;
using Xunit;
using Xunit.Gherkin.Quick;

namespace WebApiTests.StepDefinitions
{
  [FeatureFile("GeofenceThumbnail.feature")] //path relative to output folder
  public sealed class GeofenceThumbnailSteps : StepsBase
  {
    private string uri;
    private string responseRepositoryFileName;
    private string requestRepositoryFileName;
    private string operation;
    private Getter<byte[]> tileRequester;
    private Getter<MultipleThumbnailsResult> multiTileRequester;
    private Poster<RootObject, MultipleThumbnailsResult> geoJsonTileRequester;


    [Given(@"The geofence thumbnail URI is ""(.*)"" for operation ""(.*)""")]
    public void GivenTheGeofenceThumbnailURIIs(string uri, string operation)
    {
      this.uri = RestClient.TileServiceBaseUrl + uri;
      this.operation = operation;
    }

    [And(@"the expected response is in the ""(.*)"" respository")]
    public void GivenTheExpectedResponseIsInTheRespository(string fileName)
    {
      responseRepositoryFileName = fileName;
    }

    [And(@"the expected request is in the ""(.*)"" respository")]
    public void GivenTheExpectedRequestIsInTheRespository(string fileName)
    {
      requestRepositoryFileName = fileName;
    }

    [When(@"I request a Thumbnail for geofence UID ""(.*)""")]
    public void WhenIRequestAThumbnailForGeofenceUID(string geofenceUid)
    {
      uri += $"?geofenceUid={geofenceUid}";
      tileRequester = new Getter<byte[]>(uri, responseRepositoryFileName);
      switch (operation)
      {
        case "png":
          _ = tileRequester.SendRequest(uri, acceptHeader: MediaTypes.PNG);
          break;
        case "base64":
          tileRequester.DoValidRequest(HttpStatusCode.OK);
          break;
        default:
          Assert.True(false, TEST_FAIL_MESSAGE);
          break;
      }
    }

    [Then(@"The resulting thumbnail should match ""(.*)"" from the response repository within ""(.*)"" percent")]
    public void ThenTheResultingThumbnailShouldMatchFromTheResponseRepositoryWithinPercent(string responseName, string tolerance)
    {
      byte[] expectedResponse = tileRequester.ResponseRepo[responseName];

      switch (operation)
      {
        case "png":
          {
            CompareExpectedAndActualTiles(responseName, tolerance, expectedResponse, tileRequester.ByteContent);
            break;
          }
        case "base64":
          {
            CompareExpectedAndActualTiles(responseName, tolerance, expectedResponse, tileRequester.CurrentResponse);
            break;
          }
      }

    }

    [When(@"I request multiple Thumbnails")]
    public void WhenIRequestMultipleThumbnails()
    {
      multiTileRequester = new Getter<MultipleThumbnailsResult>(uri, responseRepositoryFileName);
      switch (operation)
      {
        case "multiple":
          multiTileRequester.SendRequest(uri, acceptHeader: MediaTypes.PNG);
          break;
        default:
          Assert.True(false, TEST_FAIL_MESSAGE);
          break;
      }
    }

    [Then(@"The result should match ""(.*)"" from the response repository")]
    public void ThenTheResultShouldMatchFromTheResponseRepository(string responseName)
    {
      switch (operation)
      {
        case "multiple":
          multiTileRequester.CurrentResponse.Should().Be(multiTileRequester.ResponseRepo[responseName]);
          break;
        case "geojson":
          geoJsonTileRequester.CurrentResponse.Should().Be(geoJsonTileRequester.ResponseRepo[responseName]);
          break;
        default:
          Assert.True(false, TEST_FAIL_MESSAGE);
          break;
      }
    }

    [When(@"I request a Thumbnail for ""(.*)"" from the request repository expecting ""(.*)""")]
    public void WhenIRequestAThumbnailForFromTheRequestRepositoryExpecting(string requestName, string httpStatusCode)
    {
      var statusCode = Enum.Parse<HttpStatusCode>(httpStatusCode);
      geoJsonTileRequester = new Poster<RootObject, MultipleThumbnailsResult>(uri, requestRepositoryFileName, responseRepositoryFileName);
      geoJsonTileRequester.DoRequest(requestName, statusCode);
    }

    [Then(@"I should get error code (.*) and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.Equal(errorCode, geoJsonTileRequester.CurrentResponse?.Code);
      Assert.Equal(message, geoJsonTileRequester.CurrentResponse?.Message);
    }
  }
}
