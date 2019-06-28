using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionElevation.feature")]
  public class CompactionElevationSteps : FeatureGetRequestBase<JObject>
  { }
}
