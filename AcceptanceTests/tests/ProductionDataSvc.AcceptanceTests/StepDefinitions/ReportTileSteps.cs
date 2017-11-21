using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ReportTile")]
  public class ReportTileSteps
  {
    private string url;
    private string projectUid;
    private string filterUid;
    //private string queryParameters = string.Empty;
    private Getter<TileResult> tileRequester;

    [Given(@"the Report Tile service URI ""(.*)""")]
    public void GivenTheReportTileServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a filterUid ""(.*)""")]
    public void GivenAFilterUid(string filterUid)
    {
      this.filterUid = filterUid;
    }

    [When(@"I request a Report Tile")]
    public void WhenIRequestAReportTile()
    {
      tileRequester = new Getter<TileResult>(MakeUrl());
      tileRequester.DoValidRequest();
    }

    [Then(@"the Report Tile result should be")]
    public void ThenTheReportTileResultShouldBe(string multilineText)
    {
      TileResult expected = JsonConvert.DeserializeObject<TileResult>(multilineText);
      Assert.AreEqual(expected, tileRequester.CurrentResponse);
    }

    private string MakeUrl()
    {
      var fullUrl = string.Format("{0}?projectUid={1}&width=256&height=256&overlays=DxfLinework", url, projectUid);
      if (!string.IsNullOrEmpty(filterUid))
      {
        fullUrl = string.Format("{0}&filterUid={1}", fullUrl, filterUid);
      }
      //fullUrl += queryParameters;
      return fullUrl;
    }
  }
}
