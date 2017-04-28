using System;
using System.Collections.Immutable;
using ProjectWebApiCommon.ResultsHandling;
using Repositories.DBModels;
using Newtonsoft.Json;

namespace ProjectWebApiCommon.Models
{

  /// <summary>
  /// The request representation used to Create an imported file. 
  /// </summary>
  public class CreateImportedFileRequest 
  {
    /// <summary>
    /// The project which the file will be associated with
    /// the project must belong to the customer in the header
    /// </summary>
    [JsonProperty(PropertyName = "ProjectUID", Required = Required.Always)]
    public Guid ProjectUID { get; set; }


    /// <summary>
    /// Must be a valid design file type
    /// </summary>
    [JsonProperty(PropertyName = "ImportedFileType", Required = Required.Always)]
    public ImportedFileType ImportedFileType { get; set; }

    /// <summary>
    /// The name of the design file type
    /// </summary>
    [JsonProperty(PropertyName = "ImportedFileType", Required = Required.Always)]
    public string ImportedFileTypeName => this.ImportedFileType.ToString();

    /// <summary>
    /// The name of the imported file.
    /// </summary>
    [JsonProperty(PropertyName = "Name", Required = Required.Always)]
    public string Name { get; set; }

    /// <summary>
    /// SurveyedUtc is only required where file type is SurveyedSurface.
    /// </summary>
    [JsonProperty(PropertyName = "SurveyedUtc", Required = Required.Default)]
    public DateTime? SurveyedUtc { get; set; }

    /// <summary>
    /// The files Content.
    /// </summary>
    [JsonProperty(PropertyName = "Content", Required = Required.Always)]
    public byte[] Content { get; set; }


    public override bool Equals(object obj)
    {
      var otherImportedFile = obj as CreateImportedFileRequest;
      if (otherImportedFile == null) return false;
      return otherImportedFile.ProjectUID == this.ProjectUID
             && otherImportedFile.ImportedFileType == this.ImportedFileType
             && otherImportedFile.Name == this.Name
             && otherImportedFile.SurveyedUtc == this.SurveyedUtc
             && otherImportedFile.Content == this.Content
          ;
    }
  }
}