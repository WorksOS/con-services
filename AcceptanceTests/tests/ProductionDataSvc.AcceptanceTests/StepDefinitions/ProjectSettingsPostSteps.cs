using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ProjectSettingsPost.feature")]
  public class ProjectSettingsPostSteps : FeaturePostRequestBase<ProjectSettingsRequest, ResponseBase>
  { }
}
