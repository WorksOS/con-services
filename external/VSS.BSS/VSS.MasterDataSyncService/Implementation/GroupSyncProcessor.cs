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

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class GroupSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly List<string> CustomersToBeExcluded = ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"] != null ? ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"].Split('$').Select(s => s.Trim()).ToList() : new List<string>();
    private readonly Uri _groupApiEndPointUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;


    public GroupSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("GroupService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Group api URL value cannot be empty");

      _groupApiEndPointUri = new Uri(_configurationManager.GetAppSetting("GroupService.WebAPIURI"));
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
      var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
      var saveLastInsertedUtcFlag = GetLastInsertUTC(_taskName) == default(DateTime).AddYears(1900);
      var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, saveLastUpdateUtcFlag, saveLastInsertedUtcFlag, ref isServiceStopped);

      #region Temporarily Disabled
      //MasterData Updation
      // lastProcessedId = GetLastProcessedId(_taskName);
      // var lastUpdateUtc = GetLastUpdateUTC(_taskName);
      //  var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, ref isServiceStopped);
      #endregion

      //MasterData Migrated Users
      //lastProcessedId = GetLastProcessedId(_taskName);
      var lastMigratedUtc = GetLastInsertUTC(_taskName);
      var isMigratedEventProcessed = ProcessMigratedRecords(lastProcessedId, lastMigratedUtc, ref isServiceStopped);

      return (isCreateEventProcessed || isMigratedEventProcessed);
    }

    private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, bool saveLastInsertedUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;
          Log.IfInfo(string.Format("Started Processing CreateGroupEvent. LastProcessedId : {0}", lastProcessedId));

          var tasksProcessedState = opCtx.MasterDataSyncReadOnly.Where(
            e => (e.TaskName == StringConstants.CustomerTask) || (e.TaskName == StringConstants.AssetTask))
            .Select(
              e =>
                new TaskState
                {
                  TaskName = e.TaskName,
                  lastProcessedId = e.LastProcessedID ?? Int32.MinValue,
                  InsertUtc = e.LastInsertedUTC
                })
            .ToList();


          var assetTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.AssetTask);
          var customerTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.CustomerTask);

          if (assetTaskState != null && customerTaskState != null)
          {
            assetTaskState.InsertUtc = assetTaskState.InsertUtc ?? default(DateTime).AddYears(1900);
            
            var groupDataList = (from ag in opCtx.AssetGroupReadOnly
                                 join c in opCtx.CustomerReadOnly.Where(e => e.ID <= customerTaskState.lastProcessedId) on ag.fk_CustomerID equals c.ID into customer
                                 join u in opCtx.UserReadOnly on ag.fk_UserID equals u.ID
                                 let assetsList = (from aga in opCtx.AssetGroupAssetReadOnly
                                                   join a in opCtx.AssetReadOnly.Where(e => e.InsertUTC < assetTaskState.InsertUtc || (e.InsertUTC == assetTaskState.InsertUtc && e.AssetID <= assetTaskState.lastProcessedId))
                                                     .OrderBy(e => e.InsertUTC).ThenBy(e => e.AssetID)
                                                     on aga.fk_AssetID equals a.AssetID into assetSubset
                                                   where aga.fk_AssetGroupID == ag.ID
                                                   from asset in assetSubset.DefaultIfEmpty()
                                                   select asset.AssetUID).ToList()
                                 where ag.ID > lastProcessedId && ag.UpdateUTC <= currentUtc && u.IdentityMigrationUTC <= currentUtc && u.Active && u.UserUID != null
                                 from ct in customer.DefaultIfEmpty()
                                 orderby ag.ID ascending

                                 select new
                                 {
                                   ag.ID,
                                   ag.Name,
                                   CustomerId = (long?)ct.ID,
                                   ct.CustomerUID,
                                   u.UserUID,
                                   AssetsList = assetsList,
                                   GroupUID = ag.AssetGroupUID,
                                   UpdateUTC = currentUtc
                                 }).Take(BatchSize).ToList();

            if (groupDataList.Count < 1)
            {
              Log.IfInfo(string.Format("No {0} data left for creation", _taskName));
              return false;
            }
            var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

            if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
            {
              return true;
            }

            foreach (var groupData in groupDataList)
            {
              if (isServiceStopped)
              {
                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                break;
              }

              if (groupData.CustomerUID == null || groupData.CustomerId == null || (groupData.AssetsList.Count > 0 && groupData.AssetsList.Any(e => !e.HasValue)))
              {
                Log.IfInfo("The required CustomerUID's or CustomerId's or AssetUID's CreateEvent has not been processed yet..");
                return true;
              }

              if (groupData.UserUID == null)
              {
                Log.IfInfo(string.Format("Skipping the record {0} as the UserUID value for this record is null ..", groupData.ID));
                lastProcessedId = groupData.ID;
                continue;
              }

              var createGroup = new CreateGroupEvent
              {
                GroupUID = groupData.GroupUID,
                GroupName = groupData.Name,
                CustomerUID = (Guid)groupData.CustomerUID,
                UserUID = Guid.Parse(groupData.UserUID),
                AssetUID = groupData.AssetsList,
                ActionUTC = groupData.UpdateUTC
              };

              var svcResponse = ProcessServiceRequestAndResponse(createGroup, _httpRequestWrapper,
                _groupApiEndPointUri, requestHeader, HttpMethod.Post);
							Log.IfInfo("Create Group: " + groupData.ID + " returned " + svcResponse.StatusCode);
              switch (svcResponse.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastProcessedId = groupData.ID;
                  break;
                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                  svcResponse = ProcessServiceRequestAndResponse(createGroup, _httpRequestWrapper, _groupApiEndPointUri, requestHeader, HttpMethod.Post);
                  if (svcResponse.StatusCode == HttpStatusCode.OK)
                  {
                    lastProcessedId = groupData.ID;
                  }
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return true;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createGroup));
                  lastProcessedId = groupData.ID;
                  break;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas group service");
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload = {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(createGroup)));
                  return true;
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
          //Saving last update utc if it is not set
          if (saveLastUpdateUtcFlag)
          {
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = currentUtc;
            opCtx.SaveChanges();
          }
          //Saving last inserted utc if it is not set
          if (saveLastInsertedUtcFlag)
          {
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = currentUtc;
            opCtx.SaveChanges();
          }
          if (lastProcessedId != Int32.MinValue)
          {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
            opCtx.SaveChanges();
            Log.IfInfo(string.Format("Completed Processing CreateGroupEvent. LastProcessedId : {0} ", lastProcessedId)); 
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
      Log.IfInfo(string.Format("Started Processing UpdateGroupEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
          var groupDataList = (from ag in opCtx.AssetGroupReadOnly
                               join u in opCtx.UserReadOnly on ag.fk_UserID equals u.ID
                               where ag.ID <= lastProcessedId && ag.UpdateUTC <= currentUtc && ag.UpdateUTC > lastUpdateUtc
                               orderby ag.UpdateUTC
                               select new
                               {
                                 ag.ID,
                                 ag.Name,
                                 u.UserUID,
                                 GroupUID = ag.AssetGroupUID,
                                 ag.UpdateUTC
                               }).Take(BatchSize).ToList();


          if (groupDataList.Count < 1)
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Migration", currentUtc, _taskName));
            return false;
          }
          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var groupData in groupDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }
            #region Group update Temporarily Disabled
            //var updateGroup = new UpdateGroupEvent
            //{
            //  GroupUID = groupData.GroupUID,
            //  GroupName = groupData.Name,
            //  UserUID = (groupData.UserUID != null && groupData.UserUID.Trim() != string.Empty) ? Guid.Parse(groupData.UserUID) : Guid.Empty,
            //  AssociatedAssetUID = (from aga in opCtx.AssetGroupAssetReadOnly
            //                        join a in opCtx.AssetReadOnly on aga.fk_AssetID equals a.AssetID
            //                        where aga.fk_AssetGroupID == groupData.ID
            //                        select (Guid)a.AssetUID)
            //                       .ToList(),
            //  ActionUTC = groupData.UpdateUTC
            //};

            //var svcResponse = ProcessServiceRequestAndResponse(updateGroup, _httpRequestWrapper, GroupApiEndPointUri, requestHeader, HttpMethod.Put);

            //switch (svcResponse.StatusCode)
            //{
            //  case HttpStatusCode.OK:
            //    lastUpdateUtc = groupData.UpdateUTC;
            //    break;

            //  case HttpStatusCode.Unauthorized:
            //      requestHeader = GetRequestHeaderOnAuthenticationType(isOuthRetryCall: true);

            //      svcResponse = ProcessServiceRequestAndResponse(updateGroup, _httpRequestWrapper, GroupApiEndPointUri, requestHeader, HttpMethod.Put);
            //      if (svcResponse.StatusCode == HttpStatusCode.OK)
            //      {
            //        lastUpdateUtc = groupData.UpdateUTC;
            //      }
            //    break;
            //case HttpStatusCode.InternalServerError:
            //    Log.IfError("Internal server error");
            //    return true;
            //  case HttpStatusCode.BadRequest:
            //    Log.IfError("Error in payload "+ updateGroup);
            //    break;
            //}
            #endregion
            lastUpdateUtc = groupData.UpdateUTC;
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
          Log.IfInfo(string.Format("Completed Processing UpdateGroupEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
        }
      }
      return true;
    }

    private bool ProcessMigratedRecords(long? lastProcessedId, DateTime? lastMigratedUtc, ref bool isServiceStopped)
    {
      Log.IfInfo(string.Format("Started Processing MigratedGroupEvent. LastProcessedId : {0} , LastMigratedUTC : {1}", lastProcessedId, lastMigratedUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
          var groupDataList = (from ag in opCtx.AssetGroupReadOnly
                               join u in opCtx.UserReadOnly on ag.fk_UserID equals u.ID
                               join c in opCtx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                               let assetsList = (from aga in opCtx.AssetGroupAssetReadOnly
                                                 join a in opCtx.AssetReadOnly
                                                 on aga.fk_AssetID equals a.AssetID
                                                 where aga.fk_AssetGroupID == ag.ID
                                                 select a.AssetUID).ToList()
                               where u.Active && u.UserUID != null && ag.ID <= lastProcessedId && u.IdentityMigrationUTC <= currentUtc && u.IdentityMigrationUTC > lastMigratedUtc
                               orderby u.IdentityMigrationUTC
                               select new
                               {
                                 ag.ID,
                                 ag.Name,
                                 CustomerName = c.Name,
                                 CustomerId = (long?)c.ID,
                                 c.CustomerUID,
                                 u.UserUID,
                                 AssetsList = assetsList,
                                 GroupUID = ag.AssetGroupUID,
                                 MigratedUTC = u.IdentityMigrationUTC,
                                 UpdateUtc = currentUtc,
                               }).Take(BatchSize).ToList();


          if (groupDataList.Count < 1)
          {
            lastMigratedUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Migration", currentUtc, _taskName));
            return false;
          }
          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var groupData in groupDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            var createGroup = new CreateGroupEvent
            {
              GroupUID = groupData.GroupUID,
              GroupName = groupData.Name,
              CustomerUID = (Guid)groupData.CustomerUID,
              UserUID = Guid.Parse(groupData.UserUID),
              AssetUID = groupData.AssetsList,
              ActionUTC = groupData.UpdateUtc
            };

            var svcResponse = ProcessServiceRequestAndResponse(createGroup, _httpRequestWrapper, _groupApiEndPointUri, requestHeader, HttpMethod.Post);

            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastMigratedUtc = groupData.MigratedUTC;
                break;

              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);

                svcResponse = ProcessServiceRequestAndResponse(createGroup, _httpRequestWrapper, _groupApiEndPointUri, requestHeader, HttpMethod.Post);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastMigratedUtc = groupData.MigratedUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createGroup));
                lastMigratedUtc = groupData.MigratedUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas Group service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload = {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(createGroup)));
                break;
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
          opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = lastMigratedUtc;
          opCtx.SaveChanges();
          Log.IfInfo(string.Format("Completed Processing MigratedGroupEvent. LastMigratedUTC : {0}", lastMigratedUtc));
        }
      }
      return true;
    }
  }
}
