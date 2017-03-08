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
    [Binding, Scope(Feature = "CMVSummary")]
    public class CMVSummarySteps
    {
        private Poster<CMVRequest, CMVSummaryResult> cmvSumValidatorPOST;
        private CMVSummaryResult result;

        [Given(@"the CMV Summary service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheCMVSummaryServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ReportSvcBaseUri + uri;
            cmvSumValidatorPOST = new Poster<CMVRequest, CMVSummaryResult>(uri, requestFile, resultFile);
        }

        [When(@"I request CMV Summary supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestCMVSummarySupplyingParamtersFromTheRepository(string paramName)
        {
            result = cmvSumValidatorPOST.DoValidRequest(paramName);
        }

        [When(@"I request CMV Summary supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestCMVSummarySupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            result = cmvSumValidatorPOST.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the CMV Summary response should match ""(.*)"" result from the repository")]
        public void ThenTheCMVSummaryResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(cmvSumValidatorPOST.ResponseRepo[resultName], result);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, result.Code);
        }
    }
}
