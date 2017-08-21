
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionPassCount")]
  public class CompactionPassCountSteps : BaseCompactionSteps
  {
    private Getter<CompactionPassCountSummaryResult> passCountSummaryRequester;
    private Getter<CompactionPassCountDetailedResult> passCountDetailsRequester;

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      switch (operation)
      {
        case "PassCountSummary": passCountSummaryRequester = new Getter<CompactionPassCountSummaryResult>(url, resultFileName); break;
        case "PassCountDetails": passCountDetailsRequester = new Getter<CompactionPassCountDetailedResult>(url, resultFileName); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "PassCountSummary": passCountSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "PassCountDetails": passCountDetailsRequester.QueryString.Add("ProjectUid", projectUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      switch (operation)
      {
        case "PassCountSummary": passCountSummaryRequester.QueryString.Add("filterUid", filterUid); break;
        case "PassCountDetails": passCountDetailsRequester.QueryString.Add("filterUid", filterUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "PassCountSummary": Assert.AreEqual(passCountSummaryRequester.ResponseRepo[resultName], passCountSummaryRequester.CurrentResponse); break;
        case "PassCountDetails": Assert.AreEqual(passCountDetailsRequester.ResponseRepo[resultName], passCountDetailsRequester.CurrentResponse); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "PassCountSummary": passCountSummaryRequester.DoValidRequest(url); break;
        case "PassCountDetails": passCountDetailsRequester.DoValidRequest(url); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }
  }
}
