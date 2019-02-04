using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("LineworkFile.feature")]
  public class LineworkFileSteps : FeaturePostRequestBase<JObject, GeoJson>
  { }
}
