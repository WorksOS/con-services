using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionTemperature")]
  public class CompactionTemperatureSteps : BaseCompactionSteps
  {
    private Getter<CompactionTemperatureSummaryResult> temperatureSummaryRequester;
    private Getter<CompactionTemperatureDetailResult> temperatureDetailsRequester;

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      switch (operation)
      {
        case "TemperatureSummary": temperatureSummaryRequester = new Getter<CompactionTemperatureSummaryResult>(url, resultFileName); break;
        case "TemperatureDetails": temperatureDetailsRequester = new Getter<CompactionTemperatureDetailResult>(url, resultFileName); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "TemperatureSummary": temperatureSummaryRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "TemperatureDetails": temperatureDetailsRequester.QueryString.Add("ProjectUid", projectUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      switch (operation)
      {
        case "TemperatureSummary": temperatureSummaryRequester.QueryString.Add("filterUid", filterUid); break;
        case "TemperatureDetails": temperatureDetailsRequester.QueryString.Add("filterUid", filterUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "TemperatureSummary": Assert.AreEqual(temperatureSummaryRequester.ResponseRepo[resultName], temperatureSummaryRequester.CurrentResponse); break;
        case "TemperatureDetails": Assert.AreEqual(temperatureDetailsRequester.ResponseRepo[resultName], temperatureDetailsRequester.CurrentResponse); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "TemperatureSummary": temperatureSummaryRequester.DoValidRequest(url); break;
        case "TemperatureDetails": temperatureDetailsRequester.DoValidRequest(url); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }
  }
}
