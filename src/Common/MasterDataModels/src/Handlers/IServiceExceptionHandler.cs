using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.Handlers
{
  /// <summary>
  /// Common controller ServiceException handler.
  /// </summary>
  public interface IServiceExceptionHandler
  {
    /// <summary>
    /// Correctly throw ServiceException for controller types.
    /// </summary>
    ServiceException ThrowServiceException(HttpStatusCode statusCode, int errorNumber, string resultCode = null, string errorMessage1 = null, string errorMessage2 = null, Exception innerException = null);

    /// <summary>
    /// Return a server error result object used when returning an IActionResult type.
    /// </summary>
    ContractExecutionResult CreateServiceError(HttpStatusCode statusCode, int errorNumber, string resultCode = null, string errorMessage1 = null, string errorMessage2 = null, Exception innerException = null);
  }
}
