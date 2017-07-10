using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.MasterDataProxies.Models;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface IPreferenceProxy
  {
    Task<UserPreferenceData> GetUserPreferences(IDictionary<string, string> customHeaders = null);
  }
}
