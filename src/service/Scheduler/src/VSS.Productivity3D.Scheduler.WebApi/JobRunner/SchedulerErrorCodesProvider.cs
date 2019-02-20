using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// Provides scheduler specific errors
  /// </summary>
  public class SchedulerErrorCodesProvider : ContractExecutionStatesEnum
  {
    public SchedulerErrorCodesProvider()
    {
      DynamicAddwithOffset("Failed to execute VSS job {0}: {1}", (int)SchedulerErrorCodes.VSSJobExecutionFailure);
      DynamicAddwithOffset("Failed to create VSS job {0}: {1}", (int)SchedulerErrorCodes.VSSJobCreationFailure);
      DynamicAddwithOffset("Cannot find VSS job with uid {0}", (int)SchedulerErrorCodes.MissingVSSJob);
    }

    /// <summary>
    /// Utility method for creating an error result
    /// </summary>
    public ContractExecutionResult CreateErrorResult(SchedulerErrorCodes errorNumber, string errorMessage1 = null, string errorMessage2 = null)
    {
      return new ContractExecutionResult(GetErrorNumberwithOffset((int)errorNumber),
        string.Format(FirstNameWithOffset((int)errorNumber), errorMessage1 ?? "null", errorMessage2 ?? "null"));
    }    
  }

  /// <summary>
  /// Scheduler specific error codes
  /// </summary>
  public enum SchedulerErrorCodes
  {
    Success = 0,
    VSSJobExecutionFailure = 1,
    VSSJobCreationFailure = 2,
    MissingVSSJob = 3
  }
}
