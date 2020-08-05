using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Enums;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Types;
using VSS.WebApi.Common;

namespace VSS.TRex.TAGFiles.Executors
{
  /// <summary>
  /// Execute internal business logic to handle submission of a TAG file to TRex
  /// </summary>
  public class SubmitTAGFileExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubmitTAGFileExecutor>();

    private readonly bool _tagFileArchiving = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_TAGFILE_ARCHIVING", Consts.ENABLE_TAGFILE_ARCHIVING);

    /// <summary>
    /// Local static/singleton TAG file buffer queue reference to use when adding TAG files to the queue
    /// </summary>
    private readonly ITAGFileBufferQueue _queue = DIContext.Obtain<Func<ITAGFileBufferQueue>>()();

    private bool OutputInformationalRequestLogging = true;
    private readonly bool _isDeviceGatewayEnabled = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_DEVICE_GATEWAY", Consts.ENABLE_DEVICE_GATEWAY);
    private readonly ITPaaSApplicationAuthentication _tPaaSApplicationAuthentication = DIContext.Obtain<ITPaaSApplicationAuthentication>();


    /// <summary>
    /// Receive a TAG file to be processed, validate TAG File Authorization for the file, and add it to the 
    /// queue to be processed.
    /// </summary>
    /// <param name="projectId">Project ID to be used as an override to any project ID that may be determined via TAG file authorization</param>
    /// <param name="assetId">Asset ID to be used as an override to any Asset ID that may be determined via TAG file authorization</param>
    /// <param name="tagFileName">Name of the physical tag file for archiving and logging</param>
    /// <param name="tagFileContent">The content of the TAG file to be processed, expressed as a byte array</param>
    /// <param name="tccOrgId">Used by TFA service to match VL customer to TCC org when looking for project if multiple projects and/or machine ID not in tag file</param>
    /// <param name="treatAsJohnDoe">The TAG file will be processed as if it were a john doe machine is projectId is also specified</param>
    /// <param name="tagFileSubmissionFlags">A flag set controlling how certain aspects of managing a submitted TAG file should be managed</param>
    public async Task<SubmitTAGFileResponse> ExecuteAsync(Guid? projectId, Guid? assetId, string tagFileName, byte[] tagFileContent, 
      string tccOrgId, bool treatAsJohnDoe, TAGFileSubmissionFlags tagFileSubmissionFlags)
    {
      if (OutputInformationalRequestLogging)
        _log.LogInformation($"#In# SubmitTAGFileResponse. Processing {tagFileName} TAG file into ProjectUID:{projectId}, asset:{assetId}");
      
      var response = new SubmitTAGFileResponse
      {
        FileName = tagFileName,
        Success = false,
        Message = "TRex unknown result (SubmitTAGFileResponse.Execute)",
        Code = (int)TRexTagFileResultCode.TRexUnknownException,
      };

      try
      {
        try
        {
          // wrap up details into obj
          var td = new TagFileDetail
          {
            assetId = assetId,
            projectId = projectId,
            tagFileName = tagFileName,
            tagFileContent = tagFileContent,
            tccOrgId = tccOrgId,
            IsJohnDoe = treatAsJohnDoe
          };

          // Validate tag file submission
          ContractExecutionResult result;
          result = TagfileValidator.PreScanTagFile(td, out var tagFilePreScan);
          
          if (result.Code == (int) TRexTagFileResultCode.Valid)
          {
            if (_isDeviceGatewayEnabled)
              SendDeviceStatusToDeviceGateway(td, tagFilePreScan);

            result = await TagfileValidator.ValidSubmission(td, tagFilePreScan);
          }

          response.Code = result.Code;
          response.Message = result.Message;
          
          if (result.Code == (int) TRexTagFileResultCode.Valid && td.projectId != null) // If OK add to process queue
          {
            // First archive the tag file
            if (_tagFileArchiving && tagFileSubmissionFlags.HasFlag(TAGFileSubmissionFlags.AddToArchive))
            {
              _log.LogInformation($"#Progress# SubmitTAGFileResponse. Archiving tag file:{tagFileName}, ProjectUID:{td.projectId}");
              if (! await TagFileRepository.ArchiveTagfileS3(td))
              {
                _log.LogError($"SubmitTAGFileResponse. Failed to archive tag file. Returning TRexQueueSubmissionError error. ProjectUID:{td.projectId}, AssetUID:{td.assetId}, Tagfile:{tagFileName}");
                throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"SubmitTAGFileResponse. Failed to archive tag file {tagFileName} to S3"));
              }
            }

            // switch from nullable to not nullable
            var validProjectId = td.projectId ?? Guid.Empty;
            var validAssetId = td.assetId ?? Guid.Empty;

            if (OutputInformationalRequestLogging)
              _log.LogInformation($"#Progress# SubmitTAGFileResponse. Submitting tag file to TagFileBufferQueue. ProjectUID:{validProjectId}, AssetUID:{validAssetId}, Tagfile:{tagFileName}, JohnDoe:{td.IsJohnDoe} ");

            var tagKey = new TAGFileBufferQueueKey(tagFileName, validProjectId, validAssetId);
            var tagItem = new TAGFileBufferQueueItem
            {
              InsertUTC = DateTime.UtcNow,
              ProjectID = validProjectId,
              AssetID = validAssetId,
              FileName = tagFileName,
              Content = tagFileContent,
              IsJohnDoe = td.IsJohnDoe,
              SubmissionFlags = tagFileSubmissionFlags
            };

            if (_queue == null)
            {
              throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "SubmitTAGFileResponse. Processing queue not available"));
            }

