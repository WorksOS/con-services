using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionSummaryVolumes.feature")]
  public class CompactionSummaryVolumesSteps : FeatureGetRequestBase<JObject>
  { }
}
