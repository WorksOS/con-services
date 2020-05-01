using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class ProjectDataListResult : ContractExecutionResult
  {
    public List<ProjectData> ProjectDescriptors { get; set; }

    public ProjectDataListResult()
    {
      ProjectDescriptors = new List<ProjectData>();
    }

    public ProjectDataListResult(int code, string message)
    {
      Code = code;
      Message = message;
    }

    public ProjectDataListResult(int code, string message, List<ProjectData> projectDescriptors)
    {
      Code = code;
      Message = message;
      ProjectDescriptors = projectDescriptors;
    }
  }
}
