using System.Net;
using VSS.Productivity3D.ProjectWebApiCommon.Models;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;

namespace VSS.Productivity3D.ProjectWebApi.Internal
{
  /// <summary>
  /// Common controller ServiceException handler.
  /// </summary>
  public class ServiceExceptionHandler : IServiceExceptionHandler
  {
    /// <summary>
    /// The contract execution states enum
    /// </summary>
    private readonly ContractExecutionStatesEnum _contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    /// <summary>
    /// Correctly throw ServiceException for controller types.
    /// </summary>
    public ServiceException ThrowServiceException(HttpStatusCode statusCode, int errorNumber, string resultCode = null, string errorMessage1 = null, string errorMessage2 = null)
    {
      throw new ServiceException(statusCode,
        new ContractExecutionResult(_contractExecutionStatesEnum.GetErrorNumberwithOffset(errorNumber),
          string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(errorNumber), resultCode, errorMessage1 ?? "null", errorMessage2 ?? "null")));
    }

    /// <summary>
    /// Correctly throw ServiceException for controller types.
    /// </summary>
    public ServiceException ThrowServiceException(HttpStatusCode statusCode, int errorNumber, int resultCode,
      string errorMessage1 = null, string errorMessage2 = null)
    {
      return ThrowServiceException(statusCode, errorNumber, resultCode.ToString(), errorMessage1, errorMessage2);
    }
  }
}