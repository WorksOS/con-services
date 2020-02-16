using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;


namespace VSS.Hosted.VLCommon.Services.MDM.Common
{
  public class TPassAuthorizationManager : ITpassAuthorizationManager
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IHttpRequestWrapper _requestWrapper;
    private readonly ICacheManager _cacheManager;
    private const string DefaultTokenBaseUrl = "https://identity-stg.trimble.com/token";

    public TPassAuthorizationManager(IHttpRequestWrapper requestWrapper,ICacheManager cacheManager)
    {
      _requestWrapper = requestWrapper;
      _cacheManager = cacheManager;
    }

    public KeyValuePair<string, string> GetBearerAuthHeader(string bearerToken)
    {
      if (string.IsNullOrWhiteSpace(bearerToken))
      {
        Log.IfError("Bearer token is empty");
        return new KeyValuePair<string, string>("Authorization", StringConstants.InvalidHeader);
      }
      return new KeyValuePair<string, string>("Authorization", "Bearer " + bearerToken);
    }

    public KeyValuePair<string, string> GetBasicAuthHeader(AppCredentials appCredentials)
    {
      string authHeader = string.Empty;
      if (!ValidateAppCredentials(appCredentials))
      {
        Log.IfError("Invalid Consumerkey/ConsumerSecret");
        return new KeyValuePair<string, string>("Authorization", StringConstants.InvalidHeader);
      }

      string strBasicToken = string.Concat(appCredentials.ConsumerKey, ":", appCredentials.ConsumerSecret);
      var plainTextBytes = Encoding.UTF8.GetBytes(strBasicToken);
      authHeader = Convert.ToBase64String(plainTextBytes);
      return new KeyValuePair<string, string>("Authorization", "Basic " + authHeader);
    }

    public string GetNewToken(AppCredentials appCredentials, string tokenBaseUrl)
    {
      if (string.IsNullOrWhiteSpace(tokenBaseUrl))
      {
        tokenBaseUrl = DefaultTokenBaseUrl;
      }

      if (!ValidateAppCredentials(appCredentials))
      {
        return null;
      }

      var tokenUrl = string.Concat(tokenBaseUrl, "?grant_type=client_credentials");
      var basicAuthHeader = GetBasicAuthHeader(appCredentials);

      if (basicAuthHeader.Value == StringConstants.InvalidHeader)
      {
        return null;
      }

      ServiceRequestMessage requestMessage = new ServiceRequestMessage
      {
        RequestMethod = HttpMethod.Post,
        RequestHeaders = new List<KeyValuePair<string, string>> { basicAuthHeader },
        RequestContentType = StringConstants.UrlEncodedContentType,
        RequestUrl = new Uri(tokenUrl),
        RequestEncoding = Encoding.UTF8
      };

      var response = _requestWrapper.RequestDispatcher(requestMessage);
      if (response.StatusCode == HttpStatusCode.OK)
      {
        var oauthtoken = JsonHelper.DeserializeJsonToObject<OAuthToken>(response.Content.ReadAsStringAsync().Result);
        if (!string.IsNullOrWhiteSpace(oauthtoken.access_token))
        {
          _cacheManager.UpdateCacheItem(new CacheItem(_cacheManager.GetCacheKey(),oauthtoken.access_token),oauthtoken.expires_in);
          return oauthtoken.access_token; 
        }
      }
      return null;
    }

    #region ValidateTokenCredentials
    //public bool ValidateTokenCredentials(TokenCredentials credentials)
    //{
    //  if (credentials == null)
    //  {
    //    Log.IfError("Token Api's Credentials are empty");
    //    return false;
    //  }
    //  if (string.IsNullOrWhiteSpace(credentials.UserName))
    //  {
    //    Log.IfError("Token Api's username is empty");
    //    return false;
    //  }
    //  if (string.IsNullOrWhiteSpace(credentials.Password))
    //  {
    //    Log.IfError("Token Api's Password is empty");
    //    return false;
    //  }
    //  return true;
    //}
    #endregion

    public bool ValidateAppCredentials(AppCredentials credentials)
    {
      if (credentials == null)
      {
        Log.IfError("Token Api's Credentials are empty");
        return false;
      }
      if (string.IsNullOrWhiteSpace(credentials.ConsumerKey))
      {
        Log.IfError("Token Api's ConsumerKey is empty");
        return false;
      }
      if (string.IsNullOrWhiteSpace(credentials.ConsumerSecret))
      {
        Log.IfError("Token Api's ConsumerSecret is empty");
        return false;
      }
      return true;
    }
  }
}
