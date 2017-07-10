
using System.Collections.Generic;
using System.Threading.Tasks;
using MasterDataModels.Models;

namespace MasterDataProxies.Interfaces
{
  public interface IPreferenceProxy
  {
    Task<UserPreferenceData> GetUserPreferences(IDictionary<string, string> customHeaders = null);
  }
}
