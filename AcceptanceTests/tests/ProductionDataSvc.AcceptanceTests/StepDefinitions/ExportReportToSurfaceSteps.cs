using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ExportReportToSurface.feature")]
  public class ExportReportToSurfaceSteps : FeatureGetRequestBase
  {
    [Then(@"the export result should be of a minimum length")]
    public void ThenTheExportResultShouldSuccessful()
    {
      Assert.True(GetResponseHandler.ByteContent.Length > 100, " length of response should be > 100");
    }
  }
}
