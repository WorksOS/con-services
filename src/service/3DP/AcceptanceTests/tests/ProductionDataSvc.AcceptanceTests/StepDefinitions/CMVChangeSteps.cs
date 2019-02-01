using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CMVChange.feature")]
  public class CMVChangeSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
