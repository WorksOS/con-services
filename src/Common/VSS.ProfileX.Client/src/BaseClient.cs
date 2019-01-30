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
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.ProfileX.Client
{
  public abstract class BaseClient
  {
    private const string PROFILE_X_URL_KEY = "PROFILE_X_URL";

    private readonly string baseUrl;

    protected readonly IConfigurationStore Configuration;
    protected readonly ILogger Log;
    protected readonly IWebRequest webClient;

    protected BaseClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient)
    {
      Configuration = configuration;
      webClient = gracefulClient;
      Log = logger.CreateLogger(GetType().Name);

      baseUrl = configuration.GetValueString(PROFILE_X_URL_KEY);
      if (string.IsNullOrEmpty(baseUrl))
      {
        throw new ArgumentException($"Missing environment variable {PROFILE_X_URL_KEY}");
      }
    }

    protected Task<TRes> GetData<TRes>(string route,
      IDictionary<string, string> parameters = null,
      IDictionary<string, string> customHeaders = null) where TRes: class, new()
    {
      var url = ConvertToUrl(route, parameters);
      Log.LogDebug($"GetData: {url}");
      // TODO attempt to catch an error from here
      return webClient.ExecuteRequest<TRes>(url, null, customHeaders, HttpMethod.Get);
    }

    protected Task<TRes> PostData<TReq, TRes>(string route,
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
        return webClient.ExecuteRequest<TRes>(url, ms, customHeaders, HttpMethod.Post);
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

    protected Task<TRes> UpdateData<TReq, TRes>(string route,
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
        return webClient.ExecuteRequest<TRes>(url, ms, customHeaders, HttpMethod.Put);
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