using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("SummarySpeed.feature")]
  public class SummarySpeedSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
