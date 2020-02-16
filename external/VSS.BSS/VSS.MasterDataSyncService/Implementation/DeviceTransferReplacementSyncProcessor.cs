using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class DeviceTransferReplacementSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Uri DeviceApiEndPointUri;
    private readonly Uri AssociateDeviceAssetUri;
    private readonly Uri DissociateDeviceAssetUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;

    public DeviceTransferReplacementSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("DeviceService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Device api URL value cannot be empty");

      DeviceApiEndPointUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI"));
      AssociateDeviceAssetUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI") + "/associatedeviceasset");
      DissociateDeviceAssetUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI") + "/dissociatedeviceasset");
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
      var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, saveLastUpdateUtcFlag, ref isServiceStopped);
      return (isCreateEventProcessed);
    }

    // Based on book mark value of this task process records that are greated than the insertutc in Asset table
    private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;
          Log.IfInfo(string.Format("Started Processing entries in AssetDeviceHistory LastProcessedId : {0}", lastProcessedId));

          var assetDeviceHistory = (from adh in opCtx.AssetDeviceHistoryReadOnly
                                join a in opCtx.AssetReadOnly on adh.fk_AssetID equals a.AssetID
                                join d in opCtx.DeviceReadOnly on adh.fk_DeviceID equals d.ID
                                where adh.ID > lastProcessedId
                                orderby adh.ID ascending
                                select new
                                {
                                  adh.ID,
                                  d.DeviceUID,
                                  a.AssetUID,
                                  a.fk_DeviceID,
                                  UpdateUTC = currentUtc
                                }).Take(BatchSize).ToList();

          if (assetDeviceHistory.Count < 1)
          {
            Log.IfInfo(string.Format("No {0} data left in AssetDeviceHistory", _taskName));
            return false;
          }
          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var item in assetDeviceHistory)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            var dissociateDeviceAsset = new DissociateDeviceAssetEvent()
            {
              AssetUID = (Guid)item.AssetUID,
              DeviceUID = (Guid)item.DeviceUID,
              ActionUTC = item.UpdateUTC
            };

            // All the entries in AssetDeviceHistory table will be considered as DissociateDeviceAsset
            var isDissociateEventSuccess = ProcessRequest(dissociateDeviceAsset, HttpMethod.Post,DissociateDeviceAssetUri);
						Log.IfInfo("Dissociate Asset: " + item.AssetUID + "device: " + item.DeviceUID + " returned " + isDissociateEventSuccess);
            if (!isDissociateEventSuccess)
              return false;
            else
            {
              if (item.fk_DeviceID != (long) DeviceTypeEnum.MANUALDEVICE)
              {
                //If the asset in assetdevicehistory has any valid device then it is assumed as devicereplacement/devicetransfer and sending associatedeviceasset value with new deviceid
                var newDeviceUid = (from a in opCtx.AssetReadOnly
                  join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                  where a.AssetUID == item.AssetUID
                  select new {d.DeviceUID}).FirstOrDefault();

                if (newDeviceUid != null && newDeviceUid.DeviceUID.HasValue)
                {
                  var associateDeviceAsset = new AssociateDeviceAssetEvent()
                  {
                    AssetUID = (Guid) item.AssetUID,
                    DeviceUID = newDeviceUid.DeviceUID.Value
                  };
                  bool isAssociateEventSuccess = ProcessRequest(associateDeviceAsset, HttpMethod.Post, AssociateDeviceAssetUri);
									Log.IfInfo("Associate Asset: " + item.AssetUID + "device: " + newDeviceUid.DeviceUID.Value + " returned " + isAssociateEventSuccess);
                  if (!isAssociateEventSuccess)
                    return false;
                }
                else
                  Log.ErrorFormat("Device UID doesn't exists for the asset {0}", item.AssetUID);
              }
              lastProcessedId = item.ID;
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} Records {1} \n {2}", _taskName, e.Message, e.StackTrace));
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
            Log.IfInfo(string.Format("Completed Processing entries in AssetDeviceHistory LastProcessedId : {0} ", lastProcessedId));
          }
          else
          {
						Log.IfInfo(string.Format("No Records Processed in AssetDeviceHistory"));
          }
        }
      }
      return true;
    }

    private bool ProcessRequest<T>(T deviceEvent, HttpMethod requestMethod, Uri requestUri)
    {
      var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

      if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
      {
        return false;
      }

      var svcResponse = ProcessServiceRequestAndResponse(deviceEvent, _httpRequestWrapper,
                       requestUri, requestHeader, requestMethod);

      switch (svcResponse.StatusCode)
      {
        case HttpStatusCode.OK:
          return true;
        case HttpStatusCode.Unauthorized:
          requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
          svcResponse = ProcessServiceRequestAndResponse(deviceEvent, _httpRequestWrapper, requestUri, requestHeader, requestMethod);
          if (svcResponse.StatusCode == HttpStatusCode.OK)
          {
            return true;
          }
					Log.IfWarn("Unauthorized");
          break;
        case HttpStatusCode.InternalServerError:
          Log.IfError("Internal server error");
          break;
        case HttpStatusCode.BadRequest:
          Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(deviceEvent));
          return true;
        case HttpStatusCode.Forbidden:
          Log.IfError("Forbidden status code received while hitting Tpaas Device service");
          break;
        default:
          Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(deviceEvent)));
          break;
      }
      return false;
    }
  }
}
