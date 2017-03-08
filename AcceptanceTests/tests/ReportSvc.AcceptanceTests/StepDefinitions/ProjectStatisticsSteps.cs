using System;
using TechTalk.SpecFlow;
using System.Net;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using ReportSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReportSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "ProjectStatistics")]
    public class ProjectStatisticsSteps
    {
        private Poster<StatisticsParameters, ProjectStatistics> prjStatsRequester;

        [Given(@"the Project Stats service URI ""(.*)""")]
        public void GivenTheProjectStatsServiceURI(string uri)
        {
            uri = RaptorClientConfig.ReportSvcBaseUri + uri;
            prjStatsRequester = new Poster<StatisticsParameters, ProjectStatistics>(uri);
        }

        [Given(@"a Project Statistics project id (.*)")]
        public void GivenAProjectStatisticsProjectId(long pId)
        {
            prjStatsRequester.CurrentRequest = new StatisticsParameters() { projectId = pId, excludedSurveyedSurfaceIds = new long[] {} };
        }

        [Given(@"I decide to exclude all surveyed surfaces")]
        public void GivenIDecideToExcludeAllSurveyedSurfaces()
        {
            // 111 is always the dummy ID specified when a surveyed surface is created
            prjStatsRequester.CurrentRequest.excludedSurveyedSurfaceIds = new long[] { 111 };
        }

        [When(@"I request the project statistics")]
        public void WhenIRequestTheProjectStatistics()
        {
            prjStatsRequester.DoValidRequest();
        }

        [When(@"I request the project statistics expecting BadRequest")]
        public void WhenIRequestTheProjectStatisticsExpectingBadRequest()
        {
            prjStatsRequester.DoInvalidRequest();
        }

        [Then(@"I should get the following project statistics")]
        public void ThenIShouldGetTheFollowingProjectStatistics(Table projectStats)
        {
            ProjectStatistics expectedResult = new ProjectStatistics();

            foreach (var row in projectStats.Rows)
            {
                expectedResult.startTime = Convert.ToDateTime(row["startTime"]);
                expectedResult.endTime = Convert.ToDateTime(row["endTime"]);
                expectedResult.cellSize = Convert.ToDouble(row["cellSize"]);
                expectedResult.indexOriginOffset = Convert.ToInt32(row["indexOriginOffset"]);
                expectedResult.extents = new BoundingBox3DGrid()
                {
                    maxX = Convert.ToDouble(row["maxX"]),
                    maxY = Convert.ToDouble(row["maxY"]),
                    maxZ = Convert.ToDouble(row["maxZ"]),
                    minX = Convert.ToDouble(row["minX"]),
                    minY = Convert.ToDouble(row["minY"]),
                    minZ = Convert.ToDouble(row["minZ"])
                };
            }

            Assert.AreEqual(expectedResult, prjStatsRequester.CurrentResponse);
        }

        [Then(@"I should get error code (.*)")]
        public void ThenIShouldGetErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, prjStatsRequester.CurrentResponse.Code);
        }
    }
}
