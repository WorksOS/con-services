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
      CustomerTccOrg customerTCCOrg = null;
      try
      {
        customerTCCOrg = await dataRepository.LoadCustomerByTccOrgId(request.tccOrgId).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      //CustomerTccOrg customerTCCOrg;
      //try
      //{
      //  customerTCCOrg = await dataRepository.LoadCustomerByTccOrgId(request.tccOrgId).ConfigureAwait(false);
      //}
      //catch (Exception e)
      //{
      //  throw new ServiceException(HttpStatusCode.InternalServerError,
      //    TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
      //      ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      //}

      //if (request.tccOrgId) 
      //  log.LogDebug( $"get CustUID")
      //else
      //if (request.projectId)
      //  log.LogDebug($"get projects CustUID")
      //else
      //if (request.assetId)
      //  log.LogDebug($"get assets CustUID, with a cust sub")
      //  ie no tccorgid or 

      var createTagFileErrorEvent = new CreateTagFileErrorEvent()
      {
        //CustomerUID = ,
        MachineName = request.MachineName(),
        DisplaySerialNumber = request.DisplaySerialNumber(),
        TagFileCreatedUTC = request.TagFileDateTimeUtc(),
        //ErrorCode = request.error,
        //todo convert to 1-based
        //AssetUID =,
        DeviceSerialNumber = request.deviceSerialNumber,
        //ProjectUID = ,
        ActionUTC = actionUtc
      };

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