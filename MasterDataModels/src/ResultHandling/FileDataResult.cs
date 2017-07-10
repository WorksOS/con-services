using System.Collections.Generic;
using MasterDataModels.Models;

namespace MasterDataModels.ResultHandling
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
