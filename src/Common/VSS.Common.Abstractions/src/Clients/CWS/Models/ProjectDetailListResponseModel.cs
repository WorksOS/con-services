using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectDetailListResponseModel
  {
    public ProjectDetailListResponseModel()
    {
      Projects = new List<ProjectDetailResponseModel>();
    }

    /// <summary>
    /// Projects
    /// </summary>
    [JsonProperty("projects")]
    public List<ProjectDetailResponseModel> Projects { get; set; }

  }

}