            if (_queue.Add(tagKey, tagItem)) // Add tag file to queue
            {
              response.Success = true;
              response.Message = "";
              response.Code = (int)TRexTagFileResultCode.Valid;

              // Commented out top reduce logging
              // Log.LogInformation($"Added TAG file {tagKey.FileName} representing asset {tagKey.AssetUID} within project {tagKey.ProjectUID} into the buffer queue");
            }
            else
            {
              response.Success = false;
              response.Message = "SubmitTAGFileResponse. Failed to submit tag file to processing queue. Request already exists";
              response.Code = (int)TRexTagFileResultCode.TRexQueueSubmissionError;

              _log.LogWarning(response.Message);
            }
          }
          else
          {
            response.Success = false;
          }
        }
        catch (Exception e) // catch all exceptions here
        {
          _log.LogError(e, $"#Exception# SubmitTAGFileResponse. Exception occured processing {tagFileName} Exception:");
          throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"SubmitTAGFileResponse. Exception {e.Message}"));
        }
      }
      finally
      {
        if (OutputInformationalRequestLogging)
          _log.LogInformation($"#Out# SubmitTAGFileResponse. Processed {tagFileName} Result: {response.Success}, Message:{response.Message} Code:{response.Code}");
      }
      return response;
    }

    /// <summary>
    /// Send devices lastKnownStatus to cws deviceGateway aka connected site
    ///     Don't need to await as this process should be fire and forget
    ///        We don't care if the post is valid, or device exists etc
    /// </summary>
    public void SendDeviceStatusToDeviceGateway(TagFileDetail tagFileDetail, TAGFilePreScan tagFilePreScan)
    {
      if (tagFilePreScan.PlatformType == CWSDeviceTypeEnum.EC520 ||
          tagFilePreScan.PlatformType == CWSDeviceTypeEnum.EC520W ||
           tagFilePreScan.PlatformType == CWSDeviceTypeEnum.Unknown)
        _log.LogInformation($"#Progress# {nameof(SendDeviceStatusToDeviceGateway)} Not an applicable DeviceType: {tagFilePreScan.PlatformType}");
      else
      {
        var seedLatitude = MathUtilities.RadiansToDegrees(tagFilePreScan.SeedLatitude ?? 0.0);
        var seedLongitude = MathUtilities.RadiansToDegrees(tagFilePreScan.SeedLongitude ?? 0.0);
        var seedNorthing = tagFilePreScan.SeedNorthing;
        var seedEasting = tagFilePreScan.SeedEasting;
        if (Math.Abs(seedLatitude) < Consts.TOLERANCE_DECIMAL_DEGREE && Math.Abs(seedLongitude) < Consts.TOLERANCE_DECIMAL_DEGREE)
        {
          // This check is also done as a pre-check as the scenario is very frequent, to avoid the TFA API call overhead.
          var message = $"#Progress# {nameof(SendDeviceStatusToDeviceGateway)} tagfile: {tagFileDetail.tagFileName} doesn't have a valid Seed Lat/Long. {tagFilePreScan.SeedLatitude}/{tagFilePreScan.SeedLongitude}. ";
          if (seedNorthing != null && seedEasting != null)
            message += $" It does have a Seed Northing/Easting {seedNorthing}/{seedEasting} however local grids are not currently supported for deviceGateway.";
          _log.LogWarning(message);
        }
        else
        {
          var deviceLksModel = new DeviceLKSModel
          {
            TimeStamp = tagFilePreScan.SeedTimeUTC,
            Latitude = seedLatitude,
            Longitude = seedLongitude,
            Height = tagFilePreScan.SeedHeight,
            AssetSerialNumber = tagFilePreScan.HardwareID,
            AssetNickname = tagFilePreScan.MachineID,

            AppName = (tagFilePreScan.PlatformType == CWSDeviceTypeEnum.TMC) ? "TMC" : "GCS900",
            AppVersion = tagFilePreScan.ApplicationVersion,
            DesignName = tagFilePreScan.DesignName,


            // PlatformType is only passed as part of DeviceName {platformType}-{assetSerialNumber}
            AssetType = tagFilePreScan.MachineType.GetEnumMemberValue(),
        
            Devices = string.IsNullOrWhiteSpace(tagFilePreScan.RadioSerial) ? null :
              new List<ConnectedDevice>
              {
                new ConnectedDevice
                {
                  Model = tagFilePreScan.RadioType,
                  SerialNumber = tagFilePreScan.RadioSerial
                }
              }
          };
          _log.LogInformation($"#Progress# {nameof(SendDeviceStatusToDeviceGateway)} Posting deviceLks to cws deviceGateway: {JsonConvert.SerializeObject(deviceLksModel)}");

          var cwsDeviceGatewayClient = DIContext.Obtain<ICwsDeviceGatewayClient>();
          var customHeaders = _tPaaSApplicationAuthentication.CustomHeaders();

          _log.LogInformation($"#Progress# {nameof(SendDeviceStatusToDeviceGateway)} Got customHeaders");

          // don't await this call, should be fire and forget
          cwsDeviceGatewayClient.CreateDeviceLKS($"{deviceLksModel.AssetType}-{deviceLksModel.AssetSerialNumber}", deviceLksModel, customHeaders)
            .ContinueWith((task) =>
            {
              if (task.IsFaulted)
              {
                _log.LogError(task.Exception, $"#Progress# {nameof(SendDeviceStatusToDeviceGateway)}: Error Sending to Connected Site", null);
              }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        _log.LogInformation($"#Progress# {nameof(SendDeviceStatusToDeviceGateway)} Post to ces deviceGateway completed");
      }
    }
  }
}
