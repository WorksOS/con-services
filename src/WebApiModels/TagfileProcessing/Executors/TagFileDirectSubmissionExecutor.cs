using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  /// <summary>
  /// For submitting direct submitted TAG files to TRex and Raptor.
  /// for now: we will ALWAYS send to Raptor, but only send to TRex if configured.
  ///          if TRex fails, then we will continue sending to Raptor
  /// </summary>
  public class TagFileDirectSubmissionExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CompactionTagFileRequest;
      var result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
        "3dPm Unknown exception.");

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY"), out var useTrexGateway);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY"), out var useRaptorGateway);

      if (useTrexGateway)
      {
        request.Validate();

        // gobbles any exception
        result = await CallTRexEndpoint(request).ConfigureAwait(false);

        if (result.Code == 0)
        {
          log.LogDebug($"PostTagFile (Direct TRex): Successfully imported TAG file '{request.FileName}'.");
        }
        else
        {
          log.LogDebug($"PostTagFile (Direct TRex): Failed to import TAG file '{request.FileName}', {result.Message}");
        }
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
        if (result.Code == 0)
        {
          log.LogDebug($"PostTagFile (Direct Raptor): Successfully imported TAG file '{request.FileName}'.");
        }
        else
        {
          log.LogDebug(
            $"PostTagFile (Direct Raptor): Failed to import TAG file '{request.FileName}', {result.Message}");
        }
      }

      // For direct endpoint, tag files are archived to s3, mainly for support.
      // For nonDirect the harvester uses TCC (for now, soon to be s3)
      var data = new MemoryStream(request.Data);
      if (useRaptorGateway)
      {
        ArchiveFile((TTAGProcServerProcessResult) result.Code, data, request.FileName, request.OrgId);
      }
      else if (useTrexGateway)
      {
        ArchiveFile((TRexTagFileResultCode)result.Code, data, request.FileName, request.OrgId);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "No tag file processing server configured."));

      }

      return result;
    }

    private async Task<ContractExecutionResult> CallTRexEndpoint(CompactionTagFileRequest request)
    {
      var returnResult = await TagFileHelper.SendTagFileToTRex(request,
        tRexTagFileProxy, log, customHeaders, true).ConfigureAwait(false);

      log.LogInformation($"PostTagFile (Direct TRex): result: {JsonConvert.SerializeObject(returnResult)}");

      return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper((TRexTagFileResultCode)returnResult.Code));
    }

    private TagFileDirectSubmissionResult CallRaptorEndpoint(TagFileRequestLegacy tfRequest)
    {
      try
      {

        var data = new MemoryStream(tfRequest.Data);
        var returnResult = (TTAGProcServerProcessResult) tagProcessor.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor
          (tfRequest.FileName,
            data,
            tfRequest.ProjectId ?? -1, 0, 0, tfRequest.MachineId ?? -1,
            tfRequest.Boundary != null
              ? RaptorConverters.convertWGS84Fence(tfRequest.Boundary)
              : TWGS84FenceContainer.Null(), tfRequest.TccOrgId);

        log.LogInformation($"PostTagFile (Direct Raptor): result: {JsonConvert.SerializeObject(returnResult)}");
        return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(returnResult));
      }
      catch (Exception ex)
      {
        log.LogError($"PostTagFile (Direct Raptor): exception {ex.Message}");
        return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprUnknown));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    /// <summary>
    /// Save the tagfile to an S3 bucket. Mirrors the folder struucture in the tagfile harvester.
    /// </summary>
    /// <param name="resultCode">Code from Raptor</param>
    /// <param name="data">Tagfile contents to archive</param>
    /// <param name="tagFileName">Tagfile name</param>
    /// <param name="tccOrgId">ID of TCC organization who owns the tagfile</param>
    private void ArchiveFile(TTAGProcServerProcessResult resultCode, Stream data, string tagFileName, string tccOrgId)
    {
      string folderName = null;
      switch (resultCode)
      {
        case TTAGProcServerProcessResult.tpsprOK:
          folderName = configStore.GetValueString("TCCSynchProductionDataArchivedFolder") ?? "Production-Data (Archived)";
          break;

        case TTAGProcServerProcessResult.tpsprOnChooseDataModelUnableToDetermineDataModel:
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
        case TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:
          folderName = configStore.GetValueString("TCCSynchProjectBoundaryIssueFolder") ?? "Project Boundary (Issue)";
          break;

        case TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions:
        case TTAGProcServerProcessResult.tpsprOnChooseMachineUnableToDetermineMachine:
        case TTAGProcServerProcessResult.tpsprOnChooseMachineUnknownMachine:
          folderName = configStore.GetValueString("TCCSynchSubscriptionIssueFolder") ?? "Subscription (Issue)";
          break;

        case TTAGProcServerProcessResult.tpsprTFAServiceError:
          log.LogError("TFA is likely down for {0} org {1}", tagFileName, tccOrgId);
          break;

        default:
          folderName = configStore.GetValueString("TCCSynchOtherIssueFolder") ?? "Other... (Issue)";
          break;
      }

      if (!string.IsNullOrEmpty(folderName))
      {
        log.LogDebug($"Moving file {tagFileName} for org {tccOrgId} to {folderName} folder");
        transferProxy.Upload(data, GetS3Key(tagFileName, folderName, tccOrgId));
      }
    }

    /// <summary>
    /// Save the tagfile to an S3 bucket. Mirrors the folder struucture in the tagfile harvester.
    /// </summary>
    /// <param name="resultCode">Code from Raptor</param>
    /// <param name="data">Tagfile contents to archive</param>
    /// <param name="tagFileName">Tagfile name</param>
    /// <param name="tccOrgId">ID of TCC organization who owns the tagfile</param>
    private void ArchiveFile(TRexTagFileResultCode resultCode, Stream data, string tagFileName, string tccOrgId)
    {
      string folderName = null;
      switch (resultCode)
      {
        case TRexTagFileResultCode.Valid:
          folderName = configStore.GetValueString("TCCSynchProductionDataArchivedFolder") ?? "Production-Data (Archived)";
          break;

        // this was a global catch-all applied by TagFileProcessor
        // when TFA.getProjectId() or getAssetId failed.
        //case TTAGProcServerProcessResult.tpsprOnChooseDataModelUnableToDetermineDataModel: 
        
        // These were raptor errors which don't appear to be generated by TRex
        //case TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
        //case TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
        //case TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:

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
          log.LogError("TFA is likely down for {0} org {1}", tagFileName, tccOrgId);
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

      if (!string.IsNullOrEmpty(folderName))
      {
        log.LogDebug($"Moving file {tagFileName} for org {tccOrgId} to {folderName} folder");
        transferProxy.Upload(data, GetS3Key(tagFileName, folderName, tccOrgId));
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
      return $"{prefix}{parts[0]}{separator}{parts[1]}/{archiveFolder}/{nameWithoutTime}/{tagFileName}";
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
