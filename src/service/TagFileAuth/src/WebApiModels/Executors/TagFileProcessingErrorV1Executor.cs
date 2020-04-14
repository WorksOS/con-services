using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.Models.ContractExecutionStatesEnum;

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
        throw new ServiceException(HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 11));

      bool result = request.assetId > 0 && !string.IsNullOrEmpty(request.tagFileName) &&
                    Enum.IsDefined(typeof(TagFileErrorsEnum), request.error) == true;

      if (result)
      {
        var errorMessage =
          string.Format(
            $"OnTagFileProcessingErrorV1: assetID = {request.assetId}, tagFileName = {request.tagFileName}, errorNumber = {(int)request.error}, error = {Enum.GetName(typeof(TagFileErrorsEnum), request.error)}");
        log.LogDebug(errorMessage);
      }

      return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(result);
    }
    protected override Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
