using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionSummaryVolumes")]
  public class CompactionSummaryVolumesSteps
  {
    private Getter<CompactionVolumesSummaryResult> volumesSummaryRequester;
    private string url;

    [Given(@"the Compaction service URI ""(.*)""")]
    public void GivenTheCompactionServiceURI(string uri)
    {
      url = RaptorClientConfig.CompactionSvcBaseUri + uri;
    }

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      volumesSummaryRequester = new Getter<CompactionVolumesSummaryResult>(url, resultFileName);
    }

    [Given(@"project ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      volumesSummaryRequester.QueryString.Add("ProjectUid", projectUid);
    }

    [When(@"I request result ""(.*)""")]
    public void WhenIRequestResult(int httpCode)
    {
      volumesSummaryRequester.DoValidRequest(url, (HttpStatusCode)httpCode);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(volumesSummaryRequester.ResponseRepo[resultName], volumesSummaryRequester.CurrentResponse);
    }

    [Then(@"the response should contain error code (.*)")]
    public void ThenTheResponseShouldContainErrorCode(int httpCode)
    {
      this.volumesSummaryRequester.DoInvalidRequest(url, (HttpStatusCode)httpCode);
    }

    [Given(@"filter ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      if (!string.IsNullOrEmpty(filterUid))
      { volumesSummaryRequester.QueryString.Add("baseUid", filterUid); }
    }

    [Given(@"design ""(.*)""")]
    public void GivenDesign(string designUid)
    {
      if (!string.IsNullOrEmpty(designUid))
      { volumesSummaryRequester.QueryString.Add("topUid", designUid); }
    }

    [Given(@"to filter ""(.*)""")]
    public void GivenToFilter(string filterUid)
    {
      if (!string.IsNullOrEmpty(filterUid))
      { volumesSummaryRequester.QueryString.Add("topUid", filterUid); }
    }
  }
}