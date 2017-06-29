using ProjectWebApiCommon.ResultsHandling;
using System.Net;

namespace ProjectWebApi.Internal
{
  /// <summary>
  /// Common controller ServiceException handler.
  /// </summary>
  public interface IServiceExceptionHandler
  {
    /// <summary>
    /// Correctly throw ServiceException for controller types.
    /// </summary>
    ServiceException ThrowServiceException(HttpStatusCode statusCode, int errorNumber, string resultCode = null, string errorMessage1 = null, string errorMessage2 = null);

    /// <summary>
    /// Correctly throw ServiceException for controller types.
    /// </summary>
  }
}