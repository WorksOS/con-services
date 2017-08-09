
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionElevation")]
  public class CompactionElevationSteps : BaseCompactionSteps
  {
    private Getter<ProjectStatistics> projectStatisticsRequester;
    private Getter<ElevationStatisticsResult> elevationRangeRequester;

    [Given(@"startUtc ""(.*)"" and endUtc ""(.*)""")]
    public void GivenStartUtcAndEndUtc(string startUtc, string endUtc)
    {
      if (operation == "ElevationRange")
      {
        elevationRangeRequester.QueryString.Add("startUtc", startUtc);
        elevationRangeRequester.QueryString.Add("endUtc", endUtc);
      }
      else
        Assert.Fail(TEST_FAIL_MESSAGE);
    }

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      switch (operation)
      {
        case "ElevationRange": elevationRangeRequester = new Getter<ElevationStatisticsResult>(url, resultFileName); break;
        case "ProjectStatistics": projectStatisticsRequester = new Getter<ProjectStatistics>(url, resultFileName); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "ElevationRange": elevationRangeRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "ProjectStatistics": projectStatisticsRequester.QueryString.Add("ProjectUid", projectUid); break;// statsRequest = new StatisticsParameters { projectUid = projectUid }; break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"designUid ""(.*)""")]
    public void GivenDesignUid(string designUid)
    {
      if (operation == "ElevationRange")
        elevationRangeRequester.QueryString.Add("designUid", designUid);
      else
        Assert.Fail(TEST_FAIL_MESSAGE);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "ElevationRange": Assert.AreEqual(elevationRangeRequester.ResponseRepo[resultName], elevationRangeRequester.CurrentResponse); break;
        case "ProjectStatistics": Assert.AreEqual(projectStatisticsRequester.ResponseRepo[resultName], projectStatisticsRequester.CurrentResponse); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "ElevationRange": elevationRangeRequester.DoValidRequest(url); break;
        case "ProjectStatistics": projectStatisticsRequester.DoValidRequest(url); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }
  }
}
