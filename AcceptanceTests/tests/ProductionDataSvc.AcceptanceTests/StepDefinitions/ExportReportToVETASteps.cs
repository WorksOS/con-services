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


    [Then(@"the report result should match the ""(.*)"" from the repository")]
    public void ThenTheReportResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(exportReportRequester.ResponseRepo[resultName], exportReportRequester.CurrentResponse);
    }

  }
}
