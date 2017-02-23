using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;

namespace VSS.Raptor.Service.Common.Filters.Authentication
{
  public class AuthenticatedProjectStore : IAuthenticatedProjectsStore
  {
    public Dictionary<long, ProjectDescriptor> ProjectsById { get; private set; }
    public Dictionary<string, ProjectDescriptor> ProjectsByUid { get; private set; }

    public void SetAuthenticatedProjectList(List<ProjectDescriptor> projects)
    {
      var projectsById = new Dictionary<long, ProjectDescriptor>();
      var projectsByUid = new Dictionary<string, ProjectDescriptor>();
      foreach (var project in projects)
      {
        projectsById.Add(project.projectId, project);
        projectsByUid.Add(project.projectUid, project);
      }
      ProjectsById = projectsById;
      ProjectsByUid = projectsByUid;
    }
  }
}
