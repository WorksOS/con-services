using Microsoft.VisualStudio.TestTools.UnitTesting;
using TAGProcServiceDecls;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.ResultHandling
{
  [TestClass]
  public class TagFileProcessResultHelperTests
  {
    internal class Type
    {
      public const string NULL = "";
      public const string PERMANENT = "Permanent";
      public const string TEMPORARY = "Temporary";
    }

    [TestMethod]
    [DataRow(TTAGProcServerProcessResult.tpsprOK, Type.PERMANENT, true, "success")]
    [DataRow(TTAGProcServerProcessResult.tpsprUnknown, Type.TEMPORARY, false, "Tagfile Unknown error.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure, Type.TEMPORARY, true, "OnSubmissionBase. Connection Failure.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnSubmissionVerbConnectionFailure, Type.TEMPORARY, true, "OnSubmissionVerb. Connection Failure.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnSubmissionResultConnectionFailure, Type.TEMPORARY, true, "OnSubmissionResult. Connection Failure.")]
    [DataRow(TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData, Type.PERMANENT, true, "The TAG file was found to be corrupted on its pre-processing scan.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseMachineUnknownMachine, Type.TEMPORARY, false, "OnChooseMachine. Unknown Machine AssetID.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidTagFile, Type.PERMANENT, true, "OnChooseMachine. Invalid TagFile on selecting machine AssetID.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions, Type.TEMPORARY, false, "OnChooseMachine. Machine Subscriptions Invalid.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseMachineUnableToDetermineMachine, Type.TEMPORARY, false, "OnChooseMachine. Unable To Determine Machine.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseDataModelUnableToDetermineDataModel, Type.TEMPORARY, true, "OnChooseDataModel. Unable To Determine DataModel.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid, Type.TEMPORARY, true, "OnChooseDataModel. Could Not Convert DataModel Boundary To Grid.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseDataModelNoGridEpochsFoundInTAGFile, Type.PERMANENT, true, "OnChooseDataModel. No GridEpochs Found In TAGFile.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices, Type.TEMPORARY, true, "OnChooseDataModel. Supplied DataModel Boundary Contains Insufficient Vertices.")]
    [DataRow(TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary, Type.TEMPORARY, true, "OnChooseDataModel. First Epoch Blade Position Does Not Lie Within Project Boundary.")]
    [DataRow(TTAGProcServerProcessResult.tpsprFailedEventDateValidation, Type.NULL, null, "OnOverrideEvent. Failed on event's date validation.")]
    [DataRow(TTAGProcServerProcessResult.tpsprInvalidTagFileSubmissionMessageType, Type.NULL, null, "OnProcessTAGFile. Invalid tag file submission message type.")]
    [DataRow(TTAGProcServerProcessResult.tpsprTAGFileAlreadyExistsInProcessingFolderForDataModel, Type.PERMANENT, true, "OnProcessTAGFile. TAG file already exists in data model's processing folder.")]
    [DataRow(TTAGProcServerProcessResult.tpsprTAGFileAlreadyExistsInProcessingArchivalQueueForDataModel, Type.PERMANENT, true, "OnProcessTAGFile. TAG file already exists in data model's processing archival queue.")]
    [DataRow(TTAGProcServerProcessResult.tpsprServiceStopped, Type.TEMPORARY, false, "OnProcessTAGFile. Service has been stopped.")]
    [DataRow(TTAGProcServerProcessResult.tpsprFailedValidation, Type.NULL, null, "OnOverrideEvent. Failed on target data validation.")]
    [DataRow(TTAGProcServerProcessResult.tpsprTFAServiceError, Type.TEMPORARY, false, "TFA service error. Can not request Project or Asset from TFA.")]
    public void Should_set_fields_correctly_When_given_Raptor_response_code(TTAGProcServerProcessResult code, string type, bool? continuable, string message)
    {
      var helper = new TagFileProcessResultHelper(code);

      Assert.AreEqual((int)code, helper.Code);
      Assert.AreEqual(type, helper.Type);
      Assert.AreEqual(continuable, helper.Continuable);
      Assert.AreEqual(message, helper.Message);
    }
  }
}
