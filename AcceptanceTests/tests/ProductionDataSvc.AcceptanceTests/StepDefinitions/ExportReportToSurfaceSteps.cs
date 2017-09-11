using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System.Net;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ExportReportToSurface")]
  public sealed class ExportReportToSurfaceSteps
  {
    private string url;

    private Getter<ExportReportResult> exportReportRequester;

    [Given(@"the Export Report To Surface service URI ""(.*)"" and the result file ""(.*)""")]
    public void GivenTheExportReportToSurfaceServiceURIAndTheResultFile(string url, string resultFileName)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
      exportReportRequester = new Getter<ExportReportResult>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      exportReportRequester.QueryString.Add("ProjectUid", projectUid);
    }

    [Given(@"fileName is ""(.*)""")]
    public void GivenFileNameIs(string fileName)
    {
      exportReportRequester.QueryString.Add("fileName", fileName);
    }

    [Given(@"tolerance ""(.*)""")]
    public void GivenTolerance(string tolerance)
    {
      exportReportRequester.QueryString.Add("tolerance", tolerance);
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      exportReportRequester.QueryString.Add("filterUid", filterUid);
    }

    [When(@"I request an Export Report Machine Passes expecting NoContent")]
    public void WhenIRequestAnExportReportMachinePassesExpectingNoContent()
    {
      exportReportRequester.DoInvalidRequest(url, HttpStatusCode.NoContent);
    }

    [When(@"I request an Export Report To Surface")]
    public void WhenIRequestAnExportReportToSurface()
    {
      exportReportRequester.DoValidRequest(url);
    }

    [When(@"I request an Export Report To Surface expecting BadRequest")]
    public void WhenIRequestAnExportReportToSurfaceExpectingBadRequest()
    {
      exportReportRequester.DoInvalidRequest(url);
    }

    [When(@"I request an Export Report To Surface expecting Unauthorized")]
    public void WhenIRequestAnExportReportToSurfaceExpectingUnauthorized()
    {
      exportReportRequester.DoInvalidRequest(url, HttpStatusCode.Unauthorized);
    }

    [When(@"I request an Export Report To Surface expecting NoContent")]
    public void WhenIRequestAnExportReportToSurfaceExpectingNoContent()
    {
      exportReportRequester.DoInvalidRequest(url, HttpStatusCode.NoContent);
    }

    [Then(@"the report result should match the ""(.*)"" from the repository")]
    public void ThenTheReportResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(exportReportRequester.ResponseRepo[resultName], exportReportRequester.CurrentResponse);
    }

    [Then(@"the report result should contain error code (.*) and error message ""(.*)""")]
    public void ThenTheReportResultShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMessage)
    {
      Assert.IsTrue(exportReportRequester.CurrentResponse.Code == errorCode && (exportReportRequester.CurrentResponse.Message == errorMessage || exportReportRequester.CurrentResponse.Message.Contains(errorMessage)),
        string.Format("Expected to see code {0} and message {1}, but got {2} and {3} instead.",
          errorCode, errorMessage, exportReportRequester.CurrentResponse.Code, exportReportRequester.CurrentResponse.Message));
    }

    [Then(@"the export result should successful")]
    public void ThenTheExportResultShouldSuccessful()
    {
      Assert.AreEqual(0, exportReportRequester.CurrentResponse.Code, " Code should be 0");
      Assert.AreEqual("success", exportReportRequester.CurrentResponse.Message, " Message should be success");
      Assert.IsTrue(exportReportRequester.CurrentResponse.ExportData.Length > 100, " length of response should be > 100");
    }
  }
}
