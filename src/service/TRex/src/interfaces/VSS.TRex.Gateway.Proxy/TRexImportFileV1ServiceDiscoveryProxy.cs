using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Mutable.Gateway.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Mutable.Gateway.Proxy
{
  /// <summary>
  /// Proxy for TRex import files services.
  /// </summary>
  public class TRexImportFileV1ServiceDiscoveryProxy : BaseServiceDiscoveryProxy, ITRexImportFileProxy
  {
    // TRex has 2 endpoints, 1 for immutable and other for mutable access
    private const string TREX_IMPORTFILE_READ_API_URL_KEY = "TREX_IMPORTFILE_READ_API_URL";
    // todoJeannie TREX_IMPORTFILE_READ_API_URL only from a TRex unit test
    public TRexImportFileV1ServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Filter;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "TREX_IMPORTFILE_CACHE_LIFE"; // todoJeannie caching in trex gateways? read and write are to 2 different endpoints

    /// <summary>
    /// Notifies TRex that a design file has been added to a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> AddFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(AddFile)}: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_WRITE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, HttpMethod.Post);
    }

    /// <summary>
    /// Notifies TRex that a design file has been updated a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> UpdateFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      // TREX_IMPORTFILE_WRITE_API_URL_KEY
      log.LogDebug($"{nameof(UpdateFile)}: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_WRITE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, HttpMethod.Put);
    }

    /// <summary>
    /// Notifies Trex that a design file has been deleted from a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> DeleteFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(DeleteFile)}: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_WRITE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, HttpMethod.Delete);
    }

    public async Task<DesignListResult> GetDesignsOfTypeForProject(Guid projectUid, ImportedFileType? importedFileType, IDictionary<string, string> customHeaders = null)
    {
      var queryParams = new Dictionary<string, string>();
      queryParams.Add("projectUid", projectUid.ToString());
      queryParams.Add("importedFileType", importedFileType.ToString());
      // return await SendImportFileRequest(TREX_IMPORTFILE_READ_API_URL_KEY, string.Empty, customHeaders, "/get", HttpMethod.Get, queryParams);
      return await GetMasterDataItemServiceDiscovery<DesignListResult>
        ($"/designs/get", projectUid.ToString(), null, customHeaders);

    }

    private async Task<ContractExecutionResult> SendImportFileRequest(string urlKey, string payload, IDictionary<string, string> customHeaders, HttpMethod method)
    {
      var response = await SendRequest<ContractExecutionResult>(urlKey, payload, customHeaders, null, method, string.Empty);
      log.LogDebug($"{nameof(SendImportFileRequest)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

    private async Task<DesignListResult> SendImportFileRequest(string urlKey, string payload, IDictionary<string, string> customHeaders, string route, HttpMethod method, IDictionary<string, string> queryParameters)
    {
      var response = await SendRequest<DesignListResult>(urlKey, payload, customHeaders, route, method, queryParameters);
      log.LogDebug($"{nameof(SendImportFileRequest)} Reader: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }
  }
}
