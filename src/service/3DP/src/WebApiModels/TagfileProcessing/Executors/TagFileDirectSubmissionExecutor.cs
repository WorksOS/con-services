using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#if RAPTOR
using TAGProcServiceDecls;
using VLPDDecls;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  /// <summary>
  /// For submitting direct submitted TAG files to TRex and Raptor.
  /// for now: we will ALWAYS send to Raptor, but only send to TRex if configured.
  /// if TRex fails, then we will continue sending to Raptor
  /// </summary>
  public class TagFileDirectSubmissionExecutor : RequestExecutorContainer
  {
    private const string TCC_FILESPACE_CONFIG_KEY = "TCC_TAGFILE_FILESPACEID";

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CompactionTagFileRequest>(item);
      var result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError);
#if RAPTOR
      var useTrexGateway = UseTRexGateway("ENABLE_TREX_GATEWAY_TAGFILE");
      var useRaptorGateway = UseRaptorGateway("ENABLE_RAPTOR_GATEWAY_TAGFILE");

      if (useTrexGateway)
      {
#endif
        request.Validate();
        result = await CallTRexEndpoint(request);

#if RAPTOR
      }

      if (useRaptorGateway)
      {
        var tfRequest = TagFileRequestLegacy.CreateTagFile(request.FileName, request.Data,
          VelociraptorConstants.NO_PROJECT_ID, null, VelociraptorConstants.NO_MACHINE_ID, false, false);
        tfRequest.Validate();
        if (tfRequest.ProjectId == VelociraptorConstants.NO_PROJECT_ID && tfRequest.Boundary != null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to process tagfile with error: Automatic tag file submissions cannot include boundary fence."));
        }

        result = CallRaptorEndpoint(tfRequest);
      }

      // For direct endpoint, tag files are archived to s3, mainly for support.
      // For nonDirect the harvester uses TCC (for now, soon to be s3)
      var data = new MemoryStream(request.Data);
      if (useRaptorGateway)
        await ArchiveFile((TAGProcServerProcessResultCode) result.Code, data, request.FileName, request.OrgId);
      else if (useTrexGateway)
        await ArchiveFile((TRexTagFileResultCode)result.Code, data, request.FileName, request.OrgId);
      else
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "No tag file processing server configured."));
      }
#endif
      return result;
    }

    private async Task<ContractExecutionResult> CallTRexEndpoint(CompactionTagFileRequest request)
    {
      var returnResult = await TagFileHelper.SendTagFileToTRex(request,
        tRexTagFileProxy, log, customHeaders, true);

      log.LogInformation($"{nameof(CallTRexEndpoint)} completed: filename {request.FileName} result {JsonConvert.SerializeObject(returnResult)}");

      var convertedResult = TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper((TRexTagFileResultCode)returnResult.Code));
      if (convertedResult.Code != 0)
        log.LogDebug($"{nameof(CallTRexEndpoint)}: Failed to import TAG file '{request.FileName}', {convertedResult.Message}");
      return convertedResult;
    }

