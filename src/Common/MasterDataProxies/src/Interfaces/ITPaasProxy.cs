using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITPaasProxy
  {
    Task<TPaasOauthResult> GetApplicationBearerToken(string grantType, Dictionary<string, string> customHeaders);
    Task<TPaasOauthResult> GetUserBearerToken(string grantType, Dictionary<string, string> customHeaders);
    Task<BaseDataResult> RevokeApplicationBearerToken(string token, Dictionary<string, string> customHeaders);
  }
}
