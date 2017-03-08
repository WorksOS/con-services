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
using ReportSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReportSvc.AcceptanceTests.StepDefinitions
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
