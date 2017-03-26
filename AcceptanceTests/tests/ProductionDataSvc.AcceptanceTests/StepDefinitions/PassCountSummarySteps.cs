using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "PassCountSummary")]
    public class PassCountSummarySteps
    {
        private Poster<SummaryPassCounts, PassCountSummaryResult> pcsValidator;
        private PassCountSummaryResult result;

        [Given(@"the Pass Count Summary service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenThePassCountSummaryServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ReportSvcBaseUri + uri;
            pcsValidator = new Poster<SummaryPassCounts, PassCountSummaryResult>(uri, requestFile, resultFile);
        }

        [When(@"I request Pass Count Summary supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestPassCountSummarySupplyingParamtersFromTheRepository(string paramName)
        {
            result = pcsValidator.DoValidRequest(paramName);
        }

        [Then(@"the Pass Count Summary response should match ""(.*)"" result from the repository")]
        public void ThenThePassCountSummaryResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(pcsValidator.ResponseRepo[resultName], result);
        }

        [When(@"I request Pass Count Summary supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestPassCountSummarySupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            result = pcsValidator.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, result.Code);
        }
    }
}
