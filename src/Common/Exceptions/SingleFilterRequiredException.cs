using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.Exceptions
{
  public class SingleFilterRequiredException : ServiceException
  {
    public SingleFilterRequiredException(HttpStatusCode statusCode, ContractExecutionResult contractExecution) :
    base(statusCode, contractExecution)
    { }
  }
}
