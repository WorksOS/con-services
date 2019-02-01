using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("PassCountSummary.feature")]
  public class PassCountSummarySteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
