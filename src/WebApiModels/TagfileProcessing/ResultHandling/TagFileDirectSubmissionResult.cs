using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling
{
  public class TagFileDirectSubmissionResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets the API error code.
    /// </summary>
    [JsonProperty(PropertyName = "Continue", Required = Required.Always)]
    public bool? Continuable { get; private set; }
    
    /// <summary>
    /// Gets the error type flag indicating whether the machine submitting the TAG file
    /// should requeue the request for later process (Temporary) or discard it (Permanent).
    /// </summary>
    [JsonProperty(PropertyName = "Type", Required = Required.Always)]
    public string Type { get; private set; }

    private TagFileDirectSubmissionResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static TagFileDirectSubmissionResult Create(TagFileProcessResultHelper resultHelper)
    {
      return new TagFileDirectSubmissionResult
      {
        Code = resultHelper.Code,
        Message = resultHelper.Message,
        Type = resultHelper.Type,
        Continuable = resultHelper.Continuable
      };
    }
  }
}
