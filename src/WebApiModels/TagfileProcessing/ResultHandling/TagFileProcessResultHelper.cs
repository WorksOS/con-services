using TAGProcServiceDecls;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling
{
  public struct TagFileProcessResultHelper
  {
    public int Code { get; }
    public string Message { get; }
    public string Type { get; }
    public bool? Continuable { get; }

    public TagFileProcessResultHelper(TTAGProcServerProcessResult resultCode) : this()
    {
      Code = (int)resultCode;

      var result = MapProcessResult(resultCode);

      Message = result.message;
      Type = result.type;
      Continuable = result.continuable;
    }

    private static (string message, string type, bool? continuable) MapProcessResult(TTAGProcServerProcessResult code)
    {
      const string temporary = "Temporary";
      const string permanent = "Permanent";

      switch (code)
      {
        case TTAGProcServerProcessResult.tpsprOK:
          {
            return (message: "Success.", type: permanent, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprUnknown:
          {
            return (message: "Tagfile Unknown error.", type: temporary, continuable: false);
          }
        case TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure:
          {
            return (message: "OnSubmissionBase. Connection Failure.", type: temporary, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprOnSubmissionVerbConnectionFailure:
          {
            return (message: "OnSubmissionVerb. Connection Failure.", type: temporary, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprOnSubmissionResultConnectionFailure:
          {
            return (message: "OnSubmissionResult. Connection Failure.", type: temporary, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData:
          {
            return (message: "The TAG file was found to be corrupted on its pre-processing scan.", type: permanent, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseMachineUnknownMachine:
          {
            return (message: "OnChooseMachine. Unknown Machine AssetID.", type: temporary, continuable: false);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidTagFile:
          {
            return (message: "OnChooseMachine. Invalid TagFile on selecting machine AssetID.", type: permanent, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions:
          {
            return (message: "OnChooseMachine. Machine Subscriptions Invalid.", type: temporary, continuable: false);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseMachineUnableToDetermineMachine:
          {
            return (message: "OnChooseMachine. Unable To Determine Machine.", type: temporary, continuable: false);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelUnableToDetermineDataModel:
          {
            return (message: "OnChooseDataModel. Unable To Determine DataModel.", type: temporary, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
          {
            return (message: "OnChooseDataModel. Could Not Convert DataModel Boundary To Grid.", type: temporary, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelNoGridEpochsFoundInTAGFile:
          {
            return (message: "OnChooseDataModel. No GridEpochs Found In TAGFile.", type: permanent, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:
          {
            return (message: "OnChooseDataModel. Supplied DataModel Boundary Contains Insufficient Vertices.", type: temporary, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
          {
            return (message: "OnChooseDataModel. First Epoch Blade Position Does Not Lie Within Project Boundary.", type: temporary, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprFailedEventDateValidation:
          {
            return (message: "OnOverrideEvent. Failed on event's date validation.", type: string.Empty, continuable: null);
          }
        case TTAGProcServerProcessResult.tpsprInvalidTagFileSubmissionMessageType:
          {
            return (message: "OnProcessTAGFile. Invalid tag file submission message type.", type: string.Empty, continuable: null);
          }
        case TTAGProcServerProcessResult.tpsprTAGFileAlreadyExistsInProcessingFolderForDataModel:
          {
            return (message: "OnProcessTAGFile. TAG file already exists in data model's processing folder.", type: permanent, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprTAGFileAlreadyExistsInProcessingArchivalQueueForDataModel:
          {
            return (message: "OnProcessTAGFile. TAG file already exists in data model's processing archival queue.", type: permanent, continuable: true);
          }
        case TTAGProcServerProcessResult.tpsprServiceStopped:
          {
            return (message: "OnProcessTAGFile. Service has been stopped.", type: temporary, continuable: false);
          }
        case TTAGProcServerProcessResult.tpsprFailedValidation:
          {
            return (message: "OnOverrideEvent. Failed on target data validation.", type: string.Empty, continuable: null);
          }
        case TTAGProcServerProcessResult.tpsprTFAServiceError:
          {
            return (message: "TFA service error. Can not request Project or Asset from TFA.", type: temporary, continuable: false);
          }
        default:
          {
            return (message: "Unknown error", type: temporary, continuable: true);
          }
      }
    }
  }
}
