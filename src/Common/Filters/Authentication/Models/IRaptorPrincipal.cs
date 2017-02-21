using System.Collections.Generic;
using System.Security.Principal;

namespace VSS.Raptor.Service.Common.Filters.Authentication.Models
{
  internal interface IRaptorPrincipal : IPrincipal
  {
    string Token { get; }
    Dictionary<long, ProjectDescriptor> Projects { get; }

  }
}