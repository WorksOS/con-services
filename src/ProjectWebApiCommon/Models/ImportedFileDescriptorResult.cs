using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// List of importedfile descriptors
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
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
  /// <seealso cref="ContractExecutionResult" />
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
  /// <seealso cref="ContractExecutionResult" />
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
    /// Gets or sets a unique file identifier's value from legacy VisionLink.
    /// </summary>
    /// <value>
    /// The file id.
    /// </value>
    public long LegacyFileId { get; set; }

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
    /// Gets or sets the units type of the DXF file.
    /// </summary>
    /// <value>The units type of the DXF file.</value>
    public DxfUnitsType DxfUnitsType { get; set; }

    /// <summary>
    /// Gets the name of the file type.
    /// </summary>
    /// <value>
    /// The name of the file type.
    /// </value>
    public string ImportedFileTypeName => ImportedFileType.ToString();

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

    /// <summary>
    /// Gets or sets the Activation State of the imported file.
    /// </summary>
    public bool IsActivated { get; set; }

    /// <summary>
    /// Gets the path of the imported file
    /// </summary>
    /// <value>
    /// Path of the imported file
    /// </value>
    public string Path => "/" + CustomerUid + "/" + ProjectUid;

    /// <summary>
    /// The minimum zoom level for DXF tiles
    /// </summary>
    public int MinZoomLevel { get; set; }

    /// <summary>
    /// The maximum zoom level for DXF tiles
    /// </summary>
    public int MaxZoomLevel { get; set; }

    public List<ImportedFileHistoryItem> ImportedFileHistory { get; set; }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      var otherImportedFile = obj as ImportedFileDescriptor;
      if (otherImportedFile == null) return false;
      return otherImportedFile.ImportedFileUid == this.ImportedFileUid
             && otherImportedFile.LegacyFileId == this.LegacyFileId
             && otherImportedFile.CustomerUid == this.CustomerUid
             && otherImportedFile.ProjectUid == this.ProjectUid
             && otherImportedFile.ImportedFileType == this.ImportedFileType
             && otherImportedFile.DxfUnitsType == this.DxfUnitsType
             && otherImportedFile.Name == this.Name
             && otherImportedFile.FileCreatedUtc == this.FileCreatedUtc
             && otherImportedFile.FileUpdatedUtc == this.FileUpdatedUtc
             && otherImportedFile.ImportedBy == this.ImportedBy
             && otherImportedFile.SurveyedUtc == this.SurveyedUtc
             && otherImportedFile.ImportedUtc == this.ImportedUtc
             && otherImportedFile.IsActivated == this.IsActivated
             && otherImportedFile.MinZoomLevel == this.MinZoomLevel
             && otherImportedFile.MaxZoomLevel == this.MaxZoomLevel
             && otherImportedFile.ImportedFileHistory == this.ImportedFileHistory
        ;
    }
  }

  public class ImportedFileHistoryItem
  {
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    

    public override bool Equals(object obj)
    {
      var otherImportedFileImport = obj as ImportedFileHistoryItem;
      if (otherImportedFileImport == null) return false;
      return otherImportedFileImport.FileCreatedUtc == FileCreatedUtc
             && otherImportedFileImport.FileUpdatedUtc == FileUpdatedUtc;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}
