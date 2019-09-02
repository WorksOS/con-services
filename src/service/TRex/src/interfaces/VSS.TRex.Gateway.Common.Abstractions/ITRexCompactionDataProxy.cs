using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.TRex.Gateway.Common.Abstractions
{
  /// <summary>
  /// Proxy interface to access the TRex Gateway WebAPIs.
  /// </summary>
  public interface ITRexCompactionDataProxy
  {
    /// <summary>
    /// Sends a request to get/save data from/to the TRex immutable/mutable database.
    /// </summary>
    Task<TResponse> SendDataPostRequest<TResponse, TRequest>(TRequest dataRequest, string route,
      IDictionary<string, string> customHeaders = null, bool mutableGateway = false) 
      where TResponse : ContractExecutionResult;

    /// <summary>
    /// Sends a request to get data as a stream from the TRex immutable database.
    /// </summary>
    Task<Stream> SendDataPostRequestWithStreamResponse<TRequest>(TRequest dataRequest, string route,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to delete data to the TRex immutable/mutable database.
    /// </summary>
    Task<TResponse> SendDataDeleteRequest<TResponse, TRequest>(TRequest dataRequest, string route, 
      IDictionary<string, string> customHeaders = null, bool mutableGateway = false)
      where TResponse : ContractExecutionResult;

    /// <summary>
    /// Sends a request to get site model data from the TRex immutable database.
    /// </summary>
    Task<TResponse> SendDataGetRequest<TResponse>(string siteModelId, string route,
      IDictionary<string, string> customHeaders = null, IDictionary<string, string> queryParameters = null) where TResponse : class, IMasterDataModel;
  }
}
