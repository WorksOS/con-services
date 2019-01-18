using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// List of file descriptors
  /// </summary>
  public class FileDataResult : BaseDataResult, IMasterDataModel
  {
    /// <summary>
    /// Gets or sets the file descriptors.
    /// </summary>
    /// <value>
    /// The file descriptors.
    /// </value>
    public List<FileData> ImportedFileDescriptors { get; set; }

    public List<string> GetIdentifiers() => ImportedFileDescriptors?
                                         .SelectMany(f => f.GetIdentifiers())
                                         .Distinct()
                                         .ToList()
                                       ?? new List<string>();
  }
}
