using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Interfaces
{
  public interface IElevationExtentsProxy
  {
    ElevationStatisticsResult GetElevationRange(long projectId, Filter filter,
      CompactionProjectSettings projectSettings);
  }
}