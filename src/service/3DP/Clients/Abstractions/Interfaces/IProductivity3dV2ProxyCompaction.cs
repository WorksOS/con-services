using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV2ProxyCompaction : IProductivity3dV2Proxy
  {
    Task<Stream> GetLineworkFromAlignment(string projectUid, string alignmentUid,
      IDictionary<string, string> customHeaders);

    Task<ProjectStatisticsResult> GetProjectStatistics(Guid projectUid,
      IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> ValidateProjectSettings(Guid projectUid, string projectSettings,
      ProjectSettingsType settingsType, IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> ValidateProjectSettings(ProjectSettingsRequest request,
      IDictionary<string, string> customHeaders = null);
  }
}
