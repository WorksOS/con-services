using System;
using System.Collections.Generic;
using Hangfire.States;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Adds custom information for failed export jobs
  /// </summary>
  public class ExportFailedState : FailedState, IState
  {
    /// <summary>
    /// Key for extracting custom failure details from failed state
    /// </summary>
    public const string EXPORT_DETAILS_KEY = "AdditionalData";

    private readonly string additionalData;

    /// <summary>
    /// 
    /// </summary>
    public ExportFailedState(Exception exception, string additionalData) : base(exception)
    {
      this.additionalData = additionalData;
    }

    /// <summary>
    /// Add custom export failure details for use by the 'get status' request
    /// </summary>
    /// <returns></returns>
    public new Dictionary<string, string> SerializeData()
    {
      var ret = base.SerializeData();
      ret[EXPORT_DETAILS_KEY] = additionalData;
      return ret;
    }
  }
}
