using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.Raptor.Service.Common.Filters.Authentication.Models
{
    public interface IAuthenticatedProjectsStore
    {
      Dictionary<long, ProjectDescriptor> ProjectsById { get; }
      Dictionary<string, ProjectDescriptor> ProjectsByUid { get; }
      void SetAuthenticatedProjectList(List<ProjectDescriptor> projects);
    }
}
