using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("PassCountSummary.feature")]
  public class PassCountSummarySteps : FeaturePostRequestBase<PassCounts, ResponseBase>
  { }
}
