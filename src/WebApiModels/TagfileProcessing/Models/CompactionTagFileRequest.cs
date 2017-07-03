using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Filters.Validation;

namespace VSS.Productivity3D.WebApiModels.TagfileProcessing.Models
{
  /// <summary>
  /// TAG file domain object. Model represents TAG file submitted to Raptor.
  /// </summary>
  public class CompactionTagFileRequest 
  {
    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    [ValidProjectUID]
    public Guid projectUid { get; private set; }

    /// <summary>
    /// The name of the TAG file.
    /// </summary>
    /// <value>Required. Shall contain only ASCII characters. Maximum length is 256 characters.</value>
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [Required]
    [ValidFilename(256)]
    [MaxLength(256)]
    public string fileName { get; private set; }

    /// <summary>
    /// The content of the TAG file as an array of bytes.
    /// </summary>
    [JsonProperty(PropertyName = "data", Required = Required.Always)]
    [Required]
    public byte[] data { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionTagFileRequest()
    { }

    /// <summary>
    /// Create instance of CompactionTagFileRequest
    /// </summary>
    /// <param name="fileName">file name</param>
    /// <param name="data">metadata</param>
    /// <param name="projectUid">project UID</param>
    /// <returns></returns>
    public static CompactionTagFileRequest CreateCompactionTagFileRequest(
      string fileName,
      byte[] data,
      Guid projectUid)
    {
      return new CompactionTagFileRequest
      {
        fileName = fileName,
        data = data,
        projectUid = projectUid
      };
    }
  }
}
