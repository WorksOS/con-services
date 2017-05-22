using System;
using System.Collections.Immutable;
using ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectWebApiCommon.Models
{

  /// <summary>
  /// List of importedfile descriptors
  /// </summary>
  /// <seealso cref="ProjectWebApiCommon.ResultsHandling.ContractExecutionResult" />
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
  /// Single importedfile descriptor
  /// </summary>
  /// <seealso cref="ProjectWebApiCommon.ResultsHandling.ContractExecutionResult" />
  public class ImportedFileDescriptorSingleResult : ContractExecutionResult
  {
    private ImportedFileDescriptor _importedFileDescriptor;

    public ImportedFileDescriptorSingleResult(ImportedFileDescriptor importedFileDescriptor)
    {
      this._importedFileDescriptor = importedFileDescriptor;
    }

    /// <summary>
    /// Gets or sets the ImportedFile descriptors.
    /// </summary>
    /// <value>
    /// The ImportedFile descriptors.
    /// </value>
    public ImportedFileDescriptor ImportedFileDescriptor
    {
      get { return _importedFileDescriptor; }
      set { _importedFileDescriptor = value; }
    }
  }


  /// <summary>
  ///   Describes VL Imported File
  /// </summary>
  public class ImportedFileDescriptor
  {
    /// <summary>
    /// Gets or sets the Project uid.
    /// </summary>
    /// <value>
    /// The Project uid.
    /// </value>
    public string ProjectUid { get; set; }
    
    /// <summary>
    /// Gets or sets the file uid.
    /// </summary>
    /// <value>
    /// The file uid.
    /// </value>
    public string ImportedFileUid { get; set; }

    /// <summary>
    /// Gets or sets the customer uid.
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    public string CustomerUid { get; set; }

    /// <summary>
    /// Gets or sets the type of the file.
    /// </summary>
    /// <value>
    /// The type of the file.
    /// </value>
    public ImportedFileType ImportedFileType { get; set; }

    /// <summary>
    /// Gets the name of the file type.
    /// </summary>
    /// <value>
    /// The name of the file type.
    /// </value>
    public string ImportedFileTypeName => this.ImportedFileType.ToString();

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the timestamp at which the file was created on users file system.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public DateTime FileCreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the timestamp at which the file was updated on users file system.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public DateTime FileUpdatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the ImportedBy email address from authentication
    /// </summary>
    /// <value>
    /// The users email address.
    /// </value>
    public string ImportedBy { get; set; }

    /// <summary>
    /// Gets or sets the Surveyed Utc. 
    /// This only applies to a file type of SurveyedSurface, else null.
    /// </summary>
    /// <value>
    /// The start date.
    /// </value>
    public DateTime? SurveyedUtc { get; set; }

    /// <summary>
    /// The Utc when the file was last imported
    /// </summary>
    /// <value>
    /// Utc date
    /// </value>
    public DateTime ImportedUtc { get; set; }

    public override bool Equals(object obj)
    {
      var otherImportedFile = obj as ImportedFileDescriptor;
      if (otherImportedFile == null) return false;
      return otherImportedFile.ImportedFileUid == this.ImportedFileUid
             && otherImportedFile.CustomerUid == this.CustomerUid
             && otherImportedFile.ProjectUid == this.ProjectUid
             && otherImportedFile.ImportedFileType == this.ImportedFileType
             && otherImportedFile.Name == this.Name
             && otherImportedFile.FileCreatedUtc == this.FileCreatedUtc
             && otherImportedFile.FileUpdatedUtc == this.FileUpdatedUtc
             && otherImportedFile.ImportedBy == this.ImportedBy
             && otherImportedFile.SurveyedUtc == this.SurveyedUtc
             && otherImportedFile.ImportedUtc == this.ImportedUtc
        ;
    }
  }
}