using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.Raptor.Service.Common.Filters.Authentication.Models
{
    public interface IAuthenticatedProjectsStore
    {
      Dictionary<long, ProjectDescriptor> GetProjectsById(string customerUid);
      Dictionary<string, ProjectDescriptor> GetProjectsByUid(string customerUid);
      void SetAuthenticatedProjectList(string customerUid, List<ProjectDescriptor> projects);
    }
}
