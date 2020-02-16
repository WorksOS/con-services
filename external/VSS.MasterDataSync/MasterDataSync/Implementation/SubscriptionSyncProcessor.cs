using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
    public class SubscriptionSyncProcessor : SyncProcessorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly List<string> CustomersToBeExcluded = System.Configuration.ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"] != null ? System.Configuration.ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"].Split('$').Select(s => s.Trim()).ToList() : new List<string>();

        private static readonly List<int> CustomerTypesToBeIncluded = new List<int>() { (int)CustomerTypeEnum.Customer, (int)CustomerTypeEnum.Dealer };

        private readonly string _taskName;
        private readonly string _subscriptionTypes;
        private readonly IHttpRequestWrapper _httpRequestWrapper;
        private readonly IConfigurationManager _configurationManager;

        private Uri _assetSubscriptionApiEndPointUri;
        private Uri _projectSubscriptionApiEndPointUri;
        private Uri _customerSubscriptionApiEndPointUri;
        private static readonly List<string> SubscriptionTypesToBeIncluded = new List<string>();

        private readonly string[] assetSubscriptionServicePlans =
        {"Essentials","CATMAINT","VLMAINT","CAT Health","Standard Health","Manual Maintenance Log",
      "Load & Cycle Monitoring","Real Time Digital Switch Alerts","1 minute Update Rate Upgrade","Connected Site Gateway","VisionLink RFID",
      "Vehicle Connect","Unified Fleet","Advanced Productivity","CAT Harvest","CAT Utilization","Standard Utilization","3D Project Monitoring",
    "CAT Locator - 6 Hours","CAT Basic - 6 Hours","CAT Basic - 4 Hours","CAT Basic - Hourly","CAT Basic - 10 Minutes","CAT Essentials - 6 Hours","CAT Essentials - 4 Hours","CAT Essentials - Hourly","CAT Essentials - 10 Minutes", "CAT Daily", "VisionLink Daily"};


        private readonly string[] customerSubscriptionServicePlans = { "Manual 3D Project Monitoring" };

        private readonly string[] projectSubscriptionServicePlans = { "Landfill", "Project Monitoring" };

        public SubscriptionSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
          : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
        {
            _taskName = taskName;
            _httpRequestWrapper = httpRequestWrapper;
            _configurationManager = configurationManager;

            if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("SubscriptionService.WebAPIURI")))
                throw new ArgumentNullException("Uri", "Subscription api URL value cannot be empty");

            _assetSubscriptionApiEndPointUri = new Uri(_configurationManager.GetAppSetting("SubscriptionService.WebAPIURI") + "/asset");
            _projectSubscriptionApiEndPointUri = new Uri(_configurationManager.GetAppSetting("SubscriptionService.WebAPIURI") + "/project");
            _customerSubscriptionApiEndPointUri = new Uri(_configurationManager.GetAppSetting("SubscriptionService.WebAPIURI") + "/customer");

            var subscriptionTypesToBeIncluded = _configurationManager.GetAppSetting("ServicePlansToBeIncludedToNextGenSync");
            if (string.IsNullOrWhiteSpace(subscriptionTypesToBeIncluded) || subscriptionTypesToBeIncluded.ToString().ToUpper() == "ALL")
                _subscriptionTypes = "ALL";
            else
            {
                var providedSubscriptionTypes = subscriptionTypesToBeIncluded.ToString().Split(',').Select(s => s.Trim());
                SubscriptionTypesToBeIncluded.AddRange(providedSubscriptionTypes);
            }

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
            var lastProcessedId = GetLastProcessedId(_taskName) ?? int.MinValue;
            var lastUpdateUtc = GetLastUpdateUTC(_taskName) ?? default(DateTime).AddYears(1900);

            //MasterData Updation
            var updateEventProcessorState = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, ref isServiceStopped);
            if (updateEventProcessorState)
            {
                //All the updated events for the migrated records are processed. Starting to Process new records
                //MasterData Insertion
                var createEventProcessorState = ProcessInsertionRecords(lastProcessedId, ref isServiceStopped);
                if (createEventProcessorState)
                {
                    Log.IfDebug("No more records to Create/Update ");
                    return false; // No Update Event and No Create Event to process
                }
            }
            return true;
        }

        private bool ProcessInsertionRecords(long? lastProcessedId, ref bool isServiceStopped)
        {
            var currentUtc = DateTime.UtcNow; //.AddSeconds(-SyncPrioritySeconds);
            var todaysKeyDate = DateTime.UtcNow.KeyDate();

            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                try
                {
                    Log.IfInfo(string.Format("Started Processing CreateSubscriptionEvent. LastProcessedId : {0}", lastProcessedId));

                    var subscriptionDataList = (from sv in opCtx.ServiceViewReadOnly
                                                join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                                join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                                where (_subscriptionTypes == "ALL" || SubscriptionTypesToBeIncluded.Contains(st.Name))
                                                join c in opCtx.CustomerReadOnly on sv.fk_CustomerID equals c.ID into customer
                                                join a in opCtx.AssetReadOnly on sv.fk_AssetID equals a.AssetID
                                                join d in opCtx.DeviceReadOnly on s.fk_DeviceID equals d.ID into device
                                                where sv.ifk_SharedViewID == null && sv.ID > lastProcessedId
                                                && sv.EndKeyDate >= todaysKeyDate
                                                && sv.IsVirtual == false
                                                from cr in customer.DefaultIfEmpty()
                                                where (cr.Name == null || (CustomerTypesToBeIncluded.Contains(cr.fk_CustomerTypeID) && !CustomersToBeExcluded.Contains(cr.Name)))
                                                from de in device.DefaultIfEmpty()
                                                orderby sv.ID ascending
                                                select new
                                                {
                                                    sv.ID,
                                                    sv.ServiceViewUID,
                                                    cr.CustomerUID,
                                                    a.AssetUID,
                                                    de.DeviceUID,
                                                    SubscriptionType = st.Name,
                                                    sv.StartKeyDate,
                                                    sv.EndKeyDate,
                                                    SharedViewID = sv.ifk_SharedViewID,
                                                    de.OwnerBSSID,
                                                    UpdateUTC = currentUtc
                                                }).Take(BatchSize).ToList();

                        if (subscriptionDataList.Count < 1)
                        {
                            Log.IfInfo(string.Format("No {0} data left for creation", _taskName));
                            return false;
                        }
                        var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);
                        ServiceResponseMessage svcResponse;
                        foreach (var subscriptionData in subscriptionDataList)
                        {
                            if (isServiceStopped)
                            {
                                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                                break;
                            }

                            if (subscriptionData.AssetUID == null || subscriptionData.CustomerUID == null ||
                                subscriptionData.DeviceUID == null)
                            {
                                Log.IfInfo("The required AssetUID,CustomerUID or DeviceUID's CreateEvent has not been processed yet..");
                                break;
                            }

                        if (assetSubscriptionServicePlans.Contains(subscriptionData.SubscriptionType))
                        {
                            var createAssetSubscription = new CreateAssetSubscriptionEvent
                            {
                                SubscriptionUID = (Guid)subscriptionData.ServiceViewUID,
                                CustomerUID = (Guid)subscriptionData.CustomerUID,
                                AssetUID = subscriptionData.AssetUID,
                                DeviceUID = (Guid)subscriptionData.DeviceUID,
                                SubscriptionType = subscriptionData.SubscriptionType,
                                Source = (subscriptionData.SharedViewID == null) ? SubscriptionSourceEnum.Store.ToString() : SubscriptionSourceEnum.SAV.ToString(),
                                StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                                EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                                ActionUTC = DateTime.UtcNow
                            };
                            svcResponse = ProcessServiceRequestAndResponse<CreateAssetSubscriptionEvent>(createAssetSubscription, _httpRequestWrapper, _assetSubscriptionApiEndPointUri, requestHeader, HttpMethod.Post);

                            if ((svcResponse.StatusCode != HttpStatusCode.OK && svcResponse.StatusCode != HttpStatusCode.BadRequest))
                            {
                                Log.IfError($"Asset Subscription Create event failed for {createAssetSubscription.SubscriptionUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(createAssetSubscription)}");
                                break;
                            }
                        }
                        else if (projectSubscriptionServicePlans.Contains(subscriptionData.SubscriptionType))
                        {
                            var createProjectSubscription = new CreateProjectSubscriptionEvent
                            {
                                SubscriptionUID = (Guid)subscriptionData.ServiceViewUID,
                                CustomerUID = (Guid)subscriptionData.CustomerUID,
                                SubscriptionType = subscriptionData.SubscriptionType,
                                StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                                EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                                ActionUTC = DateTime.UtcNow
                            };
                            svcResponse = ProcessServiceRequestAndResponse<CreateProjectSubscriptionEvent>(createProjectSubscription, _httpRequestWrapper, _projectSubscriptionApiEndPointUri, requestHeader, HttpMethod.Post);

                            if ((svcResponse.StatusCode != HttpStatusCode.OK && svcResponse.StatusCode != HttpStatusCode.BadRequest))
                            {
                                Log.IfError($"Project Subscription Create event failed for {createProjectSubscription.SubscriptionUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(createProjectSubscription)}");
                                break;
                            }
                        }
                        else if (customerSubscriptionServicePlans.Contains(subscriptionData.SubscriptionType))
                        {
                            var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent
                            {
                                SubscriptionUID = (Guid)subscriptionData.ServiceViewUID,
                                CustomerUID = (Guid)subscriptionData.CustomerUID,
                                SubscriptionType = subscriptionData.SubscriptionType,
                                StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                                EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                                ActionUTC = DateTime.UtcNow
                            };
                            svcResponse = ProcessServiceRequestAndResponse<CreateCustomerSubscriptionEvent>(createCustomerSubscriptionEvent, _httpRequestWrapper, _customerSubscriptionApiEndPointUri, requestHeader, HttpMethod.Post);

                            if (svcResponse.StatusCode != HttpStatusCode.OK && svcResponse.StatusCode != HttpStatusCode.BadRequest)
                            {
                                Log.IfError($"Customer Subscription Create event failed for {createCustomerSubscriptionEvent.SubscriptionUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(createCustomerSubscriptionEvent)}");
                                break;
                            }
                        }
                        lastProcessedId = subscriptionData.ID;
                    }
                }
                catch (Exception e)
                {
                    Log.IfError(string.Format("Exception in processing {0} Insertion {1} \n {2}", _taskName, e.Message, e.StackTrace));
                }
                finally
                {
                    if (lastProcessedId != int.MinValue)
                    {
                        //Update the last read utc to masterdatasync
                        opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
                        opCtx.SaveChanges();
                        Log.IfInfo(string.Format("Completed Processing CreateSubscriptionEvent. LastProcessedId : {0} ", lastProcessedId));
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
            Log.IfInfo(string.Format("Started Processing UpdateSubscriptionEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));

            List<UpdateSubscriptionEvent> subscriptionDataList;
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                try
                {
                    var currentUtc = DateTime.UtcNow.AddSeconds(-SyncPrioritySeconds);
                    subscriptionDataList = (from sv in opCtx.ServiceViewReadOnly
                                            join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                            join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                            where (_subscriptionTypes == "ALL" || SubscriptionTypesToBeIncluded.Contains(st.Name))
                                            join c in opCtx.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                                            join a in opCtx.AssetReadOnly on sv.fk_AssetID equals a.AssetID
                                            join d in opCtx.DeviceReadOnly on s.fk_DeviceID equals d.ID
                                            where !CustomersToBeExcluded.Contains(c.Name)
                                            && CustomerTypesToBeIncluded.Contains(c.fk_CustomerTypeID)
                                            && sv.ifk_SharedViewID == null && sv.ID <= lastProcessedId
                                            && sv.UpdateUTC <= currentUtc && sv.UpdateUTC > lastUpdateUtc
                                            && sv.IsVirtual == false
                                            orderby sv.UpdateUTC
                                            select new UpdateSubscriptionEvent
                                            {
                                                ID = sv.ID,
                                                ServiceViewUID = sv.ServiceViewUID,
                                                CustomerUID = c.CustomerUID,
                                                AssetUID = a.AssetUID,
                                                DeviceUID = d.DeviceUID,
                                                OwnerBSSID = d.OwnerBSSID,
                                                SubscriptionType = st.Name,
                                                SharedViewID = sv.ifk_SharedViewID,
                                                StartKeyDate = sv.StartKeyDate,
                                                EndKeyDate = sv.EndKeyDate,
                                                UpdateUTC = sv.UpdateUTC
                                            }).Take(BatchSize).ToList();

                    var subscriptionDataListSize = subscriptionDataList.Count();
                    if (subscriptionDataList.Count < 1)
                    {
                        lastUpdateUtc = currentUtc;
                        Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Updation", currentUtc, _taskName));
                        return true;
                    }
                    if (subscriptionDataListSize == BatchSize)
                    {
                        var firstRowUpdateUtc = subscriptionDataList[0].UpdateUTC;
                        var lastRowUpdateUtc = subscriptionDataList[subscriptionDataList.Count - 1].UpdateUTC;

                        //If the first record's update utc matches with the last record's updateutc, then select all the records having same update utc and union it
                        if (DateTime.Compare(firstRowUpdateUtc, lastRowUpdateUtc) == 0)
                        {
                            var newSubscriptionList = (from sv in opCtx.ServiceViewReadOnly
                                                       join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                                       join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                                       join c in opCtx.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                                                       join a in opCtx.AssetReadOnly on sv.fk_AssetID equals a.AssetID
                                                       join d in opCtx.DeviceReadOnly on s.fk_DeviceID equals d.ID
                                                       where !CustomersToBeExcluded.Contains(c.Name) 
                                                       && CustomerTypesToBeIncluded.Contains(c.fk_CustomerTypeID) 
                                                       && sv.ifk_SharedViewID == null && sv.ID <= lastProcessedId 
                                                       && sv.UpdateUTC == firstRowUpdateUtc && sv.IsVirtual == false
                                                       orderby sv.UpdateUTC
                                                       select new UpdateSubscriptionEvent
                                                       {
                                                           ID = sv.ID,
                                                           ServiceViewUID = sv.ServiceViewUID,
                                                           CustomerUID = c.CustomerUID,
                                                           AssetUID = a.AssetUID,
                                                           DeviceUID = d.DeviceUID,
                                                           OwnerBSSID = d.OwnerBSSID,
                                                           SubscriptionType = st.Name,
                                                           SharedViewID = sv.ifk_SharedViewID,
                                                           StartKeyDate = sv.StartKeyDate,
                                                           EndKeyDate = sv.EndKeyDate,
                                                           UpdateUTC = sv.UpdateUTC
                                                       }).Distinct().ToList();

                            subscriptionDataList = newSubscriptionList;
                        }
                        else
                        {
                            var lastIndex = subscriptionDataListSize - 1;
                            while (lastIndex > 1 && subscriptionDataList[lastIndex-1].UpdateUTC == subscriptionDataList[lastIndex].UpdateUTC)
                            {
                                --lastIndex;
                            }
                            subscriptionDataList.RemoveRange(lastIndex, subscriptionDataListSize - lastIndex);
                        }
                    }

                    var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);
                    ServiceResponseMessage svcResponse;
                    var index = 0;
                    foreach (var subscriptionData in subscriptionDataList)
                    {
                        index++;
                        if (isServiceStopped)
                        {
                            Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                            break;
                        }
                        if (assetSubscriptionServicePlans.Contains(subscriptionData.SubscriptionType))
                        {
                            var updateAssetSubscription = new UpdateAssetSubscriptionEvent
                            {
                                SubscriptionUID = (Guid)subscriptionData.ServiceViewUID,
                                CustomerUID = subscriptionData.CustomerUID,
                                AssetUID = subscriptionData.AssetUID,
                                DeviceUID = subscriptionData.DeviceUID,
                                SubscriptionType = subscriptionData.SubscriptionType,
                                Source = (subscriptionData.SharedViewID == null) ? SubscriptionSourceEnum.Store.ToString() : SubscriptionSourceEnum.SAV.ToString(),
                                StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                                EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                                ActionUTC = DateTime.UtcNow
                            };
                            svcResponse = ProcessServiceRequestAndResponse<UpdateAssetSubscriptionEvent>(updateAssetSubscription, _httpRequestWrapper, _assetSubscriptionApiEndPointUri, requestHeader, HttpMethod.Put);

                            if (svcResponse.StatusCode != HttpStatusCode.OK && svcResponse.StatusCode != HttpStatusCode.BadRequest)
                            {
                                Log.IfError($"Update Asset Subscription event failed for {updateAssetSubscription.SubscriptionUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(updateAssetSubscription)}");
                                break;
                            }
                        }
                        else if (projectSubscriptionServicePlans.Contains(subscriptionData.SubscriptionType))
                        {
                            var updateProjectSubscription = new UpdateProjectSubscriptionEvent
                            {
                                SubscriptionUID = (Guid)subscriptionData.ServiceViewUID,
                                CustomerUID = subscriptionData.CustomerUID,
                                SubscriptionType = subscriptionData.SubscriptionType,
                                StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                                EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                                ActionUTC = DateTime.UtcNow
                            };

                            svcResponse = ProcessServiceRequestAndResponse<UpdateProjectSubscriptionEvent>(updateProjectSubscription, _httpRequestWrapper, _projectSubscriptionApiEndPointUri, requestHeader, HttpMethod.Put);

                            if (svcResponse.StatusCode != HttpStatusCode.OK && svcResponse.StatusCode != HttpStatusCode.BadRequest)
                            {
                                Log.IfError($"Update Project Subscription event failed for {updateProjectSubscription.SubscriptionUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(updateProjectSubscription)}");
                                break;
                            }
                        }
                        else if (customerSubscriptionServicePlans.Contains(subscriptionData.SubscriptionType))
                        {
                            var updateCustomerSubscriptionEvent = new UpdateCustomerSubscriptionEvent
                            {
                                SubscriptionUID = (Guid)subscriptionData.ServiceViewUID,
                                StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                                EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                                ActionUTC = DateTime.UtcNow
                            };

                            svcResponse = ProcessServiceRequestAndResponse<UpdateCustomerSubscriptionEvent>(updateCustomerSubscriptionEvent, _httpRequestWrapper, _customerSubscriptionApiEndPointUri, requestHeader, HttpMethod.Put);

                            if (svcResponse.StatusCode != HttpStatusCode.OK && svcResponse.StatusCode != HttpStatusCode.BadRequest)
                            {
                                Log.IfError($"Update Customer Subscription event failed for {updateCustomerSubscriptionEvent.SubscriptionUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(updateCustomerSubscriptionEvent)}");
                                break;
                            }
                        }
                        if (index < subscriptionDataList.Count && DateTime.Compare(subscriptionDataList[index].UpdateUTC, subscriptionDataList[index - 1].UpdateUTC) == 0)
                        {
                            Log.IfInfo("Same Update UTC found hence will not update BookMark until all the updateUtc is processed : " + subscriptionDataList[index].UpdateUTC);
                        }
                        else
                        {
                            lastUpdateUtc = subscriptionData.UpdateUTC;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.IfError(string.Format("Exception in processing {0} updation {1} \n {2}", _taskName, e.Message, e.StackTrace));
                    return false;
                }
                finally
                {
                    //Update the last read utc to masterdatasync
                    opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = lastUpdateUtc;
                    opCtx.SaveChanges();
                    Log.IfInfo(string.Format("Completed Processing UpdateSubscriptionEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
                }
            }
            return true;
        }

        public class UpdateSubscriptionEvent
        {
            public long ID { get; set; }
            public Guid? ServiceViewUID { get; set; }
            public Guid? CustomerUID { get; set; }
            public Guid? AssetUID { get; set; }
            public Guid? DeviceUID { get; set; }
            public string OwnerBSSID { get; set; }
            public long? SharedViewID { get; set; }
            public string SubscriptionType { get; set; }
            public int StartKeyDate { get; set; }
            public int EndKeyDate { get; set; }
            public DateTime UpdateUTC { get; set; }
        }
    }
}
