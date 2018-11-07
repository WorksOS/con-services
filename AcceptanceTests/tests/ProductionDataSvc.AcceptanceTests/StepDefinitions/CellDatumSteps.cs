using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CellDatum.feature")]
  public sealed class CellDatumSteps : FeaturePostRequestBase<CellDatumRequest, ResponseBase>
  { }
}
