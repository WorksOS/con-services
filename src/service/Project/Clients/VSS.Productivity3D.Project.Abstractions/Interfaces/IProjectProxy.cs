using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IProjectProxy : ICacheProxy
  {
    Task<List<ProjectData>> GetProjects(string customerUid, IDictionary<string, string> customHeaders = null);

    Task<ProjectData> GetProjectForCustomer(string customerUid, string projectUid,
      IDictionary<string, string> customHeaders = null);

    //To support 3dpm v1 end points which use legacy project id
    Task<ProjectData> GetProjectForCustomer(string customerUid, long shortRaptorProjectId,
        IDictionary<string, string> customHeaders = null);

    #region applicationContext // from TFA etal

    Task<ProjectData> GetProjectApplicationContext(string projectUid, IDictionary<string, string> customHeaders = null);
    
    Task<ProjectData> GetProjectApplicationContext(long shortRaptorProjectId, IDictionary<string, string> customHeaders = null);
    
    Task<List<ProjectData>> GetIntersectingProjectsApplicationContext(string customerUid,
        double latitude, double longitude, string projectUid = null, DateTime? timeOfPosition = null, IDictionary<string, string> customHeaders = null);

    // todoMaverick not needed anymore?
    //Task<List<ProjectData>> GetIntersectingProjectsForDeviceApplicationContext(string deviceCustomerUid, string deviceUid,
    //   double latitude, double longitude, DateTime? timeOfPosition = null, IDictionary<string, string> customHeaders = null);
    
    #endregion applicationContext 

  }
}
