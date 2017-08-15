using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    [Given(@"the Compaction service URI ""(.*)""")]
    public void GivenTheCompactionServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      mdpSummaryRequester = new Getter<CompactionMdpSummaryResult>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      mdpSummaryRequester.QueryString.Add("ProjectUid", projectUid);
    }
    /*
    [Given(@"designUid ""(.*)""")]
    public void GivenDesignUid(string designUid)
    {
      mdpSummaryRequester.QueryString.Add("designUid", designUid);
    }
    */

    [Given(@"filterUid ""(.*)""")]
    public void GivenDesignUid(string filterUid)
    {
      mdpSummaryRequester.QueryString.Add("filterUid", filterUid);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(mdpSummaryRequester.ResponseRepo[resultName], mdpSummaryRequester.CurrentResponse);
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      mdpSummaryRequester.DoValidRequest(url);
    }
  }
}
