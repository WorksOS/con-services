using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "ExportGriddedCSV")]
    class ExportGriddedCSVSteps
    {
        private Poster<ExportGriddedCSVRequest, ExportGriddedCSVResult> exportGriddedCSVRequester;

        [Given(@"the Export Gridded CSV service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheExportReportServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            exportGriddedCSVRequester = new Poster<ExportGriddedCSVRequest, ExportGriddedCSVResult>(RaptorClientConfig.ReportSvcBaseUri + uri,
                requestFile, resultFile);
        }

        [When(@"I request Export Gridded CSV supplying ""(.*)"" from the request repository")]
        public void WhenIRequestExportReportSupplyingFromTheRequestRepository(string requestName)
        {
            exportGriddedCSVRequester.DoValidRequest(requestName);
        }

        [When(@"I request Export Gridded CSV supplying ""(.*)"" from the request repository expecting BadRequest")]
        public void WhenIRequestExportReportSupplyingFromTheRequestRepositoryExpectingBadRequest(string requestName)
        {
            exportGriddedCSVRequester.DoInvalidRequest(requestName);
        }

        [Then(@"the result should match ""(.*)"" from the result repository")]
        public void ThenTheResultShouldMatchFromTheResultRepository(string resultName)
        {
            Assert.AreEqual(exportGriddedCSVRequester.ResponseRepo[resultName], exportGriddedCSVRequester.CurrentResponse);
        }

        [Then(@"the result should contain error code (.*) and error message ""(.*)""")]
        public void ThenTheResultShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMsg)
        {
            Assert.IsTrue(exportGriddedCSVRequester.CurrentResponse.Code == errorCode && exportGriddedCSVRequester.CurrentResponse.Message == errorMsg,
                string.Format("Expected to see code {0} and message {1}, but got {2} and {3} instead.",
                errorCode, errorMsg, exportGriddedCSVRequester.CurrentResponse.Code, exportGriddedCSVRequester.CurrentResponse.Message));
        }
    }
}
