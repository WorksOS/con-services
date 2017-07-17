using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterDataProxies.Interfaces
{
  public interface IPreferenceProxy
  {
    Task<UserPreferenceData> GetUserPreferences(IDictionary<string, string> customHeaders = null);
  }
}
