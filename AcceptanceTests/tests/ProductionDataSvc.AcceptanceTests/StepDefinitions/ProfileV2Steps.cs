using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding]
  public class ProfileV2Steps
  {
    private string url;
    private Getter<ProfileV2Result> requester;

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
        requester.QueryString.Add("ProjectUid", projectUid);
    }

    [Given(@"the Profile service URI ""(.*)""")]
    public void GivenTheProfileServiceURI(string uri)
    {
      url = RaptorClientConfig.ProdSvcBaseUri + uri;
      requester = new Getter<ProfileV2Result>(url);
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      requester.DoValidRequest(url);
    }


    [When(@"I request Profile supplying ""(.*)"" parameters from the repository expecting http error code (.*)")]
    public void WhenIRequestProfileSupplyingParametersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
    {
      requester.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
    }

    [Then(@"the response should contain error code (.*)")]
    public void ThenTheResponseShouldContainErrorCode(int expectedCode)
    {
      Assert.AreEqual(expectedCode, requester.CurrentResponse.Code);
    }
  }
}