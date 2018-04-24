using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System.Net;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "CoordinateSystemPostValidate")]
    public class CoordinateSystemPostValidateSteps
    {
        private Poster<CoordinateSystemFile, CoordinateSystemSettings> coordSysPoster;

        [Given(@"the Coordinate service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheCoordinateServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.CoordSvcBaseUri + uri;
            coordSysPoster = new Poster<CoordinateSystemFile, CoordinateSystemSettings>(uri, requestFile, resultFile);
        }

        [When(@"I Post CoordinateSystemValidation supplying ""(.*)"" paramters from the repository")]
        public void WhenIPostCoordinateSystemValidationSupplyingParamtersFromTheRepository(string paramName)
        {
            coordSysPoster.DoValidRequest(paramName);
        }

        [Then(@"the CoordinateSystemValidation response should match ""(.*)"" result from the repository")]
        public void ThenTheCoordinateSystemValidationResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(coordSysPoster.ResponseRepo[resultName], coordSysPoster.CurrentResponse);
        }

        [When(@"I Post CoordinateSystemValidation supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIPostCoordinateSystemValidationSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
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
