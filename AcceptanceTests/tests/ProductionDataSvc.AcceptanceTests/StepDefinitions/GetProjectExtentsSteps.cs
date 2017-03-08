using System;
using TechTalk.SpecFlow;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using ProductionDataSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "GetProjectExtents")]
    public class GetProjectExtentsSteps
    {
        private Poster<ProjectExtentRequest, ProjectExtentsResult> projExtentRequester;

        [Given(@"the Project Extent service URI ""(.*)""")]
        public void GivenTheProjectExtentServiceURI(string uri)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            projExtentRequester = new Poster<ProjectExtentRequest, ProjectExtentsResult>(uri);
        }

        [Given(@"a GetProjectExtents project id (.*)")]
        public void GivenAGetProjectExtentsProjectId(int pId)
        {
            projExtentRequester.CurrentRequest = new ProjectExtentRequest() { projectId = pId };
        }

        [Given(@"I decide to exclude any surveyed surface")]
        public void GivenIDecideToExcludeAnySurveyedSurface()
        {
            // 111 is always the dummy ID specified when a surveyed surface is created
            projExtentRequester.CurrentRequest.excludedSurveyedSurfaceIds = new long[] { 111 };
        }

        [Given(@"a GetProjectExtents null project id")]
        public void GivenAGetProjectExtentsNullProjectId()
        {
            projExtentRequester.CurrentRequest = new ProjectExtentRequest() { projectId = null };
        }

        [When(@"I try to get the extents")]
        public void WhenITryToGetTheExtents()
        {
            projExtentRequester.DoValidRequest();
        }

        [When(@"I try to get the extents expecting badrequest")]
        public void WhenITryToGetTheExtentsExpectingBadrequest()
        {
            projExtentRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
        }

        [When(@"I post an empty request")]
        public void WhenIPostAnEmptyRequest()
        {
            projExtentRequester.DoInvalidRequest("", HttpStatusCode.BadRequest);
        }

        [Then(@"the following Bounding Box ThreeD Grid values should be returned")]
        public void ThenTheFollowingBoundingBoxThreeDGridValuesShouldBeReturned(Table expectedBoundingBox3DGrid)
        {
            ProjectExtentsResult expectedResult = new ProjectExtentsResult();

            foreach(var row in expectedBoundingBox3DGrid.Rows)
            {
                expectedResult.ProjectExtents = new BoundingBox3DGrid()
                {
                    maxX = Convert.ToDouble(row["maxX"]),
                    minX = Convert.ToDouble(row["minX"]),
                    maxY = Convert.ToDouble(row["maxY"]),
                    minY = Convert.ToDouble(row["minY"]),
                    maxZ = Convert.ToDouble(row["maxZ"]),
                    minZ = Convert.ToDouble(row["minZ"])
                };
            }

            Assert.AreEqual(expectedResult, projExtentRequester.CurrentResponse);
        }

        [Then(@"I should get error code (.*)")]
        public void ThenIShouldGetErrorCode(int code)
        {
            Assert.AreEqual(code, projExtentRequester.CurrentResponse.Code);
        }
    }
}
