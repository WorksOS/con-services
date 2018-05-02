using System.Net;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.ResultsHandling
{
  public class ReturnSuccessV2Result 
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
    private ReturnSuccessV2Result()
    { }


    /// <summary>
    /// CreateLongV2Result create instance
    /// </summary>
    /// <returns></returns>
    public static ReturnSuccessV2Result CreateReturnSuccessV2Result(HttpStatusCode code, bool success)
    {
      return new ReturnSuccessV2Result
      {
        Code = code,
        Success = success
      };
    }
    
  }
}
