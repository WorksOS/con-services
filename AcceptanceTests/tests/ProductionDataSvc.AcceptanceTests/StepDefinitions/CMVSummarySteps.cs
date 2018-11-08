using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CMVSummary.feature")]
  public class CMVSummarySteps : FeaturePostRequestBase<CMVRequest, ResponseBase>
  { }
}
