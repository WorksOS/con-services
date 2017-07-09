using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ExportReportToVETA")]
  public sealed class ExportReportToVETASteps
  {
    private string url;
    private string projectUid;
    private string queryParameters = string.Empty;

    private Getter<ExportReportResult> exportReportRequester;

    [Given(@"the Export Report To VETA service URI ""(.*)""")]
    public void GivenTheExportReportToVETAServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }
        
    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }
        
    [Given(@"startUtc ""(.*)"" and endUtc ""(.*)""")]
    public void GivenStartUtcAndEndUtc(string p0, string p1)
    {
        ScenarioContext.Current.Pending();
    }
        
    [Given(@"machineNames ""(.*)""")]
    public void GivenMachineNames(string p0)
    {
        ScenarioContext.Current.Pending();
    }
        
    [Given(@"fileName=Test")]
    public void GivenFileNameTest()
    {
        ScenarioContext.Current.Pending();
    }
        
    [When(@"I request an Export Report To VETA")]
    public void WhenIRequestAnExportReportToVETA()
    {
      //exportReportRequester = GetIt<CompactionCmvSummaryResult>();
    }
        
    [Then(@"the report result should be")]
    public void ThenTheReportResultShouldBe(string multilineText)
    {
        ScenarioContext.Current.Pending();
    }
  }
}
