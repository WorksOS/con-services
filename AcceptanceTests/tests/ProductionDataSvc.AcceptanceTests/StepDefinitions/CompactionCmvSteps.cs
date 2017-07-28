
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionCmv")]
  public class CompactionCmvSteps
  {
    private Getter<CompactionCmvSummaryResult> cmvSummaryRequester;
    private Getter<CompactionCmvDetailedResult> cmvDetailsRequester;
    private Getter<CompactionCmvPercentChangeResult> cmvPercentChangeRequester;

    private string url;
    private string projectUid;

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"the Compaction CMV Summary service URI ""(.*)""")]
    public void GivenTheCompactionCMVSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }
    [When(@"I request CMV summary")]
    public void WhenIRequestCMVSummary()
    {
      cmvSummaryRequester = Getter<CompactionCmvSummaryResult>.GetIt<CompactionCmvSummaryResult>(this.url, this.projectUid);
    }

    [Then(@"the CMV summary result should be")]
    public void ThenTheCMVSummaryResultShouldBe(string multilineText)
    {
      cmvSummaryRequester.CompareIt<CompactionCmvSummaryResult>(multilineText);
    }

    [Given(@"the Compaction CMV Details service URI ""(.*)""")]
    public void GivenTheCompactionCMVDetailsServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request CMV details")]
    public void WhenIRequestCMVDetails()
    {
      cmvDetailsRequester = Getter<CompactionCmvDetailedResult>.GetIt<CompactionCmvDetailedResult>(this.url, this.projectUid);
    }

    [Then(@"the CMV details result should be")]
    public void ThenTheCMVDetailsResultShouldBe(string multilineText)
    {
      cmvDetailsRequester.CompareIt<CompactionCmvDetailedResult>(multilineText);
    }

    [Given(@"the Compaction CMV % Change Summary service URI ""(.*)""")]
    public void GivenTheCompactionCMVChangeSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request CMV % change")]
    public void WhenIRequestCMVChange()
    {
      cmvPercentChangeRequester = Getter<CompactionCmvPercentChangeResult>.GetIt<CompactionCmvPercentChangeResult>(this.url, this.projectUid);
    }

    [Then(@"the CMV % Change result should be")]
    public void ThenTheCMVChangeResultShouldBe(string multilineText)
    {
      cmvPercentChangeRequester.CompareIt<CompactionCmvPercentChangeResult>(multilineText);
    }
  }
}
