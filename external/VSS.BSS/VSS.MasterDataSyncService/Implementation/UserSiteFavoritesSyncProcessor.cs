using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Nighthawk.MasterDataSync.Models;
using System.Configuration;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class UserSiteFavoritesSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Uri _userSiteFavoritesEndpoint;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;
    private readonly Uri DissociateDeviceAssetUri;

    public UserSiteFavoritesSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("GeofenceService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Geofence api URL value cannot be empty");

      _userSiteFavoritesEndpoint = new Uri(_configurationManager.GetAppSetting("GeofenceService.WebAPIURI") + "/markfavorite");
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
          lastProcessedId = lastProcessedId ?? 0;
          Log.IfInfo(string.Format("Started Processing MarkFavoritesEvent. LastProcessedId : {0}", lastProcessedId));

          var tasksProcessedState = opCtx.MasterDataSyncReadOnly.Where(
           e => (e.TaskName == StringConstants.CustomerTask) || (e.TaskName == StringConstants.GeofenceTask))
           .Select(
             e => new TaskState { TaskName = e.TaskName, lastProcessedId = e.LastProcessedID ?? Int32.MinValue, InsertUtc = e.LastInsertedUTC }).ToList();

          var geofenceTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.GeofenceTask);
          var customerTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.CustomerTask);

          if (geofenceTaskState != null && customerTaskState!= null)
          {
            geofenceTaskState.InsertUtc = geofenceTaskState.InsertUtc ?? default(DateTime).AddYears(1900);
            
            var favoritesSitesData = (from s in opCtx.UserSiteFavoritesReadOnly
              join g in opCtx.SiteReadOnly.Where(e => e.ID <= geofenceTaskState.lastProcessedId) on s.fk_SiteID equals g.ID into geofenceSubset
              join u in opCtx.UserReadOnly on s.fk_UserID equals u.ID
              join c in opCtx.CustomerReadOnly.Where(e => e.ID <= customerTaskState.lastProcessedId) on u.fk_CustomerID equals c.ID into customerSubset
              where s.ID > lastProcessedId && u.IdentityMigrationUTC <= currentUtc && u.Active && u.UserUID != null
              from gt in geofenceSubset.DefaultIfEmpty()
              from ct in customerSubset.DefaultIfEmpty()
              where gt.Visible
              orderby s.ID ascending

              select new
              {
                s.ID,
                gt.SiteUID,
                u.UserUID,
                CustomerId = (long?)ct.ID,
                ct.CustomerUID,
              }).Take(BatchSize).ToList();

            if (!favoritesSitesData.Any())
            {
              Log.IfInfo(string.Format("No {0} data left to mark favorite", _taskName));
              return false;
            }

            var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

            if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
            {
              return true;
            }

            foreach (var favoriteSite in favoritesSitesData)
            {
              if (isServiceStopped)
              {
                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                break;
              }

              if (favoriteSite.CustomerUID == null || favoriteSite.SiteUID == null || favoriteSite.CustomerId == null)
              {
                Log.IfInfo("The required CustomerUID/SiteUID/CustomerId's CreateEvent has not been processed yet..");
                return true;
              }

              if (favoriteSite.UserUID == null)
              {
                Log.IfInfo(string.Format("Skipping the record {0} as the UserUID value for this record is null ..", favoriteSite.ID));
                lastProcessedId = favoriteSite.ID;
                continue;
              }

              var favoriteSiteUser = Guid.Parse(favoriteSite.UserUID);
              var favoriteSiteCustomer = (Guid)favoriteSite.CustomerUID;
              var siteUIDMarkedFavorite = (Guid)favoriteSite.SiteUID;

              var requestUri = new Uri(_userSiteFavoritesEndpoint + "?useruid=" + favoriteSiteUser + "&customeruid=" +
                                       favoriteSiteCustomer + "&geofenceuid=" + siteUIDMarkedFavorite + "&actionutc=" +
                                       DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss"));


              var serviceRequestMessage = new ServiceRequestMessage
              {
                RequestHeaders = requestHeader,
                RequestMethod = HttpMethod.Put,
                RequestUrl = requestUri
              };

              var svcResponse = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);

              switch (svcResponse.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastProcessedId = favoriteSite.ID;
                  break;
                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                  serviceRequestMessage.RequestHeaders = requestHeader;
                  svcResponse = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);

                  if (svcResponse.StatusCode == HttpStatusCode.OK)
                  {
                    lastProcessedId = favoriteSite.ID;
                  }
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return true;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in request " + requestUri);
                  lastProcessedId = favoriteSite.ID;
                  break;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas Geofence service");
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. Request uri = {1}", svcResponse.StatusCode, requestUri));
                  return true;
              }
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} MarkFavoritesEvent {1} \n {2}", _taskName, e.Message, e.StackTrace));
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
            Log.IfInfo(string.Format("Completed Processing MarkFavoritesEvent. LastProcessedId : {0} ", lastProcessedId)); 
          }
          else
          {
            Log.IfInfo(string.Format("No Records Processed "));
          }
        }
      }
      return true;
    }

    private bool ProcessMigratedRecords(long? lastProcessedId, DateTime? lastMigratedUtc, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? 0;
          Log.IfInfo(string.Format("Started Processing MarkFavoritesEvent. LastProcessedId : {0}", lastProcessedId));

          var favoritesSitesData = (from s in opCtx.UserSiteFavoritesReadOnly
                                    join g in opCtx.SiteReadOnly on s.fk_SiteID equals g.ID
                                    join u in opCtx.UserReadOnly on s.fk_UserID equals u.ID
                                    join c in opCtx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                                    where s.ID <= lastProcessedId && u.IdentityMigrationUTC <= currentUtc && u.IdentityMigrationUTC > lastMigratedUtc && u.Active && u.UserUID != null && g.Visible
                                    orderby u.IdentityMigrationUTC ascending

                                    select new
                                    {
                                      s.ID,
                                      g.SiteUID,
                                      u.UserUID,
                                      CustomerId = (long?)c.ID,
                                      c.CustomerUID,
                                      MigratedUTC = u.IdentityMigrationUTC
                                    }).Take(BatchSize).ToList();

          if (!favoritesSitesData.Any())
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

          foreach (var favoriteSite in favoritesSitesData)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            var favoriteSiteUser = Guid.Parse(favoriteSite.UserUID);
            var favoriteSiteCustomer = (Guid)favoriteSite.CustomerUID;
            var siteUIDMarkedFavorite = (Guid)favoriteSite.SiteUID;

            var requestUri = new Uri(_userSiteFavoritesEndpoint + "?useruid=" + favoriteSiteUser + "&customeruid=" +
                                     favoriteSiteCustomer + "&geofenceuid=" + siteUIDMarkedFavorite + "&actionutc=" +
                                     DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss"));


            var serviceRequestMessage = new ServiceRequestMessage
            {
              RequestHeaders = requestHeader,
              RequestMethod = HttpMethod.Put,
              RequestUrl = requestUri
            };

            var svcResponse = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);

            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastMigratedUtc = favoriteSite.MigratedUTC;
                break;
              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                serviceRequestMessage.RequestHeaders = requestHeader;
                svcResponse = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);

                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastMigratedUtc = favoriteSite.MigratedUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in request " + requestUri);
                lastMigratedUtc = favoriteSite.MigratedUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas Geofence service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Request uri = {1}", svcResponse.StatusCode, requestUri));
                return true;
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} Migrated MarkFavoritesEvent {1} \n {2}", _taskName, e.Message, e.StackTrace));
        }
        finally
        {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = lastMigratedUtc;
            opCtx.SaveChanges();
            Log.IfInfo(string.Format("Completed Processing Migrated MarkFavoritesEvent. LastMigratedUtc : {0} ", lastMigratedUtc));
        }
      }
      return true;
    }
  }
}
