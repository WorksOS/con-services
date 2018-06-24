using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionMachineLiftIds")]
  public class CompactionMachineLiftIdsSteps : BaseCompactionSteps
  {
    private Getter<JObject> requestHandler;

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      requestHandler = new Getter<JObject>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      url = url.Replace($"{{{nameof(projectUid)}}}", projectUid);
    }

    [When(@"I send the request with expected HTTP status code ""(.*)""")]
    public void WhenISendTheRequestWithExpectedHttpStatusCode(HttpStatusCode expectedStatusCode)
    {
      requestHandler.DoValidRequest(url, expectedStatusCode);
    }

    [Then(@"the result should match the ""(.*)"" result from the repository")]
    public void ThenTheResponseShouldMatchTheResultFromTheRepository(string resultName)
    {
      var expectedResult = requestHandler.ResponseRepo[resultName];
      var actualResult = requestHandler.CurrentResponse;
      Assert.IsTrue(JToken.DeepEquals(expectedResult, actualResult));
    }
  }
}
