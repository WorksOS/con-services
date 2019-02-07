using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.Enums;
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
    [DataRow(TAGProcServerProcessResultCode.OK, Type.PERMANENT, true, "success")]
    [DataRow(TAGProcServerProcessResultCode.Unknown, Type.TEMPORARY, false, "Tagfile Unknown error.")]
    [DataRow(TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure, Type.TEMPORARY, true, "OnSubmissionBase. Connection Failure.")]
    [DataRow(TAGProcServerProcessResultCode.OnSubmissionVerbConnectionFailure, Type.TEMPORARY, true, "OnSubmissionVerb. Connection Failure.")]
    [DataRow(TAGProcServerProcessResultCode.OnSubmissionResultConnectionFailure, Type.TEMPORARY, true, "OnSubmissionResult. Connection Failure.")]
    [DataRow(TAGProcServerProcessResultCode.FileReaderCorruptedTAGFileData, Type.PERMANENT, true, "The TAG file was found to be corrupted on its pre-processing scan.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseMachineUnknownMachine, Type.TEMPORARY, false, "OnChooseMachine. Unknown Machine AssetID.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseMachineInvalidTagFile, Type.PERMANENT, true, "OnChooseMachine. Invalid TagFile on selecting machine AssetID.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseMachineInvalidSubscriptions, Type.TEMPORARY, false, "OnChooseMachine. Machine Subscriptions Invalid.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseMachineUnableToDetermineMachine, Type.TEMPORARY, false, "OnChooseMachine. Unable To Determine Machine.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseDataModelUnableToDetermineDataModel, Type.TEMPORARY, true, "OnChooseDataModel. Unable To Determine DataModel.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseDataModelCouldNotConvertDataModelBoundaryToGrid, Type.TEMPORARY, true, "OnChooseDataModel. Could Not Convert DataModel Boundary To Grid.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseDataModelNoGridEpochsFoundInTAGFile, Type.PERMANENT, true, "OnChooseDataModel. No GridEpochs Found In TAGFile.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices, Type.TEMPORARY, true, "OnChooseDataModel. Supplied DataModel Boundary Contains Insufficient Vertices.")]
    [DataRow(TAGProcServerProcessResultCode.OnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary, Type.TEMPORARY, true, "OnChooseDataModel. First Epoch Blade Position Does Not Lie Within Project Boundary.")]
    [DataRow(TAGProcServerProcessResultCode.FailedEventDateValidation, Type.NULL, null, "OnOverrideEvent. Failed on event's date validation.")]
    [DataRow(TAGProcServerProcessResultCode.InvalidTagFileSubmissionMessageType, Type.NULL, null, "OnProcessTAGFile. Invalid tag file submission message type.")]
    [DataRow(TAGProcServerProcessResultCode.TAGFileAlreadyExistsInProcessingFolderForDataModel, Type.PERMANENT, true, "OnProcessTAGFile. TAG file already exists in data model's processing folder.")]
    [DataRow(TAGProcServerProcessResultCode.TAGFileAlreadyExistsInProcessingArchivalQueueForDataModel, Type.PERMANENT, true, "OnProcessTAGFile. TAG file already exists in data model's processing archival queue.")]
    [DataRow(TAGProcServerProcessResultCode.ServiceStopped, Type.TEMPORARY, false, "OnProcessTAGFile. Service has been stopped.")]
    [DataRow(TAGProcServerProcessResultCode.FailedValidation, Type.NULL, null, "OnOverrideEvent. Failed on target data validation.")]
    [DataRow(TAGProcServerProcessResultCode.TFAServiceError, Type.TEMPORARY, false, "TFA service error. Can not request Project or Asset from TFA.")]
    public void Should_set_fields_correctly_When_given_Raptor_response_code(TAGProcServerProcessResultCode code, string type, bool? continuable, string message)
    {
      var helper = new TagFileProcessResultHelper(code);

      Assert.AreEqual((int)code, helper.Code);
      Assert.AreEqual(type, helper.Type);
      Assert.AreEqual(continuable, helper.Continuable);
      Assert.AreEqual(message, helper.Message);
    }

    [TestMethod]
    [DataRow(TRexTagFileResultCode.Valid, 0, Type.PERMANENT, true, "success")]
    [DataRow(TRexTagFileResultCode.TRexUnknownException, 19, Type.TEMPORARY, false, "OnProcessTAGFile. Service has been stopped.")]
    [DataRow(TRexTagFileResultCode.TRexInvalidTagfile, 5, Type.PERMANENT, true, "The TAG file was found to be corrupted on its pre-processing scan.")]
    [DataRow(TRexTagFileResultCode.TfaException, 21, Type.TEMPORARY, false, "TFA service error. Can not request Project or Asset from TFA.")]
    [DataRow(TRexTagFileResultCode.TRexQueueSubmissionError, 17, Type.PERMANENT, true, "OnProcessTAGFile. TAG file already exists in data model's processing folder.")]
    [DataRow(TRexTagFileResultCode.TrexTagFileReaderError, 5, Type.PERMANENT, true, "The TAG file was found to be corrupted on its pre-processing scan.")]
    [DataRow(TRexTagFileResultCode.TFABadRequestInvalidTimeOfPosition, 15, Type.NULL, null, "OnOverrideEvent. Failed on event's date validation.")]

    [DataRow(TRexTagFileResultCode.TRexBadRequestMissingProjectUid, 22, Type.NULL, null, "Internal Error. Failed validation.")]
    [DataRow(TRexTagFileResultCode.TFABadRequestInvalidLatitude, 22, Type.NULL, null, "Internal Error. Failed validation.")]
    [DataRow(TRexTagFileResultCode.TFABadRequestInvalidLongitude, 22, Type.NULL, null, "Internal Error. Failed validation.")]
    [DataRow(TRexTagFileResultCode.TFABadRequestInvalidDeviceType, 22, Type.NULL, null, "Internal Error. Failed validation.")]
    [DataRow(TRexTagFileResultCode.TFABadRequestInvalidProjectUid, 22, Type.NULL, null, "Internal Error. Failed validation.")]
    [DataRow(TRexTagFileResultCode.TFAInternalDatabaseException, 23, Type.TEMPORARY, true, "TFA service database error. Can not request Project or Asset from TFA.")]
    [DataRow(TRexTagFileResultCode.TFABadRequestMissingRadioSerialAndTccOrgId, 24, Type.PERMANENT, true, "Bad Request. Request requires either RadioSerial or TccOrgId.")]
    [DataRow(TRexTagFileResultCode.TFAManualProjectNotFound, 25, Type.TEMPORARY, false, "TFA Processing Error: Unable to find the Project requested.")]
    [DataRow(TRexTagFileResultCode.TFAManualAssetFoundButNoSubsOrProjectFound, 26, Type.TEMPORARY, false, "TFA Processing Error: Manual Import: got Asset. Unable to locate any valid Project, or Asset subscriptions.")]
    [DataRow(TRexTagFileResultCode.TFAManualNoAssetFoundAndNoProjectSubs, 27, Type.TEMPORARY, false, "TFA Processing Error: Manual Import: no Asset provided or identifyable. Unable to locate any valid Project subscriptions.")]
    [DataRow(TRexTagFileResultCode.TFAManualNoIntersectingProjectsFound, 28, Type.TEMPORARY, false, "TFA Processing Error: Manual Import: no intersecting Projects found.")]

    [DataRow(TRexTagFileResultCode.TFAManualProjectDoesNotIntersectTimeAndLocation, 29, Type.TEMPORARY, false, "TFA Processing Error: Manual Import: Project does not intersect location and time provided.")]
    [DataRow(TRexTagFileResultCode.TFAManualProjectIsArchived, 30, Type.TEMPORARY, false, "TFA Processing Error: Manual Import: cannot import to an archived Project.")]
    [DataRow(TRexTagFileResultCode.TFAManualProjectIsCivilType, 31, Type.TEMPORARY, false, "TFA Processing Error: Manual Import: cannot import to a Civil type Project.")]
    [DataRow(TRexTagFileResultCode.TFAManualLandfillHasNoSubsAtThisTime, 32, Type.TEMPORARY, false, "TFA Processing Error: Manual Import: landfill Project does not have a valid subscription at that time.")]
    [DataRow(TRexTagFileResultCode.TFAAutoNoAssetOrTccOrgIdFound, 33, Type.TEMPORARY, false, "TFA Processing Error: Auto Import: no Asset or TccOrgId is identifiable from the request.")]
    [DataRow(TRexTagFileResultCode.TFAAutoAssetOrTccOrgIdFoundButNoProject, 34, Type.TEMPORARY, false, "TFA Processing Error: Auto Import: for this RadioSerial/TccOrgId, no Project meets the time/location/subscription requirements.")]
    [DataRow(TRexTagFileResultCode.TFAAutoMultipleProjectsMatchCriteria, 35, Type.TEMPORARY, false, "TFA Processing Error: Auto Import: more than 1 Project meets the time/location/subscription requirements.")]
    [DataRow(TRexTagFileResultCode.TFAManualValidProjectsFoundButNotRequestedOne, 36, Type.TEMPORARY, false, "TFA Processing Error: Manual Import: intersecting Projects found, but not the one requested.")]
    public void Should_set_fields_correctly_When_given_TRex_response_code(TRexTagFileResultCode resultCode, int codeToReturn, string type, bool? continuable, string message)
    {
      // codeToReturn < 22 maps to a Raptor code. >21 are new to TRex
      var helper = new TagFileProcessResultHelper(resultCode);

      Assert.AreEqual(codeToReturn, helper.Code);
      Assert.AreEqual(type, helper.Type);
      Assert.AreEqual(continuable, helper.Continuable);
      Assert.AreEqual(message, helper.Message);
    }

  }
}
