using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CSIB.feature")]
  public class CSIBSteps : FeatureGetRequestBase<JObject>
  { }
}
