using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("SummaryThickness.feature")]
  public class SummaryThicknessSteps : FeaturePostRequestBase<SummaryParametersBase, ResponseBase>
  { }
}
