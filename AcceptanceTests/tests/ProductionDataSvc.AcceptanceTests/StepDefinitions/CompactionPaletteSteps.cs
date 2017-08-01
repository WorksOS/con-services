using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionPalette")]
  public class CompactionPaletteSteps
  {
    private Getter<CompactionColorPalettesResult> paletteRequester;
    private Getter<CompactionElevationPaletteResult> elevPaletteRequester;

    private string url;
    private string projectUid;
    private string queryParameters = string.Empty;

    [Given(@"the Compaction Elevation Palette service URI ""(.*)""")]
    public void GivenTheCompactionElevationPaletteServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a startUtc ""(.*)"" and an EndUtc ""(.*)""")]
    public void GivenAStartUtcAndAnEndUtc(string startUtc, string endUtc)
    {
      queryParameters = string.Format("&startUtc={0}&endUtc={1}",
        startUtc, endUtc);
    }

    [Given(@"the Compaction Palettes service URI ""(.*)""")]
    public void GivenTheCompactionPalettesServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Elevation Palette")]
    public void WhenIRequestElevationPalette()
    {
      elevPaletteRequester = Getter<CompactionElevationPaletteResult>.GetIt<CompactionElevationPaletteResult>(this.url, this.projectUid, this.queryParameters);
    }

    [When(@"I request Palettes")]
    public void WhenIRequestPalettes()
    {
      paletteRequester = Getter<CompactionColorPalettesResult>.GetIt<CompactionColorPalettesResult>(this.url, this.projectUid, this.queryParameters);
    }

    [Then(@"the Elevation Palette result should be")]
    public void ThenTheElevationPaletteResultShouldBe(string multilineText)
    {
      elevPaletteRequester.CompareIt<CompactionElevationPaletteResult>(multilineText);
    }

    [Then(@"the Palettes result should be")]
    public void ThenThePalettesResultShouldBe(string multilineText)
    {
      paletteRequester.CompareIt<CompactionColorPalettesResult>(multilineText);
    }

  }
}
