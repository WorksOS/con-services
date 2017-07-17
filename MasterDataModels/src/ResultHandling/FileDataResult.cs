using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// List of file descriptors
  /// </summary>
  public class FileDataResult : BaseDataResult
  {
    /// <summary>
    /// Gets or sets the file descriptors.
    /// </summary>
    /// <value>
    /// The file descriptors.
    /// </value>
    public List<FileData> ImportedFileDescriptors { get; set; }
  }
}
