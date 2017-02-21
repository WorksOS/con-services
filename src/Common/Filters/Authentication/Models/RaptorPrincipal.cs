using System.Collections.Generic;
using System.Security.Principal;

namespace VSS.Raptor.Service.Common.Filters.Authentication.Models
{
  public class RaptorPrincipal : IRaptorPrincipal
  {
    public bool IsInRole(string role)
    {
      return false;
    }

    public RaptorPrincipal()
    {
      this.Identity = new GenericIdentity("RaptorUser");
    }

    public RaptorPrincipal(string token, Dictionary<long, ProjectDescriptor> projects)
    {
      this.Identity = new GenericIdentity("RaptorUser");
      Token = token;
      Projects = projects;
    }

    public IIdentity Identity { get; private set; }
    public string Token { get; private set; }
    public Dictionary<long, ProjectDescriptor> Projects { get; private set; }
  }
}