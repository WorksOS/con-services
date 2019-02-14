using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ProjectSettingsPost.feature")]
  public class ProjectSettingsPostSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
