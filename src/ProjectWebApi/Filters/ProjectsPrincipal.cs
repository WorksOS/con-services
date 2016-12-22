using System.Collections.Generic;
using System.Security.Principal;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models;

namespace VSS.UnifiedProductivity.Service.WebApiModels.Filters
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