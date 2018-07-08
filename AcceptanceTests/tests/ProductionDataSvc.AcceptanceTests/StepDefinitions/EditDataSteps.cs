using System;
using TechTalk.SpecFlow;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using RaptorSvcAcceptTestsCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
   [Binding, Scope(Feature = "EditData")]
    public class EditDataSteps
    {
        private Poster<EditDataRequest, EditDataResult> editDataRequester;
        private Poster<GetEditDataRequest, GetEditDataResult> getEditDataRequester;
        private EditDataResult editDataResult;
        private GetEditDataResult getEditDataResult;
        private CellDatumResult cellDatumResult;
        private GetMachineDesignResult getMachineDesignResult;
        private LayerIdsExecutionResult layerIdsExecutionResult;

        private readonly DataEditContext _dataEditContext;

        public EditDataSteps(DataEditContext dataEditContext)
        {
            _dataEditContext = dataEditContext;
        }

        [Given(@"the edit data service URI ""(.*)""")]
        public void GivenTheEditDataServiceURI(string editDataUri)
        {
            editDataRequester = 
                new Poster<EditDataRequest, EditDataResult>(RaptorClientConfig.ProdSvcBaseUri + editDataUri);
        }

        [Given(@"the get edit data service URI ""(.*)""")]
        public void GivenTheGetEditDataServiceURI(string getEditDataUri)
        {
            getEditDataRequester = 
                new Poster<GetEditDataRequest, GetEditDataResult>(RaptorClientConfig.ProdSvcBaseUri + getEditDataUri);
        }

        [Given(@"all data edits are cleared for project (.*)")]
        public void GivenAllDataEditsAreClearedForProject(long pId)
        {
            // Requests for Undo All Edits & Get All Edits
            EditDataRequest undoAllEditsRequest = new EditDataRequest() { projectId = pId, undo = true, dataEdit = null };
            GetEditDataRequest getAllEditsRequest = new GetEditDataRequest() { projectId = pId, assetId = -1 };

            // Get all edits - clear all if necessary
            GetEditDataResult allEdits = getEditDataRequester.DoValidRequest(getAllEditsRequest);
            if(allEdits.dataEdits.Count > 0)
            {
                // Undo all edits
                editDataRequester.DoValidRequest(undoAllEditsRequest);
                Thread.Sleep(1000); // This takes time so wait a second

                // Get all edits again - stop test if still not clear
                allEdits = getEditDataRequester.DoValidRequest(getAllEditsRequest);
                if(allEdits.dataEdits.Count > 0)
                {
                    ScenarioContext.Current.Pending(); // This will make test result inclusive
                }
            }
        }

        [Given(@"GetLifts service ""(.*)"" only returns (.*) real lifts for project (.*)")]
        public void GivenGetLiftsServiceOnlyReturnsRealLiftsForProject(string getLiftsUri, int numLifts, long pId)
        {
            string fullGetLiftsUri = RaptorClientConfig.ProdSvcBaseUri + String.Format(getLiftsUri, pId);
            Getter<LayerIdsExecutionResult> getLiftRequester =
                new Getter<LayerIdsExecutionResult>(fullGetLiftsUri);

            LayerIdsExecutionResult result = getLiftRequester.DoValidRequest();
            if (result.LayerIdDetailsArray.Length != numLifts)
                ScenarioContext.Current.Pending();
        }

        [Given(@"GetMachineDesign service ""(.*)"" only returns (.*) real designs for project (.*)")]
        public void GivenGetMachineDesignServiceOnlyReturnsRealDesignsForProject(string getMachDesignUri, int numMachDesign, int pId)
        {
            string fullGetMachDesignUri = RaptorClientConfig.ProdSvcBaseUri + String.Format(getMachDesignUri, pId);
            Getter<GetMachineDesignResult> getMachDesignRequester =
                new Getter<GetMachineDesignResult>(fullGetMachDesignUri);

            GetMachineDesignResult result = getMachDesignRequester.DoValidRequest();
            if (result.designs.Count != numMachDesign)
                ScenarioContext.Current.Pending();
        }

        [Given(@"the following data edit details")]
        public void GivenTheFollowingDataEditDetails(Table allEdits)
        {

            // Get expected machine designs from feature file
            foreach (var edit in allEdits.Rows)
            {
                _dataEditContext.DataEdits.Add(new ProductionDataEdit()
                {
                    assetId = Convert.ToInt64(edit["assetId"]),
                    startUTC = Convert.ToDateTime(edit["startUTC"]),
                    endUTC = Convert.ToDateTime(edit["endUTC"]),
                    //onMachineDesignName = edit["onMachineDesignName"] == "null" ? null : edit["onMachineDesignName"],
                    onMachineDesignName = edit["onMachineDesignName"] == "null" ? null : 
                        (edit["onMachineDesignName"] == "Random" ? "VirtualDesign_" + (new Random()).Next(32768).ToString() : edit["onMachineDesignName"]),
                    //liftNumber = edit["liftNumber"] == "null" ? (int?)null : Convert.ToInt32(edit["liftNumber"])
                    liftNumber = edit["liftNumber"] == "null" ? (int?)null : 
                        (edit["liftNumber"] == "Random" ? (new Random()).Next(32768) : Convert.ToInt32(edit["liftNumber"]))
                });
            }
        }

        [Given(@"I submit the following data edits to project (.*)")]
        public void GivenISubmitTheFollowingDataEditsToProject(long pId, Table editIds)
        {
            foreach (var id in editIds.Rows)
            {
                EditDataRequest doEditRequest = new EditDataRequest
                {
                    projectId = pId,
                    undo = false,
                    dataEdit = this._dataEditContext.DataEdits[int.Parse(id["EditId"])]
                };
                editDataResult = editDataRequester.DoValidRequest(doEditRequest);
                Thread.Sleep(1000);
            }
        }

        [When(@"I try to get all edits for project (.*)")]
        public void WhenITryToGetAllEditsForProject(long pId)
        {
            GetEditDataRequest getAllEditsRequest = new GetEditDataRequest() { projectId = pId, assetId = -1 };
            getEditDataResult = getEditDataRequester.DoValidRequest(getAllEditsRequest);
        }

        [Then(@"the result should contain the following data edits")]
        public void ThenTheResultShouldContainTheFollowingDataEdits(Table editIds)
        {
            GetEditDataResult expectedResult = new GetEditDataResult();

            List<ProductionDataEdit> edits = new List<ProductionDataEdit>();
            foreach (var id in editIds.Rows)
            {
                edits.Add(this._dataEditContext.DataEdits[int.Parse(id["EditId"])]);
            }
            expectedResult.dataEdits = edits;

            Assert.AreEqual(expectedResult, getEditDataResult);
        }

        [When(@"the result matches the following data edits")]
        public void WhenTheResultMatchesTheFollowingDataEdits(Table editIds)
        {
            GetEditDataResult expectedResult = new GetEditDataResult();

            List<ProductionDataEdit> edits = new List<ProductionDataEdit>();
            foreach (var id in editIds.Rows)
            {
                edits.Add(this._dataEditContext.DataEdits[int.Parse(id["EditId"])]);
            }
            expectedResult.dataEdits = edits;

            if (getEditDataResult != expectedResult)
                ScenarioContext.Current.Pending();
        }

        [When(@"I try to undo the following edits for project (.*)")]
        public void WhenITryToUndoTheFollowingEditsForProject(long pId, Table editIds)
        {
            foreach (var id in editIds.Rows)
            {
                EditDataRequest undoEditRequest = new EditDataRequest() 
                { 
                    projectId = pId, 
                    undo = true,
                    dataEdit = this._dataEditContext.DataEdits[int.Parse(id["EditId"])]
                };
                editDataRequester.DoValidRequest(undoEditRequest);
                Thread.Sleep(1000);
            }
        }

        [Then(@"the result should be empty")]
        public void ThenTheResultShouldBeEmpty()
        {
            GetEditDataResult expectedResult = new GetEditDataResult();
            expectedResult.dataEdits = new List<ProductionDataEdit>();
            Assert.AreEqual(expectedResult, getEditDataResult);
        }

        [When(@"I request ""(.*)"" from resource ""(.*)"" at Grid Point \((.*), (.*)\) for project (.*) filtered by EditId (.*)")]
        public void WhenIRequestFromResourceAtGridPointForProjectFilteredByEditId(
             string datumTypeStr, string datumUri, double gridPtX, double gridPtY, long pId, int editId)
        {
            string fullCellDatumUri = RaptorClientConfig.ProdSvcBaseUri + datumUri;
            DisplayMode datumType = (DisplayMode)Enum.Parse(typeof(DisplayMode), datumTypeStr);
            Point gridPoint = new Point() { x = gridPtX, y = gridPtY };

            // Construct Filter from data edit (by LiftId, DesignId or both)
            FilterResult filter = new FilterResult();
            ProductionDataEdit edit = _dataEditContext.DataEdits[editId];
            if(edit.liftNumber != null)
            {
                filter.layerType = FilterLayerMethod.TagfileLayerNumber;
                filter.layerNumber = edit.liftNumber;
            }
            if(edit.onMachineDesignName != null)
            {
                filter.onMachineDesignID = 3; // It's 3 because all inserted designs are named "VirtualDesign" and that design name has id 3.
            }

            // Do cell datum request filtered by edited/inserted design id or layer id
            CellDatumRequest datumRequest = new CellDatumRequest(pId, datumType, gridPoint, filter);
            Poster<CellDatumRequest, CellDatumResult> datumRequester =
                new Poster<CellDatumRequest, CellDatumResult>(fullCellDatumUri, datumRequest);

            cellDatumResult = datumRequester.DoValidRequest();
        }

        [Then(@"the datum should be: displayMode = ""(.*)"", returnCode = ""(.*)"", value = ""(.*)"", timestamp = ""(.*)""")]
        public void ThenTheDatumShouldBeDisplayModeReturnCodeValueTimestamp(
            int displayMode, int returnCode, double value, string timestamp)
        {
            CellDatumResult expectedDatumResult = new CellDatumResult();
            expectedDatumResult.displayMode = (DisplayMode)displayMode;
            expectedDatumResult.returnCode = (short)returnCode;
            expectedDatumResult.value = value;
            expectedDatumResult.timestamp = Convert.ToDateTime(timestamp);

            Assert.AreEqual(expectedDatumResult, cellDatumResult);
        }

        [Given(@"I submit data edit with EditId (.*) to project (.*) expecting HttpResponseCode (.*)")]
        public void GivenISubmitDataEditWithEditIdToProjectExpectingHttpResponseCode(int editId, long pId, int httpCode)
        {
            EditDataRequest doEditRequest = new EditDataRequest
            {
                projectId = pId,
                undo = false,
                dataEdit = this._dataEditContext.DataEdits[editId]
            };
            editDataResult = editDataRequester.DoInvalidRequest(doEditRequest, HttpStatusCode.BadRequest);
            Thread.Sleep(1000);
        }

        [Then(@"I should get Error Code (.*) and Message ""(.*)""")]
        public void ThenIShouldGetErrorCodeAndMessage(int code, string msg)
        {
            Assert.IsTrue(editDataResult.Code == code && editDataResult.Message.Contains(msg),
                String.Format("Expected code {0}, received {1} instead; expected message to contain {2}, received {3} intead.",
                code, editDataResult.Code, msg, editDataResult.Message));
        }

        [When(@"I read back all machine designs from ""(.*)"" for project (.*)")]
        public void WhenIReadBackAllMachineDesignsFromForProject(string getDesignUri, int pId)
        {
            Getter<GetMachineDesignResult> designGetter =
                new Getter<GetMachineDesignResult>(string.Format(RaptorClientConfig.ProdSvcBaseUri + getDesignUri, pId));

            getMachineDesignResult = designGetter.DoValidRequest();
        }

        [When(@"I read back all lifts from ""(.*)"" for project (.*)")]
        public void WhenIReadBackAllLiftsFromForProject(string getLiftUri, int pId)
        {
            Getter<LayerIdsExecutionResult> liftGetter =
                new Getter<LayerIdsExecutionResult>(string.Format(RaptorClientConfig.ProdSvcBaseUri + getLiftUri, pId));

            layerIdsExecutionResult = liftGetter.DoValidRequest();
        }

        [Then(@"the machine design list should contain the design details in the following data edits")]
        public void ThenTheMachineDesignListShouldContainTheDesignDetailsInTheFollowingDataEdits(Table editIds)
        {
            foreach (var id in editIds.Rows)
            {
                string designName = this._dataEditContext.DataEdits[int.Parse(id["EditId"])].onMachineDesignName;
                Assert.IsTrue(getMachineDesignResult.designs.Exists(d => d.designName == designName),
                    "Cannot find design edit in the design list read back.");
            }
        }

        [Then(@"the lift list should contain the lift details in the following data edits")]
        public void ThenTheLiftListShouldContainTheLiftDetailsInTheFollowingDataEdits(Table editIds)
        {
            foreach (var id in editIds.Rows)
            {
                long assetId = this._dataEditContext.DataEdits[int.Parse(id["EditId"])].assetId;

                int designIndex = -1;
                if (getMachineDesignResult != null)
                {
                    designIndex = getMachineDesignResult.designs.FindIndex(d =>
                        d.designName == this._dataEditContext.DataEdits[int.Parse(id["EditId"])].onMachineDesignName);

                    if (designIndex == -1)
                        ScenarioContext.Current.Pending();                  
                }
                else
                {
                    ScenarioContext.Current.Pending();
                }

                long designId = getMachineDesignResult.designs[designIndex].designId;
                int layerId = (int)this._dataEditContext.DataEdits[int.Parse(id["EditId"])].liftNumber;
                DateTime startDate = this._dataEditContext.DataEdits[int.Parse(id["EditId"])].startUTC;
                DateTime endDate = this._dataEditContext.DataEdits[int.Parse(id["EditId"])].endUTC;

                Assert.IsTrue(layerIdsExecutionResult.LayerIdDetailsArray.ToList<LayerIdDetails>().Exists(l => l.AssetId == assetId &&
                    l.DesignId == designId && l.LayerId == layerId && l.StartDate == startDate && l.EndDate == endDate),
                    "Cannot find lift edit in the lift list read back.");
            }
        }
    }
}
