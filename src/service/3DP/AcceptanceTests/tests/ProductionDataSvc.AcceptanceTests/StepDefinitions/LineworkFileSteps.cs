using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("LineworkFile.feature")]
  public class LineworkFileSteps : FeaturePostRequestBase<DxfFileRequest, GeoJson>
  { }
}
