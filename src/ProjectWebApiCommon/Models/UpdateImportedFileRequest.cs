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
  public class UpdateImportedFileRequest 
  {
    /// <summary>
    /// The project which the file will be associated with
    /// the project must belong to the customer in the header
    /// </summary>
    [JsonProperty(PropertyName = "ProjectUID", Required = Required.Always)]
    public Guid ProjectUID { get; set; }
    
    /// <summary>
                                            /// The unique ID of the imported File
                                            /// </summary>
    [JsonProperty(PropertyName = "ImportedFileUid", Required = Required.Always)]
    public string ImportedFileUid { get; set; }

   
    /// <summary>
    /// The name of the imported file.
    /// </summary>
    [JsonProperty(PropertyName = "Name", Required = Required.Always)]
    public string Name { get; set; }

    // todo clarify what/how to update
    /// cannot change type
    /// surveyedUtc appears to be included in the fileName in CG & VL should we do the same?
    /// concept of the file created date, then update dates
   
    ///// <summary>
    ///// SurveyedUtc is only required where file type is SurveyedSurface.
    ///// </summary>
    //[JsonProperty(PropertyName = "SurveyedUtc", Required = Required.Default)]
    //public DateTime? SurveyedUtc { get; set; }

    /// <summary>
    /// The files Content.
    /// </summary>
    [JsonProperty(PropertyName = "Content", Required = Required.Always)]
    public byte[] Content { get; set; }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      var otherImportedFile = obj as UpdateImportedFileRequest;
      if (otherImportedFile == null) return false;
      return otherImportedFile.ProjectUID == this.ProjectUID
             && otherImportedFile.Name == this.Name
             && otherImportedFile.Content == this.Content
          ;
    }
  }
}