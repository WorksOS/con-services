using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Nighthawk.MasterDataSync.Models;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using System.Configuration;
using CommonMDMModels = VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
    public class APIUserSyncProcessor : SyncProcessorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Uri _apiUserApiEndPointUri;
        private readonly string _taskName;
        private readonly IHttpRequestWrapper _httpRequestWrapper;
        private readonly IConfigurationManager _configurationManager;
        private static readonly List<int> FeatureList = new List<int> { (int)FeatureEnum.StartStopService, (int)FeatureEnum.FenceAlertService, (int)FeatureEnum.FuelService, (int)FeatureEnum.EventService, (int)FeatureEnum.DiagnosticService, (int)FeatureEnum.EngineParametersService, (int)FeatureEnum.DigitalSwitchStatusService, (int)FeatureEnum.SMULocationService, (int)FeatureEnum.VLReadyAPI, (int)FeatureEnum.AEMPService };


        private static readonly string ConsumerKey = System.Configuration.ConfigurationManager.AppSettings["ConsumerKey"];
        private static readonly string ConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["ConsumerSecret"];

        private const string UserSource = "User";
        private const string UserFeatureSource = "UserFeature";

        public APIUserSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
          : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
        {
            _taskName = taskName;
            _httpRequestWrapper = httpRequestWrapper;
            _configurationManager = configurationManager;

            if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("APIUserService.WebAPIURI")))
                throw new ArgumentNullException("Uri", "APIUserService api URL value cannot be empty");

            _apiUserApiEndPointUri = new Uri(_configurationManager.GetAppSetting("APIUserService.WebAPIURI"));
        }

        public override bool Process(ref bool isServiceStopped)
        {
            bool isDataProcessed = false;
            if (LockTaskState(_taskName, TaskTimeOutInterval))
            {
                isDataProcessed = ProcessSync(ref isServiceStopped);
                UnLockTaskState(_taskName);
            }
            return isDataProcessed;
        }

        public override bool ProcessSync(ref bool isServiceStopped)
        {
            var lastProcessedId = GetLastProcessedId(_taskName);
            var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
            var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, saveLastUpdateUtcFlag, ref isServiceStopped);

            //MasterData Updation
            //lastProcessedId = GetLastProcessedId(_taskName);
            var lastUpdateUtc = GetLastUpdateUTC(_taskName);
            var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, ref isServiceStopped);
            return (isCreateEventProcessed || isUpdateEventProcessed);
        }

        private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
        {
            var currentUtc = DateTime.UtcNow.AddSeconds(-SyncPrioritySeconds); 
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                try
                {
                    lastProcessedId = lastProcessedId ?? Int32.MinValue;
                    Log.IfInfo(string.Format("Started Processing APIUserCreationEvent. LastProcessedId : {0}", lastProcessedId));
                    var userDataList = (from u in opCtx.UserReadOnly
                                        join uf in opCtx.UserFeatureReadOnly on u.ID equals uf.fk_User
                                        join c in opCtx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                                        where FeatureList.Contains(uf.fk_Feature) && u.Active && u.ID > lastProcessedId && c.fk_CustomerTypeID != (int)CustomerTypeEnum.Account
                                        group u by new { u.ID, c.CustomerUID } into g
                                        let featureList = (from f in opCtx.FeatureReadOnly
                                                           join uf1 in opCtx.UserFeatureReadOnly on f.ID equals uf1.fk_Feature
                                                           where uf1.fk_User == g.Key.ID && FeatureList.Contains(f.ID)
                                                           select f.Name).ToList()
                                        orderby g.Key.ID ascending
                                        select new
                                        {
                                            g.FirstOrDefault().ID,
                                            g.FirstOrDefault().Name,
                                            g.Key.CustomerUID,
                                            g.FirstOrDefault().PasswordHash,
                                            g.FirstOrDefault().Salt,
                                            featureList
                                        }).Take(BatchSize).ToList();

                    if (userDataList.Count < 1)
                    {
                        Log.IfInfo(string.Format("No {0} data left for APIUserCreationEvent creation", _taskName));
                        return false;
                    }

                    var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

                    if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
                    {
                        return true;
                    }

                    foreach (var user in userDataList)
                    {
                        if (isServiceStopped)
                        {
                            Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                            break;
                        }
                        var createAPIUser = new APIUserEvent
                        {
                            Customeruid = user.CustomerUID.Value,
                            UserName = user.Name,
                            PasswordHash = user.PasswordHash,
                            UserSalt = user.Salt,
                            TPaasAppUID = Guid.Empty,
                            ConsumerKey = ConsumerKey,
                            ConsumerSecret = ConsumerSecret,
                            LastLoginUserUTC = DateTime.UtcNow,
                            FeedLoginUserUID = Guid.NewGuid(),
                            UserFeatures = user.featureList,
                            Operation = OperationEnum.Create.ToString()
                        };

                        var svcResponse = ProcessServiceRequestAndResponse(createAPIUser, _httpRequestWrapper, _apiUserApiEndPointUri, requestHeader, HttpMethod.Post);
                        Log.IfInfo("Create APIUser: " + user + " returned " + svcResponse.StatusCode);
                        switch (svcResponse.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                lastProcessedId = user.ID;
                                break;
                            case HttpStatusCode.Unauthorized:
                                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                                svcResponse = ProcessServiceRequestAndResponse(createAPIUser, _httpRequestWrapper, _apiUserApiEndPointUri, requestHeader, HttpMethod.Post);
                                if (svcResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    lastProcessedId = user.ID;
                                }
                                break;
                            case HttpStatusCode.InternalServerError:
                                Log.IfError("Internal server error");
                                return true;
                            case HttpStatusCode.BadRequest:
                                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createAPIUser));
                                lastProcessedId = user.ID;
                                break;
                            case HttpStatusCode.Forbidden:
                                Log.IfError("Forbidden status code received while hitting Tpaas APIUserMigration service");
                                break;
                            default:
                                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1} ", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(createAPIUser)));
                                return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.IfError(string.Format("Exception in processing {0} Creation {1} \n {2}", _taskName, e.Message, e.StackTrace));
                }
                finally
                {
                    //Saving last update utc if it is not set
                    if (saveLastUpdateUtcFlag)
                    {
                        opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = currentUtc;
                        opCtx.SaveChanges();
                    }
                    if (lastProcessedId != Int32.MinValue)
                    {
                        //Update the last read utc to masterdatasync
                        opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
                        opCtx.SaveChanges();
                        Log.IfInfo(string.Format("Completed Processing APIUserCreationEvent. LastProcessedId : {0} ", lastProcessedId));
                    }
                    else
                    {
                        Log.IfInfo(string.Format("No Records Processed "));
                    }
                }
            }
            return true;
        }

        private bool ProcessUpdationRecords(long? lastProcessedId, DateTime? lastUpdateUtc, ref bool isServiceStopped)
        {
            Log.IfInfo(string.Format("Started Processing UpdateAPIUserEvent LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                try
                {
                    var currentUtc = DateTime.UtcNow.AddSeconds(-SyncPrioritySeconds);
                    var userDataList = (from u in opCtx.UserReadOnly
                                        join uf in opCtx.UserFeatureReadOnly on u.ID equals uf.fk_User
                                        join c in opCtx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                                        where FeatureList.Contains(uf.fk_Feature) && u.ID <= lastProcessedId && u.UpdateUTC <= currentUtc && u.UpdateUTC > lastUpdateUtc && c.fk_CustomerTypeID != (int)CustomerTypeEnum.Account
                                        group u by new { u.ID, c.CustomerUID } into g
                                        let featureList = (from f in opCtx.FeatureReadOnly
                                                           join uf1 in opCtx.UserFeatureReadOnly on f.ID equals uf1.fk_Feature
                                                           where uf1.fk_User == g.Key.ID && FeatureList.Contains(f.ID)
                                                           select f.Name).ToList()
                                        orderby g.FirstOrDefault().UpdateUTC ascending
                                        select new
                                        {
                                            Source = UserSource,
                                            g.FirstOrDefault().ID,
                                            g.FirstOrDefault().Name,
                                            g.Key.CustomerUID,
                                            g.FirstOrDefault().PasswordHash,
                                            g.FirstOrDefault().Salt,
                                            g.FirstOrDefault().Active,
                                            g.FirstOrDefault().UpdateUTC,
                                            featureList
                                        }).Take(BatchSize).ToList();

                    var userFeatureDataList = (from u in opCtx.UserReadOnly
                                               join uf in opCtx.UserFeatureReadOnly on u.ID equals uf.fk_User
                                               join c in opCtx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                                               where FeatureList.Contains(uf.fk_Feature) && uf.fk_User <= lastProcessedId && uf.UpdateUTC <= currentUtc && uf.UpdateUTC > lastUpdateUtc && c.fk_CustomerTypeID != (int)CustomerTypeEnum.Account && u.Active 
                                               group u by new { u.ID, c.CustomerUID, uf.UpdateUTC } into g
                                               let featureList = (from f in opCtx.FeatureReadOnly
                                                                  join uf1 in opCtx.UserFeatureReadOnly on f.ID equals uf1.fk_Feature
                                                                  where uf1.fk_User == g.Key.ID && FeatureList.Contains(f.ID)
                                                                  select f.Name).ToList()
                                               orderby g.Key.UpdateUTC ascending
                                               select new
                                               {
                                                   Source = UserFeatureSource,
                                                   g.FirstOrDefault().ID,
                                                   g.FirstOrDefault().Name,
                                                   g.Key.CustomerUID,
                                                   g.FirstOrDefault().PasswordHash,
                                                   g.FirstOrDefault().Salt,
                                                   g.FirstOrDefault().Active,
                                                   g.Key.UpdateUTC,
                                                   featureList
                                               }).Take(BatchSize).ToList();

                    if (!userDataList.Any() && !userFeatureDataList.Any())
                    {
                        Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Updation", currentUtc, _taskName));
                        return false;
                    }

                    var userMaxUtc = userDataList.Max(e => e.UpdateUTC as DateTime?) ?? currentUtc;
                    var userFetaureMaxUtc = userFeatureDataList.Max(e => e.UpdateUTC as DateTime?) ?? currentUtc;
                    var minOfMaxUtc = GetMinUtc(userMaxUtc, userFetaureMaxUtc);

                    var userDetailsList = userDataList.Where(e => e.UpdateUTC <= minOfMaxUtc).Union(userFeatureDataList.Where(e => e.UpdateUTC <= minOfMaxUtc)).ToList();

                    var userIdList = userDetailsList.GroupBy(e => e.ID).OrderBy(e => e.Min(s => s.UpdateUTC)).Select(e => e.Key).ToList();

                    var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

                    if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
                    {
                        return true;
                    }

                    foreach (var userID in userIdList)
                    {
                        if (isServiceStopped)
                        {
                            Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                            break;
                        }
                        var userDetail = userDetailsList.Where(e => e.ID == userID).ToList();
                        var userData = userDetail.Where(e => e.Source == UserSource).ToList();
                        var userFeatureData = userDetail.Where(e => e.Source == UserFeatureSource).ToList();
                        var userFinalData = userData.Any() ? userData : userFeatureData;
                        var utc1 = userData.Any() ? userData.Min(e => e.UpdateUTC) : currentUtc;
                        var utc2 = userFeatureData.Any() ? userFeatureData.Min(e => e.UpdateUTC) : currentUtc;
                        // Finding the minimum update utc
                        var minUtc = GetMinUtc(utc1, utc2);

                        if (userFinalData.Select(e => e.Active).First())
                        {
                            var updateAPIUser = new APIUserEvent
                            {
                                Customeruid = userFinalData.Select(e => (Guid)e.CustomerUID).First(),
                                UserName = userFinalData.Select(e => e.Name).First(),
                                PasswordHash = userFinalData.Select(e => e.PasswordHash).First(),
                                UserSalt = userFinalData.Select(e => e.Salt).First(),
                                TPaasAppUID = Guid.Empty,
                                ConsumerKey = ConsumerKey,
                                ConsumerSecret = ConsumerSecret,
                                LastLoginUserUTC = DateTime.UtcNow,
                                FeedLoginUserUID = Guid.NewGuid(),
                                UserFeatures = userFinalData.Select(e => e.featureList).First(),
                                Operation = OperationEnum.Update.ToString()
                            };

                            var svcResponse = ProcessServiceRequestAndResponse(updateAPIUser, _httpRequestWrapper, _apiUserApiEndPointUri, requestHeader, HttpMethod.Post);
                            Log.IfInfo("Update APIUSER: " + updateAPIUser.Customeruid + " returned " + svcResponse.StatusCode);
                            switch (svcResponse.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    lastUpdateUtc = minUtc;
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                                    svcResponse = ProcessServiceRequestAndResponse(updateAPIUser, _httpRequestWrapper, _apiUserApiEndPointUri, requestHeader, HttpMethod.Post);
                                    if (svcResponse.StatusCode == HttpStatusCode.OK)
                                    {
                                        lastUpdateUtc = minUtc;
                                    }
                                    break;
                                case HttpStatusCode.InternalServerError:
                                    Log.IfError("Internal server error");
                                    return true;
                                case HttpStatusCode.BadRequest:
                                    Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(updateAPIUser));
                                    lastUpdateUtc = minUtc;
                                    break;
                                case HttpStatusCode.Forbidden:
                                    Log.IfError("Forbidden status code received while hitting Tpaas APIUser service");
                                    break;
                                default:
                                    Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(updateAPIUser)));
                                    return true;
                            }
                        }
                        else
                        {
                            var requestUri = new Uri(_apiUserApiEndPointUri + "?userName=" + userFinalData.Select(e => e.Name).First() + "&customerUid=" +
                                           userFinalData.Select(e => (Guid)e.CustomerUID).First());


                            var serviceRequestMessage = new CommonMDMModels.ServiceRequestMessage
                            {
                                RequestHeaders = requestHeader,
                                RequestMethod = HttpMethod.Delete,
                                RequestUrl = requestUri
                            };

                            var svcResponse = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);

                            Log.IfInfo("Delete APIUser: " + userFinalData.Select(e => e.Name).First() + " returned " + svcResponse.StatusCode);
                            switch (svcResponse.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    lastUpdateUtc = minUtc;
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);

                                    serviceRequestMessage = new CommonMDMModels.ServiceRequestMessage
                                    {
                                        RequestHeaders = requestHeader,
                                        RequestMethod = HttpMethod.Delete,
                                        RequestUrl = requestUri
                                    };
                                    svcResponse = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);
                                    Log.IfInfo("Delete APIUser: " + userFinalData.Select(e => e.Name).First() + " returned " + svcResponse.StatusCode);
                                    if (svcResponse.StatusCode == HttpStatusCode.OK)
                                    {
                                        lastUpdateUtc = minUtc;
                                    }
                                    break;
                                case HttpStatusCode.InternalServerError:
                                    Log.IfError("Internal server error");
                                    return true;
                                case HttpStatusCode.BadRequest:
                                    Log.IfError("Error in request" + requestUri);
                                    lastUpdateUtc = minUtc;
                                    break;
                                case HttpStatusCode.Forbidden:
                                    Log.IfError("Forbidden status code received while hitting Tpaas APIUser service");
                                    break;
                                default:
                                    Log.IfError(string.Format("StatusCode : {0} Failed to process data. RequestURi : {1}", svcResponse.StatusCode, requestUri));
                                    return true;
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Log.IfError(string.Format("Exception in processing {0} updation {1} \n {2}", _taskName, e.Message, e.StackTrace));
                }
                finally
                {
                    //Update the last read utc to masterdatasync
                    opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = lastUpdateUtc;
                    opCtx.SaveChanges();
                    Log.IfInfo(string.Format("Completed Processing UpdateAPIUser. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
                }
            }
            return true;
        }

        private DateTime GetMinUtc(DateTime utc1, DateTime utc2)
        {
            return (DateTime.Compare(utc1, utc2) < 0) ? utc1 : utc2;
        }
    }
}
