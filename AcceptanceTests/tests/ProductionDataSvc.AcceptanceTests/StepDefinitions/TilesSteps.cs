using System;
using System.IO;
using TechTalk.SpecFlow;
using System.Net;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Utils;
using ProductionDataSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "Tiles")]
    public class TilesSteps
    {
        private Poster<TileRequest, TileResult> tileRequester;
        private WebHeaderCollection header;
        private byte[] pngTile;
        private string responseString;

        [Given(@"the Tile service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheTileServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            tileRequester = new Poster<TileRequest, TileResult>(uri, requestFile, resultFile);
        }

        [When(@"I request Tiles supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestTilesSupplyingParamtersFromTheRepository(string paramName)
        {
            tileRequester.DoValidRequest(paramName);
        }

        [When(@"I request Tiles supplying ""(.*)"" paramters from the repository expecting BadRequest")]
        public void WhenIRequestTilesSupplyingParamtersFromTheRepositoryExpectingBadRequest(string paramName)
        {
            tileRequester.DoInvalidRequest(paramName);
        }

        [Then(@"the Tiles response should match ""(.*)"" result from the repository")]
        public void ThenTheTilesResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(tileRequester.ResponseRepo[resultName], tileRequester.CurrentResponse);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, tileRequester.CurrentResponse.Code);
        }

        [Given(@"the PNG Tile service URI ""(.*)""")]
        public void GivenThePNGTileServiceURI(string pngTileUri)
        {
            tileRequester.Uri = RaptorClientConfig.ProdSvcBaseUri + pngTileUri;
        }

        [When(@"I request PNG Tiles supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestPNGTilesSupplyingParamtersFromTheRepository(string paramName)
        {
            var requestBodyString = JsonConvert.SerializeObject(tileRequester.RequestRepo[paramName]);
            
            var httpResponse = RaptorServicesClientUtil.DoHttpRequest(tileRequester.Uri,
                 "POST", "application/json", "image/png", requestBodyString);

            pngTile = RaptorServicesClientUtil.GetStreamContentsFromResponse(httpResponse);
            header = httpResponse.Headers;
            responseString = Encoding.Default.GetString(pngTile);
        }

        [Then(@"the PNG Tiles response should match ""(.*)"" result from the repository")]
        public void ThenThePNGTilesResponseShouldMatchResultFromTheRepository(string resultName)
        {
            TileResult test = (TileResult)JsonConvert.DeserializeObject(responseString, typeof(TileResult));
            Assert.AreEqual(tileRequester.ResponseRepo[resultName], test);
        }

      [Then(@"the Raw PNG Tiles response should match ""(.*)"" result from the repository")]
      public void ThenTheRawPNGTilesResponseShouldMatchResultFromTheRepository(string resultName)
      {
        TileResult result = new TileResult()
        {
          TileData = pngTile,
          TileOutsideProjectExtents = tileRequester.ResponseRepo[resultName].TileOutsideProjectExtents
        };
        Assert.AreEqual(tileRequester.ResponseRepo[resultName], result);
    }


    [Then(@"the X-Warning in the response header should be ""(.*)""")]
        public void ThenTheX_WarningInTheResponseHeaderShouldBe(string xWarning)
        {
            Assert.AreEqual(xWarning, header.Get("X-Warning"));
        }
    }
}
