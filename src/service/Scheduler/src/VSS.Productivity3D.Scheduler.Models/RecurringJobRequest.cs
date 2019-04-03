using System;
using System.Net;
using Cronos;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.Models
{
  public class RecurringJobRequest : JobRequest
  {
    /// <summary>
    /// Schedule for the job. Support crond format
    /// </summary>
    public string Schedule { get; set; }

    public override void Validate()
    {
      base.Validate();
      CronExpression expression;
      try
      {
        expression = CronExpression.Parse(Schedule);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid schedule - use crond format"),e);
      }

      if (expression.GetNextOccurrence(DateTime.UtcNow) < DateTime.UtcNow)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid schedule - next occurence is not in future"));
      }

    }
  }
}
