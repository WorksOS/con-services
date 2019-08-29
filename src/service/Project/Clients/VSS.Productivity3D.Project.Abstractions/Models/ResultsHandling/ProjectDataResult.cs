using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
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
