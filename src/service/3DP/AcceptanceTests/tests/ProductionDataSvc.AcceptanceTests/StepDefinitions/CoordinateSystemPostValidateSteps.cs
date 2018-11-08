using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CoordinateSystemPostValidate.feature")]
  public class CoordinateSystemPostValidateSteps : FeaturePostRequestBase<CoordinateSystemFile, ResponseBase>
  { }
}
