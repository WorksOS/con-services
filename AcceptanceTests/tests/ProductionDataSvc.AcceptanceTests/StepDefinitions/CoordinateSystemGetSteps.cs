using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "CoordinateSystemGet")]
    public class CoordinateSystemGetSteps
    {
        Getter<CoordinateSystemSettings> coordSysGetter;

        [Given(@"the Coordinate service URI ""(.*)"" and the request repo ""(.*)""")]
        public void GivenTheCoordinateServiceURIAndTheRequestRepo(string uri, string resultFile)
        {
            uri = RaptorClientConfig.CoordSvcBaseUri + uri;
            coordSysGetter = new Getter<CoordinateSystemSettings>(uri, resultFile);
        }

        [When(@"I try to get Coordinate System for project (.*)")]
        public void WhenITryToGetCoordinateSystemForProject(int projectId)
        {
            coordSysGetter.Uri = String.Format(coordSysGetter.Uri, projectId);
            coordSysGetter.DoValidRequest();
        }

        [Then(@"the CoordinateSystem response should match ""(.*)"" result from the repository")]
        public void ThenTheCoordinateSystemResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(coordSysGetter.ResponseRepo[resultName], coordSysGetter.CurrentResponse);
        }

        [When(@"I try to get Coordinate System for project (.*) expecting http error code (.*)")]
        public void WhenITryToGetCoordinateSystemForProjectExpectingHttpErrorCode(int projectId, int httpCode)
        {
            coordSysGetter.Uri = String.Format(coordSysGetter.Uri, projectId);
            coordSysGetter.DoInvalidRequest((HttpStatusCode)httpCode);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int code)
        {
            Assert.AreEqual(code, coordSysGetter.CurrentResponse.Code);
        }
    }
}
