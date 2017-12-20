namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Result of a veta export job status query
  /// </summary>
  public class JobStatusResult
  {
    /// <summary>
    /// THe S3 key where the file is stored
    /// </summary>
    public string key { get; set; }

    /// <summary>
    /// The current status of the job
    /// </summary>
    public string status { get; set; }
  }
}
