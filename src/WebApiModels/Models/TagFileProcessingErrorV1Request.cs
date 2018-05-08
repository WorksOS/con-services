using System;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// The request representation used to raise an alert for a tag file processing error if required.
  /// </summary>
  public class TagFileProcessingErrorV1Request
  {

    /// <summary>
    /// The id of the asset whose tag file has the error. 
    /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; set; }

    /// <summary>
    /// The name of the tag file with the error.
    /// </summary>
    [JsonProperty(PropertyName = "tagFileName", Required = Required.Always)]
    public string tagFileName { get; set; } = String.Empty;

    [JsonProperty(PropertyName = "error", Required = Required.Always)]
    public TagFileErrorsEnum error { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private TagFileProcessingErrorV1Request()
    {
    }

    /// <summary>
    /// Create instance of TagFileProcessingErrorRequest
    /// </summary>
    public static TagFileProcessingErrorV1Request CreateTagFileProcessingErrorRequest(long assetId, string tagFileName,
      int error)
    {
      return new TagFileProcessingErrorV1Request
      {
        assetId = assetId,
        tagFileName = tagFileName,
        error = (TagFileErrorsEnum) Enum.ToObject(typeof(TagFileErrorsEnum), error)
      };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (assetId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 9));
      }

      if (string.IsNullOrEmpty(tagFileName))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 5));
      }

      if (Enum.IsDefined(typeof(TagFileErrorsEnum), error) == false)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 4));
      }
    }
  }
}