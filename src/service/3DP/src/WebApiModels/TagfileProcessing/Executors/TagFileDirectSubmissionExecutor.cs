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
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CompactionTagFileRequest>(item);
      var result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError);
      var useTrexGateway = UseTRexGateway("ENABLE_TREX_GATEWAY_TAGFILE");
      var useRaptorGateway = UseRaptorGateway("ENABLE_RAPTOR_GATEWAY_TAGFILE");
      var s3DirectTagFileBucketName = configStore.GetValueString("AWS_DIRECT_TAGFILE_BUCKET_NAME", string.Empty);
      if (string.IsNullOrEmpty(s3DirectTagFileBucketName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Unable to identify S3 direct tagfile bucket name."));
      }

#if RAPTOR
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
#endif

      // For direct endpoint, tag files are archived to s3, mainly for support.
      var data = new MemoryStream(request.Data);
      if (useRaptorGateway)
        await TagFileHelper.ArchiveTagFile(configStore, transferProxy, log,
          (TAGProcServerProcessResultCode)result.Code, data, request.FileName, request.OrgId, 
          TagFileSubmissionType.Direct, s3DirectTagFileBucketName);
      else if (useTrexGateway)
        await TagFileHelper.ArchiveTagFile(configStore, transferProxy, log, 
          (TRexTagFileResultCode)result.Code, data, request.FileName, request.OrgId, 
          TagFileSubmissionType.Direct, s3DirectTagFileBucketName);
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
#endif

  }
}
