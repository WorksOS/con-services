using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "Report")]
  public class ReportSteps
  {
    private Getter<CompactionReportResult> gridReportRequester;
    private string url;

    [Given(@"the report service uri ""(.*)""")]
    public void GivenTheReportServiceUri(string uri)
    {
      url = RaptorClientConfig.CompactionSvcBaseUri + uri;
    }

    [Given(@"the result file '(.*)'")]
    public void GivenTheResultFile(string resultFile)
    {
      gridReportRequester = new Getter<CompactionReportResult>(url, resultFile);
    }

    [Given(@"I set request parameters projectUid '(.*)' and filterUid '(.*)'")]
    public void GivenISetRequestParametersProjectUidAndFilterUid(string projectUid, string filterUid)
    {
      gridReportRequester.QueryString.Add("projectUid", projectUid);
      gridReportRequester.QueryString.Add("filterUid", filterUid);
    }

    [Given(@"I select columns '(.*)' '(.*)' '(.*)' '(.*)' '(.*)' '(.*)'")]
    public void GivenISelectColumns(string colElevation, string colCmv, string colMdp, string colPassCount,
      string colTemperature, string colCutFill)
    {
      if (colElevation == "Y")
      {
        gridReportRequester.QueryString.Add("reportElevation", "true");
      }
      if (colCmv == "Y")
      {
        gridReportRequester.QueryString.Add("reportCMV", "true");
      }
      if (colMdp == "Y")
      {
        gridReportRequester.QueryString.Add("reportMDP", "true");
      }
      if (colPassCount == "Y")
      {
        gridReportRequester.QueryString.Add("reportPassCount", "true");
      }
      if (colTemperature == "Y")
      {
        gridReportRequester.QueryString.Add("reportTemperature", "true");
      }
      if (colCutFill == "Y")
      {
        gridReportRequester.QueryString.Add("reportCutFill", "true");
      }
    }

    [Given(@"I select Station offset report parameters '(.*)' '(.*)' '(.*)' '(.*)' '(.*)' '(.*)'")]
    public void GivenISelectStationOffsetReportParameters(string cutfillDesignUid, string alignmentUid,
      double crossSectionInterval, double startStation, double endStation, string offsets)
    {
      if (!string.IsNullOrEmpty(cutfillDesignUid))
      {
        gridReportRequester.QueryString.Add("cutfillDesignUid", cutfillDesignUid);
      }
      if (!string.IsNullOrEmpty(alignmentUid))
      {
        gridReportRequester.QueryString.Add("alignmentUid", alignmentUid);
      }
      if (crossSectionInterval > 0)
      {
        gridReportRequester.QueryString.Add("crossSectionInterval",
          crossSectionInterval.ToString(CultureInfo.InvariantCulture));
      }
      if (startStation > 0)
      {
        gridReportRequester.QueryString.Add("startStation", startStation.ToString(CultureInfo.InvariantCulture));
      }
      if (endStation > 0)
      {
        gridReportRequester.QueryString.Add("endStation", endStation.ToString(CultureInfo.InvariantCulture));
      }
      if (!string.IsNullOrEmpty(offsets))
      {
        var laoArray = Array.ConvertAll(offsets.Split(','), double.Parse);

        for (var i = 0; i < laoArray.Length; i++)
        {
          gridReportRequester.QueryString.Add($"offsets[{i}]", laoArray[i].ToString(CultureInfo.InvariantCulture));
        }
      }
    }

    [Given(@"I select grid report parameters '(.*)' '(.*)' '(.*)' '(.*)' '(.*)' '(.*)' '(.*)' '(.*)'")]
    public void GivenISelectReportParameters(string cutfillDesignUid, double gridInterval, string gridReportOption,
      double startNorthing, double startEasting, double endNorthing, double endEasting, double azimuth)
    {
      if (!string.IsNullOrEmpty(cutfillDesignUid))
      {
        gridReportRequester.QueryString.Add("cutfillDesignUid", cutfillDesignUid);
      }

      if (gridInterval > 0)
      {
        gridReportRequester.QueryString.Add("gridInterval", gridInterval.ToString(CultureInfo.InvariantCulture));
      }
      if (!string.IsNullOrEmpty(gridReportOption))
      {
        gridReportRequester.QueryString.Add("gridReportOption", gridReportOption);
      }
      if (startNorthing > 0)
      {
        gridReportRequester.QueryString.Add("startNorthing", startNorthing.ToString(CultureInfo.InvariantCulture));
      }
      if (startEasting > 0)
      {
        gridReportRequester.QueryString.Add("startEasting", startEasting.ToString(CultureInfo.InvariantCulture));
      }
      if (endNorthing > 0)
      {
        gridReportRequester.QueryString.Add("endNorthing", endNorthing.ToString(CultureInfo.InvariantCulture));
      }
      if (endEasting > 0)
      {
        gridReportRequester.QueryString.Add("endEasting", endEasting.ToString(CultureInfo.InvariantCulture));
      }
      if (azimuth > 0)
      {
        gridReportRequester.QueryString.Add("azimuth", azimuth.ToString(CultureInfo.InvariantCulture));
      }
    }

    [When(@"I request a report")]
    public void WhenIRequestAReport()
    {
      gridReportRequester.DoValidRequest(url);
    }

    [When(@"I request a report the response body should contain http code '(.*)'")]
    public void WhenIRequestAReportTheResponseBodyShouldContainHttpCode(HttpStatusCode httpCode)
    {
      gridReportRequester.DoValidRequest(url, httpCode);
    }

    [Then(@"the report '(.*)' and result should match the '(.*)' from the repository")]
    public void ThenTheReportAndResultShouldMatchTheFromTheRepository(int errorCode, string resultName)
    {
      Assert.AreEqual(errorCode, gridReportRequester.CurrentResponse.Code);
      Assert.AreEqual(gridReportRequester.CurrentResponse.Message, gridReportRequester.CurrentResponse.Message);
    }
    
    [Then(@"the grid report result should match the '(.*)' from the repository")]
    public void ThenTheGridReportResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(0, gridReportRequester.CurrentResponse.Code);
      Assert.AreEqual("success", gridReportRequester.CurrentResponse.Message);

      ValidateResponse(resultName);
    }

    [Then(@"The response body should contain Http Code '(.*)'")]
    public void ThenTheResponseBodyShouldContainHttpCode(int httpCode)
    {
      Assert.AreEqual(httpCode, (int)gridReportRequester.CurrentServiceResponse.HttpCode);
    }

    private void ValidateResponse(string resultName)
    {
      var actualResult = gridReportRequester.CurrentResponse.ReportData;
      var expectedResult = gridReportRequester.ResponseRepo[resultName].ReportData;

      // Sort the rows 
      var actualrows = actualResult.Rows.OrderBy(x => x.Northing).ThenBy(x => x.Easting);
      var expectedrows = expectedResult.Rows.OrderBy(x => x.Northing).ThenBy(x => x.Easting);
      var rowCount = actualrows.Count();
      Assert.IsTrue(rowCount == expectedrows.Count(), "Row count not the same as expected");

      var actualrowList = actualrows.ToList();
      var expectedrowList = expectedrows.ToList();
      // Check the rows are the same
      for (int rowIdx = 0; rowIdx < rowCount; rowIdx++)
      {
        Common.CompareDouble(expectedrowList[rowIdx].Easting, actualrowList[rowIdx].Easting, "Easting", rowIdx);
        Common.CompareDouble(expectedrowList[rowIdx].Northing, actualrowList[rowIdx].Northing, "Northing", rowIdx);
        Common.CompareDouble(expectedrowList[rowIdx].Elevation, actualrowList[rowIdx].Elevation, "Elevation", rowIdx);
        Common.CompareDouble(expectedrowList[rowIdx].CMV, actualrowList[rowIdx].CMV, "CMV", rowIdx);
        Common.CompareDouble(expectedrowList[rowIdx].CutFill, actualrowList[rowIdx].CutFill, "CutFill", rowIdx);
        Common.CompareDouble(expectedrowList[rowIdx].MDP, actualrowList[rowIdx].MDP, "MDP", rowIdx);
        Common.CompareDouble(expectedrowList[rowIdx].PassCount, actualrowList[rowIdx].PassCount, "PassCount", rowIdx);
        Common.CompareDouble(expectedrowList[rowIdx].Temperature, actualrowList[rowIdx].Temperature, "Temperature", rowIdx);
      }
    }
  }
}