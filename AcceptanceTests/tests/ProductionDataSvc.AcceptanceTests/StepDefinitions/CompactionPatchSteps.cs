using System.IO;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using ProtoBuf;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionPatch.feature")]
  public class CompactionPatchSteps: FeatureGetRequestBase
  {
    [Then(@"the deserialized result should match the ""(.*)"" result from the repository")]
    public void ThenTheDeserializedResultShouldMatchTheResultFromTheRepository(string resultName)
    {
      var expectedResult = GetResponseHandler.ResponseRepo[resultName];
      var expectedResultAsString = JsonConvert.SerializeObject(expectedResult);
      string actualResultAsString;

      using (var stream = new MemoryStream(GetResponseHandler.ByteContent))  
      {  
        var binaryResult = Serializer.Deserialize<PatchResult>(stream);  
        actualResultAsString = JsonConvert.SerializeObject(binaryResult);
      }

      // Deliberately compare using strings as JObject.DeepEquals() is incorrectly failing on the SubGrid property (cannot handle the number of array elements maybe?).
      // Also strings in error are easier to read in the test output.
      Assert.Equal(expectedResultAsString, actualResultAsString);
    }
  }
}
