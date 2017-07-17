using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
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
