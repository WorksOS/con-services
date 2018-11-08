using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CellPasses.feature")]
  public class CellPassesSteps : FeaturePostRequestBase<CellPassesRequest, ResponseBase>
  { }
}
