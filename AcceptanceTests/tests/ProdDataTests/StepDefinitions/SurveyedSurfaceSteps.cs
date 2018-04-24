using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;
using System.Net;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "SurveyedSurface")]
    public class SurveyedSurfaceSteps
    {
        private Getter<GetSurveydSurfacesResult> surfacesValidator;
        private GetSurveydSurfacesResult result;
        private DummyRequestResult resultPost;

        private Getter<DummyRequestResult> surfacesDeleteValidator;
        private Poster<SurveyedSurfaceRequest, DummyRequestResult> surfaceToPost;
        private DummyRequestResult resultDelete;
        private string uriProd;
        private string postAddress;

        [Given(@"the Surveyd surface service URI ""(.*)""")]
        public void GivenTheSurveydSurfaceServiceURI(string uri)
        {
            this.uriProd = uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            this.surfacesValidator = new Getter<GetSurveydSurfacesResult>(uri);
            surfacesDeleteValidator = new Getter<DummyRequestResult>(uri + "/1234/delete");
        }

        [Given(@"using repository ""(.*)""")]
        public void GivenUsingRepository(string p0)
        {
            surfaceToPost = new Poster<SurveyedSurfaceRequest, DummyRequestResult>(postAddress, p0, p0);
        }

        [Given(@"the Surveyd surface service POST URI ""(.*)""")]
        public void GivenTheSurveydSurfaceServicePOSTURI(string p0)
        {
            postAddress = RaptorClientConfig.ProdSvcBaseUri  + p0;
            surfaceToPost.Uri = RaptorClientConfig.ProdSvcBaseUri + p0;
        }



        [Given(@"a project Id (.*)")]
        public void GivenAProjectId(int projectId)
        {
            surfacesValidator.Uri = String.Format(surfacesValidator.Uri, projectId);
            surfacesDeleteValidator.Uri = String.Format(surfacesDeleteValidator.Uri, projectId);
        }

        [When(@"I request surveyd SurveyedSurfaces")]
        public void WhenIRequestSurveydSurfaces()
        {
            result = surfacesValidator.DoValidRequest();
        }

        [When(@"I delete surveyd SurveyedSurfaces")]
        public void WhenIDeleteSurveydSurfaces()
        {
            resultDelete = surfacesDeleteValidator.DoValidRequest();
        }

        [Then(@"the following machine designs should be returned")]
        public void ThenTheFollowingMachineDesignsShouldBeReturned(Table table)
        {
            GetSurveydSurfacesResult expectedResult = new GetSurveydSurfacesResult();

            // Get expected machine designs from feature file
            List<SurveyedSurfaces> expectedSurfaces = new List<SurveyedSurfaces>();
            foreach (var surface in table.Rows)
            {
                expectedSurfaces.Add(new SurveyedSurfaces()
                {
                    AsAtDate = Convert.ToDateTime(surface["AsAtDate"]),
                    Id = Convert.ToInt64(surface["SurveyedSurfaceId"]),
                    SurveyedSurface = new DesignDescriptor() { file = new FileDescriptor() { fileName = surface["fileName"], filespaceId = surface["filespaceId"] } },
                });
            }

            expectedResult.SurveyedSurfaces = expectedSurfaces;

            Assert.AreEqual(expectedResult, result);
        }

        [When(@"I post surveyd surface")]
        public void WhenIPostSurveydSurface()
        {
            surfaceToPost.DoValidRequest("PostStandardFile");
        }

        [When(@"I request Surveyed Surface expecting Bad Request")]
        public void WhenIRequestSurveyedSurfaceExpectingBadRequest()
        {
            result = surfacesValidator.DoInvalidRequest(HttpStatusCode.BadRequest);
        }

        [Then(@"the response should contain Code (.*) and Message ""(.*)""")]
        public void ThenTheResponseShouldContainCodeAndMessage(int code, string message)
        {
            Assert.IsTrue(code == result.Code && message == result.Message);
        }

        [When(@"I Post Surveyd Surface ""(.*)"" expecting Bad Request")]
        public void WhenIPostSurveydSurfaceExpectingBadRequest(string paramName)
        {
            resultPost = surfaceToPost.DoInvalidRequest(paramName, HttpStatusCode.BadRequest);
        }

        [Then(@"the Post response should contain Code (.*) and Message ""(.*)""")]
        public void ThenThePostResponseShouldContainCodeAndMessage(int code, string message)
        {
            Assert.IsTrue(code == resultPost.Code && message == resultPost.Message);
        }
    }
}
