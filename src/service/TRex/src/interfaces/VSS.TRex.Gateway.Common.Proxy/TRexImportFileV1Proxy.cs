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
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Proxy
{
  /// <summary>
  /// Proxy for TRex import files services.
  /// </summary>
  public class TRexImportFileV1Proxy : BaseTRexServiceDiscoveryProxy, ITRexImportFileProxy
  {
    public TRexImportFileV1Proxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.None;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "TREX_IMPORTFILE_CACHE_LIFE";

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
      Gateway = GatewayType.Immutable;
      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("projectUid", projectUid.ToString()),
        new KeyValuePair<string, string>("importedFileType", importedFileType.ToString())
      };
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
        return await MasterDataItemServiceDiscoveryNoCache<ContractExecutionResult>("design", customHeaders, method, payload: payload);
      }
    }
  }
}
