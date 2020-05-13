using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV2ProxyCompaction : IProductivity3dV2Proxy
  {
    Task<Stream> GetLineworkFromAlignment(Guid projectUid, Guid alignmentUid, IHeaderDictionary customHeaders);

    Task<ProjectStatisticsResult> GetProjectStatistics(Guid projectUid, IHeaderDictionary customHeaders = null);

    Task<BaseMasterDataResult> ValidateProjectSettings(Guid projectUid, string projectSettings,
      ProjectSettingsType settingsType, IHeaderDictionary customHeaders = null);

    Task<BaseMasterDataResult> ValidateProjectSettings(ProjectSettingsRequest request,
      IHeaderDictionary customHeaders = null);
  }
}
