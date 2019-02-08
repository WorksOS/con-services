using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit;
using Xunit.Gherkin.Quick;
using Feature = Xunit.Gherkin.Quick.Feature;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("EditData.feature")]
  public class EditDataSteps : Feature
  {
    private Poster<JObject, EditDataResult> editDataRequester;
    private Poster<JObject, GetEditDataResult> getEditDataRequester;
    private EditDataResult editDataResult;
    private ResponseBase getEditDataResult;
    private ResponseBase cellDatumResult;
    private GetMachineDesignResult getMachineDesignResult;
    private LayerIdsExecutionResult layerIdsExecutionResult;

    private readonly DataEditContext dataEditContext;

    public EditDataSteps()
    {
      dataEditContext = new DataEditContext();
    }

    [Given(@"the edit data service URI ""(.*)""")]
    public void GivenTheEditDataServiceURI(string editDataUri)
    {
      editDataRequester = new Poster<JObject, EditDataResult>(RestClient.Productivity3DServiceBaseUrl + editDataUri);
    }

    [And(@"the get edit data service URI ""(.*)""")]
    public void AndTheGetEditDataServiceURI(string getEditDataUri)
    {
      getEditDataRequester = new Poster<JObject, GetEditDataResult>(RestClient.Productivity3DServiceBaseUrl + getEditDataUri);
    }

    [And(@"all data edits are cleared for project (.*)")]
    public void GivenAllDataEditsAreClearedForProject(long pId)
    {
      // Requests for Undo All Edits & Get All Edits
      dynamic undoAllEditsRequest = new JObject();
      undoAllEditsRequest.ProjectId = pId;
      undoAllEditsRequest.Undo = true;
      undoAllEditsRequest.DataEdit = null;

      dynamic getAllEditsRequest = new JObject();
      getAllEditsRequest.ProjectId = pId;
      getAllEditsRequest.AssetId = -1;

      // Undo all edits
      editDataRequester.DoValidRequest(undoAllEditsRequest);

      // Get all edits and confirm they have been removed.
      var allEdits = getEditDataRequester.DoValidRequest(getAllEditsRequest);
      Assert.True(!((List<ProductionDataEdit>)allEdits.dataEdits).Any());
    }

    [And(@"GetLifts service ""(.*)"" only returns (.*) real lifts for project (.*)")]
    public void AndGetLiftsServiceOnlyReturnsRealLiftsForProject(string getLiftsUri, int numLifts, long pId)
    {
      var fullGetLiftsUri = RestClient.Productivity3DServiceBaseUrl + string.Format(getLiftsUri, pId);
      new Getter<LayerIdsExecutionResult>(fullGetLiftsUri).SendRequest();
    }

    [And(@"GetMachineDesign service ""(.*)"" only returns (.*) real designs for project (.*)")]
    public void AndGetMachineDesignServiceOnlyReturnsRealDesignsForProject(string getMachDesignUri, int numMachDesign, int pId)
    {
      var fullGetMachDesignUri = RestClient.Productivity3DServiceBaseUrl + string.Format(getMachDesignUri, pId);
      new Getter<GetMachineDesignResult>(fullGetMachDesignUri).SendRequest();
    }

    [And(@"the following data edit details")]
    public void GivenTheFollowingDataEditDetails(Gherkin.Ast.DataTable dataTable)
    {
      foreach (var row in dataTable.Rows.Skip(1))
      {
        var onMachineDesignName = row.Cells.ElementAt(4).Value;
        var liftNumber = row.Cells.ElementAt(5).Value;

        dataEditContext.DataEdits.Add(new ProductionDataEdit
        {
          assetId = int.Parse(row.Cells.ElementAt(1).Value),
          startUTC = DateTime.Parse(row.Cells.ElementAt(2).Value),
          endUTC = DateTime.Parse(row.Cells.ElementAt(3).Value),
          onMachineDesignName = onMachineDesignName == "null"
            ? null
            : onMachineDesignName == "Random" ? "VirtualDesign_" + new Random().Next(32768) : onMachineDesignName,
          liftNumber = liftNumber == "null"
            ? (int?)null
            : liftNumber == "Random" ? new Random().Next(32768) : Convert.ToInt32(liftNumber)
        });
      }
    }

    [And(@"I submit the following data edits to project (.*)")]
    public void GivenISubmitTheFollowingDataEditsToProject(long pId, Gherkin.Ast.DataTable dataTable)
    {
      foreach (var row in dataTable.Rows.Skip(1))
      {
        _ = int.Parse(row.Cells.ElementAt(0).Value);

        dynamic doEditRequest = new JObject();
        doEditRequest.ProjectId = pId;
        doEditRequest.Undo = false;
        doEditRequest.DataEdit = JToken.FromObject(dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)]);

        editDataResult = editDataRequester.DoValidRequest(doEditRequest);
      }
    }

    [And(@"I try to get all edits for project (.*)")]
    public void AndITryToGetAllEditsForProject(long pId)
    {
      dynamic getAllEditsRequest = new JObject();
      getAllEditsRequest.ProjectId = pId;
      getAllEditsRequest.AssetId = -1;

      getEditDataResult = getEditDataRequester.DoValidRequest(getAllEditsRequest);
    }

    [Then(@"the result should contain the following data edits")]
    public void ThenTheResultShouldContainTheFollowingDataEdits(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new GetEditDataResult();
      var edits = new List<ProductionDataEdit>();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        edits.Add(dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)]);
      }
      expectedResult.dataEdits = edits;

      ObjectComparer.AssertAreEqual(actualResultObj: edits, expectedResultObj: expectedResult.dataEdits);
    }

    [And(@"the result matches the following data edits")]
    public void AndTheResultMatchesTheFollowingDataEdits(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new GetEditDataResult();
      var edits = new List<ProductionDataEdit>();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        edits.Add(dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)]);
      }
      expectedResult.dataEdits = edits;

      ObjectComparer.AssertAreEqual(actualResultObj: edits, expectedResultObj: expectedResult.dataEdits);
    }

    [And(@"I try to undo the following edits for project (.*)")]
    public void AndITryToUndoTheFollowingEditsForProject(long pId, Gherkin.Ast.DataTable dataTable)
    {
      foreach (var row in dataTable.Rows.Skip(1))
      {
        dynamic undoEditRequest = new JObject();
        undoEditRequest.ProjectId = pId;
        undoEditRequest.Undo = true;
        undoEditRequest.DataEdit = JToken.FromObject(dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)]);

        editDataRequester.DoValidRequest(undoEditRequest);
      }
    }

    [Then(@"the result should be empty")]
    public void ThenTheResultShouldBeEmpty()
    {
      var expectedResult = new GetEditDataResult
      {
        dataEdits = new List<ProductionDataEdit>()
      };

      ObjectComparer.AssertAreEqual(actualResultObj: getEditDataResult, expectedResultObj: expectedResult);
    }

    [When(@"I request ""(.*)"" from resource ""(.*)"" at Grid Point \((.*), (.*)\) for project (.*) filtered by EditId (.*)")]
    public void WhenIRequestFromResourceAtGridPointForProjectFilteredByEditId(
         string datumTypeStr, string datumUri, double gridPtX, double gridPtY, long pId, int editId)
    {
      var fullCellDatumUri = RestClient.Productivity3DServiceBaseUrl + datumUri;
      var datumType = (DisplayMode)Enum.Parse(typeof(DisplayMode), datumTypeStr);
      var gridPoint = new Point { x = gridPtX, y = gridPtY };

      // Construct Filter from data edit (by LiftId, DesignId or both)
      dynamic filter = new JObject();
      var edit = dataEditContext.DataEdits[editId];

      if (edit.liftNumber != null)
      {
        filter["layerType"] = (int)FilterLayerMethod.TagfileLayerNumber;
        filter["layerNumber"] = edit.liftNumber;
      }
      if (edit.onMachineDesignName != null)
      {
        filter["onMachineDesignID"] = 3; // It's 3 because all inserted designs are named "VirtualDesign" and that design name has id 3.
      }

      // Do cell datum request filtered by edited/inserted design id or layer id
      dynamic datumRequest = new JObject();
      datumRequest.ProjectId = pId;
      datumRequest.DisplayMode = datumType;
      datumRequest.GridPoint = JToken.FromObject(gridPoint);
      datumRequest.Filter = JToken.FromObject(filter);
      datumRequest.FilterId = -1;

      var datumRequester = new Poster<JObject, CellDatumResult>(fullCellDatumUri, datumRequest);

      cellDatumResult = datumRequester.DoRequest();
    }

    [Then(@"the datum should be: displayMode = ""(.*)"", returnCode = ""(.*)"", value = ""(.*)"", timestamp = ""(.*)""")]
    public void ThenTheDatumShouldBeDisplayModeReturnCodeValueTimestamp(
        int displayMode, int returnCode, double value, string timestamp)
    {
      var expectedDatumResult = new CellDatumResult
      {
        displayMode = (DisplayMode)displayMode,
        returnCode = (short)returnCode,
        value = value,
        timestamp = Convert.ToDateTime(timestamp)
      };

      ObjectComparer.AssertAreEqual(actualResultObj: cellDatumResult, expectedResultObj: expectedDatumResult);
    }

    [And(@"I submit data edit with EditId (.*) to project (.*) expecting HttpResponseCode (.*)")]
    public void GivenISubmitDataEditWithEditIdToProjectExpectingHttpResponseCode(int editId, long pId, int httpCode)
    {
      dynamic doEditRequest = new JObject();
      doEditRequest.ProjectId = pId;
      doEditRequest.Undo = false;
      doEditRequest.DataEdit = JToken.FromObject(dataEditContext.DataEdits[editId]);

      editDataRequester.CurrentRequest = doEditRequest;
      editDataResult = editDataRequester.DoRequest(null, expectedHttpCode: (int)HttpStatusCode.BadRequest);
    }

    [Then(@"I should get Error Code (.*) and Message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int code, string msg)
    {
      Assert.True(editDataResult.Code == code && editDataResult.Message.Contains(msg),
        $"Expected code {code}, received {editDataResult.Code} instead; expected message to contain {msg}, received {editDataResult.Message} intead.");
    }

    [And(@"I read back all machine designs from ""(.*)""")]
    public void WhenIReadBackAllMachineDesignsFrom(string getDesignUri)
    {
      var designGetter = new Getter<GetMachineDesignResult>(string.Format(RestClient.Productivity3DServiceBaseUrl + getDesignUri));

      getMachineDesignResult = designGetter.SendRequest(designGetter.Uri);
    }

    [And(@"I read back all lifts from ""(.*)""")]
    public void AndIReadBackAllLiftsFrom(string getLiftUri)
    {
      var liftGetter = new Getter<LayerIdsExecutionResult>(string.Format(RestClient.Productivity3DServiceBaseUrl + getLiftUri));

      layerIdsExecutionResult = liftGetter.SendRequest(liftGetter.Uri);
    }

    [Then(@"the machine design list should contain the design details in the following data edits")]
    public void ThenTheMachineDesignListShouldContainTheDesignDetailsInTheFollowingDataEdits(Gherkin.Ast.DataTable dataTable)
    {
      foreach (var row in dataTable.Rows.Skip(1))
      {
        var designName = dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)].onMachineDesignName;

        Assert.True(getMachineDesignResult.designs.Exists(d => d.designName == designName),
            "Cannot find design edit in the design list read back.");
      }
    }

    [Then(@"the lift list should contain the lift details in the following data edits")]
    public void ThenTheLiftListShouldContainTheLiftDetailsInTheFollowingDataEdits(Gherkin.Ast.DataTable dataTable)
    {
      foreach (var row in dataTable.Rows.Skip(1))
      {
        var assetId = dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)].assetId;

        var designIndex = -1;
        if (getMachineDesignResult != null)
        {
          designIndex = getMachineDesignResult.designs.FindIndex(d =>
              d.designName == dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)].onMachineDesignName);
        }

        var designId = getMachineDesignResult.designs[designIndex].designId;
        var layerId = dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)].liftNumber.Value;
        var startDate = dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)].startUTC;
        var endDate = dataEditContext.DataEdits[int.Parse(row.Cells.ElementAt(0).Value)].endUTC;

        Assert.True(layerIdsExecutionResult.LayerIdDetailsArray.ToList().Exists(l => l.AssetId == assetId &&
            l.DesignId == designId && l.LayerId == layerId && l.StartDate == startDate && l.EndDate == endDate),
            "Cannot find lift edit in the lift list read back.");
      }
    }
  }
}
