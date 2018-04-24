using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "GetDesignBoundaries")]
  public sealed class GetDesignBoundariesSteps
  {
    private string url;

    private Getter<DesignResult> designBoundariesRequester;

    [Given(@"the Get Machine Boundaries service URI ""(.*)"" and the result file ""(.*)""")]
    public void GivenTheGetMachineBoundariesServiceURIAndTheResultFile(string url, string resultFileName)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
      designBoundariesRequester = new Getter<DesignResult>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      designBoundariesRequester.QueryString.Add("ProjectUid", projectUid);
    }
        
    [Given(@"tolerance ""(.*)""")]
    public void GivenTolerance(string tolerance)
    {
      designBoundariesRequester.QueryString.Add("tolerance", tolerance);
    }
        
    [When(@"I request design boundaries")]
    public void WhenIRequestDesignBoundaries()
    {
      designBoundariesRequester.DoValidRequest(url);
    }

    /*
    [When(@"I request design boundaries expecting BadRequest")]
    public void WhenIRequestDesignBoundariesExpectingBadRequest()
    {
      designBoundariesRequester.DoInvalidRequest(url);
    }
    */

    [When(@"I request design boundaries expecting BadRequest Unauthorized")]
    public void WhenIRequestDesignBoundariesExpectingBadRequestUnauthorized()
    {
      designBoundariesRequester.DoInvalidRequest(url, HttpStatusCode.Unauthorized);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(designBoundariesRequester.ResponseRepo[resultName], designBoundariesRequester.CurrentResponse);
    }

    [Then(@"the GetDesignBoundaries result should contain error code (.*) and error message ""(.*)""")]
    public void ThenTheGetDesignBoundariesResultShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMessage)
    {
      Assert.IsTrue(designBoundariesRequester.CurrentResponse.Code == errorCode && (designBoundariesRequester.CurrentResponse.Message == errorMessage || designBoundariesRequester.CurrentResponse.Message.Contains(errorMessage)),
        string.Format("Expected to see code {0} and message {1}, but got {2} and {3} instead.",
          errorCode, errorMessage, designBoundariesRequester.CurrentResponse.Code, designBoundariesRequester.CurrentResponse.Message));
    }
  }
}
