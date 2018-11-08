using System.Net;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.ResultsHandling
{
  public class ReturnLongV2Result
  {

    /// <value>
    ///   Result code.
    /// </value>
    [JsonProperty(PropertyName = "code", Required = Required.Always)]
    public HttpStatusCode Code { get; set; }

    /// <summary>
    /// a legacyId (project or importedFile)
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public long Id { get;  set; }
    
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
        Code = code,
        Id = id
      };
    }
    
  }
}
