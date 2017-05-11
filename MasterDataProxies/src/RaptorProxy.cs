using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Interfaces;
using MasterDataProxies.Models;
using MasterDataProxies.Interfaces;
using MasterDataProxies.ResultHandling;

namespace VSS.Raptor.Service.Common.Proxies
{
  /// <summary>
  /// Proxy to validate and post a CoordinateSystem with Raptor.
  /// </summary>
  public class RaptorProxy : BaseProxy, IRaptorProxy
  {
    public RaptorProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// Validates the CoordinateSystem for the project.
    /// </summary>
    /// <param name="coordinateSystemFileContent">The content of the CS file.</param>
    /// <param name="coordinateSystemFileName">The filename.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<CoordinateSystemSettings> CoordinateSystemValidate(byte[] coordinateSystemFileContent, string coordinateSystemFileName, IDictionary<string, string> customHeaders = null)
    {
      var urlKey = "COORDSYSVALIDATE_API_URL";
      string url = configurationStore.GetValueString(urlKey);
      log.LogDebug($"RaptorProxy.CoordinateSystemValidate: urlKey: {urlKey}  url: {url} customHeaders {customHeaders}");

      log.LogDebug($"RaptorProxy.CoordinateSystemValidate: coordinateSystemFileContent: {coordinateSystemFileContent} coordinateSystemFileName: {coordinateSystemFileName}");
      var payLoadToSend = CoordinateSystemFileValidationRequest.CreateCoordinateSystemFileValidationRequest(coordinateSystemFileContent, coordinateSystemFileName );
      log.LogDebug("RaptorProxy.CoordinateSystemValidate: payLoadToSend: {0}", payLoadToSend == null ? null : JsonConvert.SerializeObject(payLoadToSend));

      CoordinateSystemSettings response = await SendRequest<CoordinateSystemSettings>(urlKey, JsonConvert.SerializeObject(payLoadToSend), customHeaders);
      log.LogDebug("RaptorProxy.CoordinateSystemValidate: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      return response;
    }

    /// <summary>
    /// Validates and posts to Raptor, the CoordinateSystem for the project.
    /// </summary>
    /// <param name="legacyProjectId">The legacy ProjectId.</param>
    /// <param name="coordinateSystemFileContent">The content of the CS file.</param>
    /// <param name="coordinateSystemFileName">The filename.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<CoordinateSystemSettings> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent, string coordinateSystemFileName, IDictionary<string, string> customHeaders = null)
    {
      var urlKey = "COORDSYSPOST_API_URL";
      string url = configurationStore.GetValueString(urlKey);
      log.LogDebug($"RaptorProxy.CoordinateSystemPost: urlKey: {urlKey}  url: {url} customHeaders {customHeaders}");

      log.LogDebug($"RaptorProxy.CoordinateSystemPost: coordinateSystemFileContent: {coordinateSystemFileContent} coordinateSystemFileName: {coordinateSystemFileName}");
      var payLoadToSend = CoordinateSystemFile.CreateCoordinateSystemFile(legacyProjectId, coordinateSystemFileContent, coordinateSystemFileName);
      log.LogDebug("RaptorProxy.CoordinateSystemPost: payLoadToSend: {0}", payLoadToSend == null ? null : JsonConvert.SerializeObject(payLoadToSend));

      CoordinateSystemSettings response = await SendRequest<CoordinateSystemSettings>(urlKey, JsonConvert.SerializeObject(payLoadToSend), customHeaders);
      log.LogDebug("RaptorProxy.CoordinateSystemPost: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    /// <summary>
    /// Notifies Raptor that a file has been added to a project
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>
    /// <returns></returns>
    public async Task<ContractExecutionResult> AddFile(long? projectId, Guid? projectUid, string fileDescriptor, IDictionary<string, string> customHeaders = null)
    {
      var urlKey = "RAPTORADDFILE_API_URL";
      string url = configurationStore.GetValueString(urlKey);
      log.LogDebug($"RaptorProxy.AddFile: urlKey: {urlKey}  url: {url} customHeaders {customHeaders}");

      log.LogDebug($"RaptorProxy.AddFile: projectId: {projectId} projectUid: {projectUid} fileDescriptor {fileDescriptor}");
      var queryParams = $"?projectId={projectId}&projectUid={projectUid}&fileDescriptor={fileDescriptor}";
      log.LogDebug($"RaptorProxy.AddFile: queryParams: {JsonConvert.SerializeObject(queryParams)}");

      ContractExecutionResult response = await GetItem<ContractExecutionResult>(urlKey, customHeaders, queryParams);
      var message = string.Format("RaptorProxy.AddFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      log.LogDebug(message);

      return response;
    }

    /// <summary>
    /// Notifies Raptor that a file has been deleted from a project
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>
    /// <returns></returns>
    public async Task<ContractExecutionResult> DeleteFile(long? projectId, Guid? projectUid, string fileDescriptor, IDictionary<string, string> customHeaders = null)
    {
      var urlKey = "RAPTORDELETEFILE_API_URL";
      string url = configurationStore.GetValueString(urlKey);
      log.LogDebug($"RaptorProxy.DeleteFile: urlKey: {urlKey}  url: {url} customHeaders {customHeaders}");

      log.LogDebug($"RaptorProxy.DeleteFile: projectId: {projectId} projectUid: {projectUid} fileDescriptor {fileDescriptor}");
      var queryParams = $"?projectId={projectId}&projectUid={projectUid}&fileDescriptor={fileDescriptor}";
      log.LogDebug($"RaptorProxy.DeleteFile: queryParams: {JsonConvert.SerializeObject(queryParams)}");

      ContractExecutionResult response = await GetItem<ContractExecutionResult>(urlKey, customHeaders, queryParams);
      var message = string.Format("RaptorProxy.DeleteFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      log.LogDebug(message);

      return response;
    }
  }
}