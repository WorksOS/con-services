using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy for Trex import files services.
  /// </summary>
  public class TRexImportFileProxy : BaseProxy, ITRexImportFileProxy
  {
    // TRex has 2 endpoints, 1 for immutable and other for mutable access
    private const string TREX_IMPORTFILE_WRITE_API_URL_KEY = "TREX_IMPORTFILE_WRITE_API_URL";
    private const string TREX_IMPORTFILE_READ_API_URL_KEY = "TREX_IMPORTFILE_READ_API_URL";
    public TRexImportFileProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    /// <summary>
    /// Notifies Trex that a design file has been added to a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> AddFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexImportFileProxy.{nameof(AddFile)}: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_WRITE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, HttpMethod.Post);
    }

    /// <summary>
    /// Notifies Trex that a design file has been updated a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> UpdateFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexImportFileProxy.{nameof(UpdateFile)}: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_WRITE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, HttpMethod.Put);
    }

    /// <summary>
    /// Notifies Trex that a design file has been deleted from a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> DeleteFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexImportFileProxy.{nameof(DeleteFile)}: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_WRITE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, HttpMethod.Delete);
    }

    public async Task<DesignListResult> GetDesignsOfTypeForProject(Guid projectUid, ImportedFileType? importedFileType, IDictionary<string, string> customHeaders = null)
    {
      var queryParams = new Dictionary<string, string>();
      queryParams.Add("projectUid", projectUid.ToString());
      queryParams.Add("importedFileType", importedFileType.ToString());
      return await SendImportFileRequest(TREX_IMPORTFILE_READ_API_URL_KEY, string.Empty, customHeaders, "/get", HttpMethod.Get, queryParams);
    }

    private async Task<ContractExecutionResult> SendImportFileRequest(string urlKey, string payload, IDictionary<string, string> customHeaders, HttpMethod method)
    {
      var response = await SendRequest<ContractExecutionResult>(urlKey, payload, customHeaders, null, method, string.Empty);
      log.LogDebug($"TRexImportFileProxy.{nameof(SendImportFileRequest)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

    private async Task<DesignListResult> SendImportFileRequest(string urlKey, string payload, IDictionary<string, string> customHeaders, string route, HttpMethod method, IDictionary<string, string> queryParameters)
    {
      var response = await SendRequest<DesignListResult>(urlKey, payload, customHeaders, route, method, queryParameters);
      log.LogDebug($"TRexImportFileProxy.{nameof(SendImportFileRequest)} Reader: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }
  }
}