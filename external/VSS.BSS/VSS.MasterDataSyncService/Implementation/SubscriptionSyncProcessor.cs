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
using VSS.Hosted.VLCommon.Services.MDM;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class SubscriptionSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly List<string> CustomersToBeExcluded = ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"] != null ? ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"].Split('$').Select(s => s.Trim()).ToList() : new List<string>();

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
    "CAT Locator - 6 Hours","CAT Basic - 6 Hours","CAT Basic - 4 Hours","CAT Basic - Hourly","CAT Basic - 10 Minutes","CAT Essentials - 6 Hours","CAT Essentials - 4 Hours","CAT Essentials - Hourly","CAT Essentials - 10 Minutes"};


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
      if (LockTaskState(_taskName, _taskTimeOutInterval))
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
      if (updateEventProcessorState == BatchProcessorState.NoRecordsToProcess || updateEventProcessorState == BatchProcessorState.AllRecordsProcessedSuccessfully)
      {
        //All the updated events for the migrated records are processed. Starting to Process new records
        //MasterData Insertion
        var createEventProcessorState = ProcessInsertionRecords(lastProcessedId, ref isServiceStopped);
        if (createEventProcessorState == BatchProcessorState.NoRecordsToProcess)
        {
          Log.IfDebug("No more records to Create/Update ");
          return false; // No Update Event and No Create Event to process
        }
      }
      return true;
    }

    private BatchProcessorState ProcessInsertionRecords(long? lastProcessedId, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      var todaysKeyDate = DateTime.UtcNow.KeyDate();

      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          Log.IfInfo(string.Format("Started Processing CreateSubscriptionEvent. LastProcessedId : {0}", lastProcessedId));

          var tasksProcessedState = opCtx.MasterDataSyncReadOnly.Where(
            e => (e.TaskName == StringConstants.CustomerTask) || (e.TaskName == StringConstants.AssetTask) || (e.TaskName == StringConstants.DeviceTask))
            .Select(e => new TaskState { TaskName = e.TaskName, lastProcessedId = e.LastProcessedID ?? int.MinValue, InsertUtc = e.LastInsertedUTC }).ToList();

          var assetTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.AssetTask);
          var customerTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.CustomerTask);
          var deviceTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.DeviceTask);

          if (assetTaskState != null && customerTaskState != null && deviceTaskState != null)
          {
            assetTaskState.InsertUtc = assetTaskState.InsertUtc ?? default(DateTime).AddYears(1900);

            var subscriptionDataList = (from sv in opCtx.ServiceViewReadOnly
                                        join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                        join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                        where (_subscriptionTypes == "ALL" || SubscriptionTypesToBeIncluded.Contains(st.Name))
                                        join c in opCtx.CustomerReadOnly.Where(e => (e.ID <= customerTaskState.lastProcessedId)) on sv.fk_CustomerID equals c.ID into customer
                                        join a in opCtx.AssetReadOnly.Where(e => e.InsertUTC < assetTaskState.InsertUtc || (e.InsertUTC == assetTaskState.InsertUtc && e.AssetID <= assetTaskState.lastProcessedId))
                                          .OrderBy(e => e.InsertUTC).ThenBy(e => e.AssetID) on sv.fk_AssetID equals a.AssetID into asset
                                        where sv.ID > lastProcessedId && sv.UpdateUTC <= currentUtc && sv.EndKeyDate >= todaysKeyDate && sv.IsVirtual == false
                                        from cr in customer.DefaultIfEmpty()
                                        where (cr.Name == null || (CustomerTypesToBeIncluded.Contains(cr.fk_CustomerTypeID) && !CustomersToBeExcluded.Contains(cr.Name)))
                                        from at in asset.DefaultIfEmpty()
                                        join d in opCtx.DeviceReadOnly.Where(e => e.ID <= deviceTaskState.lastProcessedId) on at.fk_DeviceID equals d.ID into device
                                        from de in device.DefaultIfEmpty()
                                        orderby sv.ID ascending
                                        select new
                                        {
                                          sv.ID,
                                          sv.ServiceViewUID,
                                          cr.CustomerUID,
                                          at.AssetUID,
                                          de.DeviceUID,
                                          SubscriptionType = st.Name,
                                          sv.StartKeyDate,
                                          sv.EndKeyDate,
                                          de.OwnerBSSID,
                                          UpdateUTC = currentUtc
                                        }).Take(BatchSize).ToList();

            if (subscriptionDataList.Count < 1)
            {
              Log.IfInfo(string.Format("No {0} data left for creation", _taskName));
              return BatchProcessorState.NoRecordsToProcess;
            }

            var requestMethod = HttpMethod.Post;
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
                return BatchProcessorState.RecordExists_DependentEventsNotProcessed;
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
                  StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                  EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                  ActionUTC = subscriptionData.UpdateUTC
                };
                var processFlag = ProcessRequest(createAssetSubscription, requestMethod, _assetSubscriptionApiEndPointUri);
                if (!processFlag)
                {
                  return BatchProcessorState.RecordsExists_FailedToProcess; // Record exists but request failed due to some error
                }
                else
                  lastProcessedId = subscriptionData.ID;
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
                  ActionUTC = subscriptionData.UpdateUTC
                };
                var processFlag = ProcessRequest(createProjectSubscription, requestMethod, _projectSubscriptionApiEndPointUri);
                if (!processFlag)
                {
                  return BatchProcessorState.RecordsExists_FailedToProcess;
                }
                else
                  lastProcessedId = subscriptionData.ID;
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
                  ActionUTC = subscriptionData.UpdateUTC
                };
                var processFlag = ProcessRequest(createCustomerSubscriptionEvent, requestMethod, _customerSubscriptionApiEndPointUri);
                if (!processFlag)
                {
                  return BatchProcessorState.RecordsExists_FailedToProcess;
                }
                else
                  lastProcessedId = subscriptionData.ID;
              }
            }
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
      return BatchProcessorState.MoreRecordsToProcess;
    }

    private BatchProcessorState ProcessUpdationRecords(long? lastProcessedId, DateTime? lastUpdateUtc, ref bool isServiceStopped)
    {
      Log.IfInfo(string.Format("Started Processing UpdateSubscriptionEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
      int index = 0;
      List<UpdateSubscriptionEvent> subscriptionDataList;
      bool canInsertionCont_AfterUpdate = true;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
          subscriptionDataList = (from sv in opCtx.ServiceViewReadOnly
                                      join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                      join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                      where (_subscriptionTypes == "ALL" || SubscriptionTypesToBeIncluded.Contains(st.Name))
                                      join c in opCtx.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                                      join a in opCtx.AssetReadOnly on sv.fk_AssetID equals a.AssetID
                                      join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                                      where !CustomersToBeExcluded.Contains(c.Name) && CustomerTypesToBeIncluded.Contains(c.fk_CustomerTypeID) && sv.ID <= lastProcessedId && sv.UpdateUTC <= currentUtc && sv.UpdateUTC > lastUpdateUtc
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
                                        StartKeyDate = sv.StartKeyDate,
                                        EndKeyDate = sv.EndKeyDate,
                                        UpdateUTC = sv.UpdateUTC
                                      }).Take(BatchSize).ToList();
          var subscriptionDataListSize = subscriptionDataList.Count();
          var firstUpdateUtc = subscriptionDataListSize > 0 ? subscriptionDataList[0].UpdateUTC : default(DateTime);
          if (subscriptionDataList.Count < 1)
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Updation", currentUtc, _taskName));
            return BatchProcessorState.NoRecordsToProcess;
          }
          else if (subscriptionDataListSize == BatchSize)
          {
            //If the first record's update utc matches with the last record's updateutc, then select all the records having same update utc and union it
            if (DateTime.Compare(subscriptionDataList[0].UpdateUTC, subscriptionDataList[subscriptionDataListSize - 1].UpdateUTC) == 0)
            {
              var newSubscriptionList = (from sv in opCtx.ServiceViewReadOnly
                                         join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                         join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                         join c in opCtx.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                                         join a in opCtx.AssetReadOnly on sv.fk_AssetID equals a.AssetID
                                         join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                                         where !CustomersToBeExcluded.Contains(c.Name) && CustomerTypesToBeIncluded.Contains(c.fk_CustomerTypeID) && sv.ID <= lastProcessedId && sv.UpdateUTC == firstUpdateUtc && sv.IsVirtual == false
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
                                           StartKeyDate = sv.StartKeyDate,
                                           EndKeyDate = sv.EndKeyDate,
                                           UpdateUTC = sv.UpdateUTC
                                         }).Distinct().ToList();

              subscriptionDataList = newSubscriptionList;
            }
            else
            {
              var curr = subscriptionDataListSize - 1;
              var prev = curr - 1;
              while (prev >= 1 && subscriptionDataList[prev].UpdateUTC == subscriptionDataList[curr].UpdateUTC)
              {
                curr--;
                prev--;
              }
              subscriptionDataList.RemoveRange(curr, subscriptionDataListSize - curr);
            }
            canInsertionCont_AfterUpdate = false;
          }

          var requestMethod = HttpMethod.Put;
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
                StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                ActionUTC = subscriptionData.UpdateUTC
              };
              var processFlag = ProcessRequest(updateAssetSubscription, requestMethod, _assetSubscriptionApiEndPointUri);
              if (!processFlag)
              {
                Log.IfError("Failure in processing your request");
                return BatchProcessorState.RecordsExists_FailedToProcess; // Record exists but request failed due to some error
              }
              else if (index < subscriptionDataList.Count && DateTime.Compare(subscriptionDataList[index].UpdateUTC, subscriptionDataList[index - 1].UpdateUTC) == 0)
              {
                Log.IfInfo("Same Update UTC found hence will not update BookMark until all the updateUtc is processed : " + subscriptionDataList[index].UpdateUTC);
              }
              else
              {
                lastUpdateUtc = subscriptionData.UpdateUTC;
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
                ActionUTC = subscriptionData.UpdateUTC
              };
              var processFlag = ProcessRequest(updateProjectSubscription, requestMethod, _projectSubscriptionApiEndPointUri);
              if (!processFlag)
              {
                Log.IfError("Failure in processing your request");
                return BatchProcessorState.RecordsExists_FailedToProcess; // Record exists but request failed due to some error
              }
              else if (index < subscriptionDataList.Count && DateTime.Compare(subscriptionDataList[index].UpdateUTC, subscriptionDataList[index - 1].UpdateUTC) == 0)
              {
                Log.IfInfo("Same Update UTC found hence will not update BookMark until all the updateUtc is processed : " + subscriptionDataList[index].UpdateUTC);
              }
              else
              {
                lastUpdateUtc = subscriptionData.UpdateUTC;
              }
            }
            else if (customerSubscriptionServicePlans.Contains(subscriptionData.SubscriptionType))
            {
              var updateCustomerSubscriptionEvent = new UpdateCustomerSubscriptionEvent
              {
                SubscriptionUID = (Guid)subscriptionData.ServiceViewUID,
                StartDate = subscriptionData.StartKeyDate.FromKeyDate().StartOfDay(),
                EndDate = subscriptionData.EndKeyDate.FromKeyDate().AddDays(-1).EndOfDay().AddDays(1),
                ActionUTC = subscriptionData.UpdateUTC
              };
              var processFlag = ProcessRequest(updateCustomerSubscriptionEvent, requestMethod, _customerSubscriptionApiEndPointUri);
              if (!processFlag)
              {
                Log.IfError("Failure in processing your request");
                return BatchProcessorState.RecordsExists_FailedToProcess; // Record exists but request failed due to some error
              }
              else if (index < subscriptionDataList.Count && DateTime.Compare(subscriptionDataList[index].UpdateUTC, subscriptionDataList[index - 1].UpdateUTC) == 0)
              {
                Log.IfInfo("Same Update UTC found hence will not update BookMark until all the updateUtc is processed : " + subscriptionDataList[index].UpdateUTC);
              }
              else
              {
                lastUpdateUtc = subscriptionData.UpdateUTC;
              }

            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} updation {1} \n {2}", _taskName, e.Message, e.StackTrace));
          canInsertionCont_AfterUpdate = false;
        }
        finally
        {
          //Update the last read utc to masterdatasync
          opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = lastUpdateUtc;
          opCtx.SaveChanges();
          Log.IfInfo(string.Format("Completed Processing UpdateSubscriptionEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
        }
      }
      return (canInsertionCont_AfterUpdate) ? BatchProcessorState.AllRecordsProcessedSuccessfully : BatchProcessorState.MoreRecordsToProcess;
    }

    private bool ProcessRequest<T>(T subscriptionRequestData, HttpMethod requestMethod, Uri requestUri)
    {
      var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

      if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
      {
        return false;
      }

      var svcResponse = ProcessServiceRequestAndResponse(subscriptionRequestData, _httpRequestWrapper,
                       requestUri, requestHeader, requestMethod);

      switch (svcResponse.StatusCode)
      {
        case HttpStatusCode.OK:
          return true;
        case HttpStatusCode.Unauthorized:
          requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
          svcResponse = ProcessServiceRequestAndResponse(subscriptionRequestData, _httpRequestWrapper, requestUri, requestHeader, requestMethod);
          if (svcResponse.StatusCode == HttpStatusCode.OK)
          {
            return true;
          }
          break;
        case HttpStatusCode.InternalServerError:
          Log.IfError("Internal server error");
          break;
        case HttpStatusCode.BadRequest:
          Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(subscriptionRequestData));
          return true;
        case HttpStatusCode.Forbidden:
          Log.IfError("Forbidden status code received while hitting Tpaas Subscription service");
          break;
        default:
          Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(subscriptionRequestData)));
          break;
      }
      return false;
    }

    public class UpdateSubscriptionEvent
    {
      public long ID { get; set; }
      public Guid? ServiceViewUID { get; set; }
      public Guid? CustomerUID { get; set; }
      public Guid? AssetUID { get; set; }
      public Guid? DeviceUID { get; set; }
      public string OwnerBSSID { get; set; }
      public string SubscriptionType { get; set; }
      public int StartKeyDate { get; set; }
      public int EndKeyDate { get; set; }
      public DateTime UpdateUTC { get; set; }
    }
  }
}
