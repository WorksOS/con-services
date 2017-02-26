using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.TagFileAuth.Service.WebApiModels.Enums;

namespace VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon
{
  /// <summary>
  /// The request representation used to raise an alert for a tag file processing error if required.
  /// </summary>
  public class TagFileProcessingErrorRequest // : IValidatable, IServiceDomainObject, IHelpSample
  {
    /// <summary>
    /// The id of the asset whose tag file has the error. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; private set; }

    /// <summary>
    /// The name of the tag file with the error.
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "tagFileName", Required = Required.Always)]
    public string tagFileName { get; private set; }

    /// <summary>
    /// The type of error. Values are:
    /// -2	UnknownProject
    /// -1	UnknownCell
    /// 1	NoMatchingProjectDate
    /// 2	NoMatchingProjectArea
    /// 3	MultipleProjects
    /// 4	InvalidSeedPosition
    /// 5	InvalidOnGroundFlag
    /// 6	InvalidPosition
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "error", Required = Required.Always)]
    public TagFileErrorsEnum error { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
    //private TagFileProcessingErrorRequest()
    //{ }

    /// <summary>
    /// Create instance of TagFileProcessingErrorRequest
    /// </summary>
    public static TagFileProcessingErrorRequest CreateTagFileProcessingErrorRequest(
      long assetId,
      string tagFileName,
      TagFileErrorsEnum error
      )
    {
      return new TagFileProcessingErrorRequest
      {
        assetId = assetId,
        tagFileName = tagFileName,
        error = error
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static TagFileProcessingErrorRequest HelpSample
    {
      get
      {
        return CreateTagFileProcessingErrorRequest(3984412183889397, "1003J001SW--AFS44 0021--130903184608.tag", TagFileErrorsEnum.ProjectID_NoMatchingDateTime);
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      // ...
    }
  }
}