#if RAPTOR
    private TagFileDirectSubmissionResult CallRaptorEndpoint(TagFileRequestLegacy tfRequest)
    {
      try
      {
        var data = new MemoryStream(tfRequest.Data);
        var returnResult = (TAGProcServerProcessResultCode) tagProcessor.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor
          (tfRequest.FileName,
            data,
            tfRequest.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, 0, 0, tfRequest.MachineId ?? -1,
            tfRequest.Boundary != null
              ? RaptorConverters.ConvertWGS84Fence(tfRequest.Boundary)
              : TWGS84FenceContainer.Null(), tfRequest.TccOrgId);

        var convertedResult = TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(returnResult));
        if (convertedResult.Code == 0)
          log.LogInformation($"{nameof(CallRaptorEndpoint)} completed: filename '{tfRequest.FileName}' result {returnResult} {convertedResult.Message}");
        else
          log.LogDebug($"{nameof(CallRaptorEndpoint)}: Failed to process tagfile '{tfRequest.FileName}', {convertedResult.Message}");

        return convertedResult;
      }
      catch (Exception ex)
      {
        log.LogError(ex, $"{nameof(CallRaptorEndpoint)}");
        return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TAGProcServerProcessResultCode.Unknown));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    /// <summary>
    /// Save the tagfile to an S3 bucket. Mirrors the folder structure in the tagfile harvester.
    /// </summary>
    /// <param name="resultCode">Code from Raptor</param>
    /// <param name="data">Tagfile contents to archive</param>
    /// <param name="tagFileName">Tagfile name</param>
    /// <param name="tccOrgId">ID of TCC organization who owns the tagfile</param>
    private Task ArchiveFile(TAGProcServerProcessResultCode resultCode, MemoryStream data, string tagFileName, string tccOrgId)
    {
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
          log.LogError($"{nameof(ArchiveFile)} (Raptor): TFA is likely down for {tagFileName} org {tccOrgId}");
          break;

        default:
          folderName = configStore.GetValueString("TCCSynchOtherIssueFolder") ?? "Other... (Issue)";
          break;
      }

      return UploadTagFile(data, tagFileName, tccOrgId, folderName);
    }

    /// <summary>
    /// Save the tagfile to an S3 bucket. Mirrors the folder structure in the tagfile harvester.
    /// </summary>
    /// <param name="resultCode">Code from Raptor</param>
    /// <param name="data">Tagfile contents to archive</param>
    /// <param name="tagFileName">Tagfile name</param>
    /// <param name="tccOrgId">ID of TCC organization who owns the tagfile</param>
    private Task ArchiveFile(TRexTagFileResultCode resultCode, MemoryStream data, string tagFileName, string tccOrgId)
    {
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
          log.LogError($"{nameof(ArchiveFile)} (TRex): TFA is likely down for {tagFileName} org {tccOrgId}");
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

      return UploadTagFile(data, tagFileName, tccOrgId, folderName);
    }

    /// <summary>
    /// Update the Tag file to S3 and / or TCC 
    /// </summary>
    /// <param name="data">Memory Stream for data</param>
    /// <param name="tagFileName">Tag file name</param>
    /// <param name="tccOrgId">TCC org id</param>
    /// <param name="folderName">Folder name for the tag file</param>
    private async Task UploadTagFile(MemoryStream data, string tagFileName, string tccOrgId, string folderName)
    {
      if (string.IsNullOrEmpty(folderName)) 
        return;

      var path = GetS3Key(tagFileName, folderName, tccOrgId);
      // S3 needs a full path including file, but TCC needs a path and filename as two separate variables
      var s3FullPath = path + tagFileName;

      // TCC is very sensitive about its paths... (the method may change to have a slash at the start, we want to handle that)
      var tccPath = path.StartsWith("/") ? path : $"/{path}"; 
      var tccFileSpaceId = configStore.GetValueString(TCC_FILESPACE_CONFIG_KEY);

      log.LogDebug($"{nameof(UploadTagFile)}: Moving file {tagFileName} for org {tccOrgId} to {folderName} folder. Path: {path}, S3 Path: {s3FullPath}, TCC FilespaceID: {tccFileSpaceId}");

      
      using (var s3Stream = new MemoryStream())
      {
        // Transfer Proxy will dispose of the stream passed in, but we need it later
        // So we will create a new memory stream
        // Also we need to seek to the beginning each time as it will set the position to the end after copy (for both the src and dst)
        data.Seek(0, SeekOrigin.Begin);
        await data.CopyToAsync(s3Stream);
        s3Stream.Seek(0, SeekOrigin.Begin);

        transferProxy.Upload(s3Stream, s3FullPath);
      }

      if (!string.IsNullOrEmpty(tccFileSpaceId))
      {
        var directoryExists = await fileRepo.FolderExists(tccFileSpaceId, tccPath);
        if (!directoryExists)
          await fileRepo.MakeFolder(tccFileSpaceId, tccPath);

        using (var tccStream = new MemoryStream())
        {
          // Same thing here, make sure we create a new stream in case it's disposed in the file repo (and seek)
          data.Seek(0, SeekOrigin.Begin);
          await data.CopyToAsync(tccStream);
          tccStream.Seek(0, SeekOrigin.Begin); 
          await fileRepo.PutFile(tccFileSpaceId, tccPath, tagFileName, data, data.Length);
        }
      }
      else
      {
        log.LogWarning($"{nameof(UploadTagFile)}: Failed to upload tag file {tagFileName} to TCC due to missing key {TCC_FILESPACE_CONFIG_KEY}");
      }
    }

    /// <summary>
    /// Gets the key in the S3 bucket to save the tagfile under.
    /// </summary>
    /// <param name="tagFileName">Name of tagfile</param>
    /// <param name="archiveFolder">Archive folder</param>
    /// <param name="tccOrgId">TCC organization who ows the tagfile</param>
    /// <returns></returns>
    private string GetS3Key(string tagFileName, string archiveFolder, string tccOrgId)
    {
      //Example tagfile name: 0415J010SW--HOUK IR 29 16--170731225438.tag
      //Format: <display or ECM serial>--<machine name>--yyMMddhhmmss.tag
      //Required folder structure is <TCC org id>/<serial>--<machine name>/<archive folder>/<serial--machine name--date>/<tagfile>
      //e.g. 0415J010SW--HOUK IR 29 16/Production-Data (Archived)/0415J010SW--HOUK IR 29 16--170731/0415J010SW--HOUK IR 29 16--170731225438.tag
      const string separator = "--";
      string[] parts = tagFileName.Split(new string[] {separator}, StringSplitOptions.None);
      var nameWithoutTime = tagFileName.Substring(0, tagFileName.Length - 10);
      //TCC org ID is not provided with direct submission from machines
      var prefix = string.IsNullOrEmpty(tccOrgId) ? string.Empty : $"{tccOrgId}/";

      return $"{prefix}{parts[0]}{separator}{parts[1]}/{archiveFolder}/{nameWithoutTime}/";
    }
#endif

  }
}
