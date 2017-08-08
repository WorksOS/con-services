using System;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionTemperature")]
  public class CompactionTemperatureSteps
  {
    private Getter<CompactionTemperatureSummaryResult> temperatureSummaryRequester;

    private string url;
    private string projectUid;

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"the Compaction Temperature Summary service URI ""(.*)""")]
    public void GivenTheCompactionTemperatureSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Temperature summary")]
    public void WhenIRequestTemperatureSummary()
    {
      temperatureSummaryRequester = Getter<CompactionTemperatureSummaryResult>.GetIt<CompactionTemperatureSummaryResult>(this.url, this.projectUid);
    }

    [Then(@"the Temperature result should be")]
    public void ThenTheTemperatureResultShouldBe(string multilineText)
    {
      temperatureSummaryRequester.CompareIt<CompactionTemperatureSummaryResult>(multilineText);
    }

  }
}
