using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TagFileProcessingErrorV2Request;
      if (request == null)
        // todo serviceException refactor
        throw new ServiceException(HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ResultHandling.ContractExecutionStatesEnum.InternalProcessingError, 12));

      log.LogDebug("TagFileProcessingErrorV2Executor: Going to process request {0}",
        JsonConvert.SerializeObject(request));

      // todo do all in Validate() validation?
      bool result = !string.IsNullOrEmpty(request.tagFileName) &&
          Enum.IsDefined(typeof(TagFileErrorsEnum), request.error) == true;

      if (result)
      {
        var errorMessage =
          string.Format($"OnTagFileProcessingError: assetID = {request.assetId}, " +
                        $"tagFileName = {request.tagFileName}, displaySerialNumber = { request.DisplaySerialNumber()}, machineName = { request.MachineName()}, tagfileUtc = {request.TagFileDateTimeUtc()}" +
                        $"errorNumber = {(int)request.error}, error = {Enum.GetName(typeof(TagFileErrorsEnum), request.error)}, " + 
                        $"projectId = {request.projectId} tccOrgId = {request.tccOrgId} deviceSerialNumber = {request.deviceSerialNumber} ");
        log.LogDebug(errorMessage);

        //if (request.tccOrgId) 
        //  log.LogDebug( $"get CustUID")
        //else
        //if (request.projectId)
        //  log.LogDebug($"get projects CustUID")
        //else
        //if (request.assetId)
        //  log.LogDebug($"get assets CustUID, with a cust sub")
        //  ie no tccorgid or 

        // todo do we need radioSerial as already always have assetId. What if can't find assetId?
      }

      try
      {
        return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(result);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 13));
      }
    }
  }
}