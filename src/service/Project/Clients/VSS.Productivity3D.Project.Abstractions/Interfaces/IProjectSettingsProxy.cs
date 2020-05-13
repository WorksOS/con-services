using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IProjectSettingsProxy : ICacheProxy
  {
    Task<JObject> GetProjectSettings(string projectUid, string userId, IHeaderDictionary customHeaders);
    Task<JObject> GetProjectSettings(string projectUid, string userId, IHeaderDictionary customHeaders, ProjectSettingsType settingsType);
  }
}
