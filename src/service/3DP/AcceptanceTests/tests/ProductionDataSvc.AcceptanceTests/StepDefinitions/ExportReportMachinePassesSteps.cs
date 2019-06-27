using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ExportReportMachinePasses.feature")]
  public class ExportReportMachinePassesSteps : FeatureGetRequestBase<JObject>
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

      var expSorted = HelperUtilities.SortCsvFile(expectedResult).ToList();
      var actSorted = HelperUtilities.SortCsvFile(actualResult).ToList();

      HelperUtilities.CompareExportCsvFiles(actSorted, expSorted);
    }
  }
}
