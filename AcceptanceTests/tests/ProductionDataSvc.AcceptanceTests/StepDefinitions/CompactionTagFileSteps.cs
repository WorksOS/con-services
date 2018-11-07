using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionTagFile.feature")]
  public class CompactionTagFileSteps : FeaturePostRequestBase<CompactionTagFilePostParameter, ResponseBase>
  { }
}
