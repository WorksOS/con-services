using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("Profile.feature")]
  public class ProfileSteps : FeaturePostRequestBase<ProfileRequest, ResponseBase>
  { }
}
