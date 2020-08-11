using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class ProjectDataTBCListResult : ContractExecutionResult
  {
    public List<ProjectDataTBCSingleResult> ProjectDescriptors { get; set; }

    public ProjectDataTBCListResult()
    {
      ProjectDescriptors = new List<ProjectDataTBCSingleResult>();
    }

    public ProjectDataTBCListResult(int code, string message)
    {
      Code = code;
      Message = message;
      ProjectDescriptors = new List<ProjectDataTBCSingleResult>();
    }

    public ProjectDataTBCListResult(int code, string message, List<ProjectDataTBCSingleResult> projectDescriptors)
    {
      Code = code;
      Message = message;
      ProjectDescriptors = projectDescriptors;
    }

    public ProjectDataTBCListResult(List<ProjectDataTBCSingleResult> projectDescriptors)
    {
      ProjectDescriptors = projectDescriptors;
    }
  }
}
