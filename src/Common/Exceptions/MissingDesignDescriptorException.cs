using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.Exceptions
{
  public class MissingDesignDescriptorException : ServiceException
  {
    public MissingDesignDescriptorException(HttpStatusCode statusCode, ContractExecutionResult contractExecution) :
    base(statusCode, contractExecution)
    { }
  }
}
