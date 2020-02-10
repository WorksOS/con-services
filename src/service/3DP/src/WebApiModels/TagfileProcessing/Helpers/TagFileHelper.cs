using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Helpers
{

  /// <summary> </summary>
  public static class TagFileHelper
  {
    /// <summary>
    /// Sends tag file to TRex endpoint, retrieving result 
    /// </summary>
    public static async Task<ContractExecutionResult> SendTagFileToTRex(CompactionTagFileRequest compactionTagFileRequest,
      ITRexTagFileProxy tagFileProxy,
      ILogger log, IDictionary<string, string> customHeaders,
      bool isDirectSubmission = true)
    {
      var tRexResult = new ContractExecutionResult((int) TAGProcServerProcessResultCode.Unknown);
      try
      {
        if (isDirectSubmission)
          tRexResult = await tagFileProxy.SendTagFileDirect(compactionTagFileRequest, customHeaders);
        else
          tRexResult = await tagFileProxy.SendTagFileNonDirect(compactionTagFileRequest, customHeaders);

        return tRexResult;
      }
      catch (Exception e)
      {
        log.LogError(e, $"SendTagFileToTRex: returned exception");
      }

      return tRexResult;
    }

    /// <summary>
    /// Save the tagfile to an S3 bucket. Mirrors the folder structure in the tagfile harvester.
    /// </summary>
    public static Task ArchiveTagFile(
      IConfigurationStore configStore, ITransferProxy transferProxy, ILogger log,
      TAGProcServerProcessResultCode resultCode, MemoryStream data, string tagFileName, string tccOrgId, 
      TagFileSubmissionType tagFileSubmissionType, string s3bucketName)
    {
      // I have a feeling we will soon want to archive manual imports and split Direct by ECM500/TMC
      if (tagFileSubmissionType != TagFileSubmissionType.Auto && tagFileSubmissionType != TagFileSubmissionType.Direct)
        return (Task.CompletedTask);

      string folderName = null;
      switch (resultCode)
      {
        case TAGProcServerProcessResultCode.OK:
          folderName = configStore.GetValueString("TCCSynchProductionDataArchivedFolder") ?? "Production-Data (Archived)";
          break;

        case TAGProcServerProcessResultCode.OnChooseDataModelUnableToDetermineDataModel:
        case TAGProcServerProcessResultCode.OnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
        case TAGProcServerProcessResultCode.OnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
        case TAGProcServerProcessResultCode.OnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:
          folderName = configStore.GetValueString("TCCSynchProjectBoundaryIssueFolder") ?? "Project Boundary (Issue)";
          break;

        case TAGProcServerProcessResultCode.OnChooseMachineInvalidSubscriptions:
        case TAGProcServerProcessResultCode.OnChooseMachineUnableToDetermineMachine:
        case TAGProcServerProcessResultCode.OnChooseMachineUnknownMachine:
          folderName = configStore.GetValueString("TCCSynchSubscriptionIssueFolder") ?? "Subscription (Issue)";
          break;

        case TAGProcServerProcessResultCode.TFAServiceError:
          log.LogError($"{nameof(ArchiveTagFile)} (Raptor): TFA is likely down for {tagFileName} org {tccOrgId}");
          break;

        default:
          folderName = configStore.GetValueString("TCCSynchOtherIssueFolder") ?? "Other... (Issue)";
          break;
      }

      return UploadTagFile(transferProxy, log, data, tagFileName, tccOrgId, folderName, tagFileSubmissionType, s3bucketName);
    }

    /// <summary>
    /// Save the tagfile to an S3 bucket. Mirrors the folder structure in the tagfile harvester.
    /// </summary>
    public static Task ArchiveTagFile(IConfigurationStore configStore, ITransferProxy transferProxy, ILogger log,
      TRexTagFileResultCode resultCode, MemoryStream data, string tagFileName, string tccOrgId, 
      TagFileSubmissionType tagFileSubmissionType, string s3bucketName)
    {
      // I have a feeling we will soon want to archive manual imports and split Direct by ECM500/TMC
      if (tagFileSubmissionType != TagFileSubmissionType.Auto && tagFileSubmissionType != TagFileSubmissionType.Direct)
        return(Task.CompletedTask);

      string folderName = null;
      switch (resultCode)
      {
        case TRexTagFileResultCode.Valid:
          folderName = configStore.GetValueString("TCCSynchProductionDataArchivedFolder") ?? "Production-Data (Archived)";
          break;

        // this was a global catch-all applied by TagFileProcessor
        // when TFA.getProjectId() or getAssetId failed.
        //case TTAGProcServerProcessResult.OnChooseDataModelUnableToDetermineDataModel: 

        // These were raptor errors which don't appear to be generated by TRex
        //case TTAGProcServerProcessResult.OnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
        //case TTAGProcServerProcessResult.OnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
        //case TTAGProcServerProcessResult.OnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:

        // 'boundary' and 'Subscription' issues are too broad.
        //    The only thing they say is that re-subitting won't achieve anything until VL configuration is changed.
        //    The actual codes in TFA may be expanded (at the cost of performance) in future
        //    to be able to provide a list of possible causes
        // These indicate something wrong with VLink configuration (very broadly to do with project boundaries)
        case TRexTagFileResultCode.TFAManualAssetFoundButNoSubsOrProjectFound:
        case TRexTagFileResultCode.TFAManualNoIntersectingProjectsFound:
        case TRexTagFileResultCode.TFAManualProjectDoesNotIntersectTimeAndLocation:
        case TRexTagFileResultCode.TFAAutoAssetOrTccOrgIdFoundButNoProject:
        case TRexTagFileResultCode.TFAAutoMultipleProjectsMatchCriteria:
        case TRexTagFileResultCode.TFAManualValidProjectsFoundButNotRequestedOne:
          folderName = configStore.GetValueString("TCCSynchProjectBoundaryIssueFolder") ?? "Project Boundary (Issue)";
          break;

        // These indicate something wrong with VLink configuration (seldom to do with Subscriptions)
        case TRexTagFileResultCode.TFAManualNoAssetFoundAndNoProjectSubs:
        case TRexTagFileResultCode.TFAManualLandfillHasNoSubsAtThisTime:
          folderName = configStore.GetValueString("TCCSynchSubscriptionIssueFolder") ?? "Subscription (Issue)";
          break;

        // what should fall in this category from TRex? These should be retried.
        case TRexTagFileResultCode.TfaException:
        case TRexTagFileResultCode.TFAInternalDatabaseException:
          log.LogError($"{nameof(ArchiveTagFile)} (TRex): TFA is likely down for {tagFileName} org {tccOrgId}");
          break;

        /* which folder should these resultCodes get stored under?

        // these are a bad request
        case TRexTagFileResultCode.TFABadRequestInvalidLatitude:
        case TRexTagFileResultCode.TFABadRequestInvalidLongitude:
        case TRexTagFileResultCode.TFABadRequestInvalidTimeOfPosition:
        case TRexTagFileResultCode.TFABadRequestInvalidDeviceType:
        case TRexTagFileResultCode.TFABadRequestInvalidProjectUid:
        case TRexTagFileResultCode.TFABadRequestMissingRadioSerialAndTccOrgId:
        case TRexTagFileResultCode.TFAAutoNoAssetOrTccOrgIdFound:
        case TRexTagFileResultCode.TFAManualProjectIsArchived:
        case TRexTagFileResultCode.TFAManualProjectIsCivilType:
        case TRexTagFileResultCode.TFAManualProjectNotFound: 

        case TRexTagFileResultCode.TRexUnknownException:    
        case TRexTagFileResultCode.TRexQueueSubmissionError:

        // I don't know if these are re-tryable
        case TRexTagFileResultCode.TRexInvalidTagfile:
        case TRexTagFileResultCode.TrexTagFileReaderError:

        // these should never occur
        case TRexTagFileResultCode.TRexBadRequestMissingProjectUid:
        case TRexTagFileResultCode.TFAManualInternalErrorUnhandledPath:        
        */
        default:
          folderName = configStore.GetValueString("TCCSynchOtherIssueFolder") ?? "Other... (Issue)";
          break;
      }

      return UploadTagFile(transferProxy, log, data, tagFileName, tccOrgId, folderName, tagFileSubmissionType, s3bucketName);
    }

    /// <summary>
    /// Update the Tag file to S3
    ///  For Direct imports, tccOrdId is null
    /// </summary>
    /// <param name="data">Memory Stream for data</param>
    /// <param name="tagFileName">Tag file name</param>
    /// <param name="tccOrgId">TCC org id</param>
    /// <param name="folderName">Folder name for the tag file</param>
    public static async Task UploadTagFile(ITransferProxy transferProxy, ILogger log,
      MemoryStream data, string tagFileName, string tccOrgId, string folderName,
      TagFileSubmissionType tagFileSubmissionType, string s3bucketName)
    {
      if (string.IsNullOrEmpty(folderName))
        return;

      var path = GetS3Key(tagFileName, folderName, tccOrgId, tagFileSubmissionType);
      // S3 needs a full path including file
      var s3FullPath = path + tagFileName;
      log.LogDebug($"{nameof(UploadTagFile)}: Moving file {tagFileName} for org {tccOrgId} to {folderName} folder. Path: {path}, S3 Path: {s3FullPath}");


      using (var s3Stream = new MemoryStream())
      {
        // Transfer Proxy will dispose of the stream passed in, but we need it later
        // So we will create a new memory stream
        // Also we need to seek to the beginning each time as it will set the position to the end after copy (for both the src and dst)
        data.Seek(0, SeekOrigin.Begin);
        await data.CopyToAsync(s3Stream);
        s3Stream.Seek(0, SeekOrigin.Begin);
        transferProxy.UploadToBucket(s3Stream, s3FullPath, s3bucketName); 
      }
    }

    /// <summary>
    /// Gets the key in the S3 bucket to save the tagfile under.
    /// 
    ///   Example tagfile name: 0415J010SW--HOUK IR 29 16--170731225438.tag
    ///   Format: <display or ECM serial>--<machine name>--yyMMddhhmmss.tag
    ///   
    ///   Required folder structure for Direct submission is
    ///       <serial>--<machine name>/<archive folder>/<serial--machine name--date>/<tagfile>
    ///        e.g. 0415J010SW--HOUK IR 29 16/Production-Data (Archived)/0415J010SW--HOUK IR 29 16--170731/0415J010SW--HOUK IR 29 16--170731225438.tag
    ///   Required folder structure for Auto submission is
    ///       <TCC org id>/<machine name>/<archive folder>/<serial--machine name--date>/<tagfile>
    ///       e.g. 063a8ac3-a81e-4c28-8866-d6a13acbd666/HOUK IR 29 16/Production-Data (Archived)/0415J010SW--HOUK IR 29 16--170731/0415J010SW--HOUK IR 29 16--170731225438.tag
    ///
    /// </summary>
    /// <returns></returns>
    public static string GetS3Key(string tagFileName, string archiveFolder, string tccOrgId, TagFileSubmissionType tagFileSubmissionType)
    {
      const string separator = "--";
      string[] parts = tagFileName.Split(new string[] {separator}, StringSplitOptions.None);
      var nameWithoutTime = tagFileName.Substring(0, tagFileName.Length - 10);

      //TCC org ID is not provided with direct submission from machines
      var prefix = string.IsNullOrEmpty(tccOrgId) ? string.Empty : $"{tccOrgId}/";


      return tagFileSubmissionType == TagFileSubmissionType.Direct
        ? $"{parts[0]}{separator}{parts[1]}/{archiveFolder}/{nameWithoutTime}/"
        : $"{prefix}{parts[1]}/{archiveFolder}/{nameWithoutTime}/";
    }
  }
}

