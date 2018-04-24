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

    [When(@"I request a Schedule Export To VETA")]
    public void WhenIRequestAScheduleExportToVETA()
    {
      scheduleRequester.DoValidRequest(url);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "schedulejob":
          Assert.AreEqual(scheduleRequester.ResponseRepo[resultName], scheduleRequester.CurrentResponse);
          break;
      }
    }

  }
}
