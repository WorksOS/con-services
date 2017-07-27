using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System;
using System.Net;
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
        
    [Given(@"startUtc ""(.*)"" and endUtc ""(.*)""")]
    public void GivenStartUtcAndEndUtc(string startUtc, string endUtc)
    {
      exportReportRequester.QueryString.Add("startUtc", startUtc);
      exportReportRequester.QueryString.Add("endUtc", endUtc);
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
  }
}
