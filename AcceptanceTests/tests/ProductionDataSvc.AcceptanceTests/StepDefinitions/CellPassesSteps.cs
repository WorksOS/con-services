using System;
using System.IO;
using System.Reflection;
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
    [Binding, Scope(Feature = "CellPasses")]
    public class CellPassesSteps
    {
        private Poster<CellPassesRequest, CellPassesResult> cellPassRequester;

        [Given(@"the CellPass service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheCellPassServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            cellPassRequester = new Poster<CellPassesRequest, CellPassesResult>(uri, requestFile, resultFile);
        }

        [When(@"I request Production Data Cell Passes supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestProductionDataCellPassesSupplyingParamtersFromTheRepository(string paramName)
        {
            cellPassRequester.DoValidRequest(paramName);
        }

        [When(@"I request Cell Passes supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestCellPassesSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            cellPassRequester.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, cellPassRequester.CurrentResponse.Code);
        }

        [Then(@"the Production Data Cell Passes response should match ""(.*)"" result from the repository")]
        public void ThenTheProductionDataCellPassesResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(cellPassRequester.ResponseRepo[resultName], cellPassRequester.CurrentResponse);
        }
    }
}
