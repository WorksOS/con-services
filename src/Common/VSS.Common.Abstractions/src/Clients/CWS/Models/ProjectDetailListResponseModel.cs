using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectDetailListResponseModel : IMasterDataModel
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

    public List<string> GetIdentifiers()
    {
      return Projects != null ? Projects.SelectMany(p => p.GetIdentifiers()).ToList() : new List<string>();
    }
  }
}
