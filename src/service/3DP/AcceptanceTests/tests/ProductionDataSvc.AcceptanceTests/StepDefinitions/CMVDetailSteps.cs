using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CMVDetail.feature")]
  public class CMVDetailSteps : FeaturePostRequestBase<CMVRequest, ResponseBase>
  { }
}
