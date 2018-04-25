using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "CMVDetail")]
    public class CMVDetailSteps
    {
        private Poster<CMVRequest, CMVDetailedResult> cmvDetailValidatorPOST;
        private CMVDetailedResult result;

        [Given(@"the CMV Details service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheCMVDetailsServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ReportSvcBaseUri + uri;
            cmvDetailValidatorPOST = new Poster<CMVRequest, CMVDetailedResult>(uri, requestFile, resultFile);
        }

        [When(@"I request CMV Details supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestCMVDetailsSupplyingParamtersFromTheRepository(string paramName)
        {
            result = cmvDetailValidatorPOST.DoValidRequest(paramName);
        }

        [When(@"I request CMV Details supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestCMVDetailsSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            result = cmvDetailValidatorPOST.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the CMV Details response should match ""(.*)"" result from the repository")]
        public void ThenTheCMVDetailsResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(cmvDetailValidatorPOST.ResponseRepo[resultName], result);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, result.Code);
        }
    }
}
