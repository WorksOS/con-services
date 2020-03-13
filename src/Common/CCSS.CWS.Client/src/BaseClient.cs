using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  public abstract class BaseClient
  {
    public const string CWS_URL_KEY = "CWS_URL";

    private readonly string baseUrl;

    protected readonly IConfigurationStore Configuration;
    protected readonly ILogger Log;
    protected readonly IWebRequest webClient;

    protected BaseClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient)
    {
      Configuration = configuration;
      webClient = gracefulClient;
      Log = logger.CreateLogger(GetType().Name);

      baseUrl = configuration.GetValueString(CWS_URL_KEY);
      if (string.IsNullOrEmpty(baseUrl))
      {
        throw new ArgumentException($"Missing environment variable {CWS_URL_KEY}");
      }
    }

    protected Task<TRes> GetData<TRes>(string route,
      IDictionary<string, string> parameters = null,
      IDictionary<string, string> customHeaders = null) where TRes: class
    {
      var url = ConvertToUrl(route, parameters);
      Log.LogDebug($"GetData: {url}");
      // TODO attempt to catch an error from here
      return webClient.ExecuteRequest<TRes>(url, null, customHeaders, HttpMethod.Get);
    }

    protected async Task<TRes> PostData<TReq, TRes>(string route,
      TReq request,
      IDictionary<string, string> parameters = null,
      IDictionary<string, string> customHeaders = null) where TReq : class where TRes : class, new()
    {
      var payload = JsonConvert.SerializeObject(request);

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        var url = ConvertToUrl(route, parameters);
        Log.LogDebug($"PostData: {url}");
        // TODO attempt to catch an error from here
        // Need to await here, or else stream is closed
        return await webClient.ExecuteRequest<TRes>(url, ms, customHeaders, HttpMethod.Post);
      }
    }

    protected Task DeleteData(string route, IDictionary<string, string> parameters = null,
      IDictionary<string, string> customHeaders = null)
    {
      var url = ConvertToUrl(route, parameters);
      Log.LogDebug($"DeleteData: {url}");
      // TODO attempt to catch an error from here
      return webClient.ExecuteRequest(url, null, customHeaders, HttpMethod.Delete);
    }

    protected async Task<TRes> UpdateData<TReq, TRes>(string route,
      TReq request,
      IDictionary<string, string> parameters = null,
      IDictionary<string, string> customHeaders = null) where TReq : class where TRes : class, new()
    {
      var payload = JsonConvert.SerializeObject(request);

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        var url = ConvertToUrl(route, parameters);
        Log.LogDebug($"UpdateData: {url}");
        // TODO attempt to catch an error from here
        // Need to await here, or else stream is closed
        return await webClient.ExecuteRequest<TRes>(url, ms, customHeaders, HttpMethod.Put);
      }
    }

    protected Task CallEndpoint<TReq>(string route, TReq request, HttpMethod method, IDictionary<string, string> parameters = null,
      IDictionary<string, string> customHeaders = null) where TReq : class
    {
      var url = ConvertToUrl(route, parameters);
      Log.LogDebug($"GetMethod: {url}");

      var payload = JsonConvert.SerializeObject(request);
      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        return webClient.ExecuteRequest(url, ms, customHeaders, HttpMethod.Get);
      }
    }

    private string ConvertToUrl(string route, IDictionary<string, string> parameters = null)
    {
      if (parameters == null || parameters.Count == 0)
      {
        return $"{baseUrl}{route}";
      }

      var p = string.Join("&",
        parameters.Select(kvp => $"{HttpUtility.UrlEncode((string) kvp.Key)}={HttpUtility.UrlEncode((string) kvp.Value)}"));

      return $"{baseUrl}{route}?{p}";
    }
  }
}
