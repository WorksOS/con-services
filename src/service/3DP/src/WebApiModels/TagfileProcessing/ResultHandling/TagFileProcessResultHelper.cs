using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling
{
  public struct TagFileProcessResultHelper
  {
    public int Code { get; }
    public string Message { get; }
    public string Type { get; }
    public bool? Continuable { get; }

    public TagFileProcessResultHelper(TAGProcServerProcessResultCode resultCode) : this()
    {
      Code = (int) resultCode;

      var result = MapProcessResult(resultCode);

      Message = result.message;
      Type = result.type;
      Continuable = result.continuable;
    }

    public TagFileProcessResultHelper(TRexTagFileResultCode resultCode) : this()
    {
      var result = MapProcessResult(resultCode);

      Code = result.code;
      Message = result.message;

      Type = result.type;
      Continuable = result.continuable;
    }

    private static (int code, string message, string type, bool? continuable) MapProcessResult(
      TAGProcServerProcessResultCode resultCode)
    {
      const string temporary = "Temporary";
      const string permanent = "Permanent";

      switch (resultCode)
      {
        case TAGProcServerProcessResultCode.OK:
        {
          return (code: (int) resultCode, message: ContractExecutionResult.DefaultMessage, type: permanent,
            continuable: true);
        }
        case TAGProcServerProcessResultCode.Unknown:
        {
          return (code: (int) resultCode, message: "Tagfile Unknown error.", type: temporary, continuable: false);
        }
        case TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure:
        {
          return (code: (int) resultCode, message: "OnSubmissionBase. Connection Failure.", type: temporary,
            continuable: true);
        }
        case TAGProcServerProcessResultCode.OnSubmissionVerbConnectionFailure:
        {
          return (code: (int) resultCode, message: "OnSubmissionVerb. Connection Failure.", type: temporary,
            continuable: true);
        }
        case TAGProcServerProcessResultCode.OnSubmissionResultConnectionFailure:
        {
          return (code: (int) resultCode, message: "OnSubmissionResult. Connection Failure.", type: temporary,
            continuable: true);
        }
        case TAGProcServerProcessResultCode.FileReaderCorruptedTAGFileData:
        {
          return (code: (int) resultCode, message: "The TAG file was found to be corrupted on its pre-processing scan.",
            type: permanent,
            continuable: true);
        }
        case TAGProcServerProcessResultCode.OnChooseMachineUnknownMachine:
        {
          return (code: (int) resultCode, message: "OnChooseMachine. Unknown Machine AssetID.", type: temporary,
            continuable: false);
        }
        case TAGProcServerProcessResultCode.OnChooseMachineInvalidTagFile:
        {
          return (code: (int) resultCode, message: "OnChooseMachine. Invalid TagFile on selecting machine AssetID.",
            type: permanent,
            continuable: true);
        }
        case TAGProcServerProcessResultCode.OnChooseMachineInvalidSubscriptions:
        {
          return (code: (int) resultCode, message: "OnChooseMachine. Machine Subscriptions Invalid.", type: temporary,
            continuable: false);
        }
        case TAGProcServerProcessResultCode.OnChooseMachineUnableToDetermineMachine:
        {
          return (code: (int) resultCode, message: "OnChooseMachine. Unable To Determine Machine.", type: temporary,
            continuable: false);
        }
        case TAGProcServerProcessResultCode.OnChooseDataModelUnableToDetermineDataModel:
        {
          return (code: (int) resultCode, message: "OnChooseDataModel. Unable To Determine DataModel.", type: temporary,
            continuable: true);
        }
        case TAGProcServerProcessResultCode.OnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
        {
          return (code: (int) resultCode, message: "OnChooseDataModel. Could Not Convert DataModel Boundary To Grid.",
            type: temporary,
            continuable: true);
        }
        case TAGProcServerProcessResultCode.OnChooseDataModelNoGridEpochsFoundInTAGFile:
        {
          return (code: (int) resultCode, message: "OnChooseDataModel. No GridEpochs Found In TAGFile.",
            type: permanent, continuable: true);
        }
        case TAGProcServerProcessResultCode.OnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:
        {
          return (code: (int) resultCode,
            message: "OnChooseDataModel. Supplied DataModel Boundary Contains Insufficient Vertices.",
            type: temporary, continuable: true);
        }
        case TAGProcServerProcessResultCode.OnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
        {
          return (code: (int) resultCode,
            message: "OnChooseDataModel. First Epoch Blade Position Does Not Lie Within Project Boundary.",
            type: temporary, continuable: true);
        }
        case TAGProcServerProcessResultCode.FailedEventDateValidation:
        {
          return (code: (int) resultCode, message: "OnOverrideEvent. Failed on event's date validation.",
            type: string.Empty,
            continuable: null);
        }
        case TAGProcServerProcessResultCode.InvalidTagFileSubmissionMessageType:
        {
          return (code: (int) resultCode, message: "OnProcessTAGFile. Invalid tag file submission message type.",
            type: string.Empty,
            continuable: null);
        }
        case TAGProcServerProcessResultCode.TAGFileAlreadyExistsInProcessingFolderForDataModel:
        {
          return (code: (int) resultCode,
            message: "OnProcessTAGFile. TAG file already exists in data model's processing folder.",
            type: permanent, continuable: true);
        }
        case TAGProcServerProcessResultCode.TAGFileAlreadyExistsInProcessingArchivalQueueForDataModel:
        {
          return (code: (int) resultCode,
            message: "OnProcessTAGFile. TAG file already exists in data model's processing archival queue.",
            type: permanent, continuable: true);
        }
        case TAGProcServerProcessResultCode.ServiceStopped:
        {
          return (code: (int) resultCode, message: "OnProcessTAGFile. Service has been stopped.", type: temporary,
            continuable: false);
        }
        case TAGProcServerProcessResultCode.FailedValidation:
        {
          return (code: (int) resultCode, message: "OnOverrideEvent. Failed on target data validation.",
            type: string.Empty, continuable: null);
        }
        case TAGProcServerProcessResultCode.TFAServiceError:
        {
          return (code: (int) resultCode, message: "TFA service error. Can not request Project or Asset from TFA.",
            type: temporary,
            continuable: false);
        }
        default:
        {
          return (code: (int) resultCode, message: "Unknown error", type: temporary, continuable: true);
        }
      }
    }

    private static (int code, string message, string type, bool? continuable) MapProcessResult(
      TRexTagFileResultCode resultCode)
    {
      const string temporary = "Temporary";
      const string permanent = "Permanent";

      switch (resultCode)
      {
        case TRexTagFileResultCode.Valid:
        {
          return (code: 0, message: ContractExecutionResult.DefaultMessage, type: permanent, continuable: true);
        }

        case TRexTagFileResultCode.TRexInvalidTagfile:
        case TRexTagFileResultCode.TrexTagFileReaderError:
        {
          return (code: 5, message: "The TAG file was found to be corrupted on its pre-processing scan.",
            type: permanent,
            continuable: true);
        }

        case TRexTagFileResultCode.TFABadRequestInvalidTimeOfPosition:
        {
          return (code: 15, message: "OnOverrideEvent. Failed on event's date validation.", type: string.Empty,
            continuable: null);
        }

        case TRexTagFileResultCode.TRexQueueSubmissionError:
        {
          return (code: 17,
            message: "OnProcessTAGFile. TAG file already exists in data model's processing folder.",
            type: permanent, continuable: true);
        }

        case TRexTagFileResultCode.TRexUnknownException:
        {
          return (code: 19, message: "OnProcessTAGFile. Service has been stopped.", type: temporary,
            continuable: false);
        }

        case TRexTagFileResultCode.TfaException:
        {
          return (code: 21, message: "TFA service error. Can not request Project or Asset from TFA.",
            type: temporary, continuable: false);
        }

        // following are new messages for CTCT

        // user should never get these as these should have been validated in TRex tagFileReader which occurs before calling TFA.
        case TRexTagFileResultCode.TRexBadRequestMissingProjectUid:
        case TRexTagFileResultCode.TFABadRequestInvalidLatitude:
        case TRexTagFileResultCode.TFABadRequestInvalidLongitude:
        case TRexTagFileResultCode.TFABadRequestInvalidDeviceType:
        case TRexTagFileResultCode.TFABadRequestInvalidProjectUid:
        case TRexTagFileResultCode.TFAManualInternalErrorUnhandledPath:
        {
          return (code: 22, message: "Internal Error. Failed validation.", type: string.Empty, continuable: null);
        }

        case TRexTagFileResultCode.TFAInternalDatabaseException:
        {
          return (code: 23, message: "TFA service database error. Can not request Project or Asset from TFA.",
            type: temporary, continuable: true);
        }

        case TRexTagFileResultCode.TFABadRequestMissingRadioSerialAndTccOrgId:
        {
          return (code: 24, message: "Bad Request. Request requires either RadioSerial or TccOrgId.",
            type: permanent, continuable: true);
        }

        // the following errors require the user to make some VL change, so no point in re-trying.
        // These are expanded from the raptor errors 6,8 and 9
        case TRexTagFileResultCode.TFAManualProjectNotFound:
        {
          return (code: 25, message: "TFA Processing Error: Unable to find the Project requested.",
            type: temporary, continuable: false);
        }

        case TRexTagFileResultCode.TFAManualAssetFoundButNoSubsOrProjectFound:
        {
          return (code: 26,
            message:
            "TFA Processing Error: Manual Import: got Asset. Unable to locate any valid Project, or Asset subscriptions.",
            type: temporary, continuable: false);
        }

        case TRexTagFileResultCode.TFAManualNoAssetFoundAndNoProjectSubs:
        {
          return (code: 27,
            message:
            "TFA Processing Error: Manual Import: no Asset provided or identifyable. Unable to locate any valid Project subscriptions.",
            type: temporary, continuable: false);
        }

        case TRexTagFileResultCode.TFAManualNoIntersectingProjectsFound:
        {
          return (code: 28, message: "TFA Processing Error: Manual Import: no intersecting Projects found.",
            type: temporary, continuable: false);
        }

        case TRexTagFileResultCode.TFAManualProjectDoesNotIntersectTimeAndLocation:
        {
          return (code: 29,
            message: "TFA Processing Error: Manual Import: Project does not intersect location and time provided.",
            type: temporary, continuable: false);
        }
        case TRexTagFileResultCode.TFAManualProjectIsArchived:
        {
          return (code: 30, message: "TFA Processing Error: Manual Import: cannot import to an archived Project.",
            type: temporary, continuable: false);
        }
        case TRexTagFileResultCode.TFAManualProjectIsCivilType:
        {
          return (code: 31,
            message: "TFA Processing Error: Manual Import: cannot import to a Civil type Project.", type: temporary,
            continuable: false);
        }
        case TRexTagFileResultCode.TFAManualLandfillHasNoSubsAtThisTime:
        {
          return (code: 32,
            message:
            "TFA Processing Error: Manual Import: landfill Project does not have a valid subscription at that time.",
            type: temporary, continuable: false);
        }
        case TRexTagFileResultCode.TFAAutoNoAssetOrTccOrgIdFound:
        {
          return (code: 33,
            message: "TFA Processing Error: Auto Import: no Asset or TccOrgId is identifiable from the request.",
            type: temporary, continuable: false);
        }
        case TRexTagFileResultCode.TFAAutoAssetOrTccOrgIdFoundButNoProject:
        {
          return (code: 34,
            message:
            "TFA Processing Error: Auto Import: for this RadioSerial/TccOrgId, no Project meets the time/location/subscription requirements.",
            type: temporary, continuable: false);
        }
        case TRexTagFileResultCode.TFAAutoMultipleProjectsMatchCriteria:
        {
          return (code: 35,
            message:
            "TFA Processing Error: Auto Import: more than 1 Project meets the time/location/subscription requirements.",
            type: temporary, continuable: false);
        }
        case TRexTagFileResultCode.TFAManualValidProjectsFoundButNotRequestedOne:
        {
          return (code: 36,
            message: "TFA Processing Error: Manual Import: intersecting Projects found, but not the one requested.",
            type: temporary, continuable: false);
        }

        default:
        {
          return (code: (int)resultCode, message: "Unknown error", type: temporary, continuable: false);
        }


        // Raptor and B&A only (not supported for TRex)
        //case TTAGProcServerProcessResult.OnChooseDataModelUnableToDetermineDataModel:

        // Raptor only
        // Raptor only
        //case TTAGProcServerProcessResult.OnSubmissionBaseConnectionFailure:
        //case TTAGProcServerProcessResult.OnSubmissionVerbConnectionFailure:
        //case TTAGProcServerProcessResult.OnSubmissionResultConnectionFailure:
        //case TTAGProcServerProcessResult.OnChooseMachineUnknownMachine:
        //case TTAGProcServerProcessResult.OnChooseMachineInvalidTagFile:
        //case TTAGProcServerProcessResult.OnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
        //case TTAGProcServerProcessResult.OnChooseDataModelNoGridEpochsFoundInTAGFile:
        //case TTAGProcServerProcessResult.OnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:
        //case TTAGProcServerProcessResult.OnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
        //case TTAGProcServerProcessResult.TAGFileAlreadyExistsInProcessingArchivalQueueForDataModel:
        //case TTAGProcServerProcessResult.InvalidTagFileSubmissionMessageType:

        // unrelated to tagfiles - to do with lift/design overide. shouldn't be returned at all
        //case TTAGProcServerProcessResult.FailedValidation:

        // no longer applicable with TRex: no more OnChooseMachine or OnChooseDataModel
        //case TTAGProcServerProcessResult.OnChooseMachineInvalidSubscriptions:
        //{
        //  return (message: "OnChooseMachine. Machine Subscriptions Invalid.", type: temporary, continuable: false);
        //}
        //case TTAGProcServerProcessResult.OnChooseMachineUnableToDetermineMachine:
        //{
        //  return (message: "OnChooseMachine. Unable To Determine Machine.", type: temporary, continuable: false);
        //}

      }
    }
  }
}
