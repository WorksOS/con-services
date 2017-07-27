using System;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionSpeed")]
  public class CompactionSpeedSteps
  {
    private Getter<CompactionSpeedSummaryResult> speedSummaryRequester;

    private string url;
    private string projectUid;

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"the Compaction Speed Summary service URI ""(.*)""")]
    public void GivenTheCompactionSpeedSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Speed summary")]
    public void WhenIRequestSpeedSummary()
    {
      speedSummaryRequester = Getter<CompactionSpeedSummaryResult>.GetIt<CompactionSpeedSummaryResult>(this.url, this.projectUid);
    }

    [Then(@"the Speed result should be")]
    public void ThenTheSpeedResultShouldBe(string multilineText)
    {
      speedSummaryRequester.CompareIt<CompactionSpeedSummaryResult>(multilineText);
    }
  }
}
