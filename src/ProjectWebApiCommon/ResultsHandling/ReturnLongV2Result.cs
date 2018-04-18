using System.Net;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.ResultsHandling
{
  public class ReturnLongV2Result : ContractExecutionResult
  {
    /// <summary>
    /// The projectId
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public long id { get;  set; }
    
    /// <summary>
    /// Private constructor
    /// </summary>
    private ReturnLongV2Result()
    { }


    /// <summary>
    /// CreateLongV2Result create instance
    /// </summary>
    /// <returns></returns>
    public static ReturnLongV2Result CreateLongV2Result(HttpStatusCode code, long id)
    {
      return new ReturnLongV2Result
      {
        Code = (int) code,
        id = id
      };
    }
    
  }
}
