using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM
{
	
	public class ServiceBase
	{
		private readonly ILog _log;
		//polymorphic logger - to be used by derived classes to get their loggers
		protected ILog Logger
		{
			get { return LogManager.GetLogger(GetType()); }
		}
		private readonly HttpClient _httpClient;
        private readonly IConfigurationManager _configurationManager;
        private readonly ICacheManager _cacheManager;
        private readonly ITpassAuthorizationManager _tpassAuthManager;
        private readonly IHttpRequestWrapper _requestWrapper;
        public AppCredentials AppCredentials { get; set; }
        public string TokenUrl { get; set; }

		public ServiceBase()
		{
			_log = Logger;
				_httpClient = new HttpClient
      {
        //Timeout = new TimeSpan(0, 0, AppConfigSettings.TimeOutValue)
      };
                _configurationManager = new AppConfigurationManager();
                _cacheManager = new CacheManager();
                _requestWrapper = new HttpRequestWrapper();
                _tpassAuthManager = new TPassAuthorizationManager(_requestWrapper,_cacheManager);

		}
		protected bool DispatchRequest(string endpointUri,HttpMethod methodType, string payload = null, List<KeyValuePair<string, string>> headers = null)
		{
            var requestHeaders = GetRequestHeaderOnAuthenticationType(false);
            var httpResponseMessage = DispatchHttpRequest(endpointUri, methodType, payload, requestHeaders);
            switch (httpResponseMessage.StatusCode)
            {
                case HttpStatusCode.OK:
                    return true;
                case HttpStatusCode.Unauthorized:
                    requestHeaders = GetRequestHeaderOnAuthenticationType(isOuthRetryCall: true);
                    httpResponseMessage = DispatchHttpRequest(endpointUri, methodType, payload, requestHeaders);
                    return (httpResponseMessage.StatusCode == HttpStatusCode.OK);
                case HttpStatusCode.InternalServerError:
                    _log.IfError("Internal server error");
                    return false;
                case HttpStatusCode.BadRequest:
                    _log.IfError("Error in payload " + payload);
                    return false;
            }
            return false;

		}

        private HttpResponseMessage DispatchHttpRequest(string endpointUri, HttpMethod methodType, string payload, List<KeyValuePair<string, string>> headers)
        {
            var request = new HttpRequestMessage(methodType, endpointUri);
            if (headers != null)
                headers.ForEach(header => request.Headers.Add(header.Key, header.Value));
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            if (payload != null && (methodType == HttpMethod.Post || methodType == HttpMethod.Put))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            }
            HttpResponseMessage httpResponseMessage = null;
            _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ContinueWith(x =>
            {
                _log.InfoFormat("{0} Operation completed: ", methodType.ToString());
                httpResponseMessage = x.Result;
            },
            TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.PreferFairness).Wait();
            return httpResponseMessage;
        }

		protected List<KeyValuePair<string, string>> GetBasicAuthHeader(string consumerKey, string consumerSecret)
		{
			return new List<KeyValuePair<string, string>>
                  {
                    new KeyValuePair<string, string>("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(consumerKey + ":" + consumerSecret)))
                  };
		}

        public virtual void InitializeAuthorizationCredentials()
        {
            TokenUrl = !string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("OAuthTokenURL")) ? _configurationManager.GetAppSetting("OAuthTokenURL") : string.Empty;

            AppCredentials = new AppCredentials
            {
                ConsumerKey = !string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("OAuthConsumerKey"))
                  ? _configurationManager.GetAppSetting("OAuthConsumerKey")
                  : string.Empty,
                ConsumerSecret = !string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("OAuthConsumerSecret"))
                  ? _configurationManager.GetAppSetting("OAuthConsumerSecret")
                  : string.Empty,
            };
        }

        public List<KeyValuePair<string, string>> GetRequestHeaderOnAuthenticationType(bool isOuthRetryCall = false)
        {
            var requestHeader = new List<KeyValuePair<string, string>>
              {
                 new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)
              };

            var authenticationType = _configurationManager.GetAppSetting("AuthenticationType");
            if (!string.IsNullOrWhiteSpace(authenticationType))
            {
                switch (authenticationType.Trim().ToLower())
                {
                    case "oauth":
                        InitializeAuthorizationCredentials();
                        var cacheKey = _cacheManager.GetCacheKey();
                        if (cacheKey != null)
                        {
                            var bearerToken = GetToken(_cacheManager, _tpassAuthManager, isOuthRetryCall, cacheKey);
                            if (bearerToken != null)
                            {
                                var bearerAuthHeader = _tpassAuthManager.GetBearerAuthHeader(bearerToken);
                                if (bearerAuthHeader.Value != StringConstants.InvalidHeader)
                                {
                                    requestHeader = new List<KeyValuePair<string, string>>
                                      {
                                        bearerAuthHeader
                                      };
                                }
                                else
                                {
                                    _log.IfError("Invalid Header Info");
                                }
                            }
                            else
                            {
                                _log.IfError("Empty Bearer Token");
                            }
                        }
                        else
                        {
                            _log.IfError("Cache Key Empty");
                        }
                        return requestHeader;
                    default:
                        _log.IfError("Authentication Type not specified");
                        return requestHeader;
                }
            }
            return null;
        }

        public string GetToken(ICacheManager _cacheManager, ITpassAuthorizationManager tpassAuthManager, bool cacheReset = false, string cacheKey = null)
        {
            string bearerToken = null;
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                cacheKey = _cacheManager.GetCacheKey();
            }

            //To retrieve a new token
            if (cacheReset)
            {
                bearerToken = tpassAuthManager.GetNewToken(AppCredentials, TokenUrl);
                return bearerToken;
            }

            bearerToken = _cacheManager.GetTokenFromCache(cacheKey);

            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                bearerToken = tpassAuthManager.GetNewToken(AppCredentials, TokenUrl);
            }
            return bearerToken;
        }

	}
}
