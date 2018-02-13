using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IProjectSettingsProxy : ICacheProxy
  {
    Task<JObject> GetProjectSettings(string projectUid, string userId, IDictionary<string, string> customHeaders);
  }
}
