using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
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
