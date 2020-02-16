using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Services.Interfaces
{
  public interface IProjectMonitoringSiteProvider
  {
    IEnumerable<ProjectSiteMonitoringAccess.ProjectRouteSummary> ListRoutes(long customerId, long planId);
  }
}
