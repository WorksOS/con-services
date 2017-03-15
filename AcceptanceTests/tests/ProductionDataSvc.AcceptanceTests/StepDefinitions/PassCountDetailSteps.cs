using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "PassCountDetail")]
    public class PassCountDetailSteps
    {
        private Poster<DetailedPassCounts, PassCountDetailedResult> pcdRequester;

        [Given(@"the Pass Count Detail service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenThePassCountDetailServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ReportSvcBaseUri + uri;
            pcdRequester = new Poster<DetailedPassCounts, PassCountDetailedResult>(uri, requestFile, resultFile);
        }

        [When(@"I request Pass Count Detail supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestPassCountDetailSupplyingParamtersFromTheRepository(string requestName)
        {
            pcdRequester.DoValidRequest(requestName);
        }

        [Then(@"the Pass Count Detail response should match ""(.*)"" result from the repository")]
        public void ThenThePassCountDetailResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(pcdRequester.ResponseRepo[resultName], pcdRequester.CurrentResponse);
        }

        [When(@"I request Pass Count Detail supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestPassCountDetailSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string requestName, int httpCode)
        {
            pcdRequester.DoInvalidRequest(requestName, (HttpStatusCode)httpCode);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, pcdRequester.CurrentResponse.Code);
        }

    }
}
