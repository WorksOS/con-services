﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IProjectProxy : ICacheProxy
  {

    Task<ProjectData> GetProject(long shortRaptorProjectId, IDictionary<string, string> customHeaders = null);

    //Task<List<ProjectData>> GetProjects(string customerUid, IDictionary<string, string> customHeaders = null);

    //Task<ProjectData> GetProjectForCustomer(string customerUid, string projectUid,
    //  IDictionary<string, string> customHeaders = null);

    ////To support 3dpm v1 end points which use legacy project id
    //Task<ProjectData> GetProjectForCustomer(string customerUid, long projectId,
    //  IDictionary<string, string> customHeaders = null);
   
  }
}
