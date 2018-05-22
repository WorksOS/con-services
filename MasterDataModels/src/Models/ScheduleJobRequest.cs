using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Used to request an export job to be scheduled
  /// </summary>
  public class ScheduleJobRequest
  {
    /// <summary>
    /// The URL to call to get the export data
    /// </summary>
    [JsonProperty(PropertyName = "url", Required = Required.Always)]
    public string Url { get; set; }

    /// <summary>
    /// THe Http method to use. Default is GET.
    /// </summary>
    [JsonProperty(PropertyName = "method", Required = Required.Default)]
    public string Method { get; set; }

    /// <summary>
    /// Payload for POST requests
    /// </summary>
    [JsonProperty(PropertyName = "payload", Required = Required.Default)]
    public string Payload { get; set; }

    /// <summary>
    /// File name to save export data to
    /// </summary>
    [JsonProperty(PropertyName = "filename", Required = Required.Always)]
    public string Filename { get; set; }

    /// <summary>
    /// Optional timeout for running the scheduled job in milliseconds
    /// </summary>
    [JsonProperty(PropertyName = "timeout", Required = Required.Default)]
    public int? Timeout { get; set; }
  }
}
