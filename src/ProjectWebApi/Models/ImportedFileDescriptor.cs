using System;
using System.Collections.Immutable;
using ProjectWebApi.ResultsHandling;
using Repositories.DBModels;

namespace ProjectWebApi.Models
{

  /// <summary>
  /// Describes standard output for the ImportedFile descriptors
  /// </summary>
  /// <seealso cref="ProjectWebApi.ResultsHandling.ContractExecutionResult" />
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
  ///   Describes VL Imported File
  /// </summary>
  public class ImportedFileDescriptor
  {
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
    /// Gets or sets the Project uid.
    /// </summary>
    /// <value>
    /// The Project uid.
    /// </value>
    public string ProjectUid { get; set; }

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
    /// Gets or sets the Surveyed Utc. 
    /// This only applies to a file type of SurveyedSurface, else null.
    /// </summary>
    /// <value>
    /// The start date.
    /// </value>
    public DateTime? SurveyedUtc { get; set; }
    
    
    public override bool Equals(object obj)
    {
      var otherImportedFile = obj as ImportedFileDescriptor;
      if (otherImportedFile == null) return false;
      return otherImportedFile.ImportedFileUid == this.ImportedFileUid
             && otherImportedFile.CustomerUid == this.CustomerUid
             && otherImportedFile.ProjectUid == this.ProjectUid
             && otherImportedFile.ImportedFileType == this.ImportedFileType
             && otherImportedFile.Name == this.Name
             && otherImportedFile.SurveyedUtc == this.SurveyedUtc
          ;
    }
  }
}