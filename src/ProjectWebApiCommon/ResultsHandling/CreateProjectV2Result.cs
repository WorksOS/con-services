using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.ResultsHandling
{
  public class CreateProjectV2Result : ContractExecutionResult
  {
    /// <summary>
    /// The projectId
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public long projectId { get;  set; }
    
    /// <summary>
    /// Private constructor
    /// </summary>
    private CreateProjectV2Result()
    { }


    /// <summary>
    /// ProjectSettingsResult create instance
    /// </summary>
    /// <returns></returns>
    public static CreateProjectV2Result CreateAProjectV2Result(int projectId)
    {
      return new CreateProjectV2Result
      {
        projectId = projectId
      };
    }
    
  }
}
