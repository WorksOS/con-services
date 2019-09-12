using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV1Proxy
  {
    Task<T> ExecuteGenericV1Request<T>(string route, object payload, IDictionary<string, string> customHeaders = null)
      where T : class, IMasterDataModel;

    Task<T> ExecuteGenericV1Request<T>(string route, IList<KeyValuePair<string, string>> queryParameters, IDictionary<string, string> customHeaders = null)
      where T : class, IMasterDataModel;
  }
}
