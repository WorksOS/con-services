using System.Net;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.ResultsHandling
{
  public class CreateProjectV2Result : ContractExecutionResult
  {
    /// <summary>
    /// The projectId
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public long id { get;  set; }
    
    /// <summary>
    /// Private constructor
    /// </summary>
    private CreateProjectV2Result()
    { }


    /// <summary>
    /// CreateAProjectV2Result create instance
    /// </summary>
    /// <returns></returns>
    public static CreateProjectV2Result CreateAProjectV2Result(HttpStatusCode code, int projectId)
    {
      return new CreateProjectV2Result
      {
        Code = (int) code,
        id = projectId
      };
    }
    
  }
}
