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

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class CustomerUserSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Uri _associateCustomerUserUri;
    private readonly Uri _dissociateCustomerUserUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;

    public CustomerUserSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("CustomerService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Customer api URL value cannot be empty");

      _associateCustomerUserUri = new Uri(_configurationManager.GetAppSetting("CustomerService.WebAPIURI") + "/associatecustomeruser");
      _dissociateCustomerUserUri = new Uri(_configurationManager.GetAppSetting("CustomerService.WebAPIURI") + "/dissociatecustomeruser");
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
      //MasterData Insertion
      var lastProcessedId = GetLastProcessedId(_taskName);
      var lastMigrationUTC = GetLastMigrationUTC(_taskName);
      var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
      var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, lastMigrationUTC, saveLastUpdateUtcFlag, ref isServiceStopped);


      //MasterData Updation
      var lastUpdateUtc = GetLastUpdateUTC(_taskName);
      var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastMigrationUTC, lastUpdateUtc, ref isServiceStopped);
      return (isCreateEventProcessed || isUpdateEventProcessed);
    }

    private DateTime? GetLastMigrationUTC(string _taskName)
    {
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var lastUpdateUtcData = opCtx.MasterDataSyncReadOnly.SingleOrDefault(t => t.TaskName == _taskName);
        if (lastUpdateUtcData != null)
          return lastUpdateUtcData.LastInsertedUTC ?? default(DateTime).AddYears(1900);
      }
      return null;
    }

    private bool ProcessInsertionRecords(long? lastProcessedId, DateTime? lastMigrationUTC, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;

      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? int.MinValue;
          Log.IfInfo(string.Format("Started Processing CustomerUserEvent. LastMigrationUTC : {0} LastProcessedId : {1}", lastMigrationUTC, lastProcessedId));

          TaskState customerTaskState = (from m in opCtx.MasterDataSyncReadOnly
                                         where m.TaskName == StringConstants.CustomerTask
                                         select new TaskState() { lastProcessedId = m.LastProcessedID ?? Int32.MinValue, InsertUtc = m.LastInsertedUTC }).FirstOrDefault();

          if (customerTaskState != null)
          {
            var customerUserList = (from u in opCtx.UserReadOnly
                                    join c in opCtx.CustomerReadOnly.Where(e => e.ID <= customerTaskState.lastProcessedId) on u.fk_CustomerID equals c.ID into customerSubset
                                    where (u.IdentityMigrationUTC > lastMigrationUTC || (u.IdentityMigrationUTC == lastMigrationUTC && u.ID > lastProcessedId)) && u.UpdateUTC <= currentUtc && u.Active && u.UserUID != null
                                    from ct in customerSubset.DefaultIfEmpty()
                                    orderby u.IdentityMigrationUTC, u.ID
                                    select new
                                    {
                                      u.ID,
                                      u.IdentityMigrationUTC,
                                      u.UserUID,
                                      ct.CustomerUID,
                                      u.Active,
                                      u.fk_CustomerID,
                                      UpdateUTC = currentUtc
                                    }).Take(BatchSize).ToList();

            if (customerUserList.Count < 1)
            {
              Log.IfInfo(string.Format("No {0} data left for association", _taskName));
              return false;
            }

            var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

            if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
            {
              return true;
            }

            foreach (var customerUserData in customerUserList)
            {
              if (isServiceStopped)
              {
                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                break;
              }

              if (customerUserData.CustomerUID == null || customerUserData.fk_CustomerID == null)
              {
                Log.IfInfo("The required CustomerUID's CreateEvent has not been processed yet..");
                return true;
              }

              var associateCustomerUserEvent = new AssociateCustomerUserEvent
              {
                UserUID = Guid.Parse(customerUserData.UserUID),
                CustomerUID = (Guid)customerUserData.CustomerUID,
                ActionUTC = customerUserData.UpdateUTC
              };

              var svcResponse = ProcessServiceRequestAndResponse(associateCustomerUserEvent, _httpRequestWrapper,
                _associateCustomerUserUri, requestHeader, HttpMethod.Post);
							Log.IfInfo("Associate CU C: " + customerUserData.CustomerUID + "U: " + customerUserData.UserUID + " returned " + svcResponse.StatusCode);
              switch (svcResponse.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastMigrationUTC = customerUserData.IdentityMigrationUTC;
                  lastProcessedId = customerUserData.ID;
                  break;
                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                  svcResponse = ProcessServiceRequestAndResponse(associateCustomerUserEvent, _httpRequestWrapper, _associateCustomerUserUri, requestHeader, HttpMethod.Post);
                  if (svcResponse.StatusCode == HttpStatusCode.OK)
                  {
                    lastMigrationUTC = customerUserData.IdentityMigrationUTC;
                    lastProcessedId = customerUserData.ID;
                  }
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return true;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(associateCustomerUserEvent));
                  lastMigrationUTC = customerUserData.IdentityMigrationUTC;
                  lastProcessedId = customerUserData.ID;
                  break;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas CustomerUser service");
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(associateCustomerUserEvent)));
                  return true;
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
          //Saving last update utc if it is not set
          if (saveLastUpdateUtcFlag)
          {
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = currentUtc;
            opCtx.SaveChanges();
          }
          if (lastMigrationUTC != default(DateTime).AddYears(1900))
          {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = lastMigrationUTC;
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
            opCtx.SaveChanges();
            Log.IfInfo(string.Format("Completed Processing AssociateCustomerEvent. LastMigrationUTC : {0} LastProcessedId : {1}", lastMigrationUTC, lastProcessedId));
          }
          else
          {
            Log.IfInfo(string.Format("No Records Processed "));
          }
        }
      }
      return true;
    }

    private bool ProcessUpdationRecords(long? lastProcessedId, DateTime? lastMigrationUTC, DateTime? lastUpdateUtc, ref bool isServiceStopped)
    {
      Log.IfInfo(string.Format("Started Processing DissociateCustomerUserEvent. LastProcessedId : {0} ,LastMigratedUTC {1}, LastUpdatedUTC : {2}", lastProcessedId, lastMigrationUTC,lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
          var customerDataList = (from u in opCtx.UserReadOnly
                                  join c in opCtx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                                  join au in opCtx.UserReadOnly.Where(e => e.Active && e.UserUID != null) on new { u.UserUID,u.fk_CustomerID} equals new { au.UserUID,au.fk_CustomerID} into activeUserSubset
                                  from aus in activeUserSubset.DefaultIfEmpty()
                                  where aus.UserUID == null && ((u.IdentityMigrationUTC == lastMigrationUTC && u.ID <= lastProcessedId) || u.IdentityMigrationUTC < lastMigrationUTC)
                                  && u.UpdateUTC <= currentUtc && u.UpdateUTC > lastUpdateUtc && !u.Active && u.UserUID != null
                                  orderby u.UpdateUTC, u.ID
                                  select new
                                  {
                                    u.ID,
                                    u.UserUID,
                                    c.CustomerUID,
                                    u.UpdateUTC
                                  }).Take(BatchSize).ToList();


          if (customerDataList.Count < 1)
          {
            Log.IfInfo(string.Format("No {0} data left for dissociation", _taskName));
            return false;
          }

          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var customerUserData in customerDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            var dissociateCustomerUserEvent = new DissociateCustomerUserEvent
            {
              UserUID = Guid.Parse(customerUserData.UserUID),
              CustomerUID = (Guid)customerUserData.CustomerUID,
              ActionUTC = customerUserData.UpdateUTC
            };


            var svcResponse = ProcessServiceRequestAndResponse(dissociateCustomerUserEvent, _httpRequestWrapper, _dissociateCustomerUserUri, requestHeader, HttpMethod.Post);
						Log.IfInfo("Dissociate CU C: " + customerUserData.CustomerUID + "U: " + customerUserData.UserUID + " returned " + svcResponse.StatusCode);
            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastUpdateUtc = customerUserData.UpdateUTC;
                break;
              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(dissociateCustomerUserEvent, _httpRequestWrapper, _dissociateCustomerUserUri, requestHeader, HttpMethod.Post);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastUpdateUtc = customerUserData.UpdateUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(dissociateCustomerUserEvent));
                lastUpdateUtc = customerUserData.UpdateUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(dissociateCustomerUserEvent)));
                return true;
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
          Log.IfInfo(string.Format("Completed Processing DissociateCustomerUserEvent. LastProcessedId : {0} , LastMigratedUTC : {1} ,LastUpdateUTC : {2}", lastProcessedId, lastMigrationUTC,lastUpdateUtc));
        }
      }
      return true;
    }
  }
}
