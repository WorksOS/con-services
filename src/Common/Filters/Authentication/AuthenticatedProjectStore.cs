using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;

namespace VSS.Raptor.Service.Common.Filters.Authentication
{
  public class AuthenticatedProjectStore : IAuthenticatedProjectsStore
  {
    private readonly Dictionary<string, Dictionary<long, ProjectDescriptor>> customerProjectsById = 
      new Dictionary<string, Dictionary<long, ProjectDescriptor>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, ProjectDescriptor>> customerProjectsByUid = 
      new Dictionary<string, Dictionary<string, ProjectDescriptor>>(StringComparer.OrdinalIgnoreCase);

    public Dictionary<long, ProjectDescriptor> GetProjectsById(string customerUid)
    {
      return customerProjectsById[customerUid];
    }

    public Dictionary<string, ProjectDescriptor> GetProjectsByUid(string customerUid)
    {
      return customerProjectsByUid[customerUid];
    }

    public void SetAuthenticatedProjectList(string customerUid, List<ProjectDescriptor> projects)
    {
      var projectsById = new Dictionary<long, ProjectDescriptor>();
      var projectsByUid = new Dictionary<string, ProjectDescriptor>(StringComparer.OrdinalIgnoreCase);
      foreach (var project in projects)
      {
        projectsById.Add(project.projectId, project);
        projectsByUid.Add(project.projectUid, project);
      }
      if (customerProjectsById.ContainsKey(customerUid))
        customerProjectsById.Remove(customerUid);
      customerProjectsById.Add(customerUid, projectsById);
      if (customerProjectsByUid.ContainsKey(customerUid))
        customerProjectsByUid.Remove(customerUid);
      customerProjectsByUid.Add(customerUid, projectsByUid);
    }
  }
}
