using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionPatch")]
  public class CompactionPatchSteps: BaseCompactionSteps
  {
    private Getter<JObject> requestHandler;
    
    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      requestHandler = new Getter<JObject>(url, resultFileName);
    }

    [When(@"I request result with expected status result ""(.*)""")]
    public void WhenIRequestResultWithExpectedStatusResult(HttpStatusCode expectedStatusCode)
    {
      requestHandler.DoValidRequest(url, expectedStatusCode);
    }

    [When(@"I request result with Accept header ""(.*)"" and expected status result ""(.*)""")]
    public void WhenIRequestResultWithAcceptHeaderAndExpectedStatusResult(string acceptHeaderValue, HttpStatusCode expectedStatusCode)
    {
      requestHandler.Send("application/x-protobuf", "application/json");
    }
    
    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      requestHandler.AddQueryParam("projectUid", projectUid);
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      requestHandler.AddQueryParam("filterUid", filterUid);
    }

    [Given(@"patchId ""(.*)""")]
    public void GivenPatchId(int patchId)
    {
      requestHandler.AddQueryParam("patchId", patchId.ToString());
    }

    [Given(@"mode ""(.*)""")]
    public void GivenMode(int mode)
    {
      requestHandler.AddQueryParam("mode", mode.ToString());
    }

    [Given(@"patchSize ""(.*)""")]
    public void GivenPatchSize(int patchSize)
    {
      requestHandler.AddQueryParam("patchSize", patchSize.ToString());
    }

    [Given(@"cellDownSample ""(.*)""")]
    public void GivenCellDownSample(int cellDownSample)
    {
      requestHandler.AddQueryParam("cellDownSample", cellDownSample.ToString());
    }

    [Then(@"the result should match the ""(.*)"" result from the repository")]
    public void ThenTheResponseShouldMatchTheResultFromTheRepository(string resultName)
    {
      var expectedResult = requestHandler.ResponseRepo[resultName];
      var actualResult = requestHandler.CurrentResponse;

      Assert.IsTrue(JToken.DeepEquals(expectedResult, actualResult));
    }

    [Then(@"the deserialized result should match the ""(.*)"" result from the repository")]
    public void ThenTheDeserializedResultShouldMatchTheResultFromTheRepository(string resultName)
    {
      var expectedResult = requestHandler.ResponseRepo[resultName];
      var expecteResultAsString = JsonConvert.SerializeObject(expectedResult);
      string actualResultAsString;

      using (var stream = requestHandler.HttpResponseMessage.Content.ReadAsStreamAsync().Result)
      {
        var protobufResult = ProtoBuf.Serializer.Deserialize<PatchResult>(stream);
        var actualResult = JObject.FromObject(protobufResult);

        actualResultAsString = JsonConvert.SerializeObject(actualResult);
      }

      // Deliberately compare using strings as JObject.DeepEquals() is incorrectly failing on the SubGrid property (cannot handle the number of array elements maybe?).
      Assert.AreEqual(expecteResultAsString, actualResultAsString);
    }
  }
}
