using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.TagFile;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which sends an alert if required for a tag file processing error.
  /// </summary>
  public class TagFileProcessingErrorV2Executor : RequestExecutorContainer
  {

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
            ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 13));

      var processedOk = false;

      var errorMessage =
          string.Format($"OnTagFileProcessingError: assetID = {request.assetId}, " +
                        $"tagFileName = {request.tagFileName}, displaySerialNumber = { request.DisplaySerialNumber()}, machineName = { request.MachineName()}, tagfileUtc = {request.TagFileDateTimeUtc()}" +
                        $"errorNumber = {(int)request.error}, error = {Enum.GetName(typeof(TagFileErrorsEnum), request.error)}, " + 
                        $"projectId = {request.projectId} tccOrgId = {request.tccOrgId} deviceSerialNumber = {request.deviceSerialNumber} ");
      log.LogInformation(errorMessage);

      var actionUtc = DateTime.UtcNow;

      // Try to find a customerUid - from tccorg/asset or project
      string customerUid = null;
      CustomerTccOrg customerTCCOrg = null;
      Project project = null;
      Asset asset = null;
      if (!string.IsNullOrEmpty(request.tccOrgId))
      {
        try
        {
          customerTCCOrg = await dataRepository.LoadCustomerByTccOrgId(request.tccOrgId);
          log.LogDebug(
            $"TagFileProcessingErrorV2Executor: tccOrgId {JsonConvert.SerializeObject(customerTCCOrg)}");
        }
        catch (Exception e)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
              ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
        }
      }

      // todo can projectId be -1 thru -3? in which case we may be able to identify a customer from it?

      if (request.projectId != null && request.projectId > 0)
      {
        try
        {
          project = await dataRepository.LoadProject(request.projectId.Value);
          log.LogDebug($"TagFileProcessingErrorV2Executor: projectId {JsonConvert.SerializeObject(project)}");
        }
        catch (Exception e)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
              ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
        }
      }

      if (request.assetId != null && request.assetId > 0)
      {
        try
        {
          asset = await dataRepository.LoadAsset(request.assetId.Value);
          log.LogDebug($"TagFileProcessingErrorV2Executor: assetId {JsonConvert.SerializeObject(asset)}");
        }
        catch (Exception e)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
              ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
        }
      }
      
      customerUid = customerTCCOrg?.CustomerUID;
      // todo what if project customer different to tccOrgId customer?
      customerUid = customerUid ?? project?.CustomerUID;
      customerUid = customerUid ?? asset?.OwningCustomerUID;

      // no customer found, get outta town.
      if (customerUid == null)
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 29));


      // todo what if this assetUid is different to Asset (from AssetId) or could it be used to obtain the Asset and therefore customer?
      AssetDevice assetDevice = null;
      if (!string.IsNullOrEmpty(request.deviceSerialNumber))
      {
        try
        {
          //assetDevice = await dataRepository.GetAssociatedAsset(request.deviceSerialNumber);
          log.LogDebug($"TagFileProcessingErrorV2Executor: deviceSerialNumber {JsonConvert.SerializeObject(assetDevice)}");
        }
        catch (Exception e)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
              ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
        }
      }

      var createTagFileErrorEvent = new CreateTagFileErrorEvent()
      {
        CustomerUID = Guid.Parse(customerUid),
        MachineName = request.MachineName(),
        DisplaySerialNumber = request.DisplaySerialNumber(),
        TagFileCreatedUTC = request.TagFileDateTimeUtc(),
        ErrorCode = 0, //(int) request.error,  //todo convert to 1-based
        AssetUID = asset == null ? (Guid?)null : Guid.Parse(asset?.AssetUID),
        DeviceSerialNumber = request.deviceSerialNumber,
        ProjectUID = project == null ? (Guid?)null : Guid.Parse(project?.ProjectUID),
        ActionUTC = actionUtc
      };

      //todo write createTagFileErrorEvent somewhere.....

      try
      {
        return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(processedOk);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
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