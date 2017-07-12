using MasterDataModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface IPreferenceProxy
  {
    Task<UserPreferenceData> GetUserPreferences(IDictionary<string, string> customHeaders = null);
  }
}
