using System.Net;
using VSS.MasterData.Project.WebAPI.Common.Models;

namespace VSS.MasterData.Project.Services.WebAPI.Internal
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