using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature="SummarySpeed")]
    public class SummarySpeedSteps
    {
        private Poster<SummarySpeedRequest, SummarySpeedResult> ssRequester;

        [Given(@"the Summary Speed service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheSummarySpeedServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            ssRequester = new Poster<SummarySpeedRequest, SummarySpeedResult>(RaptorClientConfig.ReportSvcBaseUri + uri, 
                requestFile, resultFile);
        }
        
        [When(@"I request Summary Speed supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestSummarySpeedSupplyingParamtersFromTheRepository(string paramName)
        {
            ssRequester.DoValidRequest(paramName);
        }
        
        [When(@"I request Summary Speed supplying ""(.*)"" paramters from the repository expecting BadRequest")]
        public void WhenIRequestSummarySpeedSupplyingParamtersFromTheRepositoryExpectingBadRequest(string paramName)
        {
            ssRequester.DoInvalidRequest(paramName);
        }
        
        [Then(@"the response should match ""(.*)"" result from the repository")]
        public void ThenTheResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(ssRequester.ResponseRepo[resultName], ssRequester.CurrentResponse);
        }

        [Then(@"the response body should contain Code (.*) and Message ""(.*)""")]
        public void ThenTheResponseBodyShouldContainCodeAndMessage(int expectedCode, string expectedMessage)
        {
            Assert.IsTrue(ssRequester.CurrentResponse.Code == expectedCode && ssRequester.CurrentResponse.Message == expectedMessage,
                string.Format("Expected code {0} and message {1}, but received {2} and {3} instead.",
                expectedCode, expectedMessage, ssRequester.CurrentResponse.Code, ssRequester.CurrentResponse.Message));
        }
    }
}
