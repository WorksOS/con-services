using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("GeoJson.feature")]
  public class GeoJsonSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
