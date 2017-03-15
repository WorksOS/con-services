using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "SummaryThickness")]
    public class SummaryThicknessSteps
    {
        private Poster<SummaryParametersBase, SummaryThicknessResult> stRequester;

        [Given(@"the Summary Thickness service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheSummaryThicknessServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            stRequester = new Poster<SummaryParametersBase, SummaryThicknessResult>(RaptorClientConfig.ReportSvcBaseUri + uri, requestFile, resultFile);
        }

        [When(@"I request Summary Thickness supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestSummaryThicknessSupplyingParamtersFromTheRepository(string paramName)
        {
            stRequester.DoValidRequest(paramName);
        }

        [Then(@"the response should match ""(.*)"" result from the repository")]
        public void ThenTheResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(stRequester.ResponseRepo[resultName], stRequester.CurrentResponse);
        }

        [When(@"I make invalid request for Summary Thickness supplying ""(.*)"" paramters from the repository")]
        public void WhenIMakeInvalidRequestForSummaryThicknessSupplyingParamtersFromTheRepository(string paramName)
        {
            stRequester.DoInvalidRequest(paramName);
        }

        [Then(@"the response body should contain Code (.*) and Message ""(.*)""")]
        public void ThenTheResponseBodyShouldContainCodeAndMessage(int expectedCode, string expectedMessage)
        {
            Assert.IsTrue(stRequester.CurrentResponse.Code == expectedCode && stRequester.CurrentResponse.Message == expectedMessage,
                string.Format("Expected code {0} and message {1}, but received {2} and {3} instead.",
                expectedCode, expectedMessage, stRequester.CurrentResponse.Code, stRequester.CurrentResponse.Message));
        }
    }
}
