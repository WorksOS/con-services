using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
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
