using Newtonsoft.Json;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Result of an export job status query
  /// </summary>
  public class JobStatusResult
  {
    /// <summary>
    /// The S3 key where the file is stored. 
    /// This is the full path and filename of the zipped file.
    /// </summary>
    [JsonProperty(PropertyName = "key", Required = Required.Default)]
    public string Key { get; set; }

    /// <summary>
    /// The current status of the job
    /// </summary>
    [JsonProperty(PropertyName = "status", Required = Required.Always)]
    public string Status { get; set; }

    /// <summary>
    /// The redirect url for downloading on successful completion.
    /// Only set when the status is 'succeeded'.
    /// </summary>
    [JsonProperty(PropertyName = "downloadLink", Required = Required.Default)]
    public string DownloadLink { get; set; }

    /// <summary>
    /// The details of why the job failed
    /// Only set when the status is 'failed'.
    /// </summary>
    [JsonProperty(PropertyName = "failureDetails", Required = Required.Default)]
    public FailureDetails FailureDetails { get; set; }
  }
}
