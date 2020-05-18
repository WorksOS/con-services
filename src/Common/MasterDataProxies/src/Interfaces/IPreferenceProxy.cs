using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IPreferenceProxy 
  {
    Task<UserPreferenceData> GetUserPreferences(IHeaderDictionary customHeaders = null);

    Task<UserPreferenceData> GetShortCachedUserPreferences(string userId, TimeSpan invalidation,
      IHeaderDictionary customHeaders = null);
  }
}
