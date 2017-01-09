using System.Collections.Generic;
using System.Security.Principal;
using VSS.Project.WebApi.Configuration.Principal.Models;

namespace VSS.Project.Service.WebApiModels.Filters
{
    public class ProjectsPrincipal : GenericPrincipal
    {
        public Dictionary<long, ProjectDescriptor> AvailableProjects { get; private set; }

        public ProjectsPrincipal(IIdentity identity, string[] roles, Dictionary<long, ProjectDescriptor> projects ) : base(identity, roles)
        {
            AvailableProjects = projects;
        }
    }
}