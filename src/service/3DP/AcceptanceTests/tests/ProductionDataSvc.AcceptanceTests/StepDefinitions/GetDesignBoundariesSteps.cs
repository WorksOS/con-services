using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("GetDesignBoundaries.feature")]
  public class GetDesignBoundariesSteps : FeatureGetRequestBase<JObject>
  { }
}
