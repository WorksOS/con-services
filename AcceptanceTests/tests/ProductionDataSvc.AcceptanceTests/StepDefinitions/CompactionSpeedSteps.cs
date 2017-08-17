using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionSpeed")]
  public class CompactionSpeedSteps : BaseCompactionSteps
  {
    private Getter<CompactionSpeedSummaryResult> speedSummaryRequester;

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      switch (operation)
      {
        case "SpeedSummary": speedSummaryRequester = new Getter<CompactionSpeedSummaryResult>(url, resultFileName); break;
        case "SpeedDetails": ScenarioContext.Current.Pending(); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "SpeedSummary": speedSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "SpeedDetails": ScenarioContext.Current.Pending(); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      switch (operation)
      {
        case "SpeedSummary": speedSummaryRequester.QueryString.Add("filterUid", filterUid); break;
        case "SpeedDetails": ScenarioContext.Current.Pending(); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "SpeedSummary": Assert.AreEqual(speedSummaryRequester.ResponseRepo[resultName], speedSummaryRequester.CurrentResponse); break;
        case "SpeedDetails": ScenarioContext.Current.Pending(); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "SpeedSummary": speedSummaryRequester.DoValidRequest(url); break;
        case "SpeedDetails": ScenarioContext.Current.Pending(); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }
  }
}
