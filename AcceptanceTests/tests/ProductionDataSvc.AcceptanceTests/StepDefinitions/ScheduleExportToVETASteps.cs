using System;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ScheduleExportToVETA")]
  public class ScheduleExportToVETASteps
  {
    private string url;
    private string operation;

    private Getter<ScheduleResult> scheduleRequester;
    private Getter<RequestResult> statusRequester;
    private Getter<ExportReportResult> downloadRequester;


    // [Given(@"the Compaction service URI ""(.*)"" for operation ""(.*)""")]
    [Given(@"the Export Report To VETA service URI ""(.*)"" for operation ""(.*)"" and the result file ""(.*)""")]
    public void GivenTheExportReportToVETAServiceURIAndTheResultFile(string url, string operation, string resultFileName)
    {
      this.url = $"{RaptorClientConfig.CompactionSvcBaseUri}{url}/{operation}";
      this.operation = operation;
      switch (operation)
      {
        case "schedulejob": scheduleRequester = new Getter<ScheduleResult>(this.url, resultFileName);
          break;
        case "status":
          statusRequester = new Getter<RequestResult>(this.url, resultFileName);
          break;
        case "download":
          this.url += "test"; //alternative end point for testing. Returns ExportResult rather than FileStreamResult
          downloadRequester = new Getter<ExportReportResult>(this.url, resultFileName);
          break;
      }    
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "schedulejob":
          scheduleRequester.QueryString.Add("ProjectUid", projectUid);
          break;
        case "status":
          statusRequester.QueryString.Add("ProjectUid", projectUid);
          break;
        case "download":
          downloadRequester.QueryString.Add("ProjectUid", projectUid);
          break;
      }
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      scheduleRequester.QueryString.Add("filterUid", filterUid);
    }

    [Given(@"fileName ""(.*)""")]
    public void GivenFileName(string fileName)
    {
      scheduleRequester.QueryString.Add("fileName", fileName);
    }

    [Given(@"JobId ""(.*)""")]
    public void GivenJobId(string jobId)
    {
      switch (operation)
      {
          case "status":
            statusRequester.QueryString.Add("jobId", jobId);
            break;
        case "download":
          downloadRequester.QueryString.Add("jobId", jobId);
          break;
      }
    }

    [When(@"I request a Schedule Export To VETA")]
    public void WhenIRequestAScheduleExportToVETA()
    {
      scheduleRequester.DoValidRequest(url);
    }

    [When(@"I request a Export To VETA Status")]
    public void WhenIRequestAExportToVETAStatus()
    {
      statusRequester.DoValidRequest(url);
    }

    [When(@"I request a Export To VETA Download")]
    public void WhenIRequestAExportToVETADownload()
    {
      downloadRequester.DoValidRequest(url);
    }



    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "schedulejob":
          Assert.AreEqual(scheduleRequester.ResponseRepo[resultName], scheduleRequester.CurrentResponse);
          break;
        case "status":
          Assert.AreEqual(statusRequester.ResponseRepo[resultName], statusRequester.CurrentResponse);
          break;
        case "download":
          Assert.AreEqual(0, downloadRequester.CurrentResponse.Code);
          Assert.AreEqual("success", downloadRequester.CurrentResponse.Message);
          var actualResult = Encoding.Default.GetString(Common.Decompress(downloadRequester.CurrentResponse.ExportData));
          var expectedResult = Encoding.Default.GetString(Common.Decompress(downloadRequester.ResponseRepo[resultName].ExportData));
          var expSorted = Common.SortCsvFileIntoString(expectedResult.Substring(3));
          var actSorted = Common.SortCsvFileIntoString(actualResult.Substring(3));

          Assert.IsTrue(expSorted == actSorted, "Expected CSV file does not match actual");
          break;
      }
      
    }

    [When(@"I request an Export To VETA Status expecting InternalServerError")]
    public void WhenIRequestAnExportToVETAStatusExpectingInternalServerError()
    {
      statusRequester.DoInvalidRequest(url, HttpStatusCode.InternalServerError);
    }

    [When(@"I request an Export To VETA Download expecting InternalServerError")]
    public void WhenIRequestAnExportToVETADownloadExpectingInternalServerError()
    {
      downloadRequester.DoInvalidRequest(url, HttpStatusCode.InternalServerError);
    }

    [Then(@"the report result should contain error code (.*) and error message ""(.*)""")]
    public void ThenTheReportResultShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMessage)
    {
      int code = -1;
      string message = null;
      switch (operation)
      {
          case "status":
            code = statusRequester.CurrentResponse.Code;
            message = statusRequester.CurrentResponse.Message;
            break;
        case "download":
          code = downloadRequester.CurrentResponse.Code;
          message = downloadRequester.CurrentResponse.Message;
          break;
      }
      Assert.IsTrue(code == errorCode && (message == errorMessage || message.Contains(errorMessage)),
        $"Expected to see code {errorCode} and message {errorMessage}, but got {code} and {message} instead.");
    }
  }
}
