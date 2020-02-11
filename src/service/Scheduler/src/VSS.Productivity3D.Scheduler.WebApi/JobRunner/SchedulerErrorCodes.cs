namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
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
