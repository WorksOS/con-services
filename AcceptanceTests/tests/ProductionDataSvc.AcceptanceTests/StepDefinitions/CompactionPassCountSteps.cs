
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionPassCount")]
  public class CompactionPassCountSteps
  {
    private Getter<CompactionPassCountSummaryResult> passCountSummaryRequester;
    private Getter<CompactionPassCountDetailedResult> passCountDetailsRequester;

    private string url;
    private string projectUid;

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"the Compaction Passcount Summary service URI ""(.*)""")]
    public void GivenTheCompactionPasscountSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Passcount summary")]
    public void WhenIRequestPasscountSummary()
    {
      passCountSummaryRequester = Getter<CompactionPassCountSummaryResult>.GetIt<CompactionPassCountSummaryResult>(this.url, this.projectUid);
    }

    [Then(@"the Passcount summary result should be")]
    public void ThenThePasscountResultShouldBe(string multilineText)
    {
      passCountSummaryRequester.CompareIt<CompactionPassCountSummaryResult>(multilineText);
    }

    [Given(@"the Compaction Passcount Details service URI ""(.*)""")]
    public void GivenTheCompactionPasscountDetailsServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Passcount details")]
    public void WhenIRequestPasscountDetails()
    {
      passCountDetailsRequester = Getter<CompactionPassCountDetailedResult>.GetIt<CompactionPassCountDetailedResult>(this.url, this.projectUid);
    }

    [Then(@"the Passcount details result should be")]
    public void ThenThePasscountDetailsResultShouldBe(string multilineText)
    {
      passCountDetailsRequester.CompareIt<CompactionPassCountDetailedResult>(multilineText);
    }

  }
}
