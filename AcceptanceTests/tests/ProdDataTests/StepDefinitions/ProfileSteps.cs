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
    [Binding, Scope(Feature = "Profile")]
    public class ProfileSteps
    {
        private Poster<ProfileRequest, ProfileResult> profileRequester;

        [Given(@"the Profile service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheProfileServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            profileRequester = new Poster<ProfileRequest, ProfileResult>(uri, requestFile, resultFile);
        }

        [When(@"I request Profile supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestProfileSupplyingParamtersFromTheRepository(string paramName)
        {
            profileRequester.DoValidRequest(paramName);
        }

        [When(@"I request Profile supplying ""(.*)"" paramters from the repository expecting http error code (.*)")]
        public void WhenIRequestProfileSupplyingParamtersFromTheRepositoryExpectingHttpErrorCode(string paramName, int httpCode)
        {
            profileRequester.DoInvalidRequest(paramName, (HttpStatusCode)httpCode);
        }

        [Then(@"the Profile response should match ""(.*)"" result from the repository")]
        public void ThenTheProfileResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(profileRequester.ResponseRepo[resultName], profileRequester.CurrentResponse);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, profileRequester.CurrentResponse.Code);
        }
    }
}
