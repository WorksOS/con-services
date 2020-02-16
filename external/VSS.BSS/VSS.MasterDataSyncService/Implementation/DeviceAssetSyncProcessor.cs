using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class DeviceAssetSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;
    private readonly Uri AssociateDeviceAssetUri;

    public DeviceAssetSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager) 
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("DeviceService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Device api URL value cannot be empty");

      AssociateDeviceAssetUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI") + "/associatedeviceasset");
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
      //MasterData Assocation Event b/w Device and Asset
      var lastProcessedId = GetLastProcessedId(_taskName);
      var lastInsertUtc = GetLastInsertUTC(_taskName);
      var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
      var isAssociateEventProcessed = ProcessInsertionRecords(lastProcessedId, lastInsertUtc, saveLastUpdateUtcFlag, ref isServiceStopped);
      return isAssociateEventProcessed;
    }

    // Based on book mark value of this task process records that are greated than the insertutc in Asset table
    private bool ProcessInsertionRecords(long? lastProcessedId, DateTime? lastInsertUtc, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;

          Log.IfInfo(string.Format("Started Processing AssociateDeviceAssetEvent. LastProcessedId : {0} , LastInsertedUTC : {1}", lastProcessedId,
            lastInsertUtc));

          var tasksProcessedState = opCtx.MasterDataSyncReadOnly.Where(
           e => (e.TaskName == StringConstants.DeviceTask) || (e.TaskName == StringConstants.AssetTask)).Select(
             e =>
               new TaskState
               {
                 TaskName = e.TaskName,
                 lastProcessedId = e.LastProcessedID,
                 InsertUtc = e.LastInsertedUTC
               })
           .ToList();


          var assetTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.AssetTask);
          var deviceTaskState = tasksProcessedState.FirstOrDefault(e => e.TaskName == StringConstants.DeviceTask);

          var associateEventDataList = (from a in opCtx.AssetReadOnly
                            join d in opCtx.DeviceReadOnly.Where(e => e.ID <= deviceTaskState.lastProcessedId) on a.fk_DeviceID equals d.ID into deviceSubset
                                        where ((a.InsertUTC == assetTaskState.InsertUtc && a.AssetID <= assetTaskState.lastProcessedId) || a.InsertUTC < assetTaskState.InsertUtc) 
                            && ((a.InsertUTC == lastInsertUtc && a.AssetID > lastProcessedId) || a.InsertUTC > lastInsertUtc)
                            from de in deviceSubset.DefaultIfEmpty()
                            orderby a.InsertUTC, a.AssetID
                            select new
                            {
                              a.AssetUID,
                              a.AssetID,
                              de.DeviceUID,
                              de.OwnerBSSID,
                              a.InsertUTC
                            }).Take(BatchSize).ToList();


          if (associateEventDataList.Count < 1)
          {
            Log.IfInfo(string.Format("No {0} data left for AssociateDeviceAsset Event", _taskName));
            return false;
          }

          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var associateEventData in associateEventDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            if (associateEventData.DeviceUID == null)
            {
              Log.IfInfo("The required DeviceUID's CreateEvent has not been processed yet..");
              return true;
            }

            var associateDeviceAssetEvent = new AssociateDeviceAssetEvent
            {
              DeviceUID = (Guid) associateEventData.DeviceUID,
              AssetUID = (Guid) associateEventData.AssetUID,
              ActionUTC = currentUtc
            };

            var svcResponseForDeviceAssetAssociation = ProcessServiceRequestAndResponse(associateDeviceAssetEvent,_httpRequestWrapper, AssociateDeviceAssetUri, requestHeader, HttpMethod.Post);
            Log.IfInfo("Associaate asset "+associateEventData.AssetUID+ " device " + associateEventData.DeviceUID + " returned " + svcResponseForDeviceAssetAssociation.StatusCode);
            switch (svcResponseForDeviceAssetAssociation.StatusCode)
            {
              case HttpStatusCode.OK:
                lastProcessedId = associateEventData.AssetID;
                lastInsertUtc = associateEventData.InsertUTC;
                break;
              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponseForDeviceAssetAssociation = ProcessServiceRequestAndResponse(associateDeviceAssetEvent,_httpRequestWrapper, AssociateDeviceAssetUri, requestHeader, HttpMethod.Post);
                
                if (svcResponseForDeviceAssetAssociation.StatusCode == HttpStatusCode.OK)
                {
                  lastProcessedId = associateEventData.AssetID;
                  lastInsertUtc = associateEventData.InsertUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(associateDeviceAssetEvent));
                lastProcessedId = associateEventData.AssetID;
                lastInsertUtc = associateEventData.InsertUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas Customer service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload  = {1} ", svcResponseForDeviceAssetAssociation.StatusCode, JsonHelper.SerializeObjectToJson(associateDeviceAssetEvent)));
                return true;
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} AssociateDeviceAsset Event {1} \n {2}", _taskName, e.Message,
            e.StackTrace));
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
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = lastInsertUtc;
            opCtx.SaveChanges();

            Log.IfInfo(
              string.Format("Completed Processing AssociateDeviceAssetEvent. LastProcessedId : {0} , LastInsertedUTC : {1}",
                lastProcessedId, lastInsertUtc)); 
          }
          else
          {
            Log.IfInfo("No Records Processed");
          }
        }
      }
      return true;
    }
  }
}
