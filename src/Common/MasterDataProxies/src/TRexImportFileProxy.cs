﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    private const string TREX_IMPORTFILE_API_URL_KEY = "TREX_IMPORTFILE_API_URL";
    public TRexImportFileProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    /// <summary>
    /// Notifies Trex that a design file has been added to a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> AddFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexImportFileProxy.AddFile: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, "POST");
    }

    /// <summary>
    /// Notifies Trex that a design file has been updated a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> UpdateFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexImportFileProxy.UpdateFile: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, "PUT");
    }

    /// <summary>
    /// Notifies Trex that a design file has been deleted from a project
    /// </summary>   
    /// <returns></returns>
    public async Task<ContractExecutionResult> DeleteFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexImportFileProxy.DeleteFile: designRequest: {JsonConvert.SerializeObject(designRequest)}");
      return await SendImportFileRequest(TREX_IMPORTFILE_API_URL_KEY, JsonConvert.SerializeObject(designRequest), customHeaders, "DELETE");
    }

    public async Task<DesignListResult> GetDesignsForProject(Guid projectUid, IDictionary<string, string> customHeaders = null)
    {
      var queryParams = new Dictionary<string, string>();
      queryParams.Add("projectUid", projectUid.ToString());
      return await SendImportFileRequest(TREX_IMPORTFILE_API_URL_KEY, string.Empty, customHeaders, "GET", queryParams);
    }
    public async Task<DesignListResult> GetDesignsOfTypeForProject(Guid projectUid, ImportedFileType importedFileType, IDictionary<string, string> customHeaders = null)
    {
      var queryParams = new Dictionary<string, string>();
      queryParams.Add("projectUid", projectUid.ToString());
      queryParams.Add("importedFileType", importedFileType.ToString());
      return await SendImportFileRequest(TREX_IMPORTFILE_API_URL_KEY, string.Empty, customHeaders, "GET", queryParams);
    }
    private async Task<ContractExecutionResult> SendImportFileRequest(string urlKey, string payload, IDictionary<string, string> customHeaders, string method)
    {
      var response = await SendRequest<ContractExecutionResult>(urlKey, payload, customHeaders, null, method, String.Empty);
      log.LogDebug("TRexImportFileProxy.SendImportFileRequest: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    private async Task<DesignListResult> SendImportFileRequest(string urlKey, string payload, IDictionary<string, string> customHeaders, string method, IDictionary<string, string> queryParameters)
    {
      var response = await SendRequest<DesignListResult>(urlKey, payload, customHeaders, null, method, queryParameters);
      log.LogDebug("TRexImportFileProxy.SendImportFileRequest: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }
  }
}