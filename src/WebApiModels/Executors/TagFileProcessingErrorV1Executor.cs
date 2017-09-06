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
  public class TagFileProcessingErrorV1Executor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the tag file processing error request and creates an alert if required.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a TagFileProcessingErrorResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TagFileProcessingErrorV1Request;
      if (request == null)
        // todo serviceException refactor
        throw new ServiceException(HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 10));

      log.LogDebug("TagFileProcessingErrorV1Executor: Going to process request {0}",
        JsonConvert.SerializeObject(request));

      bool result = request.assetId > 0 && !string.IsNullOrEmpty(request.tagFileName) &&
                    Enum.IsDefined(typeof(TagFileErrorsEnum), request.error) == true;

      if (result)
      {
        var errorMessage =
          string.Format(
            $"OnTagFileProcessingError: assetID = {request.assetId}, tagFileName = {request.tagFileName}, errorNumber = {(int) request.error}, error = {Enum.GetName(typeof(TagFileErrorsEnum), request.error)}");
        log.LogDebug(errorMessage);
      }

      try
      {
        return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(result);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 11));
      }

    }
  }
}