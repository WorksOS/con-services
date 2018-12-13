using System.Text;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ExportReportToVETA.feature")]
  public class ExportReportToVETASteps : FeatureGetRequestBase
  {
    /// <summary>
    /// This method unzips the csv file. It then sorts it before comparing the actual with expected.
    /// </summary>
    [Then(@"the report result csv should match the ""(.*)"" from the repository")]
    public void ThenTheReportResultCsvShouldMatchTheFromTheRepository(string resultName)
    {
      var expectedResultAsBinary = GetResponseHandler.ResponseRepo[resultName]["exportData"].ToObject<byte[]>();
      var expectedResult = Encoding.Default.GetString(HelperUtilities.Decompress(expectedResultAsBinary));
      var actualResult = Encoding.Default.GetString(HelperUtilities.Decompress(GetResponseHandler.ByteContent));
      
      var expSorted = HelperUtilities.SortCsvFileIntoString(expectedResult);
      var actSorted = HelperUtilities.SortCsvFileIntoString(actualResult);

      Assert.True(expSorted == actSorted, "Expected CSV file does not match actual");
    }
  }
}
