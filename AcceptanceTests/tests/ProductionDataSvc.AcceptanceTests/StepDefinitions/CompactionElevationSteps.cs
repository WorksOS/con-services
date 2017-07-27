
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionElevation")]
  public class CompactionElevationSteps
  {
    private Getter<ProjectStatistics> projectStatisticsRequester;
    private Getter<ElevationStatisticsResult> elevationRangeRequester;
    private string queryParameters = string.Empty;

    private string url;
    private string projectUid;

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"the Compaction Elevation Range service URI ""(.*)""")]
    public void GivenTheCompactionElevationRangeServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a startUtc ""(.*)"" and an EndUtc ""(.*)""")]
    public void GivenAStartUtcAndAnEndUtc(string startUtc, string endUtc)
    {
      queryParameters = string.Format("&startUtc={0}&endUtc={1}",
        startUtc, endUtc);
    }

    [When(@"I request Elevation Range")]
    public void WhenIRequestElevationRange()
    {
      elevationRangeRequester = Getter<ElevationStatisticsResult>.GetIt<ElevationStatisticsResult>(this.url, this.projectUid, this.queryParameters);
    }

    [Then(@"the Elevation Range result should be")]
    public void ThenTheElevationRangeResultShouldBe(string multilineText)
    {
      elevationRangeRequester.CompareIt<ElevationStatisticsResult>(multilineText);
    }

    [Given(@"the Compaction Project Statistics service URI ""(.*)""")]
    public void GivenTheCompactionProjectStatisticsServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [When(@"I request Project Statistics")]
    public void WhenIRequestProjectStatistics()
    {
      projectStatisticsRequester = Getter<ProjectStatistics>.GetIt<ProjectStatistics>(this.url, this.projectUid, this.queryParameters);
    }

    [Then(@"the Project Statistics result should be")]
    public void ThenTheProjectStatisticsResultShouldBe(string multilineText)
    {
      projectStatisticsRequester.CompareIt<ProjectStatistics>(multilineText);
    }
  }
}
