using System.Collections.Generic;
using MasterDataModels.Models;
using VSS.Productivity3D.MasterDataProxies.Models;

namespace MasterDataModels.ResultHandling
{
  public class ProjectDataResult : BaseDataResult
  {  
    /// <summary>
    /// Gets or sets the project descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public List<ProjectData> ProjectDescriptors { get; set; }
  }
}
