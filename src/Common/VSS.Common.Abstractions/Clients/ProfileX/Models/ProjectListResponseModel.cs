using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.ProfileX.Models
{
  public class ProjectListResponseModel
  {
    public ProjectListResponseModel()
    {
      Data = new List<ProjectResponseModel>();
    }

    /// <summary>
    /// Projects
    /// </summary>
    [JsonProperty("data")]
    public List<ProjectResponseModel> Data { get; set; }

    /// <summary>
    /// Returned as true if the result has more records to display. Helps in pagination. False implies that there are no more records to display.
    /// </summary>
    [JsonProperty("hasMore")]
    public bool HasMore { get; set; }
  }
}