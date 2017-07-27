using System;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionMdp")]
  public class CompactionMdpSteps
  {
    private Getter<CompactionMdpSummaryResult> mdpSummaryRequester;

    private string url;
    private string projectUid;

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"the Compaction MDP Summary service URI ""(.*)""")]
    public void GivenTheCompactionMDPSummaryServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request MDP summary")]
    public void WhenIRequestMDPSummary()
    {
      mdpSummaryRequester = Getter<CompactionMdpSummaryResult>.GetIt<CompactionMdpSummaryResult>(this.url, this.projectUid);
    }

    [Then(@"the MDP result should be")]
    public void ThenTheMDPResultShouldBe(string multilineText)
    {
      mdpSummaryRequester.CompareIt<CompactionMdpSummaryResult>(multilineText);
    }
  }
}
