using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "CoordinateConversion")]
    public class CoordinateConversionSteps
    {
        private Poster<CoordinateConversionRequest, CoordinateConversionResult> coordConversionRequester;
        private CoordinateConversionRequest currentCsConvertRequest = new CoordinateConversionRequest();

        [Given(@"the Coordinate Conversion service URI ""(.*)""")]
        public void GivenTheCoordinateConversionServiceURI(string uri)
        {
            uri = RaptorClientConfig.CoordSvcBaseUri + uri;
            coordConversionRequester = new Poster<CoordinateConversionRequest, CoordinateConversionResult>(uri);
        }

        [Given(@"a project id (.*)")]
        public void GivenAProjectId(int projectId)
        {
            currentCsConvertRequest.projectId = projectId;
        }

        [Given(@"the coordinate conversion type ""(.*)""")]
        public void GivenTheCoordinateConversionType(string conversionType)
        {
            if (conversionType == "LatLonToNorthEast")
                currentCsConvertRequest.conversionType = TwoDCoordinateConversionType.LatLonToNorthEast;
            if (conversionType == "NorthEastToLatLon")
                currentCsConvertRequest.conversionType = TwoDCoordinateConversionType.NorthEastToLatLon;
        }

        [Given(@"these coordinates")]
        public void GivenTheseCoordinates(Table points)
        {
            List<TwoDConversionCoordinate> coordinates = new List<TwoDConversionCoordinate>();

            foreach (var point in points.Rows)
            {
                coordinates.Add(new TwoDConversionCoordinate()
                {
                    x = Convert.ToDouble(point["x"]),
                    y = Convert.ToDouble(point["y"])
                });
            }

            currentCsConvertRequest.conversionCoordinates = coordinates.ToArray();
        }

        [When(@"I request the coordinate conversion")]
        public void WhenIRequestTheCoordinateConversion()
        {
            coordConversionRequester.CurrentRequest = currentCsConvertRequest;
            coordConversionRequester.DoValidRequest();
        }

        [Then(@"the result should be these")]
        public void ThenTheResultShouldBeThese(Table points)
        {
            CoordinateConversionResult expectedResult = new CoordinateConversionResult();
            List<TwoDConversionCoordinate> expectedCoordinates = new List<TwoDConversionCoordinate>();

            foreach (var point in points.Rows)
            {
                expectedCoordinates.Add(new TwoDConversionCoordinate()
                {
                    x = Convert.ToDouble(point["x"]),
                    y = Convert.ToDouble(point["y"])
                });
            }

            expectedResult.conversionCoordinates = expectedCoordinates.ToArray();

            Assert.AreEqual(expectedResult, coordConversionRequester.CurrentResponse);
        }

        [When(@"I request the coordinate conversion expecting (.*)")]
        public void WhenIRequestTheCoordinateConversionExpecting(int httpCode)
        {
            coordConversionRequester.CurrentRequest = currentCsConvertRequest;
            coordConversionRequester.DoInvalidRequest((HttpStatusCode)httpCode);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int errorCode)
        {
            Assert.AreEqual(errorCode, coordConversionRequester.CurrentResponse.Code);
        }
    }
}
