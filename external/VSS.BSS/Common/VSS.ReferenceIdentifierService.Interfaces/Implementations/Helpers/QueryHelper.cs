using System;
using System.Configuration;
using Newtonsoft.Json;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Helpers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers
{
  public class QueryHelper : IQueryHelper
  {
    private readonly IHttpClientWrapper _httpClientWrapper;
    private readonly string _baseUri;
    private const string EmptyBody = "";

    public QueryHelper(IHttpClientWrapper httpClientWrapper)
    {
      _httpClientWrapper = httpClientWrapper;
       _baseUri = ConfigurationManager.AppSettings["ReferenceIdentifierServiceBaseUri"];
    }

    #region IQueryHelper Members

    public T QueryServiceToCreate<T, K>(string svcUri, K queryObject)
    {
      var response = _httpClientWrapper.Post(
        String.Format("{0}/{1}/{2}", _baseUri, svcUri, ControllerConstants.QueryServicePostActionName),
        queryObject,
        EmptyBody);

      return JsonConvert.DeserializeObject<T>(response.RawText,
        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
    }

    public T QueryServiceToUpdate<T, K>(string svcUri, K queryObject)
    {
      var response = _httpClientWrapper.Put(
        String.Format("{0}/{1}/{2}", _baseUri, svcUri, ControllerConstants.QueryServicePutActionName),
        queryObject,
        EmptyBody);

      return JsonConvert.DeserializeObject<T>(response.RawText,
        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
    }

    public T QueryServiceToRetrieve<T, K>(string svcUri, K queryObject)
    {
      var response = _httpClientWrapper.Get(
        String.Format("{0}/{1}/{2}", _baseUri, svcUri, ControllerConstants.QueryServiceGetActionName),
        queryObject);

      return JsonConvert.DeserializeObject<T>(response.RawText,
        new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects});
    }

    /// <summary>
    /// used when the query string has been created manually
    /// because easyhttp can not create it correctly on it's own since it does not know what the query parameters are
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="svcUri"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public T GetByQuery<T>(string svcUri, string query, string action)
    {
      var response = _httpClientWrapper.Get(
        String.Format("{0}/{1}/{2}?{3}", _baseUri, svcUri, action, query));

      return JsonConvert.DeserializeObject<T>(response.RawText,
        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
    }

    #endregion
  }
}
