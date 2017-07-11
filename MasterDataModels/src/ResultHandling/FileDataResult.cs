using System.Collections.Generic;
using MasterDataModels.Models;
using VSS.Productivity3D.MasterDataProxies.Models;

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
