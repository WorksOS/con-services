using System.Net;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class ReturnSuccessV5Result 
  {
    /// <value>
    ///   Result code.
    /// </value>
    [JsonProperty(PropertyName = "code", Required = Required.Always)]
    public HttpStatusCode Code { get; set; }

    /// <summary>
    /// Succeeded or not
    /// </summary>
    [JsonProperty(PropertyName = "success")]
    public bool Success { get;  set; }
    
    /// <summary>
    /// Private constructor
    /// </summary>
    private ReturnSuccessV5Result()
    { }


    /// <summary>
    /// CreateLongV2Result create instance
    /// </summary>
    /// <returns></returns>
    public static ReturnSuccessV5Result CreateReturnSuccessV5Result(HttpStatusCode code, bool success)
    {
      return new ReturnSuccessV5Result
      {
        Code = code,
        Success = success
      };
    }
    
  }
}
