using System.Collections.Immutable;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  /// <summary>
  /// List of importedfile descriptors
  /// </summary>
  public class ImportedFileDescriptorListResult : ContractExecutionResult
  {
    /// <summary>
    /// The file descriptors for 3dpm imported files
    /// </summary>
    public ImmutableList<ImportedFileDescriptor> ImportedFileDescriptors { get; set; }

    /// <summary>
    /// The file descriptors for CWS project configuration files
    /// </summary>
    public ImmutableList<ProjectConfigurationFileResponseModel> ProjectConfigFileDescriptors { get; set; }
  }

  /// <summary>
  /// List of activated ImportedFile descriptors
  /// </summary>
  public class ActivatedFileDescriptorListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the activated ImportedFile descriptors.
    /// </summary>
    /// <value>
    /// The activated ImportedFile descriptors.
    /// </value>
    public ImmutableList<ActivatedFileDescriptor> ActivatedFileDescriptors { get; set; }
  }

  /// <summary>
  /// Single importedfile descriptor
  /// </summary>
  public class ImportedFileDescriptorSingleResult : ContractExecutionResult
  {
    public ImportedFileDescriptorSingleResult()
    {

    }

    public ImportedFileDescriptorSingleResult(ImportedFileDescriptor importedFileDescriptor)
    {
      ImportedFileDescriptor = importedFileDescriptor;
    }

    public ImportedFileDescriptorSingleResult(ProjectConfigurationFileResponseModel projectConfigFileDescriptor)
    {
      ProjectConfigFileDescriptor = projectConfigFileDescriptor;
    }

    /// <summary>
    /// The file descriptor for a 3dpm imported file
    /// </summary>
    public ImportedFileDescriptor ImportedFileDescriptor { get; set; }

    /// <summary>
    /// The file descriptor for a CWS project configuration file
    /// </summary>
    public ProjectConfigurationFileResponseModel ProjectConfigFileDescriptor { get; set; }
  }
}
