using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionDesignProfile")]
  public class CompactionDesignProfileSteps
  {
    private Getter<CompactionProfileResult<CompactionDesignProfileResult>> profileRequester;

    private string url;
    private string projectUid;
    private string queryParameters = string.Empty;

    [Given(@"the Compaction Profile service URI ""(.*)""")]
    public void GivenTheCompactionProfileServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }
    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a startLatDegrees ""(.*)"" and a startLonDegrees ""(.*)"" and an endLatDegrees ""(.*)"" And an endLonDegrees ""(.*)""")]
    public void GivenAStartLatDegreesAndAStartLonDegreesAndAnEndLatDegreesAndAnEndLonDegrees(Decimal startLatDegrees, Decimal startLonDegrees, Decimal endLatDegrees, Decimal endLonDegrees)
    {
      queryParameters =
        $"&startLatDegrees={startLatDegrees}&startLonDegrees={startLonDegrees}&endLatDegrees={endLatDegrees}&endLonDegrees={endLonDegrees}";
    }

    [Given(@"a importedFileUid ""(.*)""")]
    public void GivenAImportedFileUid(string importedFileUid)
    {
      queryParameters += $"&importedFileUid={importedFileUid}";
    }

    [When(@"I request a Compaction Design Profile")]
    public void WhenIRequestACompactionProfile()
    {
      profileRequester = Getter<CompactionProfileResult<CompactionDesignProfileResult>>.GetIt<CompactionProfileResult<CompactionDesignProfileResult>>(url, projectUid, queryParameters);
    }

    [Then(@"the Compaction Design Profile should be")]
    public void ThenTheCompactionProfileShouldBe(string multilineText)
    {
      profileRequester.CompareIt<CompactionProfileResult<CompactionDesignProfileResult>>(multilineText);
    }
  }
}