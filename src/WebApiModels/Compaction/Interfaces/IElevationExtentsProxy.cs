using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Interfaces
{
  public interface IElevationExtentsProxy
  {
    ElevationStatisticsResult GetElevationRange(long projectId, Filter filter);
  }
}
