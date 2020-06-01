using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  /// <summary>
  /// List of file descriptors
  /// </summary>
  public class FileDataResult : BaseDataResult, IMasterDataModel
  {
    /// <summary>
    /// The file descriptors for 3dpm imported files
    /// </summary>
    public List<FileData> ImportedFileDescriptors { get; set; }

    /// <summary>
    /// The file descriptors for CWS project configuration files
    /// </summary>
    public List<ProjectConfigurationFileResponseModel> ProjectConfigFileDescriptors { get; set; }

    public List<string> GetIdentifiers() => ImportedFileDescriptors?
                                              .SelectMany(f => f.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ??
                                            ProjectConfigFileDescriptors?
                                              .SelectMany(f => f.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();

  }
}
