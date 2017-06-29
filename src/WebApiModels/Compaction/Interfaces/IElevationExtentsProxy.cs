using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Interfaces
{
  public interface IElevationExtentsProxy
  {
    ElevationStatisticsResult GetElevationRange(IASNodeClient raptorClient, long projectId, Filter filter);
  }
}
