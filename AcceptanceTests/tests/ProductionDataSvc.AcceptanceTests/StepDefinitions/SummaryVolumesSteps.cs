using ProductionDataSvc.AcceptanceTests.Helpers;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("SummaryVolumes.feature")]
  public class SummaryVolumesSteps : FeaturePostRequestBase<SummaryVolumesParameters, ResponseBase>
  {
    [Given(@"I require surveyed surface")]
    public void GivenIRequireSurveyedSurface()
    {
      BeforeAndAfter.CreateSurveyedSurface();
    }

    [Given(@"I require old surveyed surface")]
    public void GivenIRequireOldSurveyedSurface()
    {
      BeforeAndAfter.CreateOldSurveyedSurface();
    }
  }
}
