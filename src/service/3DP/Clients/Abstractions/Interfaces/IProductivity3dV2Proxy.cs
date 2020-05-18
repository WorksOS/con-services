using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV2Proxy
  {
    Task<T> ExecuteGenericV2Request<T>(string route, HttpMethod method, Stream body = null, IHeaderDictionary customHeaders = null)
      where T : class, IMasterDataModel;
  }
}
