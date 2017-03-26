using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "CoordinateSystemPost")]
    public class CoordinateSystemPostSteps
    {
        private Poster<CoordinateSystemFile, CoordinateSystemSettings> coordSysPoster;

        [Given(@"the Coordinate service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheCoordinateServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.CoordSvcBaseUri + uri;
            coordSysPoster = new Poster<CoordinateSystemFile, CoordinateSystemSettings>(uri, requestFile, resultFile);
        }

        [When(@"I Post CoordinateSystem supplying ""(.*)"" paramters from the repository")]
        public void WhenIPostCoordinateSystemSupplyingParamtersFromTheRepository(string paramName)
        {
            coordSysPoster.DoValidRequest(paramName);
        }

        [Then(@"the CoordinateSystem response should match ""(.*)"" result from the repository")]
        public void ThenTheCoordinateSystemResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(coordSysPoster.ResponseRepo[resultName], coordSysPoster.CurrentResponse);
        }

        [When(@"I Post CoordinateSystem supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIPostCoordinateSystemSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            coordSysPoster.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, coordSysPoster.CurrentResponse.Code);
        }
    }
}
