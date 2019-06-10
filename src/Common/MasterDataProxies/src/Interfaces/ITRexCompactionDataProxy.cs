using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Proxies.Interfaces
{
  /// <summary>
  /// Proxy interface to access the TRex Gateway WebAPIs.
  /// </summary>
  public interface ITRexCompactionDataProxy
  {
    /// <summary>
    /// Sends a request to get/save data from/to the TRex immutable/mutable database.
    /// </summary>
    /// <param name="dataRequest"></param>
    /// <param name="route"></param>
    /// <param name="customHeaders"></param>
    /// <param name="mutableGateway"></param>
    /// <returns></returns>
    Task<TResponse> SendDataPostRequest<TResponse, TRequest>(TRequest dataRequest, string route,
      IDictionary<string, string> customHeaders = null, bool mutableGateway = false) where TResponse : ContractExecutionResult;

    /// <summary>
    /// Sends a request to get data as a stream from the TRex immutable database.
    /// </summary>
    /// <param name="dataRequest"></param>
    /// <param name="route"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<Stream> SendDataPostRequestWithStreamResponse<TRequest>(TRequest dataRequest, string route,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get site model data from the TRex immutable database.
    /// </summary>
    /// <param name="siteModelId"></param>
    /// <param name="route"></param>
    /// <param name="customHeaders"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    Task<TResponse> SendDataGetRequest<TResponse>(string siteModelId, string route,
      IDictionary<string, string> customHeaders = null, string queryParameters = null);
  }
}
