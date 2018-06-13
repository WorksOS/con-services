using TechTalk.SpecFlow;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using ProductionDataSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
   [Binding, Scope(Feature = "Patch")]
    public class PatchSteps
    {
        private Poster<PatchRequest, PatchResultStructured> patchRequester;

        [Given(@"the Patch service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenThePatchServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            patchRequester = new Poster<PatchRequest, PatchResultStructured>(uri, requestFile, resultFile);
        }

        [When(@"I request Production Data Patch supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestProductionDataPatchSupplyingParamtersFromTheRepository(string paramName)
        {
            patchRequester.DoValidRequest(paramName);
        }

        [When(@"I request Patch supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestPatchSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            patchRequester.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the Production Data Patch response should match ""(.*)"" result from the repository")]
        public void ThenTheProductionDataPatchResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(patchRequester.ResponseRepo[resultName], patchRequester.CurrentResponse);
        }

        [Then(@"the response cell should contain error code (.*)")]
        public void ThenTheResponseCellShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, patchRequester.CurrentResponse.Code);
        }
    }
}
