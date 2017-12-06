using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System.Net;
using System.Text;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ExportReportToVETA")]
  public sealed class ExportReportToVETASteps
  {
    private string url;

    private Getter<ExportReportResult> exportReportRequester;


    [Given(@"the Export Report To VETA service URI ""(.*)"" and the result file ""(.*)""")]
    public void GivenTheExportReportToVETAServiceURIAndTheResultFile(string url, string resultFileName)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
      exportReportRequester = new Getter<ExportReportResult>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      //this.projectUid = projectUid;
      exportReportRequester.QueryString.Add("ProjectUid", projectUid);
    }

    [Given(@"machineNames ""(.*)""")]
    public void GivenMachineNames(string machineNames)
    {
      exportReportRequester.QueryString.Add("machineNames", machineNames);
    }

    [Given(@"fileName is ""(.*)""")]
    public void GivenFileNameIs(string fileName)
    {
      exportReportRequester.QueryString.Add("fileName", fileName);
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      exportReportRequester.QueryString.Add("filterUid", filterUid);
    }

    [When(@"I request an Export Report To VETA")]
    public void WhenIRequestAnExportReportToVETA()
    {
      exportReportRequester.DoValidRequest(url);
    }

    [When(@"I request an Export Report To VETA expecting BadRequest")]
    public void WhenIRequestAnExportReportToVETAExpectingBadRequest()
    {
      exportReportRequester.DoInvalidRequest(url);
    }

    [When(@"I request an Export Report To VETA expecting Unauthorized")]
    public void WhenIRequestAnExportReportToVETAExpectingUnauthorized()
    {
      exportReportRequester.DoInvalidRequest(url, HttpStatusCode.Unauthorized);
    }

    [When(@"I request an Export Report To VETA expecting NoContent")]
    public void WhenIRequestAnExportReportToVETAExpectingNoContent()
    {
      exportReportRequester.DoInvalidRequest(url, HttpStatusCode.NoContent);
    }

    //[Then(@"the report result should match the ""(.*)"" from the repository")]
    //public void ThenTheReportResultShouldMatchTheFromTheRepository(string resultName)
    //{
    //  Assert.AreEqual(exportReportRequester.ResponseRepo[resultName], exportReportRequester.CurrentResponse);
    //}

    [Then(@"the report result csv should match the ""(.*)"" from the repository")]
    public void ThenTheReportResultCsvShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(0, exportReportRequester.CurrentResponse.Code);
      Assert.AreEqual("success", exportReportRequester.CurrentResponse.Message);
      var actualResult = Encoding.Default.GetString(Common.Decompress(exportReportRequester.CurrentResponse.ExportData));
      var expectedResult = Encoding.Default.GetString(Common.Decompress(exportReportRequester.ResponseRepo[resultName].ExportData));
      var expSorted = Common.SortCsvFileIntoString(expectedResult.Substring(3));
      var actSorted = Common.SortCsvFileIntoString(actualResult.Substring(3));

      Assert.IsTrue(expSorted == actSorted, "Expected CSV file does not match actual");
    }


    [Then(@"the report result should contain error code (.*) and error message ""(.*)""")]
    public void ThenTheReportResultShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMessage)
    {
      Assert.IsTrue(exportReportRequester.CurrentResponse.Code == errorCode && (exportReportRequester.CurrentResponse.Message == errorMessage || exportReportRequester.CurrentResponse.Message.Contains(errorMessage)),
        $"Expected to see code {errorCode} and message {errorMessage}, but got {exportReportRequester.CurrentResponse.Code} and {exportReportRequester.CurrentResponse.Message} instead.");
    }
  }
}