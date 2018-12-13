using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CellDatum.feature")]
  public class CellDatumSteps : FeaturePostRequestBase<CellDatumRequest, ResponseBase>
  { }
}
