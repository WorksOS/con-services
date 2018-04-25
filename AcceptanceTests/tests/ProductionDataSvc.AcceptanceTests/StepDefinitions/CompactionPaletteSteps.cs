using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionPalette")]
  public class CompactionPaletteSteps : BaseCompactionSteps
  {
    private Getter<CompactionColorPalettesResult> paletteRequester;
    private Getter<CompactionElevationPaletteResult> elevPaletteRequester;

    [Given(@"startUtc ""(.*)"" and endUtc ""(.*)""")]
    public void GivenStartUtcAndEndUtc(string startUtc, string endUtc)
    {
      if (operation == "ElevationPalette")
      {
          elevPaletteRequester.QueryString.Add("startUtc", startUtc);
          elevPaletteRequester.QueryString.Add("endUtc", endUtc);
      }
    }

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      switch (operation)
      {
        case "ElevationPalette": elevPaletteRequester = new Getter<CompactionElevationPaletteResult>(url, resultFileName); break;
        case "CompactionPalettes": paletteRequester = new Getter<CompactionColorPalettesResult>(url, resultFileName); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "ElevationPalette": elevPaletteRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "CompactionPalettes": paletteRequester.QueryString.Add("ProjectUid", projectUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      if (operation == "ElevationPalette")
        elevPaletteRequester.QueryString.Add("filterUid", filterUid);
      else
        Assert.Fail(TEST_FAIL_MESSAGE);
    }

    [Given(@"designUid ""(.*)""")]
    public void GivenDesignUid(string designUid)
    {
      if (operation == "ElevationPalette")
        elevPaletteRequester.QueryString.Add("designUid", designUid);
      else
        Assert.Fail(TEST_FAIL_MESSAGE);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "ElevationPalette": Assert.AreEqual(elevPaletteRequester.ResponseRepo[resultName], elevPaletteRequester.CurrentResponse); break;
        case "CompactionPalettes": Assert.AreEqual(paletteRequester.ResponseRepo[resultName], paletteRequester.CurrentResponse); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "ElevationPalette": elevPaletteRequester.DoValidRequest(url); break;
        case "CompactionPalettes": paletteRequester.DoValidRequest(url); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }
  }
}
