using System.Net;
using Microsoft.Extensions.Logging;
using WebApiModels.Enums;
using System;
using Newtonsoft.Json;
using WebApiModels.Models;
using WebApiModels.ResultHandling;

namespace WebApiModels.Executors
{
  /// <summary>
  /// The executor which sends an alert if required for a tag file processing error.
  /// </summary>
  public class TagFileProcessingErrorExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the tag file processing error request and creates an alert if required.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a TagFileProcessingErrorResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      TagFileProcessingErrorRequest request = item as TagFileProcessingErrorRequest;
      log.LogDebug("TagFileProcessingErrorExecutor: Going to process request {0}", JsonConvert.SerializeObject(request));

      bool result = false;

      // if it got past the validation, it is complete.ok, check again
      if (request.assetId > 0 && !string.IsNullOrEmpty(request.tagFileName) && Enum.IsDefined(typeof(TagFileErrorsEnum), request.error) == true)
        result = true; 
          
      if (result)
      {
        var errorMessage = string.Format("OnTagFileProcessingError: assetID = {0}, tagFileName = {1}, errorNumber = {2}, error = {3}", request.assetId, request.tagFileName, (int)request.error, Enum.GetName(typeof(TagFileErrorsEnum), request.error));
        log.LogDebug(errorMessage);
      }

      try
      {
        return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(result);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to create an alert for tag file processing error"));
      }

    }
  }
}