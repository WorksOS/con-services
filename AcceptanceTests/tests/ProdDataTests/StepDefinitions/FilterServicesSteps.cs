using System;
using System.Linq;
using TechTalk.SpecFlow;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using RaptorSvcAcceptTestsCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "FilterServices")]
    public class FilterServicesSteps
    {
        private Poster<FilterResult, FiltersResult> filterPoster;
        private Getter<FiltersResult> allFilterGetter;
        private Getter<FiltersResult> singleFilterGetter;

        private Poster<TileRequest, TileResult> tileRequester;

        [Given(@"the Filter service URI ""(.*)"" with a test project ID (.*)")]
        public void GivenTheFilterServiceURIWithATestProjectID(string filterSvcUri, long pId)
        {
            filterPoster = new Poster<FilterResult, FiltersResult>(RaptorClientConfig.ProdSvcBaseUri + string.Format(filterSvcUri, pId));
            allFilterGetter = new Getter<FiltersResult>(RaptorClientConfig.ProdSvcBaseUri + string.Format(filterSvcUri, pId));
            singleFilterGetter = new Getter<FiltersResult>(RaptorClientConfig.ProdSvcBaseUri + string.Format(filterSvcUri, pId));
        }

        [Given(@"a unique filter")]
        public void GivenAUniqueFilter()
        {
            FilterResult uniqueFilter = new FilterResult();
            uniqueFilter.name = DateTime.Now.ToString("s"); // Format "2008-06-15T21:15:07"
            uniqueFilter.polygonGrid = new List<Point>() {
                new Point() { x = 2321.520, y = 1206.662 },
                new Point() { x = 2322.540, y = 1206.662 },
                new Point() { x = 2322.540, y = 1206.322 },
                new Point() { x = 2321.520, y = 1206.322 },
            };

            filterPoster.CurrentRequest = uniqueFilter;
        }

        [Given(@"I can successfully save this unique filter")]
        public void GivenICanSuccessfullySaveThisUniqueFilter()
        {
            if (filterPoster.DoValidRequest() != null)
            {
                filterPoster.CurrentRequest.ID = filterPoster.CurrentResponse.FilterId; // For GET filter validation
                singleFilterGetter.Uri = singleFilterGetter.Uri + "/" + filterPoster.CurrentResponse.FilterId;

                // For applying saved filter
                if(tileRequester != null)
                {
                    tileRequester.CurrentRequest.filterId1 = filterPoster.CurrentResponse.FilterId; 
                }
            }
            else
            {
                // Test inconclusive if filter save unsuccessful - should never get here
                ScenarioContext.Current.Pending();
            }
        }

        [Given(@"the Tile service URI ""(.*)"" and the following request")]
        public void GivenTheTileServiceURIAndTheFollowingRequest(string tileSvcUri, string tileReqStr)
        {
            tileRequester = new Poster<TileRequest, TileResult>(RaptorClientConfig.ProdSvcBaseUri + tileSvcUri, 
                JsonConvert.DeserializeObject<TileRequest>(tileReqStr));
        }

        [When(@"I try to retrieve all saved filters for the test project")]
        public void WhenITryToRetrieveAllSavedFiltersForTheTestProject()
        {
            allFilterGetter.DoValidRequest();
        }

        [When(@"I try to retrieve the single unique filter I just saved")]
        public void WhenITryToRetrieveTheSingleUniqueFilterIJustSaved()
        {
            singleFilterGetter.DoValidRequest();
        }

        [When(@"I request the tile")]
        public void WhenIRequestTheTile()
        {
            tileRequester.DoValidRequest();
        }

        [When(@"I try to retrieve filter with ID (.*) expecting BadRequest")]
        public void WhenITryToRetrieveFilterWithIDExpectingBadRequest(long filterID)
        {
            singleFilterGetter.Uri = singleFilterGetter.Uri + "/" + filterID;
            singleFilterGetter.DoInvalidRequest();
        }

        [When(@"I try to retrieve all saved filters for the test project expecting BadRequest")]
        public void WhenITryToRetrieveAllSavedFiltersForTheTestProjectExpectingBadRequest()
        {
            allFilterGetter.DoInvalidRequest();
        }

        [Then(@"the all-filter list retrieved should contain the filter I just saved")]
        public void ThenTheAll_FilterListRetrievedShouldContainTheFilterIJustSaved()
        {
            Assert.IsTrue(allFilterGetter.CurrentResponse.FiltersArray.ToList().Contains(filterPoster.CurrentRequest),
                string.Format("Filter {0} is not in {1}", filterPoster.CurrentRequest, allFilterGetter.CurrentResponse));
        }

        [Then(@"the single filter retrieved should match the filter I just saved")]
        public void ThenTheSingleFilterRetrievedShouldMatchTheFilterIJustSaved()
        {
            Assert.AreEqual(filterPoster.CurrentRequest, singleFilterGetter.CurrentResponse.FiltersArray[0]);
        }

        [Then(@"the response should contain the same tile data as the following one")]
        public void ThenTheResponseShouldContainTheSameTileDataAsTheFollowingOne(string tileRespStr)
        {
            Assert.AreEqual(JsonConvert.DeserializeObject<TileResult>(tileRespStr), tileRequester.CurrentResponse);
        }

        [Then(@"the response should contain Code (.*) and Message ""(.*)""")]
        public void ThenTheResponseShouldContainCodeAndMessage(int code, string message)
        {
            if(singleFilterGetter.CurrentResponse != null)
                Assert.IsTrue(singleFilterGetter.CurrentResponse.Code == code &&
                    singleFilterGetter.CurrentResponse.Message == message);
            if (allFilterGetter.CurrentResponse != null)
                Assert.IsTrue(allFilterGetter.CurrentResponse.Code == code &&
                    allFilterGetter.CurrentResponse.Message == message);
        }

        [Then(@"the FiltersArray in the response should be empty")]
        public void ThenTheFiltersArrayInTheResponseShouldBeEmpty()
        {
            Assert.IsTrue(allFilterGetter.CurrentResponse.FiltersArray.Length == 0,
                string.Format("Expected empty FiltersArray, but actual FiltersArray contains {0} elements.", 
                allFilterGetter.CurrentResponse.FiltersArray.Length));
        }
    }
}
