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
using VSS.Hosted.VLCommon.Services.MDM;
using System.Configuration;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class CustomerAssetSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    //private static readonly List<string> CustomersToBeExcluded = ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"] != null ? ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"].Split('$').Select(s => s.Trim()).ToList() : new List<string>();

    private static readonly List<int> CustomerTypesToBeIncluded = new List<int>() { (int)CustomerTypeEnum.Customer, (int)CustomerTypeEnum.Dealer };

    private readonly Uri _associateCustomerAssetUri;
    private readonly Uri _dissociateCustomerAssetUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;

    public CustomerAssetSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("CustomerService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Customer api URL value cannot be empty");

      _associateCustomerAssetUri = new Uri(_configurationManager.GetAppSetting("CustomerService.WebAPIURI") + "/associatecustomerasset");
      _dissociateCustomerAssetUri = new Uri(_configurationManager.GetAppSetting("CustomerService.WebAPIURI") + "/dissociatecustomerasset");
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
      var todaysKeyDate = currentUtc.KeyDate();

      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          Log.IfInfo(string.Format("Started Processing CustomerAssetEvent. LastProcessedId : {0}", lastProcessedId));

          var tasksProcessedState = opCtx.MasterDataSyncReadOnly.Where(
            e => (e.TaskName == StringConstants.CustomerTask) || (e.TaskName == StringConstants.AssetTask)).Select(
              e =>
                new TaskState
                {
                  TaskName = e.TaskName,
                  lastProcessedId = e.LastProcessedID ?? int.MinValue,
                  InsertUtc = e.LastInsertedUTC
                })
            .ToList();


          var assetTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.AssetTask);
          var customerTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.CustomerTask);

          if (assetTaskState != null && customerTaskState != null)
          {
            assetTaskState.InsertUtc = assetTaskState.InsertUtc ?? default(DateTime).AddYears(1900);

            var customerAssetList = (from sv in opCtx.ServiceViewReadOnly
                                     join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                     join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                     join c in opCtx.CustomerReadOnly.Where(c => (c.ID <= customerTaskState.lastProcessedId)) on sv.fk_CustomerID equals c.ID into customerSubset
                                     from cs in customerSubset.DefaultIfEmpty()
                                     where cs.Name == null || (CustomerTypesToBeIncluded.Contains(cs.fk_CustomerTypeID))
                                     join ct in opCtx.CustomerTypeReadOnly on cs.fk_CustomerTypeID equals ct.ID into customerTypeSubset
                                     join a in opCtx.AssetReadOnly.Where(e => e.InsertUTC < assetTaskState.InsertUtc || (e.InsertUTC == assetTaskState.InsertUtc && e.AssetID <= assetTaskState.lastProcessedId))
                                       .OrderBy(e => e.InsertUTC).ThenBy(e => e.AssetID) on sv.fk_AssetID equals a.AssetID into assetSubset
                                     where sv.ID > lastProcessedId && sv.UpdateUTC <= currentUtc && st.IsCore && sv.EndKeyDate > todaysKeyDate
                                     from cts in customerTypeSubset.DefaultIfEmpty()
                                     from at in assetSubset.DefaultIfEmpty()
                                     join d in opCtx.DeviceReadOnly on at.fk_DeviceID equals d.ID into deviceSubset
                                     from ds in deviceSubset.DefaultIfEmpty()
                                     orderby sv.ID ascending
                                     select new
                                     {
                                       sv.ID,
                                       sv.ifk_SharedViewID,
                                       cs.CustomerUID,
                                       CustomerName = cs.Name,
                                       ds.OwnerBSSID,
                                       at.AssetUID,
                                       AssetId = (long?)at.AssetID,
                                       cts.Name,
                                       UpdateUTC = currentUtc
                                     }).Take(BatchSize).ToList();

            if (customerAssetList.Count < 1)
            {
              Log.IfInfo(string.Format("No {0} data left for association", _taskName));
              return BatchProcessorState.NoRecordsToProcess;
            }

            var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

            if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
            {
              return BatchProcessorState.RecordsExists_FailedToProcess;
            }

            foreach (var customerAssetData in customerAssetList)
            {
              if (isServiceStopped)
              {
                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                break;
              }

              if (customerAssetData.AssetUID == null || customerAssetData.CustomerUID == null)
              {
                Log.IfInfo("The required CustomerUID/AssetUID's CreateEvent has not been processed yet..");
                return BatchProcessorState.RecordExists_DependentEventsNotProcessed;
              }

              string relationShipType = customerAssetData.Name;
              if (customerAssetData.ifk_SharedViewID != null)
              {
                relationShipType = "SharedOwner";
              }
              var associateCustomerAssetEvent = new AssociateCustomerAssetEvent
              {
                CustomerUID = (Guid)customerAssetData.CustomerUID,
                AssetUID = (Guid)customerAssetData.AssetUID,
                RelationType = relationShipType,
                ActionUTC = customerAssetData.UpdateUTC
              };

              var svcResponse = ProcessServiceRequestAndResponse(associateCustomerAssetEvent, _httpRequestWrapper, _associateCustomerAssetUri, requestHeader, HttpMethod.Post);
							Log.IfInfo("Associate Customer: " + customerAssetData.CustomerUID + " asset: " + customerAssetData.AssetUID + " returned " + svcResponse.StatusCode);

              switch (svcResponse.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastProcessedId = customerAssetData.ID;
                  break;
                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                  svcResponse = ProcessServiceRequestAndResponse(associateCustomerAssetEvent, _httpRequestWrapper, _associateCustomerAssetUri, requestHeader, HttpMethod.Post);
                  if (svcResponse.StatusCode == HttpStatusCode.OK)
                  {
                    lastProcessedId = customerAssetData.ID;
                  }
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return BatchProcessorState.RecordsExists_FailedToProcess;
								case HttpStatusCode.Conflict:
									Log.InfoFormat("Duplicate customer asset association asset {0} customer {1}", customerAssetData.AssetUID.GetValueOrDefault(), customerAssetData.CustomerUID.GetValueOrDefault());
									lastProcessedId = customerAssetData.ID;
		              break;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(associateCustomerAssetEvent));
                  lastProcessedId = customerAssetData.ID;
                  break;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas Customer service");
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(associateCustomerAssetEvent)));
                  return BatchProcessorState.RecordsExists_FailedToProcess;
              }
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} Association {1} \n {2}", _taskName, e.Message, e.StackTrace));
        }
        finally
        {
          if (lastProcessedId != int.MinValue)
          {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
            opCtx.SaveChanges();
            Log.IfInfo(string.Format("Completed Processing CustomerAssetEvent. LastProcessedId : {0} ", lastProcessedId));
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
      bool canInsertionCont_AfterUpdate = true;
      Log.IfInfo(string.Format("Started Processing UpdateCustomerAssetEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
          var todaysKeyDate = currentUtc.KeyDate();
          var customerAssetList = (from sv in opCtx.ServiceViewReadOnly
                                   join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                   join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                   join c in opCtx.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                                   join a in opCtx.AssetReadOnly on sv.fk_AssetID equals a.AssetID
                                   join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                                   where CustomerTypesToBeIncluded.Contains(c.fk_CustomerTypeID) &&
                                   sv.ID <= lastProcessedId && sv.UpdateUTC <= currentUtc && sv.UpdateUTC > lastUpdateUtc && st.IsCore && sv.EndKeyDate <= todaysKeyDate
                                   orderby sv.UpdateUTC, sv.ID
                                   select new
                                   {
                                     sv.ID,
                                     c.CustomerUID,
                                     a.AssetUID,
                                     sv.UpdateUTC,
                                     a.AssetID
                                   }).Take(BatchSize).ToList();

          var customerAssetListSize = customerAssetList.Count();
          if (customerAssetListSize < 1)
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Updation", currentUtc, _taskName));
            return BatchProcessorState.NoRecordsToProcess;
          }
          else if(customerAssetListSize == BatchSize)
          {
            //Remove the last record since it is unknown whether the next record following this is having the same utc
            customerAssetList.RemoveAt(--customerAssetListSize);
            //If the first record's update utc matches with the last record's updateutc, then select all the records having same update utc and union it
            if (DateTime.Compare(customerAssetList[0].UpdateUTC, customerAssetList[customerAssetListSize - 1].UpdateUTC) == 0)
            {
              var newCustomerAssetList = (from sv in opCtx.ServiceViewReadOnly
                                          join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                          join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                          join c in opCtx.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                                          join a in opCtx.AssetReadOnly on sv.fk_AssetID equals a.AssetID
                                          join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                                          where CustomerTypesToBeIncluded.Contains(c.fk_CustomerTypeID) &&
                                          sv.ID <= lastProcessedId && sv.UpdateUTC == customerAssetList[0].UpdateUTC && st.IsCore && sv.EndKeyDate <= todaysKeyDate
                                          orderby sv.UpdateUTC, sv.ID
                                          select new
                                          {
                                            sv.ID,
                                            c.CustomerUID,
                                            a.AssetUID,
                                            sv.UpdateUTC,
                                            a.AssetID
                                          }).Distinct().ToList();

              customerAssetList = newCustomerAssetList;
            }
            else
            {
              var curr = customerAssetListSize - 1;
              var prev = curr - 1;
              while (prev >= 1 && customerAssetList[prev].UpdateUTC == customerAssetList[curr].UpdateUTC)
              {
                curr--;
                prev--;
              }
              customerAssetList.RemoveRange(curr, customerAssetListSize - curr);
            }
            canInsertionCont_AfterUpdate = false;
          }       
          
          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return BatchProcessorState.RecordsExists_FailedToProcess;
          }

          foreach (var customerAssetData in customerAssetList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            var dissociateCustomerAssetEvent = new DissociateCustomerAssetEvent
            {
              CustomerUID = (Guid)customerAssetData.CustomerUID,
              AssetUID = (Guid)customerAssetData.AssetUID,
              ActionUTC = customerAssetData.UpdateUTC
            };

            var svcResponse = ProcessServiceRequestAndResponse(dissociateCustomerAssetEvent, _httpRequestWrapper, _dissociateCustomerAssetUri, requestHeader, HttpMethod.Post);
						Log.IfInfo("Dissociate Customer: " + customerAssetData.CustomerUID + " asset: " + customerAssetData.AssetUID + " returned " + svcResponse.StatusCode);
            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastUpdateUtc = customerAssetData.UpdateUTC;
                break;

              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(dissociateCustomerAssetEvent, _httpRequestWrapper, _dissociateCustomerAssetUri, requestHeader, HttpMethod.Post);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastUpdateUtc = customerAssetData.UpdateUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return BatchProcessorState.RecordsExists_FailedToProcess;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(dissociateCustomerAssetEvent));
                lastUpdateUtc = customerAssetData.UpdateUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1} ", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(dissociateCustomerAssetEvent)));
                return BatchProcessorState.RecordsExists_FailedToProcess;
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
          Log.IfInfo(string.Format("Completed Processing UpdateCustomerAssetEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
        }
      }
      return (canInsertionCont_AfterUpdate) ? BatchProcessorState.AllRecordsProcessedSuccessfully : BatchProcessorState.MoreRecordsToProcess;
    }
  }
}
