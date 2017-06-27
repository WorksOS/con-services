using ProjectWebApiCommon.ResultsHandling;
using System.Net;

namespace ProjectWebApi.Internal
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
    public ServiceException ThrowServiceException(HttpStatusCode statusCode, int errorNumber, string resultCode = null, string errorMessage = null)
    {
      throw new ServiceException(statusCode,
        new ContractExecutionResult(_contractExecutionStatesEnum.GetErrorNumberwithOffset(errorNumber),
          string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(errorNumber), resultCode, errorMessage ?? "null")));
    }

    public ServiceException ThrowServiceException(HttpStatusCode statusCode, int errorNumber, int resultCode,
      string errorMessage = null)
    {
      return ThrowServiceException(statusCode, errorNumber, resultCode.ToString(), errorMessage);
    }
  }
}