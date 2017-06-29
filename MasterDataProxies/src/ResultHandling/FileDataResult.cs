using System.Collections.Generic;
using MasterDataProxies.Models;

namespace MasterDataProxies.ResultHandling
{
  /// <summary>
  /// List of file descriptors
  /// </summary>
  public class FileDataResult
  {
    /// <summary>
    ///   Defines machine-readable code.
    /// </summary>
    /// <value>
    ///   Result code.
    /// </value>
    public int Code { get; protected set; }

    /// <summary>
    ///   Defines user-friendly message.
    /// </summary>
    /// <value>
    ///   The message string.
    /// </value>
    public string Message { get; protected set; }

    /// <summary>
    /// Gets or sets the file descriptors.
    /// </summary>
    /// <value>
    /// The file descriptors.
    /// </value>
    public List<FileData> ImportedFileDescriptors { get; set; }
  }
}
