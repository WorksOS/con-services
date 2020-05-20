using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV1Proxy
  {
    Task<T> ExecuteGenericV1Request<T>(string route, object payload, IHeaderDictionary customHeaders = null)
      where T : class, IMasterDataModel;

    Task<T> ExecuteGenericV1Request<T>(string route, IList<KeyValuePair<string, string>> queryParameters, IHeaderDictionary customHeaders = null)
      where T : class, IMasterDataModel;
  }
}
