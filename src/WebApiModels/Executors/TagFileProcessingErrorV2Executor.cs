using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.Notifications.Enums;
using VSS.VisionLink.Interfaces.Events.Notifications.Events;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which sends an alert if required for a tag file processing error.
  /// </summary>
  public class TagFileProcessingErrorV2Executor : RequestExecutorContainer
  {
    protected TagFileErrorMappings tagFileErrorMappings = new TagFileErrorMappings();

    /// <summary>
    /// Processes the tag file processing error request and creates an alert if required.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a TagFileProcessingErrorResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as TagFileProcessingErrorV2Request;
      if (request == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 13));

      var processedOk = false;

      var errorMessage =
          string.Format($"OnTagFileProcessingError: assetID = {request.assetId}, " +
                        $"tagFileName = {request.tagFileName}, displaySerialNumber = { request.DisplaySerialNumber()}, machineName = { request.MachineName()}, tagfileUtc = {request.TagFileDateTimeUtc()}" +
                        $"errorNumber = {(int)request.error}, error = {Enum.GetName(typeof(TagFileErrorsEnum), request.error)}, " + 
                        $"projectId = {request.projectId} tccOrgId = {request.tccOrgId} deviceSerialNumber = {request.deviceSerialNumber} ");
      log.LogInformation(errorMessage);

      var actionUtc = DateTime.UtcNow;

      // Try to find a customerUid - from tccorg/asset/project
      string customerUid = null;
      CustomerTccOrg customerTCCOrg = null;
      Project project = null;
      Asset asset = null;
      if (!string.IsNullOrEmpty(request.tccOrgId))
      {
        customerTCCOrg = await dataRepository.LoadCustomerByTccOrgId(request.tccOrgId);
        if (customerTCCOrg == null)
          log.LogError($"TagFileProcessingErrorV2Executor: tccOrgId Not found)");
        else
          log.LogDebug($"TagFileProcessingErrorV2Executor: tccOrgId {JsonConvert.SerializeObject(customerTCCOrg)}");
      }

      // can TFHarvester send a projectId -1 thru -3? 
      //    if so we may be able to identify a customer from it using boundaries and subs
      if (request.projectId != null && request.projectId > 0)
      {

        project = await dataRepository.LoadProject(request.projectId.Value);
        if (project == null)
          log.LogError($"TagFileProcessingErrorV2Executor: project Not found)");
        else
          log.LogDebug($"TagFileProcessingErrorV2Executor: project {JsonConvert.SerializeObject(project)}");
      }

      if (request.assetId != null && request.assetId > 0)
      {
        asset = await dataRepository.LoadAsset(request.assetId.Value);
        if (asset == null)
          log.LogError($"TagFileProcessingErrorV2Executor: asset Not found)");
        else
          log.LogDebug($"TagFileProcessingErrorV2Executor: asset {JsonConvert.SerializeObject(asset)}");
      }

      // if no assetid how about getting it and possibly customerUid from DeviceSerialNumber/DeviceType?
      // todo what if this assetUid is different to Asset (from AssetId) or could it be used to obtain the Asset and therefore customer?
      //AssetDeviceIds assetDeviceIds = null;
      //if (asset == null && !string.IsNullOrEmpty(request.deviceSerialNumber))
      //{
      //  try
      //  {
      //    assetDeviceIds = await dataRepository.LoadAssetDevice(request.deviceSerialNumber, request.DeviceTypeString());
      //    if (assetDeviceIds == null)
      //      log.LogError($"TagFileProcessingErrorV2Executor: assetDeviceIds Not found)");
      //    else
      //      log.LogDebug($"TagFileProcessingErrorV2Executor: assetDeviceIds {JsonConvert.SerializeObject(assetDeviceIds)}");
      //  }
      //  catch (Exception e)
      //  {
      //    throw new ServiceException(HttpStatusCode.InternalServerError,
      //      TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
      //        ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      //  }
      //}

      customerUid = customerTCCOrg?.CustomerUID;
      // what if project customer is different to tccOrgId customer?
      customerUid = customerUid ?? project?.CustomerUID;
      customerUid = customerUid ?? asset?.OwningCustomerUID;

      // even if no customerUid is found, create an event with everything available
      var createTagFileErrorEvent = new CreateTagFileErrorEvent()
      {
        TagFileErrorUID = Guid.NewGuid(),
        MachineName = request.MachineName(),
        DisplaySerialNumber = request.DisplaySerialNumber(),
        TagFileCreatedUTC = request.TagFileDateTimeUtc(),
        ErrorCode = (TagFileError) tagFileErrorMappings.tagFileErrorTypes.Find(st => string.Equals(st.name, request.error.ToString(), StringComparison.OrdinalIgnoreCase)).NotificationEnum,
        CustomerUID = string.IsNullOrEmpty(customerUid) ? (Guid?)null : Guid.Parse(customerUid),
        AssetUID = asset == null ? (Guid?)null : Guid.Parse(asset?.AssetUID),
        DeviceSerialNumber = request.deviceSerialNumber,
        DeviceType = request.deviceType,
        ProjectUID = project == null ? (Guid?)null : Guid.Parse(project?.ProjectUID),
        TccOrgId = request.tccOrgId,
        LegacyAssetId = request.assetId,
        LegacyProjectId = request.projectId,
        ActionUTC = actionUtc
      };

      try
      {
        var messagePayload = JsonConvert.SerializeObject(new { CreateTagFileErrorEvent = createTagFileErrorEvent });
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(createTagFileErrorEvent.TagFileErrorUID.ToString(), messagePayload)
          });
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 31, e.Message));
      }

      processedOk = true;

      try
      {
        return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(processedOk);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 14));
      }
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}