using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon
{
  /// <summary>
  /// The request representation used to raise an alert for a tag file processing error if required.
  /// </summary>
  public class TagFileProcessingErrorRequest
  {

    private long _assetId;
    private string _tagFileName;
    private TagFileErrorsEnum _error;

    /// <summary>
    /// The id of the asset whose tag file has the error. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get { return _assetId; } private set { _assetId = value; } }

    /// <summary>
    /// The name of the tag file with the error.
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "tagFileName", Required = Required.Always)]
    public string tagFileName { get { return _tagFileName; } private set { _tagFileName = value; } }

    [Required]
    [JsonProperty(PropertyName = "error", Required = Required.Always)]
    public TagFileErrorsEnum error { get { return _error; } private set { _error = value; } }

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
    /// Example for Help
    /// </summary>
    public static TagFileProcessingErrorRequest HelpSample
    {
      get
      {
        return CreateTagFileProcessingErrorRequest(3984412183889397, "1003J001SW--AFS44 0021--130903184608.tag", (int)TagFileErrorsEnum.ProjectID_NoMatchingDateTime);
      }
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