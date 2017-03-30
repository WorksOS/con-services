
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "Compaction")]
  public sealed class CompactionSteps
  {
    private string url;

    private Getter<CompactionCmvSummaryResult> cmvSummaryRequester;
    private Getter<CompactionMdpSummaryResult> mdpSummaryRequester;
    private Getter<CompactionPassCountSummaryResult> passCountSummaryRequester;
    private Getter<CompactionSpeedSummaryResult> speedSummaryRequester;
    private Getter<CompactionTemperatureSummaryResult> temperatureSummaryRequester;
    private Getter<CompactionCmvPercentChangeResult> cmvPercentChangeRequester;
    private Poster<StatisticsParameters, ProjectStatistics> projectStatisticsPoster;
    private Getter<ElevationStatisticsResult> elevationRangeRequester;
    private Poster<CompactionTileRequest, TileResult> tilePoster;

    private CompactionTileRequest tileRequest;
    private StatisticsParameters statsRequest;
    private string projectUid;


    [Given(@"the Compaction CMV Summary service URI ""(.*)""")]
    public void GivenTheCompactionCMVSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [When(@"I request CMV summary")]
    public void WhenIRequestCMVSummary()
    {
      cmvSummaryRequester = GetIt<CompactionCmvSummaryResult>();
    }

    [Then(@"the CMV result should be")]
    public void ThenTheCMVResultShouldBe(string multilineText)
    {
      CompareIt<CompactionCmvSummaryResult>(multilineText, cmvSummaryRequester);
    }

    [Given(@"the Compaction MDP Summary service URI ""(.*)""")]
    public void GivenTheCompactionMDPSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request MDP summary")]
    public void WhenIRequestMDPSummary()
    {
      mdpSummaryRequester = GetIt<CompactionMdpSummaryResult>();
    }

    [Then(@"the MDP result should be")]
    public void ThenTheMDPResultShouldBe(string multilineText)
    {
      CompareIt<CompactionMdpSummaryResult>(multilineText, mdpSummaryRequester);
    }

    [Given(@"the Compaction Passcount Summary service URI ""(.*)""")]
    public void GivenTheCompactionPasscountSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Passcount summary")]
    public void WhenIRequestPasscountSummary()
    {
      passCountSummaryRequester = GetIt<CompactionPassCountSummaryResult>();
    }

    [Then(@"the Passcount result should be")]
    public void ThenThePasscountResultShouldBe(string multilineText)
    {
      CompareIt<CompactionPassCountSummaryResult>(multilineText, passCountSummaryRequester);
    }

    [Given(@"the Compaction Temperature Summary service URI ""(.*)""")]
    public void GivenTheCompactionTemperatureSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Temperature summary")]
    public void WhenIRequestTemperatureSummary()
    {
      temperatureSummaryRequester = GetIt<CompactionTemperatureSummaryResult>();
    }

    [Then(@"the Temperature result should be")]
    public void ThenTheTemperatureResultShouldBe(string multilineText)
    {
      CompareIt<CompactionTemperatureSummaryResult>(multilineText, temperatureSummaryRequester);
    }

    [Given(@"the Compaction Speed Summary service URI ""(.*)""")]
    public void GivenTheCompactionSpeedSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Speed summary")]
    public void WhenIRequestSpeedSummary()
    {
      speedSummaryRequester = GetIt<CompactionSpeedSummaryResult>();
    }

    [Then(@"the Speed result should be")]
    public void ThenTheSpeedResultShouldBe(string multilineText)
    {
      CompareIt<CompactionSpeedSummaryResult>(multilineText, speedSummaryRequester);
    }

    [Given(@"the Compaction CMV % Change Summary service URI ""(.*)""")]
    public void GivenTheCompactionCMVChangeSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request CMV % change")]
    public void WhenIRequestCMVChange()
    {
      cmvPercentChangeRequester = GetIt<CompactionCmvPercentChangeResult>();
    }

    [Then(@"the CMV % Change result should be")]
    public void ThenTheCMVChangeResultShouldBe(string multilineText)
    {
      CompareIt<CompactionCmvPercentChangeResult>(multilineText, cmvPercentChangeRequester);
    }

    [Given(@"the Compaction Elevation Range service URI ""(.*)""")]
    public void GivenTheCompactionElevationRangeServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Elevation Range")]
    public void WhenIRequestElevationRange()
    {
      elevationRangeRequester = GetIt<ElevationStatisticsResult>();
    }

    [Then(@"the Elevation Range result should be")]
    public void ThenTheElevationRangeResultShouldBe(string multilineText)
    {
      CompareIt<ElevationStatisticsResult>(multilineText, elevationRangeRequester);
    }

    [Given(@"the Compaction Project Statistics service URI ""(.*)""")]
    public void GivenTheCompactionProjectStatisticsServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Project Statistics")]
    public void WhenIRequestProjectStatistics()
    {
      statsRequest = new StatisticsParameters { projectUid = this.projectUid };
      projectStatisticsPoster = PostIt<StatisticsParameters, ProjectStatistics>(statsRequest);
    }

    [Then(@"the Project Statistics result should be")]
    public void ThenTheProjectStatisticsResultShouldBe(string multilineText)
    {
      CompareIt(multilineText, projectStatisticsPoster);
    }

    [Given(@"the Compaction Tiles service URI ""(.*)""")]
    public void GivenTheCompactionTilesServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a displayMode ""(.*)"" and a bbox ""(.*)"" and a width ""(.*)"" and a height ""(.*)""")]
    public void GivenADisplayModeAndABboxLLAndAWidthAndAHeight(int mode, string bbox, int width, int height)
    {
      string[] parts = bbox.Split(new char[] { ',' });
      BoundingBox2DLatLon latLngs = new BoundingBox2DLatLon
      {
        bottomLeftLon = double.Parse(parts[1]),
        bottomleftLat = double.Parse(parts[0]),
        topRightLon = double.Parse(parts[3]),
        topRightLat = double.Parse(parts[2]),
      };
      tileRequest = new CompactionTileRequest
      {
        projectUid = this.projectUid,
        mode = (DisplayMode)mode,
        boundBoxLL = latLngs,
        width = (ushort)width,
        height = (ushort)height
      };
    }


    [When(@"I request a Tile")]
    public void WhenIRequestATile()
    {
      tilePoster = PostIt<CompactionTileRequest, TileResult>(tileRequest);
    }

    [Then(@"the Tile result should be")]
    public void ThenTheTileResultShouldBe(string multilineText)
    {
      CompareIt(multilineText, tilePoster);
    }


    private Getter<T> GetIt<T>()
    {
      this.url = string.Format("{0}?projectUid={1}", this.url, projectUid);
      Getter<T> getter = new Getter<T>(this.url);
      getter.DoValidRequest();
      return getter;
    }

    private Poster<T, U> PostIt<T, U>(T request)
    {
      Poster<T, U> poster = new Poster<T, U>(this.url, request);
      //poster.CurrentRequest = request;
      poster.DoValidRequest();
      return poster;
    }

    private void CompareIt<T>(string multilineText, Getter<T> requester)
    {
      T expected = JsonConvert.DeserializeObject<T>(multilineText);
      Assert.AreEqual(expected, requester.CurrentResponse);
    }

    private void CompareIt<T, U>(string multilineText, Poster<T, U> poster)
    {
      U expected = JsonConvert.DeserializeObject<U>(multilineText);
      Assert.AreEqual(expected, poster.CurrentResponse);
    }

  }
}
