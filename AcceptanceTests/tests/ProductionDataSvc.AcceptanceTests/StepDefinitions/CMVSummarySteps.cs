using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "CMVSummary")]
    public class CMVSummarySteps
    {
        private Poster<CMVRequest, CMVSummaryResult> cmvSumValidatorPOST;
        private CMVSummaryResult result;

        [Given(@"the CMV Summary service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheCMVSummaryServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ReportSvcBaseUri + uri;
            cmvSumValidatorPOST = new Poster<CMVRequest, CMVSummaryResult>(uri, requestFile, resultFile);
        }

        [When(@"I request CMV Summary supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestCMVSummarySupplyingParamtersFromTheRepository(string paramName)
        {
            result = cmvSumValidatorPOST.DoValidRequest(paramName);
        }

        [When(@"I request CMV Summary supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestCMVSummarySupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            result = cmvSumValidatorPOST.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the CMV Summary response should match ""(.*)"" result from the repository")]
        public void ThenTheCMVSummaryResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(cmvSumValidatorPOST.ResponseRepo[resultName], result);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, result.Code);
        }
    }
}
