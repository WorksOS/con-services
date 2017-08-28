
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionCmv")]
  public class CompactionCmvSteps : BaseCompactionSteps
  {
    private Getter<CompactionCmvSummaryResult> cmvSummaryRequester;
    private Getter<CompactionCmvDetailedResult> cmvDetailsRequester;
    private Getter<CompactionCmvPercentChangeResult> cmvPercentChangeRequester;

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      switch (operation)
      {
        case "CMVSummary": cmvSummaryRequester = new Getter<CompactionCmvSummaryResult>(url, resultFileName); break;
        case "CMVDetails": cmvDetailsRequester = new Getter<CompactionCmvDetailedResult>(url, resultFileName); break;
        case "CMVPercentChangeSummary": cmvPercentChangeRequester = new Getter<CompactionCmvPercentChangeResult>(url, resultFileName); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "CMVSummary": cmvSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "CMVDetails": cmvDetailsRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "CMVPercentChangeSummary": cmvPercentChangeRequester.QueryString.Add("ProjectUid", projectUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      switch (operation)
      {
        case "CMVSummary": cmvSummaryRequester.QueryString.Add("filterUid", filterUid); break;
        case "CMVDetails": cmvDetailsRequester.QueryString.Add("filterUid", filterUid); break;
        case "CMVPercentChangeSummary": cmvPercentChangeRequester.QueryString.Add("filterUid", filterUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "CMVSummary": Assert.AreEqual(cmvSummaryRequester.ResponseRepo[resultName], cmvSummaryRequester.CurrentResponse); break;
        case "CMVDetails": Assert.AreEqual(cmvDetailsRequester.ResponseRepo[resultName], cmvDetailsRequester.CurrentResponse); break;
        case "CMVPercentChangeSummary": Assert.AreEqual(cmvPercentChangeRequester.ResponseRepo[resultName], cmvPercentChangeRequester.CurrentResponse); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "CMVSummary": cmvSummaryRequester.DoValidRequest(url); break;
        case "CMVDetails": cmvDetailsRequester.DoValidRequest(url); break;
        case "CMVPercentChangeSummary": cmvPercentChangeRequester.DoValidRequest(url); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

  }
}
