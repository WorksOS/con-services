using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  public class CompactionSummaryVolumesSteps
  {
    [Given(@"the Summary Volumes service URI ""(.*)""")]
    public void GivenTheSummaryVolumesServiceURI(string p0)
    {
      ScenarioContext.Current.Pending();
    }

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string p0)
    {
      ScenarioContext.Current.Pending();
    }

  }
}