using System.Text;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("AlignmentLinework.feature")]
  public class AlignmentLineworkSteps : FeatureGetRequestBase
  {
    /// <summary>
    /// This method unzips the dxf file. 
    /// </summary>
    [Then(@"the report result dxf should match the ""(.*)"" from the repository")]
    public void ThenTheReportResultDxfShouldMatchTheFromTheRepository(string resultName)
    {
      //Note: to create the expected base64 string in the response repo, use Postman to 'Send and Download' a request and get the response as a zip file.
      //Then use an online encoder to get the Base64 string e.g. https://www.browserling.com/tools/file-to-base64
      var expectedResultAsBinary = GetResponseHandler.ResponseRepo[resultName]["dxfData"].ToObject<byte[]>();
      var expectedResult = Encoding.Default.GetString(HelperUtilities.Decompress(expectedResultAsBinary));
      var actualResult = Encoding.Default.GetString(HelperUtilities.Decompress(GetResponseHandler.ByteContent));
      Assert.True(expectedResult == actualResult, "Expected DXF file does not match actual");
    }
  }
}
