using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Exceptions
{
  public class TwoFiltersRequiredException : ServiceException
  {
    public TwoFiltersRequiredException(HttpStatusCode statusCode, ContractExecutionResult contractExecution) :
      base(statusCode, contractExecution)
    { }
  }
}
