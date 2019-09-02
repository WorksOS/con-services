using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Scheduler.Models
{
  /// <summary>
  /// Result of an export job schedule request
  /// </summary>
  public class ScheduleJobResult : IMasterDataModel
  {
    /// <summary>
    /// The job ID of the scheduled job
    /// </summary>
    [JsonProperty(PropertyName = "jobId", Required = Required.Always)]
    public string JobId { get; set; }

    public List<string> GetIdentifiers() => new List<string>(){JobId};
  }
}
