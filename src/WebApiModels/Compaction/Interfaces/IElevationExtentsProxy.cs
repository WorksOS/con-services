using System;
using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApiModels.Compaction.Interfaces
{
  public interface IElevationExtentsProxy
  {
    ElevationStatisticsResult GetElevationRange(long projectId, Guid projectUid, FilterResult filter, CompactionProjectSettings projectSettings, IDictionary<string, string> customHeaders);
  }
}
