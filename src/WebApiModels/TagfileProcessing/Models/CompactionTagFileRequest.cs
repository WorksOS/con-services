using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.MasterData.Models.FIlters;
using VSS.Productivity3D.Common.Filters.Validation;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models
{
  /// <summary>
  /// TAG file domain object. Model represents TAG file submitted to Raptor.
  /// </summary>
  public class CompactionTagFileRequest
  {
    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public Guid? ProjectUid { get; private set; }

    /// <summary>
    /// The name of the TAG file.
    /// </summary>
    /// <value>Required. Shall contain only ASCII characters. Maximum length is 256 characters.</value>
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [Required]
    [ValidFilename(256)]
    [MaxLength(256)]
    public string FileName { get; private set; }

    /// <summary>
    /// The content of the TAG file as an array of bytes.
    /// </summary>
    [JsonProperty(PropertyName = "data", Required = Required.Always)]
    [Required]
    public byte[] Data { get; private set; }

    /// <summary>
    /// Defines Org ID (either from TCC or Connect) to support project-based subs
    /// </summary>
    [JsonProperty(PropertyName = "OrgID", Required = Required.Default)]
    public string OrgId { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionTagFileRequest()
    { }

    /// <summary>
    /// Create instance of CompactionTagFileRequest
    /// </summary>
    /// <param name="fileName">file name</param>
    /// <param name="data">metadata</param>
    /// <param name="orgId">Organisation Id.</param>
    /// <param name="projectUid">project UID</param>
    public static CompactionTagFileRequest CreateCompactionTagFileRequest(
      string fileName,
      byte[] data,
      string orgId = null,
      Guid? projectUid = null)
    {
      return new CompactionTagFileRequest
      {
        FileName = fileName,
        Data = data,
        OrgId = orgId,
        ProjectUid = projectUid
      };
    }
  }
}
