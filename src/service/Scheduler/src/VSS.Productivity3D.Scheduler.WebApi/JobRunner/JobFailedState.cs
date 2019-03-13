using System;
using System.Collections.Generic;
using Hangfire.States;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// Adds custom information for failed VSS jobs
  /// </summary>
  public class JobFailedState : FailedState, IState
  {
    /// <summary>
    /// Key for extracting custom failure details from failed state
    /// </summary>
    public const string JOB_DETAILS_KEY = "NotificationData";

    private readonly string notificationData;

    /// <summary>
    /// 
    /// </summary>
    public JobFailedState(Exception exception, string notificationData) : base(exception)
    {
      this.notificationData = notificationData;
    }

    /// <summary>
    /// Add custom job failure details to be saved in the database
    /// </summary>
    public new Dictionary<string, string> SerializeData()
    {
      var ret = base.SerializeData();
      ret[JOB_DETAILS_KEY] = notificationData;
      return ret;
    }
  }

}
