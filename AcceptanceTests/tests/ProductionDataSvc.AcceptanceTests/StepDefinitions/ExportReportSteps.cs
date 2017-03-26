using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature="ExportReport")]
    public class ExportReportSteps
    {
        private Poster<ExportReportRequest, ExportReportResult> exportReportRequester;

        [Given(@"the Export Report service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheExportReportServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            exportReportRequester = new Poster<ExportReportRequest, ExportReportResult>(RaptorClientConfig.ReportSvcBaseUri + uri,
                requestFile, resultFile);
        }
        
        [When(@"I request Export Report supplying ""(.*)"" from the request repository")]
        public void WhenIRequestExportReportSupplyingFromTheRequestRepository(string requestName)
        {
            exportReportRequester.DoValidRequest(requestName);
        }
        
        [When(@"I request Export Report supplying ""(.*)"" from the request repository expecting BadRequest")]
        public void WhenIRequestExportReportSupplyingFromTheRequestRepositoryExpectingBadRequest(string requestName)
        {
            exportReportRequester.DoInvalidRequest(requestName);
        }
        
        [Then(@"the result should match ""(.*)"" from the result repository")]
        public void ThenTheResultShouldMatchFromTheResultRepository(string resultName)
        {
            Assert.AreEqual(exportReportRequester.ResponseRepo[resultName], exportReportRequester.CurrentResponse);
        }
        
        [Then(@"the result should contain error code (.*) and error message ""(.*)""")]
        public void ThenTheResultShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMsg)
        {
            Assert.IsTrue(exportReportRequester.CurrentResponse.Code == errorCode && exportReportRequester.CurrentResponse.Message == errorMsg,
                string.Format("Expected to see code {0} and message {1}, but got {2} and {3} instead.",
                errorCode, errorMsg, exportReportRequester.CurrentResponse.Code, exportReportRequester.CurrentResponse.Message));
        }
    }
}
