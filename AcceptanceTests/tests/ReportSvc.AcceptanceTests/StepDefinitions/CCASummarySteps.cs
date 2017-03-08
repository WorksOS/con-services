using System;
using TechTalk.SpecFlow;
using ReportSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReportSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "CCASummary")]
    public class CCASummarySteps
    {
        private Poster<CCARequest, CCASummaryResult> ccaRequester;

        [Given(@"the CCA Summary service URI '(.*)', request repo '(.*)' and result repo '(.*)'")]
        public void GivenTheCCASummaryServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            ccaRequester = new Poster<CCARequest, CCASummaryResult>(RaptorClientConfig.ReportSvcBaseUri + uri,
                requestFile, resultFile);
        }
        
        [When(@"I request CCA Summary supplying '(.*)' paramters from the repository")]
        public void WhenIRequestCCASummarySupplyingParamtersFromTheRepository(string paramName)
        {
            ccaRequester.DoValidRequest(paramName);
        }
        
        [Then(@"the CCA Summary response should match '(.*)' result from the repository")]
        public void ThenTheCCASummaryResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(ccaRequester.ResponseRepo[resultName], ccaRequester.CurrentResponse);
        }
    }
}
