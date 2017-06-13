using System;
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
      url = string.Format("{0}?projectUid={1}", url, projectUid);
      if (!string.IsNullOrEmpty(fileUid))
      {
        url = string.Format("{0}&fileUids={1}", url, fileUid);
      }
      url += queryParameters;
      tileRequester = new Getter<TileResult>(url);
      tileRequester.DoValidRequest();
    }

    [Then(@"the Dxf Tile result should be")]
    public void ThenTheDxfTileResultShouldBe(string multilineText)
    {
      TileResult expected = JsonConvert.DeserializeObject<TileResult>(multilineText);
      //Assert.IsTrue(expected.Code == tileRequester.CurrentResponse.Code && expected.Message == tileRequester.CurrentResponse.Message);
      Assert.AreEqual(expected, tileRequester.CurrentResponse);
    }
  }
}
