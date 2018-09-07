
using System.Net;
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
    private Getter<ProjectExtents> projectExtentsRequester;
    private Getter<ElevationStatisticsResult> elevationRangeRequester;
    private Getter<AlignmentStationRangeResult> alignmentRequester;

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
        case "GetAlignmentStationRange": alignmentRequester = new Getter<AlignmentStationRangeResult>(url, resultFileName); break;
        case "ElevationRange": elevationRangeRequester = new Getter<ElevationStatisticsResult>(url, resultFileName); break;
        case "ProjectStatistics": projectStatisticsRequester = new Getter<ProjectStatistics>(url, resultFileName); break;
        case "ProjectExtents": projectExtentsRequester = new Getter<ProjectExtents>(url, resultFileName); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "GetAlignmentStationRange": alignmentRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "ElevationRange": elevationRangeRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "ProjectStatistics": projectStatisticsRequester.QueryString.Add("ProjectUid", projectUid); break;
        case "ProjectExtents": projectExtentsRequester.QueryString.Add("ProjectUid", projectUid); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      if (operation == "ElevationRange")
      {
        if (!string.IsNullOrEmpty(filterUid))
          elevationRangeRequester.QueryString.Add("filterUid", filterUid);
      }
      else
        Assert.Fail(TEST_FAIL_MESSAGE);
    }

    [Given(@"fileUid ""(.*)""")]
    public void GivenFileUid(string fileUid)
    {
      if (operation == "GetAlignmentStationRange")
        alignmentRequester.QueryString.Add("alignmentFileUid", fileUid);
      else
        Assert.Fail(TEST_FAIL_MESSAGE);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "GetAlignmentStationRange": Assert.AreEqual(alignmentRequester.ResponseRepo[resultName], alignmentRequester.CurrentResponse); break;
        case "ElevationRange": Assert.AreEqual(elevationRangeRequester.ResponseRepo[resultName], elevationRangeRequester.CurrentResponse); break;
        case "ProjectStatistics": Assert.AreEqual(projectStatisticsRequester.ResponseRepo[resultName], projectStatisticsRequester.CurrentResponse); break;
        case "ProjectExtents": Assert.AreEqual(projectExtentsRequester.ResponseRepo[resultName], projectExtentsRequester.CurrentResponse); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      switch (operation)
      {
        case "GetAlignmentStationRange": alignmentRequester.DoValidRequest(url); break;
        case "ElevationRange": elevationRangeRequester.DoValidRequest(url); break;
        case "ProjectStatistics": projectStatisticsRequester.DoValidRequest(url); break;
        case "ProjectExtents": projectExtentsRequester.DoValidRequest(url); break;
        default: Assert.Fail(TEST_FAIL_MESSAGE); break;
      }
    }

    [When(@"I request a Station Range Expecting BadRequest")]
    public void WhenIRequestAReportTileExpectingBadRequest()
    {
      alignmentRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
    }

  
    [Then(@"I should get error code ""(.*)"" and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.AreEqual(errorCode, alignmentRequester.CurrentResponse.Code);
      Assert.AreEqual(message, alignmentRequester.CurrentResponse.Message);
    }



  }
}
