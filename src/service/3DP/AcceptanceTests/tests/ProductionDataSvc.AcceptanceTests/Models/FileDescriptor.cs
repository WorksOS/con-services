namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// Description to identify a file by its location in TCC.
  /// </summary>
  public class FileDescriptor : RequestBase
  {
    /// <summary>
    /// The id of the filespace in TCC where the file is located.
    /// </summary>
    public string filespaceId { get; set; }

    /// <summary>
    /// The full path of the file.
    /// </summary>
    public string path { get; set; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    public string fileName { get; set; }
  }
}
