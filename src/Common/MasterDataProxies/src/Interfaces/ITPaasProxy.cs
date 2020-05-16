using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITPaasProxy
  {
    Task<TPaasOauthResult> GetApplicationBearerToken(string grantType, IHeaderDictionary customHeaders);
    Task<BaseDataResult> RevokeApplicationBearerToken(string token, IHeaderDictionary customHeaders);
  }
}
