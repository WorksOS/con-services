using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IProjectProxy : ICacheProxy
  {
    Task<List<ProjectData>> GetProjects(string customerUid, IHeaderDictionary customHeaders = null);

    Task<ProjectData> GetProjectForCustomer(string customerUid, string projectUid, IHeaderDictionary customHeaders = null);

    //To support 3dpm v1 end points which use legacy project id
    Task<ProjectData> GetProjectForCustomer(string customerUid, long shortRaptorProjectId, IHeaderDictionary customHeaders = null);
  }
}
