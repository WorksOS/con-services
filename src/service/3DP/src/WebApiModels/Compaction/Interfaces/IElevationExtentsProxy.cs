using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Interfaces
{
  public interface IElevationExtentsProxy
  {
    Task<ElevationStatisticsResult> GetElevationRange(long projectId, Guid projectUid, FilterResult filter, CompactionProjectSettings projectSettings, IDictionary<string, string> customHeaders);
  }
}
