namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Result of a veta export job status query
  /// </summary>
  public class JobStatusResult
  {
    /// <summary>
    /// The S3 key where the file is stored. 
    /// This is the full path and filename of the zipped file.
    /// </summary>
    public string key { get; set; }

    /// <summary>
    /// The current status of the job
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// The redirect url for downloading on successful completion.
    /// Only set when the status is 'succeeded'.
    /// </summary>
    public string downloadLink { get; set; }
  }
}
