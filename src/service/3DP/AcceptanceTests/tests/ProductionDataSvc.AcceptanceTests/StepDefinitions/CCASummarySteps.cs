using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CCASummary.feature")]
  public class CCASummarySteps : FeaturePostRequestBase<CCARequest, ResponseBase>
  { }
}
