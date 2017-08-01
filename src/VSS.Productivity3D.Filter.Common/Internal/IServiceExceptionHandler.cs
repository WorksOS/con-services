using System.Net;
using VSS.Productivity3D.Filter.Common.Models;

namespace VSS.Productivity3D.Filter.Common.Internal
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
  }
}