using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;
using System.Collections.Generic;
using System.Net;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "GetLifts")]
    public class GetLiftsSteps
    {
        private Getter<LayerIdsExecutionResult> liftIdRequester;

        [Given(@"the Lift ID service URI ""(.*)""")]
        public void GivenTheLiftIDServiceURI(string uri)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            liftIdRequester = new Getter<LayerIdsExecutionResult>(uri);
        }

        [Given(@"a GetLifts project Id (.*)")]
        public void GivenAGetLiftsProjectId(int projectId)
        {
            liftIdRequester.Uri = String.Format(liftIdRequester.Uri, projectId);
        }

        [When(@"I request lift ids")]
        public void WhenIRequestLiftIds()
        {
            liftIdRequester.DoValidRequest();
        }

        [When(@"I request lift ids expecting Bad Request")]
        public void WhenIRequestLiftIdsExpectingBadRequest()
        {
            liftIdRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
        }

        [Then(@"the following lift details should be returned")]
        public void ThenTheFollowingLiftDetailsShouldBeReturned(Table layers)
        {
            LayerIdsExecutionResult expectedResult = new LayerIdsExecutionResult();

            // Get expected machine designs from feature file
            List<LayerIdDetails> expectedLayers = new List<LayerIdDetails>();
            foreach (var layer in layers.Rows)
            {
                expectedLayers.Add(new LayerIdDetails()
                {
                    AssetId = Convert.ToInt64(layer["AssetId"]),
                    DesignId = Convert.ToInt64(layer["DesignId"]),
                    LayerId = Convert.ToInt64(layer["LayerId"]),
                    StartDate = Convert.ToDateTime(layer["StartDate"]),
                    EndDate = Convert.ToDateTime(layer["EndDate"])
                });
            }

            expectedResult.LayerIdDetailsArray = expectedLayers.ToArray();

            Assert.AreEqual(expectedResult, liftIdRequester.CurrentResponse);
        }

        [Then(@"the response should contain Code (.*) and Message ""(.*)""")]
        public void ThenTheResponseShouldContainCodeAndMessage(int code, string message)
        {
            Assert.IsTrue(liftIdRequester.CurrentResponse.Code == code && liftIdRequester.CurrentResponse.Message == message,
                string.Format("Expected Code {0} and Message {1}, but got Code {2} and Message {3} instead.",
                code, message, liftIdRequester.CurrentResponse.Code, liftIdRequester.CurrentResponse.Message));
        }

    }
}
