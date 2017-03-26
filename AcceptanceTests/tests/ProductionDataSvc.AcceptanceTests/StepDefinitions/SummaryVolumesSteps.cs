using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "SummaryVolumes")]
    public class SummaryVolumesSteps
    {
        private Poster<SummaryVolumesParameters, SummaryVolumes> svRequester;

        [Given(@"the Summary Volumes service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheSummaryVolumesServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ReportSvcBaseUri + uri;
            svRequester = new Poster<SummaryVolumesParameters, SummaryVolumes>(uri, requestFile, resultFile);
        }

        [When(@"I request Summary Volumes supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestSummaryVolumesSupplyingParamtersFromTheRepository(string paramName)
        {
            svRequester.DoValidRequest(paramName);
        }

        [When(@"I request Summary Volumes supplying ""(.*)"" paramters from the repository expecting error http code (.*)")]
        public void WhenIRequestSummaryVolumesSupplyingParamtersFromTheRepositoryExpectingErrorHttpCode(string paramName, int httpCode)
        {
            svRequester.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the response body should contain Error Code (.*)")]
        public void ThenTheResponseBodyShouldContainErrorCode(int errorCode)
        {
            Assert.AreEqual(errorCode, svRequester.CurrentResponse.Code);
        }

        [Then(@"the response should match ""(.*)"" result from the repository")]
        public void ThenTheResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(svRequester.ResponseRepo[resultName], svRequester.CurrentResponse);
        }
    }
}
