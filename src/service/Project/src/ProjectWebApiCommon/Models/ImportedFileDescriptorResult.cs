using System.Collections.Immutable;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// List of importedfile descriptors
  /// </summary>
  public class ImportedFileDescriptorListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the ImportedFile descriptors.
    /// </summary>
    /// <value>
    /// The ImportedFile descriptors.
    /// </value>
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
    public ImportedFileDescriptorSingleResult(ImportedFileDescriptor importedFileDescriptor)
    {
      ImportedFileDescriptor = importedFileDescriptor;
    }

    /// <summary>
    /// Gets or sets the ImportedFile descriptors.
    /// </summary>
    /// <value>
    /// The ImportedFile descriptors.
    /// </value>
    public ImportedFileDescriptor ImportedFileDescriptor { get; set; }
  }
}
