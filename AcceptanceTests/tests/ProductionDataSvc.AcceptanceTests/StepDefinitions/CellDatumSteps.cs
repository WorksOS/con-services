using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding]
    public class CellDatumSteps
    {
        private Poster<CellDatumRequest, CellDatumResult> cellDatumRequester;

        [Given(@"the CellDatum service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheCellDatumServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            cellDatumRequester = new Poster<CellDatumRequest, CellDatumResult>(uri, requestFile, resultFile);
        }

        [When(@"I request Production Data Cell Datum supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestProductionDataCellDatumSupplyingParamtersFromTheRepository(string paramName)
        {
            cellDatumRequester.DoValidRequest(paramName);
        }

        [When(@"I request Cell Datum supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestCellDatumSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            cellDatumRequester.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, cellDatumRequester.CurrentResponse.Code);
        }

        [Then(@"the Production Data Cell Datum response should match ""(.*)"" result from the repository")]
        public void ThenTheProductionDataCellDatumResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(cellDatumRequester.ResponseRepo[resultName], cellDatumRequester.CurrentResponse);
        }
    }
}
