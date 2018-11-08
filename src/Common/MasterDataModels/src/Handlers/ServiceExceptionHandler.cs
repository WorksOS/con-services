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
    /// <summary>
    /// The contract execution states enum
    /// </summary>
    private readonly IErrorCodesProvider _contractExecutionStatesEnum;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceExceptionHandler"/> class.
    /// </summary>
    /// <param name="errorProvider">The error provider.</param>
    public ServiceExceptionHandler(IErrorCodesProvider errorProvider)
    {
      _contractExecutionStatesEnum = errorProvider;
    }

    /// <summary>
    /// Correctly throw ServiceException for controller types.
    /// </summary>
    public ServiceException ThrowServiceException(HttpStatusCode statusCode, int errorNumber, string resultCode = null, string errorMessage1 = null, string errorMessage2 = null, Exception innerException = null)
    {
      throw new ServiceException(statusCode,
        new ContractExecutionResult(_contractExecutionStatesEnum.GetErrorNumberwithOffset(errorNumber),
          string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(errorNumber), resultCode, errorMessage1 ?? "null", errorMessage2 ?? "null")),innerException);
    }
  }
}