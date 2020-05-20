using System;
using System.Net;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Common.Exceptions
{
  /// <summary>
  /// Common controller ServiceException handler.
  /// </summary>
  public class ServiceExceptionHandler : IServiceExceptionHandler
  {
    private readonly IErrorCodesProvider _contractExecutionStatesEnum;

    public ServiceExceptionHandler(IErrorCodesProvider errorProvider)
    {
      _contractExecutionStatesEnum = errorProvider;
    }

    /// <inheritdoc />
    public ServiceException ThrowServiceException(HttpStatusCode statusCode, int errorNumber, string resultCode = null, string errorMessage1 = null, string errorMessage2 = null, Exception innerException = null)
    {
      throw new ServiceException(statusCode,
        new ContractExecutionResult(_contractExecutionStatesEnum.GetErrorNumberwithOffset(errorNumber),
          string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(errorNumber), resultCode, errorMessage1 ?? "null", errorMessage2 ?? "null")), innerException);
    }

    /// <inheritdoc />
    public ContractExecutionResult CreateServiceError(HttpStatusCode statusCode, int errorNumber, string resultCode = null, string errorMessage1 = null, string errorMessage2 = null, Exception innerException = null)
    {
      return new ContractExecutionResult(_contractExecutionStatesEnum.GetErrorNumberwithOffset(errorNumber),
                                         string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(errorNumber), resultCode, errorMessage1 ?? "null", errorMessage2 ?? "null"));
    }
  }
}
