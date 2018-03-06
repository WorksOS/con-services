using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.Exceptions
{
  public class TwoFiltersRequiredException : ServiceException
  {
    public TwoFiltersRequiredException(HttpStatusCode statusCode, ContractExecutionResult contractExecution) :
    base(statusCode, contractExecution)
    { }
  }
}
