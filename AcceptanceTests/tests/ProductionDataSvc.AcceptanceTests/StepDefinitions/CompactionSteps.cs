
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
    private Getter<CompactionCmvDetailedResult> cmvDetailsRequester;
    private Getter<CompactionMdpSummaryResult> mdpSummaryRequester;
    private Getter<CompactionPassCountSummaryResult> passCountSummaryRequester;
    private Getter<CompactionPassCountDetailedResult> passCountDetailsRequester;
    private Getter<CompactionSpeedSummaryResult> speedSummaryRequester;
    private Getter<CompactionTemperatureSummaryResult> temperatureSummaryRequester;
    private Getter<CompactionCmvPercentChangeResult> cmvPercentChangeRequester;
    private Poster<StatisticsParameters, ProjectStatistics> projectStatisticsPoster;
    private Getter<ElevationStatisticsResult> elevationRangeRequester;
    private Getter<TileResult> tileRequester;
    private Getter<CompactionColorPalettesResult> paletteRequester;
    private Getter<CompactionElevationPaletteResult> elevPaletteRequester;

    private StatisticsParameters statsRequest;
    private string projectUid;
    private string queryParameters = string.Empty;
    private string operation;

    [Given(@"the Compaction CMV Summary service URI ""(.*)""")]
    public void GivenTheCompactionCMVSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"startUtc ""(.*)"" and endUtc ""(.*)""")]
    public void GivenStartUtcAndEndUtc(string startUtc, string endUtc)
    {
      switch (operation)
      {
        case "ElevationRange":
          elevationRangeRequester.QueryString.Add("startUtc", startUtc);
          elevationRangeRequester.QueryString.Add("endUtc", endUtc);
          break;
        case "ElevationPalette":
          elevPaletteRequester.QueryString.Add("startUtc", startUtc);
          elevPaletteRequester.QueryString.Add("endUtc", endUtc);
          break;
      }
    }
    
    [When(@"I request CMV summary")]
    public void WhenIRequestCMVSummary()
    {
      cmvSummaryRequester = GetIt<CompactionCmvSummaryResult>();
    }

    [Then(@"the CMV summary result should be")]
    public void ThenTheCMVSummaryResultShouldBe(string multilineText)
    {
      CompareIt<CompactionCmvSummaryResult>(multilineText, cmvSummaryRequester);
    }

    [Given(@"the Compaction service URI ""(.*)"" for operation ""(.*)""")]
    public void GivenTheCompactionServiceURIForOperation(string url, string operation)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
      this.operation = operation;
    }

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      switch (operation)
      {
        case "CMVSummary": cmvSummaryRequester = new Getter<CompactionCmvSummaryResult>(url, resultFileName); break;
        case "MDPSummary": mdpSummaryRequester = new Getter<CompactionMdpSummaryResult>(url, resultFileName); break;
        case "PassCountSummary": passCountSummaryRequester = new Getter<CompactionPassCountSummaryResult>(url, resultFileName); break;
        case "PassCountDetails": passCountDetailsRequester = new Getter<CompactionPassCountDetailedResult>(url, resultFileName); break;
        case "TemperatureSummary": temperatureSummaryRequester = new Getter<CompactionTemperatureSummaryResult>(url, resultFileName); break;
        case "SpeedSummary": speedSummaryRequester = new Getter<CompactionSpeedSummaryResult>(url, resultFileName); break;
        case "CMVPercentChangeSummary": cmvPercentChangeRequester = new Getter<CompactionCmvPercentChangeResult>(url, resultFileName); break;
        case "ElevationRange": elevationRangeRequester = new Getter<ElevationStatisticsResult>(url, resultFileName); break;
        case "ProjectStatistics": projectStatisticsPoster = new Poster<StatisticsParameters, ProjectStatistics>(url, null, resultFileName); break;
        case "ProductionDataTiles": tileRequester = new Getter<TileResult>(url, resultFileName); break;
        case "ElevationPalette": elevPaletteRequester = new Getter<CompactionElevationPaletteResult>(url, resultFileName); break;
        case "CompactionPalettes": paletteRequester = new Getter<CompactionColorPalettesResult>(url, resultFileName); break;
      }
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "CMVSummary": cmvSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "MDPSummary": mdpSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "PassCountSummary": passCountSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "PassCountDetails": passCountDetailsRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "TemperatureSummary": temperatureSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "SpeedSummary": speedSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "CMVPercentChangeSummary": cmvPercentChangeRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "ElevationRange": elevationRangeRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "ProjectStatistics": statsRequest = new StatisticsParameters { projectUid = projectUid }; break;
        case "ProductionDataTiles": tileRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "ElevationPalette": elevPaletteRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "CompactionPalettes": paletteRequester.QueryString.Add("ProjectUid", projectUid); break;
      }
    }

    [Given(@"designUid ""(.*)""")]
    public void GivenDesignUid(string designUid)
    {
      switch (operation)
      {
        case "CMVSummary": cmvSummaryRequester.QueryString.Add("designUid", designUid); break;
        case "MDPSummary": mdpSummaryRequester.QueryString.Add("designUid", designUid); break;
        case "PassCountSummary": passCountSummaryRequester.QueryString.Add("designUid", designUid); break;
        case "PassCountDetails": passCountDetailsRequester.QueryString.Add("designUid", designUid); break;
        case "TemperatureSummary": temperatureSummaryRequester.QueryString.Add("designUid", designUid); break;
        case "SpeedSummary": speedSummaryRequester.QueryString.Add("designUid", designUid); break;
        case "CMVPercentChangeSummary": cmvPercentChangeRequester.QueryString.Add("designUid", designUid); break;
        case "ElevationRange": elevationRangeRequester.QueryString.Add("designUid", designUid); break;
        case "ProductionDataTiles": tileRequester.QueryString.Add("designUid", designUid); break;
        case "ElevationPalette": elevPaletteRequester.QueryString.Add("designUid", designUid); break;
      }
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "CMVSummary": Assert.AreEqual(cmvSummaryRequester.ResponseRepo[resultName], cmvSummaryRequester.CurrentResponse); break;
        case "MDPSummary": Assert.AreEqual(mdpSummaryRequester.ResponseRepo[resultName], mdpSummaryRequester.CurrentResponse); break;
        case "PassCountSummary": Assert.AreEqual(passCountSummaryRequester.ResponseRepo[resultName], passCountSummaryRequester.CurrentResponse); break;
        case "PassCountDetails": Assert.AreEqual(passCountDetailsRequester.ResponseRepo[resultName], passCountDetailsRequester.CurrentResponse); break;
        case "TemperatureSummary": Assert.AreEqual(temperatureSummaryRequester.ResponseRepo[resultName], temperatureSummaryRequester.CurrentResponse); break;
        case "SpeedSummary": Assert.AreEqual(speedSummaryRequester.ResponseRepo[resultName], speedSummaryRequester.CurrentResponse); break;
        case "CMVPercentChangeSummary": Assert.AreEqual(cmvPercentChangeRequester.ResponseRepo[resultName], cmvPercentChangeRequester.CurrentResponse); break;
        case "ElevationRange": Assert.AreEqual(elevationRangeRequester.ResponseRepo[resultName], elevationRangeRequester.CurrentResponse); break;
        case "ProjectStatistics": Assert.AreEqual(projectStatisticsPoster.ResponseRepo[resultName], projectStatisticsPoster.CurrentResponse); break;
        case "ProductionDataTiles": Assert.AreEqual(tileRequester.ResponseRepo[resultName], tileRequester.CurrentResponse); break;
        case "ElevationPalette": Assert.AreEqual(elevPaletteRequester.ResponseRepo[resultName], elevPaletteRequester.CurrentResponse); break;
        case "CompactionPalettes": Assert.AreEqual(paletteRequester.ResponseRepo[resultName], paletteRequester.CurrentResponse); break;
      }
    }

    [When(@"I request MDP summary")]
    public void WhenIRequestMDPSummary()
    {
      mdpSummaryRequester.DoValidRequest(url);
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "CMVSummary": cmvSummaryRequester.DoValidRequest(url); break;
        case "MDPSummary": mdpSummaryRequester.DoValidRequest(url); break;
        case "PassCountSummary": passCountSummaryRequester.DoValidRequest(url); break;
        case "PassCountDetails": passCountDetailsRequester.DoValidRequest(url); break;
        case "TemperatureSummary": temperatureSummaryRequester.DoValidRequest(url); break;
        case "SpeedSummary": speedSummaryRequester.DoValidRequest(url); break;
        case "CMVPercentChangeSummary": cmvPercentChangeRequester.DoValidRequest(url); break;
        case "ElevationRange": elevationRangeRequester.DoValidRequest(url); break;
        case "ProjectStatistics": projectStatisticsPoster.DoValidRequest(statsRequest); break;
        case "ProductionDataTiles": tileRequester.DoValidRequest(url); break;
        case "ElevationPalette": elevPaletteRequester.DoValidRequest(url); break;
        case "CompactionPalettes": paletteRequester.DoValidRequest(url); break;
      }
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

    [Then(@"the Passcount summary result should be")]
    public void ThenThePasscountResultShouldBe(string multilineText)
    {
      CompareIt<CompactionPassCountSummaryResult>(multilineText, passCountSummaryRequester);
    }

    [Given(@"the Compaction Passcount Details service URI ""(.*)""")]
    public void GivenTheCompactionPasscountDetailsServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Passcount details")]
    public void WhenIRequestPasscountDetails()
    {
      passCountDetailsRequester = GetIt<CompactionPassCountDetailedResult>();
    }

    [Then(@"the Passcount details result should be")]
    public void ThenThePasscountDetailsResultShouldBe(string multilineText)
    {
      CompareIt<CompactionPassCountDetailedResult>(multilineText, passCountDetailsRequester);
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

    [Given(@"displayMode ""(.*)"" and bbox ""(.*)"" and width ""(.*)"" and height ""(.*)""")]
    public void GivenDisplayModeAndBboxLLAndWidthAndHeight(int mode, string bbox, int width, int height)
    {
      //queryParameters = string.Format("&mode={0}&BBOX={1}&WIDTH={2}&HEIGHT={3}", 
      //  mode, bbox, width, height);

      if (operation != "ProductionDataTiles")
        return;

      tileRequester.QueryString.Add("mode", mode.ToString());
      tileRequester.QueryString.Add("bbox", bbox);
      tileRequester.QueryString.Add("width", width.ToString());
      tileRequester.QueryString.Add("height", height.ToString());
    }

    [When(@"I request a Tile")]
    public void WhenIRequestATile()
    {
      tileRequester = GetIt<TileResult>();
    }

    [Then(@"the Tile result should be")]
    public void ThenTheTileResultShouldBe(string multilineText)
    {
      CompareIt<TileResult>(multilineText, tileRequester);
    }

    [Given(@"the Compaction Elevation Palette service URI ""(.*)""")]
    public void GivenTheCompactionElevationPaletteServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Elevation Palette")]
    public void WhenIRequestElevationPalette()
    {
      elevPaletteRequester = GetIt<CompactionElevationPaletteResult>();
    }

    [Then(@"the Elevation Palette result should be")]
    public void ThenTheElevationPaletteResultShouldBe(string multilineText)
    {
      CompareIt<CompactionElevationPaletteResult>(multilineText, elevPaletteRequester);
    }

    [Given(@"the Compaction Palettes service URI ""(.*)""")]
    public void GivenTheCompactionPalettesServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Palettes")]
    public void WhenIRequestPalettes()
    {
      paletteRequester = GetIt<CompactionColorPalettesResult>();
    }

    [Then(@"the Palettes result should be")]
    public void ThenThePalettesResultShouldBe(string multilineText)
    {
      CompareIt<CompactionColorPalettesResult>(multilineText, paletteRequester);
    }

    [Given(@"the Compaction CMV Details service URI ""(.*)""")]
    public void GivenTheCompactionCMVDetailsServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request CMV details")]
    public void WhenIRequestCMVDetails()
    {
      cmvDetailsRequester = GetIt<CompactionCmvDetailedResult>();      
    }

    [Then(@"the CMV details result should be")]
    public void ThenTheCMVDetailsResultShouldBe(string multilineText)
    {
      CompareIt<CompactionCmvDetailedResult>(multilineText, cmvDetailsRequester);
    }

    private Getter<T> GetIt<T>()
    {
      this.url = string.Format("{0}?projectUid={1}", this.url, projectUid);     
      this.url += this.queryParameters;
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
