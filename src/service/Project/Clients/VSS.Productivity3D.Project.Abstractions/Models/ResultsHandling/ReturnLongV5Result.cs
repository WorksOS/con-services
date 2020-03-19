using System.Net;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class ReturnLongV5Result
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
    private ReturnLongV5Result()
    { }


    /// <summary>
    /// CreateLongV2Result create instance
    /// </summary>
    /// <returns></returns>
    public static ReturnLongV5Result CreateLongV5Result(HttpStatusCode code, long id)
    {
      return new ReturnLongV5Result
      {
        Code = code,
        Id = id
      };
    }
    
  }
}
