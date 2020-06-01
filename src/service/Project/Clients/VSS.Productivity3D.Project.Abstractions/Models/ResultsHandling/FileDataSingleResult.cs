using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  /// <summary>
  /// Single file descriptor
  /// </summary>
  public class FileDataSingleResult : BaseDataResult
  {
    /// <summary>
    /// The file descriptor for a 3dpm imported file
    /// </summary>
    public FileData ImportedFileDescriptor { get; set; }

    /// <summary>
    /// The file descriptor for a CWS project configuration file
    /// </summary>
    public ProjectConfigurationFileResponseModel ProjectConfigFileDescriptor { get; set; }
  }
}
