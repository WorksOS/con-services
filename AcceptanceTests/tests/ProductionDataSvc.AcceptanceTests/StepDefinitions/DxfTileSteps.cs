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
    private string fileUid;
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

    [Given(@"a fileUid ""(.*)""")]
    public void GivenAFileUid(string fileUid)
    {
      this.fileUid = fileUid;
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
      //Assert.IsTrue(expected.Code == tileRequester.CurrentResponse.Code && expected.Message == tileRequester.CurrentResponse.Message);
      Assert.AreEqual(expected, tileRequester.CurrentResponse);
    }

    [When(@"I request a Dxf Tile Expecting NoContent")]
    public void WhenIRequestADxfTileExpectingNoContent()
    {
      tileRequester = new Getter<TileResult>(MakeUrl());
      tileRequester.DoInvalidRequest(HttpStatusCode.NoContent);
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
      if (!string.IsNullOrEmpty(fileUid))
      {
        fullUrl = string.Format("{0}&fileUids={1}", fullUrl, fileUid);
      }
      fullUrl += queryParameters;
      return fullUrl;
    }


  }
}
