using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon
{
  /// <summary>
  /// The request representation used to raise an alert for a tag file processing error if required.
  /// </summary>
  public class TagFileProcessingErrorRequest
  {
    /// <summary>
    /// The id of the asset whose tag file has the error. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; set; }

    /// <summary>
    /// The name of the tag file with the error.
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "tagFileName", Required = Required.Always)]
    public string tagFileName { get; set; }

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
    public TagFileErrorsEnum error { get; set; }

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
      if (string.IsNullOrEmpty(tagFileName) || error == 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
                   new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                   "Must have filename and error number"));
      }
    }
  }
}