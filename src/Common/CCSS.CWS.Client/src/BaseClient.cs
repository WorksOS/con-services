using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  public abstract class BaseClient : BaseServiceDiscoveryProxy
  {
    public override bool IsInsideAuthBoundary => false;
    public override ApiService InternalServiceType => ApiService.None;
    public override ApiType Type => ApiType.Public;
    public override string CacheLifeKey => "CWS_CACHE_LIFE";

    protected const int DefaultPageSize = 50;

    protected int FromRow = 0;
    protected int RowCount = 200;

    protected BaseClient(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
      dataCache, serviceResolution)
    { }

    // NOTE: must have a uid or userId for cache key
    protected async Task<TRes> GetData<TRes>(string route, Guid? uid, Guid? userId,
      IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null,
      string uniqueIdCacheKey = null) where TRes : class, IMasterDataModel
    {
      try
      {
        var result = await GetMasterDataItemServiceDiscovery<TRes>(route, uid?.ToString(), userId?.ToString(), customHeaders, parameters, uniqueIdCacheKey);
        return result;
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return null;
        }

        throw;
      }
    }

    protected async Task<TRes> GetDataNoCache<TRes>(string route,
      IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null) where TRes : class
    {
      try
      {
        var result = await SendMasterDataItemServiceDiscoveryNoCache<TRes>(route, customHeaders, HttpMethod.Get, parameters);
        return result;
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return null;
        }

        throw;
      }
    }

    /// <summary>
    /// Gets data from CWS that supports paging via the HasMore property
    /// This method gets ALL data in one go.
    /// </summary>
    /// <typeparam name="TListModel">The List Result Model representing the API Result (including the HasMore Property, and List of Models)</typeparam>
    /// <typeparam name="TModel">The Actual model the list represents</typeparam>
    protected async Task<TListModel> GetAllPagedData<TListModel, TModel>(string route, Guid? uid, Guid? userId,
      IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null)
      where TModel : IMasterDataModel
      where TListModel : class, IMasterDataModel, ISupportsPaging<TModel>, new()
    {
      var results = new List<TModel>();
      TListModel apiResult;
      var currentPage = 0;
      do
      {
        var cacheKeyPaging = $"{currentPage}-{DefaultPageSize}";
        var queryParameters = WithLimits(currentPage * DefaultPageSize, DefaultPageSize);
        if (parameters != null)
        {
          queryParameters.AddRange(parameters);
          // Make sure we cache extra query paramters, as they'll return different results
          cacheKeyPaging = parameters.Aggregate(cacheKeyPaging, (current, keyValuePair) => current + $"{keyValuePair.Key}/{keyValuePair.Value}");
        }

        apiResult = await GetMasterDataItemServiceDiscovery<TListModel>(route, uid?.ToString(), userId?.ToString(), customHeaders, queryParameters, cacheKeyPaging);
        if (apiResult != null)
          results.AddRange(apiResult.Models);

        currentPage++;

      } while (apiResult?.HasMore ?? false);

      return new TListModel { Models = results };
    }

    protected async Task<TRes> PostData<TReq, TRes>(string route,
      TReq request,
      IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null) where TReq : class where TRes : class, IMasterDataModel
    {
      try
      {
        var payload = JsonConvert.SerializeObject(request);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        return await SendMasterDataItemServiceDiscoveryNoCache<TRes>(route, customHeaders, HttpMethod.Post, parameters, ms);

      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return null;
        }

        throw;
      }
    }

    protected async Task PostData<TReq>(string route,
      TReq request,
      IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null) where TReq : class
    {
      try
      {
        var payload = JsonConvert.SerializeObject(request);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        await SendMasterDataItemServiceDiscoveryNoCache(route, customHeaders, HttpMethod.Post, parameters, ms);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return;
        }

        throw;
      }
    }

    protected Task UploadData(string uploadUrl, Stream payload,
      IHeaderDictionary customHeaders = null)
    {
      return webRequest.ExecuteRequestAsStreamContent(uploadUrl, HttpMethod.Put, customHeaders, payload);
    }

    protected async Task<byte[]> DownloadData(string downloadUrl, IHeaderDictionary customHeaders = null)
    {
      var response = await webRequest.ExecuteRequestAsStreamContent(downloadUrl, HttpMethod.Get, customHeaders);
      using (var responseStream = await response.ReadAsStreamAsync())
      {
        responseStream.Position = 0;
        byte[] file = new byte[responseStream.Length];
        responseStream.Read(file, 0, file.Length);
        return file;
      }
    }

    protected async Task<TRes> DeleteData<TRes>(string route, IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null) where TRes : class, IMasterDataModel
    {
      try
      {
        return await SendMasterDataItemServiceDiscoveryNoCache<TRes>(route, customHeaders, HttpMethod.Delete, parameters);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return null;
        }

        throw;
      }
    }

    protected async Task DeleteData(string route, IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null)
    {
      try
      {
        await SendMasterDataItemServiceDiscoveryNoCache(route, customHeaders, HttpMethod.Delete, parameters);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return;
        }

        throw;
      }
    }

    protected async Task<TRes> UpdateData<TReq, TRes>(string route,
      TReq request,
      IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null) where TReq : class where TRes : class, IMasterDataModel
    {
      try
      {
        var payload = JsonConvert.SerializeObject(request);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        // Need to await this, as we need the stream (if we return the task, the stream is disposed)
        return await SendMasterDataItemServiceDiscoveryNoCache<TRes>(route, customHeaders, HttpMethod.Put, parameters, ms);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return null;
        }

        throw;
      }
    }

    protected async Task UpdateData<TReq>(string route,
      TReq request,
      IList<KeyValuePair<string, string>> parameters = null,
      IHeaderDictionary customHeaders = null) where TReq : class
    {
      try
      {
        var payload = JsonConvert.SerializeObject(request);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        // Need to await this, as we need the stream (if we return the task, the stream is disposed)
        await SendMasterDataItemServiceDiscoveryNoCache(route, customHeaders, HttpMethod.Put, parameters, ms);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return;
        }

        throw;
      }
    }

    protected List<KeyValuePair<string, string>> WithLimits(int fromRow, int rowCount)
    {
      return new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("from", fromRow.ToString()),
        new KeyValuePair<string, string>("limit", rowCount.ToString())
      };
    }
  }
}
