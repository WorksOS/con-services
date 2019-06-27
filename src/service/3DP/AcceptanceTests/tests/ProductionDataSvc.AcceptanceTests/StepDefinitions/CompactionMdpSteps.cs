using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionMdp.feature")]
  public class CompactionMdpSteps : FeatureGetRequestBase<JObject>
  { }
}
