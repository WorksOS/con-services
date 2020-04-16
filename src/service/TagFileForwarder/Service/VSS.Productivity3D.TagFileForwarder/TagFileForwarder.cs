using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.TagFileForwarder
{
  public class TagFileForwarder : BaseProxy, ITagFileForwarder
  {
    private const int MAX_RETRIES = 3;
    private const string URL_KEY = "TAG_FILE_FORWARD_URL";
    private readonly string _url; 
    public TagFileForwarder(IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache) : base(configurationStore, logger, dataCache)
    {
      _url = configurationStore.GetValueString(URL_KEY, null);
      IsEnabled = !string.IsNullOrEmpty(_url);
      if(IsEnabled)
        log.LogInformation($"Tag File Forwarding forwarding to {_url}");
    }

    public TagFileForwarder(IConfigurationStore configurationStore, ILoggerFactory logger) : this(configurationStore, logger, null)
    {
    }

    public bool IsEnabled { get; private set; }

    public Task<ContractExecutionResult> SendTagFileDirect(CompactionTagFileRequest compactionTagFileRequest, IDictionary<string, string> customHeaders = null)
    {
      return SendTagFileWithRetry(compactionTagFileRequest, "/tagfiles/direct", customHeaders);
    }

    public Task<ContractExecutionResult> SendTagFileNonDirect(CompactionTagFileRequest compactionTagFileRequest, IDictionary<string, string> customHeaders = null)
    {
      return SendTagFileWithRetry(compactionTagFileRequest, "/tagfiles", customHeaders);
    }

    /// <summary>
    /// This method is created so we can test the retry logic, but we can't mock the base proxy class currently
    /// </summary>
    public virtual Task<ContractExecutionResult> SendSingleTagFile(CompactionTagFileRequest request, string route, IDictionary<string, string> customHeaders)
    {
      // Only use 1 retry here, we will handle retries ourselves
      return SendRequest<ContractExecutionResult>(URL_KEY, JsonConvert.SerializeObject(request), customHeaders, route, HttpMethod.Post, null, null, 1);
    }

    private async Task<ContractExecutionResult> SendTagFileWithRetry(CompactionTagFileRequest request, string route, IDictionary<string, string> customHeaders, int maxRetries = MAX_RETRIES)
    {
      var retry = 1;
      var result = new ContractExecutionResult();

      // Attempt to call the remote server 3 times, if it returns a 0 (Ok) code, then pass that result on
      // If an error occurs, either exception or non-zero code, retry up to 3 times
      // If an error still occurs, return the last error to the caller
      while (retry <= MAX_RETRIES)
      {
        try
        {
          result = await SendSingleTagFile(request, route, customHeaders);
          if(result == null) 
          {
            log.LogWarning($"Empty result returned. Retry count {retry} of {MAX_RETRIES}");
          }
          else if (result.Code != 0)
          {
            log.LogWarning($"Non Zero result returned, Error Code: {result.Code}, Message: {result.Message}. Retry count {retry} of {MAX_RETRIES}");
          }
          else
          {
            // Good result
            log.LogInformation($"Tag File {request.FileName} uploaded. Retry count {retry} of {MAX_RETRIES}");
            return result;
          }
        }
        catch (Exception e)
        {
          log.LogWarning($"Failed to upload Tag File, retry count {retry} of {MAX_RETRIES}. Exception ({e.GetType().Name}): {e.Message}");
          result = new ContractExecutionResult(1, e.Message);
        }

        retry++;
      }

      return result ?? (new ContractExecutionResult(1, $"No response from Server {_url}"));
    }

  }
}
