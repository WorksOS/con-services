using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ExportGriddedCSV.feature")]
  public class ExportGriddedCSVSteps : FeaturePostRequestBase<ExportGriddedCSVRequest, ResponseBase>
  { }
}
