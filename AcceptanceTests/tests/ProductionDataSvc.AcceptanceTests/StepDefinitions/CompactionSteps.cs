
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
    private Getter<ProjectStatistics> projectStatisticsRequester;
    private Getter<ElevationStatisticsResult> elevationRangeRequester;
    private Getter<TileResult> tileRequester;
    private Getter<CompactionColorPalettesResult> paletteRequester;
    private Getter<CompactionElevationPaletteResult> elevPaletteRequester;

    //private StatisticsParameters statsRequest;
    private string queryParameters = string.Empty;
    private string operation;

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
        case "ProjectStatistics": projectStatisticsRequester = new Getter<ProjectStatistics>(url, resultFileName); break;
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
        case "ProjectStatistics": projectStatisticsRequester.QueryString.Add("ProjectUid", projectUid); break;// statsRequest = new StatisticsParameters { projectUid = projectUid }; break;
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
        case "ProjectStatistics": Assert.AreEqual(projectStatisticsRequester.ResponseRepo[resultName], projectStatisticsRequester.CurrentResponse); break;
        case "ProductionDataTiles": Assert.AreEqual(tileRequester.ResponseRepo[resultName], tileRequester.CurrentResponse); break;
        case "ElevationPalette": Assert.AreEqual(elevPaletteRequester.ResponseRepo[resultName], elevPaletteRequester.CurrentResponse); break;
        case "CompactionPalettes": Assert.AreEqual(paletteRequester.ResponseRepo[resultName], paletteRequester.CurrentResponse); break;
      }
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
        case "ProjectStatistics": projectStatisticsRequester.DoValidRequest(url); break;
        case "ProductionDataTiles": tileRequester.DoValidRequest(url); break;
        case "ElevationPalette": elevPaletteRequester.DoValidRequest(url); break;
        case "CompactionPalettes": paletteRequester.DoValidRequest(url); break;
      }
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

  }
}
