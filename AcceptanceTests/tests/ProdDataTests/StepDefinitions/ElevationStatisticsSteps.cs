using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "ElevationStatistics")]
    public class ElevationStatisticsSteps
    {
        private Poster<ElevationStatisticsRequest, ElevationStatisticsResult> elevStatsValidatorPOST;
        private ElevationStatisticsResult result;

        [Given(@"the ElevationStatistics service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheElevationStatisticsServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ReportSvcBaseUri + uri;
            elevStatsValidatorPOST = new Poster<ElevationStatisticsRequest, ElevationStatisticsResult>(uri, requestFile, resultFile);
        }
        
        [When(@"I request Elevation Statistics supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestElevationStatisticsSupplyingParamtersFromTheRepository(string paramName)
        {
            result = elevStatsValidatorPOST.DoValidRequest(paramName);
        }
        
        [When(@"I request Elevation Statistics supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestElevationStatisticsSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            result = elevStatsValidatorPOST.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }
        
        [Then(@"the Elevation Statistics response should match ""(.*)"" result from the repository")]
        public void ThenTheElevationStatisticsResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(elevStatsValidatorPOST.ResponseRepo[resultName], result);
        }
        
        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, result.Code);
        }
    }
}
