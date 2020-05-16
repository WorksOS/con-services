using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IProjectInternalProxy : ICacheProxy
  {
    Task<List<ProjectData>> GetProjects(string customerUid, IHeaderDictionary customHeaders = null);

    Task<ProjectData> GetProject(string projectUid, IHeaderDictionary customHeaders = null);

    Task<ProjectData> GetProject(long shortRaptorProjectId, IHeaderDictionary customHeaders = null);

    Task<ProjectDataResult> GetIntersectingProjects(string customerUid,
        double latitude, double longitude, string projectUid = null, IHeaderDictionary customHeaders = null);

  }
}
