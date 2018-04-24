using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ExportReport")]
  public class ExportReportSteps
  {
    private Poster<ExportReportRequest, ExportReportResult> exportReportRequester;

    [Given(@"the Export Report service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
    public void GivenTheExportReportServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
    {
      exportReportRequester = new Poster<ExportReportRequest, ExportReportResult>(RaptorClientConfig.ReportSvcBaseUri + uri,
          requestFile, resultFile);
    }

    [When(@"I request Export Report supplying ""(.*)"" from the request repository")]
    public void WhenIRequestExportReportSupplyingFromTheRequestRepository(string requestName)
    {
      exportReportRequester.DoValidRequest(requestName);
    }

    [When(@"I request Export Report supplying ""(.*)"" from the request repository expecting NoContent")]
    public void WhenIRequestExportReportSupplyingFromTheRequestRepositoryExpectingNoContent(string requestName)
    {
      exportReportRequester.DoInvalidRequest(requestName, HttpStatusCode.NoContent);
    }

    [When(@"I request Export Report supplying ""(.*)"" from the request repository expecting BadRequest")]
    public void WhenIRequestExportReportSupplyingFromTheRequestRepositoryExpectingBadRequest(string requestName)
    {
      exportReportRequester.DoInvalidRequest(requestName, HttpStatusCode.BadRequest);
    }

    [Then(@"the result should match ""(.*)"" from the result repository")]
    public void ThenTheResultShouldMatchFromTheResultRepository(string resultName)
    {
      Assert.AreEqual(exportReportRequester.ResponseRepo[resultName], exportReportRequester.CurrentResponse);
    }

    [Then(@"the result should contain error code (.*) and error message ""(.*)""")]
    public void ThenTheResultShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMsg)
    {
      Assert.IsTrue(exportReportRequester.CurrentResponse.Code == errorCode && exportReportRequester.CurrentResponse.Message == errorMsg,
        $"Expected to see code {errorCode} and message {errorMsg}, but got {exportReportRequester.CurrentResponse.Code} and {exportReportRequester.CurrentResponse.Message} instead.");
    }
  }
}