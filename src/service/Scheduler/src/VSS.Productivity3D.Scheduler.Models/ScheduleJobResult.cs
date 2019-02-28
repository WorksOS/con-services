using Newtonsoft.Json;

namespace VSS.Productivity3D.Scheduler.Models
{
  /// <summary>
  /// Result of an export job schedule request
  /// </summary>
  public class ScheduleJobResult
  {
    /// <summary>
    /// The job ID of the scheduled job
    /// </summary>
    [JsonProperty(PropertyName = "jobId", Required = Required.Always)]
    public string JobId { get; set; }
  }
}
