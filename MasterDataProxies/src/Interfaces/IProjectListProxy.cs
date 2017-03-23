using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Raptor.Service.Common.Proxies.Models;

namespace VSS.Raptor.Service.Common.Interfaces
{
  public interface IProjectListProxy
  {
    List<ProjectData> GetProjects(string customerUid, IDictionary<string, string> customHeaders = null);
  }
}
