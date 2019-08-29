using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.Models
{
  /// <summary>
  /// Request for running a VSS job
  /// </summary>
  public class JobRequest
  {
    /// <summary>
    /// Unique id for the job
    /// </summary>
    public Guid JobUid { get; set; }
    /// <summary>
    /// Any parameters required for setting up the job. Optional.
    /// </summary>
    public object SetupParameters { get; set; }
    /// <summary>
    /// Any parameters required for running the job. 
    /// </summary>
    public object RunParameters { get; set; }
    /// <summary>
    /// Any parameters required for tearing down the job. Optional.
    /// </summary>
    public object TearDownParameters { get; set; }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public virtual void Validate()
    {
      if (JobUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing VSS job id"));
      }
    }
  }
}
