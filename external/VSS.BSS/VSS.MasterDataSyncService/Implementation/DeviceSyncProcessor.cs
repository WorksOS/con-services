using System;
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
using System.Collections.Generic;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class DeviceSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Uri DeviceApiEndPointUri;
    private readonly Uri AssociateDeviceAssetUri;
    private readonly Uri DissociateDeviceAssetUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;

    public DeviceSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
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

      //MasterData Updation
      //lastProcessedId = GetLastProcessedId(_taskName);
      var lastUpdateUtc = GetLastUpdateUTC(_taskName);
      var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, ref isServiceStopped);
      return (isCreateEventProcessed || isUpdateEventProcessed);
    }

    private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;
          Log.IfInfo(string.Format("Started Processing CreateDeviceEvent LastProcessedId : {0}", lastProcessedId));

          var deviceDataList = (from d in opCtx.DeviceReadOnly
                                join dt in opCtx.DeviceTypeReadOnly on d.fk_DeviceTypeID equals dt.ID
                                join ds in opCtx.DeviceStateReadOnly on d.fk_DeviceStateID equals ds.ID
                                where d.ID > lastProcessedId && d.UpdateUTC <= currentUtc
                                orderby d.ID ascending
                                select new
                                {
                                  d.ID,
                                  d.DeviceUID,
                                  d.GpsDeviceID,
                                  d.OwnerBSSID,
                                  DeviceType = dt.Name,
                                  DeviceState = ds.Description,
                                  d.DeregisteredUTC,
                                  UpdateUTC = currentUtc
                                }).Take(BatchSize).ToList();

          if (deviceDataList.Count < 1)
          {
            Log.IfInfo(string.Format("No {0} data left for creation", _taskName));
            return false;
          }
          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var deviceData in deviceDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            var createDevice = new CreateDeviceEvent
            {
              DeviceUID = (Guid)deviceData.DeviceUID,
              DeviceSerialNumber = deviceData.GpsDeviceID,
              DeviceType = deviceData.DeviceType,
              DeviceState = deviceData.DeviceState,
              DeregisteredUTC = deviceData.DeregisteredUTC,
              ModuleType = null,
              MainboardSoftwareVersion = null,
              RadioFirmwarePartNumber = null,
              GatewayFirmwarePartNumber = null,
              DataLinkType = null,
              ActionUTC = deviceData.UpdateUTC
            };


            var svcResponseForDeviceCreation = ProcessServiceRequestAndResponse(createDevice, _httpRequestWrapper, DeviceApiEndPointUri, requestHeader, HttpMethod.Post);
						Log.IfInfo("Create Device: " + deviceData.DeviceUID + " returned " + svcResponseForDeviceCreation.StatusCode);
            switch (svcResponseForDeviceCreation.StatusCode)
            {
              case HttpStatusCode.OK:
                lastProcessedId = deviceData.ID;
                break;
              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponseForDeviceCreation = ProcessServiceRequestAndResponse(createDevice, _httpRequestWrapper, DeviceApiEndPointUri, requestHeader, HttpMethod.Post);
                if (svcResponseForDeviceCreation.StatusCode == HttpStatusCode.OK)
                {
                  lastProcessedId = deviceData.ID;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createDevice));
                lastProcessedId = deviceData.ID;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas Device service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponseForDeviceCreation.StatusCode, JsonHelper.SerializeObjectToJson(createDevice)));
                return true;
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
          if (lastProcessedId != Int32.MinValue)
          {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
            opCtx.SaveChanges();
            Log.IfInfo(string.Format("Completed Processing CreateDeviceEvent LastProcessedId : {0} ", lastProcessedId)); 
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
      Log.IfInfo(string.Format("Started Processing UpdateDeviceEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
          var deviceDataList = (from d in opCtx.DeviceReadOnly
                                join dt in opCtx.DeviceTypeReadOnly on d.fk_DeviceTypeID equals dt.ID
                                join ds in opCtx.DeviceStateReadOnly on d.fk_DeviceStateID equals ds.ID
                                join customer in opCtx.CustomerReadOnly on d.OwnerBSSID equals customer.BSSID
                                join cr in opCtx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer) on customer.ID equals cr.fk_ClientCustomerID into customerRelationshipSubset
                                from crt in customerRelationshipSubset.DefaultIfEmpty()
                                join crc in opCtx.CustomerReadOnly on crt.fk_ParentCustomerID equals crc.ID into customerRelationshipCustomerSubset
                                from crct in customerRelationshipCustomerSubset.DefaultIfEmpty()
                                where d.ID <= lastProcessedId && d.UpdateUTC <= currentUtc && d.UpdateUTC > lastUpdateUtc
                                orderby d.UpdateUTC
                                select new
                                {
                                  d.ID,
                                  d.DeviceUID,
                                  d.GpsDeviceID,
                                  d.OwnerBSSID,
                                  DeviceType = dt.Name,
                                  OwningCustomerUID = customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Account ? crct.CustomerUID : customer.CustomerUID,
                                  DeviceState = ds.Description,
                                  d.DeregisteredUTC,
                                  d.UpdateUTC
                                }).Take(BatchSize).ToList();


          if (deviceDataList.Count < 1)
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Updation", currentUtc, _taskName));
            return false;
          }

          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var deviceData in deviceDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            var updateDevice = new UpdateDeviceEvent
                    {
                      DeviceUID = (Guid)deviceData.DeviceUID,
                      OwningCustomerUID = deviceData.OwningCustomerUID,
                      DeviceSerialNumber = deviceData.GpsDeviceID,
                      DeviceType = deviceData.DeviceType,
                      DeviceState = deviceData.DeviceState,
                      DeregisteredUTC = deviceData.DeregisteredUTC,
                      ModuleType = null,
                      MainboardSoftwareVersion = null,
                      RadioFirmwarePartNumber = null,
                      GatewayFirmwarePartNumber = null,
                      DataLinkType = null,
                      ActionUTC = deviceData.UpdateUTC
                    };

            var svcResponse = ProcessServiceRequestAndResponse(updateDevice, _httpRequestWrapper, DeviceApiEndPointUri, requestHeader, HttpMethod.Put);
						Log.IfInfo("Update Device: " + deviceData.DeviceUID + " returned " + svcResponse.StatusCode);
            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastUpdateUtc = deviceData.UpdateUTC;
                break;

              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(updateDevice, _httpRequestWrapper, DeviceApiEndPointUri, requestHeader, HttpMethod.Put);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastUpdateUtc = deviceData.UpdateUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(updateDevice));
                lastUpdateUtc = deviceData.UpdateUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(updateDevice)));
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
          Log.IfInfo(string.Format("Completed Processing UpdateDeviceEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
        }
      }
      return true;
    }
  }
}
