using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ExportReportMachinePasses")]
  public sealed class ExportReportMachinePassesSteps
  {
    private string url;

    private Getter<ExportReportResult> exportReportRequester;

    [Given(@"the Machine Passes Export Report service URI ""(.*)"" and the result file ""(.*)""")]
    public void GivenTheMachinePassesExportReportServiceURIAndTheResultFile(string url, string resultFileName)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
      exportReportRequester = new Getter<ExportReportResult>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      exportReportRequester.QueryString.Add("ProjectUid", projectUid);
    }
        
    [Given(@"startUtc ""(.*)"" and endUtc ""(.*)""")]
    public void GivenStartUtcAndEndUtc(string startUtc, string endUtc)
    {
      exportReportRequester.QueryString.Add("startUtc", startUtc);
      exportReportRequester.QueryString.Add("endUtc", endUtc);
    }

    [Given(@"coordType ""(.*)""")]
    public void GivenCoordType(int coordType)
    {
      exportReportRequester.QueryString.Add("coordType", coordType.ToString());
    }
        
    [Given(@"outputType ""(.*)""")]
    public void GivenOutputType(int outputType)
    {
      exportReportRequester.QueryString.Add("outputType", outputType.ToString());
    }
        
    [Given(@"restrictOutput ""(.*)""")]
    public void GivenRestrictOutput(string restrictOutput)
    {
      exportReportRequester.QueryString.Add("restrictOutput", restrictOutput);
    }
        
    [Given(@"rawDataOutput ""(.*)""")]
    public void GivenRawDataOutput(string rawDataOutput)
    {
      exportReportRequester.QueryString.Add("rawDataOutput", rawDataOutput);
    }
        
    [Given(@"fileName is ""(.*)""")]
    public void GivenFileNameIs(string fileName)
    {
      exportReportRequester.QueryString.Add("fileName", fileName);
    }
        
    [When(@"I request an Export Report Machine Passes")]
    public void WhenIRequestAnExportReportMachinePasses()
    {
      exportReportRequester.DoValidRequest(url);
    }

    [When(@"I request an Export Report Machine Passes expecting BadRequest")]
    public void WhenIRequestAnExportReportMachinePassesExpectingBadRequest()
    {
      exportReportRequester.DoInvalidRequest(url);
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
