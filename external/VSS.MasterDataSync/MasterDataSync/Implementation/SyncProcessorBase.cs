using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
    public abstract class SyncProcessorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int DefaultTaskTimeOutInterval = 620000;
        protected const int DefaultBatchSize = 1000;
        protected readonly int DefaultSyncPrioritySeconds = 60;
        protected readonly int SyncPrioritySeconds;
        protected readonly int BatchSize;
        protected readonly int TaskTimeOutInterval;
        protected readonly IConfigurationManager ConfigurationManager;
        private readonly ICacheManager _cacheManager;
        private readonly ITpassAuthorizationManager _tPassAuthManager;
        public AppCredentials AppCredentials { get; set; }
        public string TokenUrl { get; set; }

        protected SyncProcessorBase(IConfigurationManager configurationManager, IHttpRequestWrapper httpRequestWrapper, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
        {
            BatchSize = !string.IsNullOrWhiteSpace(configurationManager.GetAppSetting("BatchSize"))
               ? Convert.ToInt32(configurationManager.GetAppSetting("BatchSize"))
               : DefaultBatchSize;
            SyncPrioritySeconds = Int32.TryParse(System.Configuration.ConfigurationManager.AppSettings["SyncPriorityDelaySeconds"], out var seconds) ? seconds : DefaultSyncPrioritySeconds;
            ConfigurationManager = configurationManager;
            _cacheManager = cacheManager;
            _tPassAuthManager = tpassAuthorizationManager;
            TaskTimeOutInterval = (System.Configuration.ConfigurationManager.AppSettings["TaskTimeoutInterval"] != null) ? (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["TaskTimeoutInterval"]) / 60000) : DefaultTaskTimeOutInterval;
        }

        public virtual void InitializeAuthorizationCredentials()
        {
            TokenUrl = !string.IsNullOrWhiteSpace(ConfigurationManager.GetAppSetting("OAuthTokenURL")) ? ConfigurationManager.GetAppSetting("OAuthTokenURL") : string.Empty;

            AppCredentials = new AppCredentials
            {
                ConsumerKey = !string.IsNullOrWhiteSpace(ConfigurationManager.GetAppSetting("OAuthConsumerKey"))
                ? ConfigurationManager.GetAppSetting("OAuthConsumerKey")
                : string.Empty,
                ConsumerSecret = !string.IsNullOrWhiteSpace(ConfigurationManager.GetAppSetting("OAuthConsumerSecret"))
                ? ConfigurationManager.GetAppSetting("OAuthConsumerSecret")
                : string.Empty,
            };
        }

        public virtual long? GetLastProcessedId(string taskName)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var lastProcessData = opCtx.MasterDataSyncReadOnly.SingleOrDefault(t => t.TaskName == taskName);
                if (lastProcessData != null)
                    return lastProcessData.LastProcessedID;
            }
            return null;
        }

        public virtual DateTime? GetLastInsertUTC(string taskName)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var lastInsertUtcData = opCtx.MasterDataSyncReadOnly.SingleOrDefault(t => t.TaskName == taskName);
                if (lastInsertUtcData != null)
                    return lastInsertUtcData.LastInsertedUTC ?? default(DateTime).AddYears(1900);
            }
            return null;
        }

        public virtual DateTime? GetLastUpdateUTC(string taskName)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var lastUpdateUtcData = opCtx.MasterDataSyncReadOnly.SingleOrDefault(t => t.TaskName == taskName);
                if (lastUpdateUtcData != null)
                    return lastUpdateUtcData.LastUpdatedUTC;
            }
            return null;
        }

        public virtual DateTime? GetLastCustomerUpdateUTC(string taskName)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var lastUpdateUtcData = opCtx.MasterDataSyncReadOnly.SingleOrDefault(t => t.TaskName == taskName);
                if (lastUpdateUtcData != null)
                    return lastUpdateUtcData.LastInsertedUTC;
            }
            return null;
        }

        public virtual bool LockTaskState(string taskName, int taskTimeOutInterval)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var task = opCtx.MasterDataSyncReadOnly.SingleOrDefault(t => t.TaskName == taskName);
                if (task == null) return false;
                if (task.ServerName != Environment.MachineName)
                    Log.IfInfo($"Warn: Previsously the task was processed by {task.ServerName}. Current Servername: {Environment.MachineName}");
                if (!task.InProgress)
                {
                    opCtx.MasterDataSync.Single(t => t.TaskName == taskName).InProgress = true;
                    opCtx.MasterDataSync.Single(t => t.TaskName == taskName).ServerName = Environment.MachineName;
                    opCtx.MasterDataSync.Single(t => t.TaskName == taskName).StartUTC = DateTime.UtcNow;
                    opCtx.SaveChanges();
                    Log.IfInfo(string.Format("Started processing {0} Task", taskName));
                    return true;
                }
                //Timed out task
                //if (task.InProgress && (DbFunctions.DiffMinutes(task.StartUTC, DateTime.UtcNow) >= taskTimeOutInterval))
                if (task.InProgress && CalculateTaskTimeDifference((DateTime)task.StartUTC, DateTime.UtcNow) >= taskTimeOutInterval)
                {
                    opCtx.MasterDataSync.Single(t => t.TaskName == taskName).ServerName = Environment.MachineName;
                    opCtx.MasterDataSync.Single(t => t.TaskName == taskName).StartUTC = DateTime.UtcNow;
                    opCtx.SaveChanges();
                    Log.IfInfo(string.Format("{0} Task Timed Out. I am({1}) picking up this task", taskName, Environment.MachineName));
                    return true;
                }
                return false;
            }
        }

        private double CalculateTaskTimeDifference(DateTime startUtc, DateTime currentTime)
        {
            TimeSpan temp = currentTime - startUtc;
            return temp.TotalMinutes;
        }

        public virtual void UnLockTaskState(string taskName)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var inProgress = opCtx.MasterDataSyncReadOnly.Single(t => t.TaskName == taskName).InProgress;
                if (!inProgress) return;

                opCtx.MasterDataSync.Single(t => t.TaskName == taskName).InProgress = false;
                opCtx.MasterDataSync.Single(t => t.TaskName == taskName).EndUTC = DateTime.UtcNow;
                opCtx.SaveChanges();
                Log.IfDebug(string.Format("Completed Processing {0} Task", taskName));
            }
        }

        public static void UnLockAllTaskState()
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var taskList = opCtx.MasterDataSyncReadOnly.Where(e => e.InProgress && (e.ServerName == Environment.MachineName)).ToList();
                foreach (var task in taskList)
                {
                    opCtx.MasterDataSync.Single(t => t.TaskName == task.TaskName).InProgress = false;
                    opCtx.MasterDataSync.Single(t => t.TaskName == task.TaskName).EndUTC = DateTime.UtcNow;
                    opCtx.SaveChanges();
                }
                Log.IfInfo(string.Format("Releasing all my ({0}) tasks", Environment.MachineName));
            }
        }

        public virtual ServiceResponseMessage ProcessServiceRequestAndResponse<T>(T requestData, IHttpRequestWrapper httpRequestWrapper, Uri requestUri, List<KeyValuePair<string, string>> requestHeaders, HttpMethod requestMethod,bool isRetryCall=false)
        {
            var payload = JsonHelper.SerializeObjectToJson(requestData);

            Log.Debug($"payload :{payload}");

            ServiceRequestMessage svcRequestMessage = new ServiceRequestMessage
            {
                RequestContentType = StringConstants.JsonContentType,
                RequestEncoding = Encoding.UTF8,
                RequestMethod = requestMethod,
                RequestPayload = payload,
                RequestUrl = requestUri,
                RequestHeaders = requestHeaders
            };

            var svcResponse = httpRequestWrapper.RequestDispatcher(svcRequestMessage);
            switch (svcResponse.StatusCode)
            {
                case HttpStatusCode.Unauthorized:  // If token expired, regenerate the token and try again.
                    if (!isRetryCall)
                    requestHeaders = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                    svcResponse = ProcessServiceRequestAndResponse(requestData, httpRequestWrapper, requestUri, requestHeaders, requestMethod,true);
                    break;
            }

            return svcResponse;
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

        public virtual Dictionary<string, string> GetRequestHeaderDictionaryWithAuthenticationType(bool isOuthRetryCall = false)
        {
            return GetRequestHeaderListWithAuthenticationType(isOuthRetryCall).
                  ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

        }

        public virtual List<KeyValuePair<string, string>> GetRequestHeaderListWithAuthenticationType(bool isOuthRetryCall = false)
        {
            var requestHeader = new List<KeyValuePair<string, string>>
      {
         new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)
      };

            var authenticationType = ConfigurationManager.GetAppSetting("AuthenticationType");
            if (!string.IsNullOrWhiteSpace(authenticationType))
            {
                switch (authenticationType.Trim().ToLower())
                {
                    case "oauth":
                        InitializeAuthorizationCredentials();
                        var cacheKey = _cacheManager.GetCacheKey();
                        if (cacheKey != null)
                        {
                            var bearerToken = GetToken(_cacheManager, _tPassAuthManager, isOuthRetryCall, cacheKey);
                            if (bearerToken != null)
                            {
                                var bearerAuthHeader = _tPassAuthManager.GetBearerAuthHeader(bearerToken);
                                if (bearerAuthHeader.Value != StringConstants.InvalidHeader)
                                {
                                    requestHeader = new List<KeyValuePair<string, string>>
                  {
                    bearerAuthHeader
                  };
                                }
                                else
                                {
                                    Log.IfError("Invalid Header Info");
                                }
                            }
                            else
                            {
                                Log.IfError("Empty Bearer Token");
                            }
                        }
                        else
                        {
                            Log.IfError("Cache Key Empty");
                        }
                        return requestHeader;
                    default:
                        Log.IfError("Authentication Type not specified");
                        return requestHeader;
                }
            }
            return requestHeader;
        }

        public abstract bool Process(ref bool isServiceStopped);

        public abstract bool ProcessSync(ref bool isServiceStopped);

    }
}