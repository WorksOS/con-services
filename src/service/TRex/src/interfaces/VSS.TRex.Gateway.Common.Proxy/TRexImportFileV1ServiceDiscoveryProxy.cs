using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
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
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Proxy
{
  /// <summary>
  /// Proxy for TRex import files services.
  /// </summary>
  public class TRexImportFileV1ServiceDiscoveryProxy : BaseTRexServiceDiscoveryProxy, ITRexImportFileProxy
  {
    // TRex has 2 endpoints, 1 for immutable and other for mutable access
    private const string TREX_IMPORTFILE_WRITE_API_URL_KEY = "TREX_IMPORTFILE_WRITE_API_URL";
    private const string TREX_IMPORTFILE_READ_API_URL_KEY = "TREX_IMPORTFILE_READ_API_URL";

    public TRexImportFileV1ServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Filter;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "TREX_IMPORTFILE_CACHE_LIFE";

    public override GatewayType Gateway { get => Gateway ; set => Gateway = GatewayType.None; }


    /// <summary>
    /// Notifies TRex that a design file has been added to a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> AddFile(DesignRequest designRequest,
      IDictionary<string, string> customHeaders = null)
    {
      Gateway = GatewayType.Mutable;
      return await SendImportFileRequest(designRequest, customHeaders, HttpMethod.Post);
    }

    /// <summary>
    /// Notifies TRex that a design file has been updated a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> UpdateFile(DesignRequest designRequest,
      IDictionary<string, string> customHeaders = null)
    {
      Gateway = GatewayType.Mutable;
      return await SendImportFileRequest(designRequest, customHeaders, HttpMethod.Put);
    }

    /// <summary>
    /// Notifies Trex that a design file has been deleted from a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> DeleteFile(DesignRequest designRequest,
      IDictionary<string, string> customHeaders = null)
    {
      Gateway = GatewayType.Mutable;
      return await SendImportFileRequest(designRequest, customHeaders, HttpMethod.Delete);
    }

    public async Task<DesignListResult> GetDesignsOfTypeForProject(Guid projectUid, ImportedFileType? importedFileType,
        IDictionary<string, string> customHeaders = null)
    {
      Gateway = GatewayType.None;
      var queryParams = new Dictionary<string, string>();
      queryParams.Add("projectUid", projectUid.ToString());
      queryParams.Add("importedFileType", importedFileType.ToString());
      //  // return await SendImportFileRequest(TREX_IMPORTFILE_READ_API_URL_KEY, string.Empty, customHeaders, "/get", HttpMethod.Get, queryParams);
      return await GetMasterDataItemServiceDiscovery<DesignListResult>
        ($"/design/get", projectUid.ToString(), null, customHeaders, queryParams);
    }

    private async Task<ContractExecutionResult> SendImportFileRequest(DesignRequest designRequest,
      IDictionary<string, string> customHeaders, HttpMethod method)
    {
      var jsonData = JsonConvert.SerializeObject(designRequest);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        // Need to await this, as we need the stream (if we return the task, the stream is disposed)
        return await MasterDataItemServiceDiscoveryNoCache("design", customHeaders, method, payload: payload);
      }
    }
  }
}
