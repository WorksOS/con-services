namespace VSS.Productivity3D.Models.Enums
{
  /// <summary>
  /// The set of generic response codes various parts of the system emit...
  /// </summary>
  public enum TAGProcServerProcessResultCode
  {
    OK,
    Unknown,
    OnSubmissionBaseConnectionFailure,
    OnSubmissionVerbConnectionFailure,
    OnSubmissionResultConnectionFailure,
    FileReaderCorruptedTAGFileData,
    OnChooseMachineUnknownMachine,
    OnChooseMachineInvalidTagFile,
    OnChooseMachineInvalidSubscriptions,
    OnChooseMachineUnableToDetermineMachine,
    OnChooseDataModelUnableToDetermineDataModel,
    OnChooseDataModelCouldNotConvertDataModelBoundaryToGrid,
    OnChooseDataModelNoGridEpochsFoundInTAGFile,
    OnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices,
    OnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary,
    FailedEventDateValidation,
    InvalidTagFileSubmissionMessageType,
    TAGFileAlreadyExistsInProcessingFolderForDataModel,
    TAGFileAlreadyExistsInProcessingArchivalQueueForDataModel,
    ServiceStopped,
    FailedValidation, // generic error
    TFAServiceError
  }
}
