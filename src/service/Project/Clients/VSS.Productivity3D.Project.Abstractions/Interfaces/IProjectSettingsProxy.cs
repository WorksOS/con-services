using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IProjectSettingsProxy : ICacheProxy
  {
    Task<CompactionProjectSettingsColors> GetProjectSettingsColors(string projectUid, string userId, IHeaderDictionary customHeaders,
      IServiceExceptionHandler serviceExceptionHandler);

    Task<CompactionProjectSettings> GetProjectSettingsTargets(string projectUid, string userId, IHeaderDictionary customHeaders,
      IServiceExceptionHandler serviceExceptionHandler);
  }
}
