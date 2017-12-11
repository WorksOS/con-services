using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Single file descriptor
  /// </summary>
  public class FileDataSingleResult : BaseDataResult
  {
    /// <summary>
    /// Gets or sets the ImportedFile descriptors.
    /// </summary>
    /// <value>
    /// The ImportedFile descriptors.
    /// </value>
    public FileData ImportedFileDescriptor { get; set; }
  }
}
