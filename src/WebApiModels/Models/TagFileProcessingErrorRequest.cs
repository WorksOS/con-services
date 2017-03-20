using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using WebApiModels.Enums;
using WebApiModels.ResultHandling;

namespace WebApiModels.Models
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
    public long assetId  { get; set; } 

    /// <summary>
    /// The name of the tag file with the error.
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "tagFileName", Required = Required.Always)]
    public string tagFileName { get; set; } = String.Empty;

    [Required]
    [JsonProperty(PropertyName = "error", Required = Required.Always)]
    public TagFileErrorsEnum error { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private TagFileProcessingErrorRequest()
    { }

    /// <summary>
    /// Create instance of TagFileProcessingErrorRequest
    /// </summary>
    public static TagFileProcessingErrorRequest CreateTagFileProcessingErrorRequest(long assetId, string tagFileName, int error)
    {
      return new TagFileProcessingErrorRequest
      {
        assetId = assetId,
        tagFileName = tagFileName,
        error = (TagFileErrorsEnum)Enum.ToObject(typeof(TagFileErrorsEnum), error)
      };
    }
    

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (assetId <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("Must have assetId {0}", assetId)));
      }

      if (string.IsNullOrEmpty(tagFileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
                   new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                   "Must have filename"));
      }

      if (Enum.IsDefined(typeof(TagFileErrorsEnum), error) == false)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
                   new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                   "Must have valid error number"));
      }
    }
  }
}