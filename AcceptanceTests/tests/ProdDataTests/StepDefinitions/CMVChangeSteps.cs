using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature="CMVChange")]
    public class CMVChangeSteps
    {
        private Poster<CMVChangeSummaryRequest, CMVChangeSummaryResult> cmvChangeRequester;

        [Given(@"the CMV change summary service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheCMVChangeSummaryServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            cmvChangeRequester = new Poster<CMVChangeSummaryRequest, CMVChangeSummaryResult>(RaptorClientConfig.ReportSvcBaseUri + uri, 
                requestFile, resultFile);
        }
        
        [When(@"I request CMV change summary supplying ""(.*)"" from the request repository")]
        public void WhenIRequestCMVChangeSummarySupplyingFromTheRequestRepository(string requestName)
        {
            cmvChangeRequester.DoValidRequest(requestName);
        }
        
        [When(@"I request CMV change summary supplying ""(.*)"" from the request repository expecting BadRequest")]
        public void WhenIRequestCMVChangeSummarySupplyingFromTheRequestRepositoryExpectingBadRequest(string requestName)
        {
            cmvChangeRequester.DoInvalidRequest(requestName);
        }
        
        [Then(@"the result should match ""(.*)"" from the result repository")]
        public void ThenTheResultShouldMatchFromTheResultRepository(string resultName)
        {
            Assert.AreEqual(cmvChangeRequester.ResponseRepo[resultName], cmvChangeRequester.CurrentResponse);
        }
        
        [Then(@"the reuslt should contain error code (.*) and error message ""(.*)""")]
        public void ThenTheReusltShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMsg)
        {
            Assert.IsTrue(cmvChangeRequester.CurrentResponse.Code == errorCode && cmvChangeRequester.CurrentResponse.Message == errorMsg,
                string.Format("Expected to see code {0} and message {1}, but got {2} and {3} instead.", 
                errorCode, errorMsg, cmvChangeRequester.CurrentResponse.Code, cmvChangeRequester.CurrentResponse.Message));
        }
    }
}
