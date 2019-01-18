using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  public class ProjectDataResult : BaseDataResult, IMasterDataModel
  {  
    /// <summary>
    /// Gets or sets the project descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public List<ProjectData> ProjectDescriptors { get; set; }

    public List<string> GetIdentifiers()
    {
      return ProjectDescriptors?
               .SelectMany(p => p.GetIdentifiers())
               .Distinct()
               .ToList()
             ?? new List<string>();
    }
  }
}
