using Newtonsoft.Json;

namespace VSS.MasterData.Models.ResultHandling
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
