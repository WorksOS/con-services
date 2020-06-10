using System.Collections.Immutable;
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

    /// <summary>
    /// The file descriptor for a 3dpm imported file
    /// </summary>
    public ImportedFileDescriptor ImportedFileDescriptor { get; set; }
  }
}
