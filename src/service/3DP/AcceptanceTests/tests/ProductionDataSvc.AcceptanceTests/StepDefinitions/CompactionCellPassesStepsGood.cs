using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionCellPasses.feature")]
  public class CompactionCellPassesGoodSteps : FeatureGetRequestBase<JArray>
  { }
}
