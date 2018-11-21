using ProductionDataSvc.AcceptanceTests.Helpers;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("SummaryVolumes.feature")]
  public class SummaryVolumesSteps : FeaturePostRequestBase<SummaryVolumesParameters, ResponseBase>
  {
    [And(@"I require surveyed surface")]
    public void AndIRequireSurveyedSurface()
    {
      BeforeAndAfter.CreateSurveyedSurface();
    }
  }
}
