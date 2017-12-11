using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "DxfTile")]
  public class DxfTileSteps
  {
    private string url;
    private string projectUid;
    private string fileType;
    private string queryParameters = string.Empty;
    private Getter<TileResult> tileRequester;


    [Given(@"the Dxf Tile service URI ""(.*)""")]
    public void GivenTheDxfTileServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a bbox ""(.*)"" and a width ""(.*)"" and a height ""(.*)""")]
    public void GivenABboxAndAWidthAndAHeight(string bbox, int width, int height)
    {
      queryParameters = string.Format("&BBOX={0}&WIDTH={1}&HEIGHT={2}",
        bbox, width, height);
    }

    [Given(@"a fileType ""(.*)""")]
    public void GivenAFileType(string fileType)
    {
      this.fileType = fileType;
    }

    [When(@"I request a Dxf Tile")]
    public void WhenIRequestADxfTile()
    {
      tileRequester = new Getter<TileResult>(MakeUrl());
      tileRequester.DoValidRequest();
    }

    [Then(@"the Dxf Tile result should be")]
    public void ThenTheDxfTileResultShouldBe(string multilineText)
    {
      TileResult expected = JsonConvert.DeserializeObject<TileResult>(multilineText);
      Assert.AreEqual(expected, tileRequester.CurrentResponse);
    }

    [Then(@"the Dxf Tile result image should be match within ""(.*)"" percent")]
    public void ThenTheDxfTileResultImageShouldBeMatchWithinPercent(string difference, string multilineText)
    {
      double imageDifference = 0;
      if (!string.IsNullOrEmpty(difference))
      {
        imageDifference = Convert.ToDouble(difference) / 100;
      }
      TileResult expected = JsonConvert.DeserializeObject<TileResult>(multilineText);
      var expectedTileData = expected.TileData;
      var actualTileData = tileRequester.CurrentResponse.TileData;
      var expFileName = "Expected_" + ScenarioContext.Current.ScenarioInfo.Title + ".jpg";
      var actFileName = "Actual_" + ScenarioContext.Current.ScenarioInfo.Title + ".jpg";
      var diff = Common.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);
      Console.WriteLine("Actual Difference % = " + diff * 100);
      Console.WriteLine("Actual filename = " + actFileName);
      Console.WriteLine(tileRequester.CurrentResponse);
      Assert.IsTrue(Math.Abs(diff) < imageDifference, "Actual Difference:" + diff * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");
    }

    [When(@"I request a Dxf Tile Expecting NoContent")]
    public void WhenIRequestADxfTileExpectingNoContent()
    {
      tileRequester = new Getter<TileResult>(MakeUrl());
      tileRequester.DoInvalidRequest(HttpStatusCode.NoContent);
    }

    [When(@"I request a Dxf Tile Expecting BadRequest")]
    public void WhenIRequestADxfTileExpectingBadRequest()
    {
      tileRequester = new Getter<TileResult>(MakeUrl());
      tileRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
    }


    [Then(@"I should get no response body")]
    public void ThenIShouldGetNoResponseBody()
    {
      Assert.IsNull(tileRequester.CurrentResponse);
    }

    [Then(@"I should get error code (.*) and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.AreEqual(errorCode, tileRequester.CurrentResponse.Code);
      Assert.AreEqual(message, tileRequester.CurrentResponse.Message);
    }



    private string MakeUrl()
    {
      var fullUrl = string.Format("{0}?projectUid={1}", url, projectUid);
      if (!string.IsNullOrEmpty(fileType))
      {
        fullUrl = string.Format("{0}&fileType={1}", fullUrl, fileType);
      }
      fullUrl += queryParameters;
      return fullUrl;
    }


  }
}
