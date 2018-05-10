using System;
using System.Collections.Generic;
using Hangfire.States;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Adds custom information for failed export jobs
  /// </summary>
  public class ExportFailedState : FailedState, IState
  {
    public const string EXPORT_DETAILS_KEY = "AdditionalData";

    private string additionalData;

    /// <summary>
    /// 
    /// </summary>
    public ExportFailedState(Exception exception, string additonalData) : base(exception)
    {
      this.additionalData = additonalData;
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
