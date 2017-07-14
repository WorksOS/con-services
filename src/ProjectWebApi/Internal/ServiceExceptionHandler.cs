using System.Net;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Internal
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
  }
}