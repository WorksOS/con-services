using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionCutFill")]
  public class CompactionCutFillSteps : BaseCompactionSteps
  {
    private Getter<CompactionCutFillDetailedResult> cutFillDetailsRequester;

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      switch (operation)
      {
        case "CutFillDetails": cutFillDetailsRequester = new Getter<CompactionCutFillDetailedResult>(url, resultFileName); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }
        
    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "CutFillDetails": cutFillDetailsRequester.QueryString.Add("projectUid", projectUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"a cutfillDesignUid ""(.*)""")]
    public void GivenACutfillDesignUid(string cutfillDesignUid)
    {
      switch (operation)
      {
        case "CutFillDetails": cutFillDetailsRequester.QueryString.Add("cutfillDesignUid", cutfillDesignUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }
        
    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      switch (operation)
      {
        case "CutFillDetails": cutFillDetailsRequester.QueryString.Add("filterUid", filterUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "CutFillDetails": cutFillDetailsRequester.DoValidRequest(url); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "CutFillDetails": Assert.AreEqual(cutFillDetailsRequester.ResponseRepo[resultName], cutFillDetailsRequester.CurrentResponse); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }
  }
}
