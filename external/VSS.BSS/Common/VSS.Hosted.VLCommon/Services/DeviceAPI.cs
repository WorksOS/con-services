using log4net;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using VSS.Hosted.VLCommon.Resources;
using VSS.Hosted.VLCommon.TrimTracMessages;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Hosted.VLCommon.Services.MDM;

namespace VSS.Hosted.VLCommon
{
  public class DeviceAPI : IDeviceAPI
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private static readonly bool EnableNextGenSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.DeviceAPI.EnableSync"]);
    private UUIDSequentialGuid deviceUUID = new UUIDSequentialGuid();

    #region Generic Methods

    public Device CreateDevice(INH_OP opCtx, string ibKey, string ownerBSSID, string gpsDeviceID, DeviceTypeEnum deviceType,
          TimeSpan? samplingInterval, TimeSpan? reportingInterval, TimeSpan? lowPowerInterval, TimeSpan? bitReportInterval, bool isReadOnly)
    {
      int deviceTypeID = (int)deviceType;

      if (!ValidGpsDeviceID(gpsDeviceID, deviceType))
        throw new InvalidOperationException(string.Format("Invalid GPS Device ID {0}", gpsDeviceID));

      // only check unique GPS Device ID and Type when an actual device is being created
      if (!(deviceType == DeviceTypeEnum.MANUALDEVICE ||
          (AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)deviceType), AppFeatureEnum.SupportBlankGPSDeviceID) && string.IsNullOrEmpty(gpsDeviceID))))
      {
        bool deviceExists = (from d in opCtx.DeviceReadOnly
                             where d.GpsDeviceID == gpsDeviceID && d.fk_DeviceTypeID == deviceTypeID
                             select 1).Any();
        if (deviceExists)
          throw new InvalidOperationException(string.Format("An active device with GpsDeviceID {0} of type {1} already exists", gpsDeviceID, deviceType.ToString()), new IntentionallyThrownException());
      }

      Device newDevice = new Device() { ID = -1, IBKey = ibKey, OwnerBSSID = ownerBSSID, GpsDeviceID = gpsDeviceID, UpdateUTC = DateTime.UtcNow };

      //Set Device properties and associations
      newDevice.fk_DeviceTypeID = (int)deviceType;
      newDevice.fk_DeviceStateID = (int)DeviceStateEnum.Provisioned;
      newDevice.DeviceUID = deviceUUID.CreateGuid();
      opCtx.Device.AddObject(newDevice);
      ResetDailyReport(opCtx, newDevice.ID, (DeviceStateEnum)newDevice.fk_DeviceStateID);

      if (opCtx.SaveChanges() < 1)
        throw new InvalidOperationException(string.Format("Failed to create device with Serial Number {0}", newDevice.GpsDeviceID));

      if (EnableNextGenSync)
      {
        var deviceDetails = new
        {
          DeviceUID = (Guid)newDevice.DeviceUID,
          DeviceSerialNumber = newDevice.GpsDeviceID,
          DeviceType = deviceType.ToString(),
          DeviceState = DeviceStateEnum.Provisioned.ToString(),
          ActionUTC = DateTime.UtcNow
        };

        var success = API.DeviceService.CreateDevice(deviceDetails);
        if (!success)
        {
          log.IfWarnFormat("Error occurred while creating Device in VSP stack. Serial Number :{0}", newDevice.GpsDeviceID);
        }
      }

      if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.PLInOut.ToValString())
        CreatePLDevice(opCtx, newDevice.GpsDeviceID, deviceType, isReadOnly);
      else if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.MTSInOut.ToValString())
        CreateMTSDevice(opCtx, newDevice.GpsDeviceID, bitReportInterval.Value, lowPowerInterval.Value, samplingInterval.Value, reportingInterval.Value, deviceType);
      else if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.TrimTracInOut.ToValString())
      {
        TTDevice ttDevice = null;
        CreateTTDevice(opCtx, IMEI2UnitID(newDevice.GpsDeviceID), newDevice.GpsDeviceID, DeviceStateEnum.Provisioned, out ttDevice);
      }

      return newDevice;
    }

    public Device CreateDevice(INH_OP opCtx, string ibKey, string ownerBSSID, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      int deviceTypeID = (int)deviceType;

      if (!ValidateStoreGpsDeviceID(gpsDeviceID, deviceType))
        throw new InvalidOperationException(string.Format("Invalid Device Serial Number {0}", gpsDeviceID));

      Device newDevice = new Device() { ID = -1, IBKey = ibKey, OwnerBSSID = ownerBSSID, GpsDeviceID = gpsDeviceID, UpdateUTC = DateTime.UtcNow };

      //Set Device properties and associations
      newDevice.fk_DeviceTypeID = (int)deviceType;
      newDevice.fk_DeviceStateID = (int)DeviceStateEnum.Provisioned;
      newDevice.DeviceUID = deviceUUID.CreateGuid();
      opCtx.Device.AddObject(newDevice);
      ResetDailyReport(opCtx, newDevice.ID, (DeviceStateEnum)newDevice.fk_DeviceStateID);

      if (opCtx.SaveChanges() < 1)
        throw new InvalidOperationException(string.Format("Failed to create device with Serial Number {0}", newDevice.GpsDeviceID));


      if (EnableNextGenSync)
      {
        var deviceDetails = new
        {
          DeviceUID = (Guid)newDevice.DeviceUID,
          DeviceSerialNumber = newDevice.GpsDeviceID,
          DeviceType = deviceType.ToString(),
          DeviceState = DeviceStateEnum.Provisioned.ToString(),
          ActionUTC = DateTime.UtcNow
        };

        var success = API.DeviceService.CreateDevice(deviceDetails);
        if (!success)
        {
          log.IfWarnFormat("Error occurred while creating Device in VSP stack. Serial Number :{0}", newDevice.GpsDeviceID);
        }
      }

      return newDevice;
    }

    public bool ActivateDevice(INH_OP opCtx, string ibKey, DeviceStateEnum deviceState)
    {
      var device = (from d in opCtx.Device
                    where d.IBKey == ibKey
                    select new { ID = d.ID, TypeID = d.fk_DeviceTypeID, GpsDeviceID = d.GpsDeviceID }).SingleOrDefault();

      if (device == null)
        throw new InvalidOperationException(string.Format("Device IBKey {0} not found", ibKey));

      switch ((DeviceTypeEnum)device.TypeID)
      {
        case DeviceTypeEnum.TrimTrac:
          UpdateTTDeviceState(opCtx, device.GpsDeviceID, deviceState);
          break;
        case DeviceTypeEnum.CrossCheck:
        case DeviceTypeEnum.Series522:
        case DeviceTypeEnum.Series523:
        case DeviceTypeEnum.Series521:
        case DeviceTypeEnum.SNM940:
        case DeviceTypeEnum.SNM941:
        case DeviceTypeEnum.PL420:
        case DeviceTypeEnum.PL421:
        case DeviceTypeEnum.SNM451:
        case DeviceTypeEnum.PL431:
        case DeviceTypeEnum.TM3000:
        case DeviceTypeEnum.TAP66:
        case DeviceTypeEnum.PLE641:
        case DeviceTypeEnum.PL641:
        case DeviceTypeEnum.PL631:
        case DeviceTypeEnum.PLE631:
        case DeviceTypeEnum.PLE641PLUSPL631:
        case DeviceTypeEnum.PL231:
        case DeviceTypeEnum.PL241:
        case DeviceTypeEnum.DCM300:
        case DeviceTypeEnum.PL131:
        case DeviceTypeEnum.PL141:
        case DeviceTypeEnum.PL440:
        case DeviceTypeEnum.PL240:
        case DeviceTypeEnum.PL161:
        case DeviceTypeEnum.PL542:
        case DeviceTypeEnum.PLE642:
        case DeviceTypeEnum.PLE742:
        case DeviceTypeEnum.PL240B:
          UpdateOpDeviceState(opCtx, device.GpsDeviceID, deviceState, device.TypeID);
          break;
        case DeviceTypeEnum.MANUALDEVICE:
        default:
          break;
      }

      return true;
    }

    public bool DeActivateDevice(INH_OP opCtx, string ibKey, DeviceStateEnum deviceState)
    {
      Device device = (from d in opCtx.Device where d.IBKey == ibKey select d).SingleOrDefault();

      if (device == null)
        throw new InvalidOperationException(string.Format("Device IBKey {0} not found", ibKey));

      device.fk_DeviceStateID = (int)deviceState;
      device.UpdateUTC = DateTime.UtcNow;

      ResetDailyReport(opCtx, device.ID, deviceState);

      if (AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId(device.fk_DeviceTypeID), AppFeatureEnum.DeviceDeActivation))
        DeactivateTrimTrac(opCtx, device);

      bool updated = (opCtx.SaveChanges() > 0);

      if (updated && EnableNextGenSync)
      {
        var updateEvent = new
        {
          DeviceUID = (Guid)device.DeviceUID,
          DeviceState = deviceState.ToString(),
          ActionUTC = DateTime.UtcNow
        };

        var result = API.DeviceService.UpdateDevice(updateEvent);
        if (!result)
        {
          log.IfInfoFormat("Error occurred while DeActivate device state in VSP stack. GpsDeviceId :{0}, New Device State:{1}",
            device.GpsDeviceID, deviceState.ToString());
        }
      }

      return true;
    }

    public void UpdateDeviceState(long deviceId, DeviceStateEnum deviceState, INH_OP outerNHOpCtx = null)
    {
      bool updated = false;
      if (deviceId <= 0)
        throw new InvalidOperationException("deviceId argument is not valid.");

      if (outerNHOpCtx == null)
      {
        using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          UpdateDeviceStateInternals(deviceId, deviceState, opCtx);
          updated = (opCtx.SaveChanges()) > 0;
        }
      }
      else
      {
        // Not calling .SaveChanges() so callers who provide outerNHOpCtx can manage the db transaction
        UpdateDeviceStateInternals(deviceId, deviceState, outerNHOpCtx);
      }

      INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>();

      var bssID = (from d in ctx.Device
                   where d.ID == deviceId
                   select d.OwnerBSSID).FirstOrDefault();


      if (updated && EnableNextGenSync)
      {
        using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          var DeviceGuid = (from device in opCtx.Device
                            where device.ID == deviceId
                            select device.DeviceUID).FirstOrDefault();
          var updateEvent = new
          {
            DeviceUID = (Guid)DeviceGuid,
            DeviceState = deviceState.ToString(),
            ActionUTC = DateTime.UtcNow,
          };
          var result = API.DeviceService.UpdateDevice(updateEvent);
          if (!result)
          {
            log.IfInfoFormat("Error occurred while updating device state in VSP stack. DeviceId :{0}, New Device State:{1}",
              deviceId, deviceState.ToString());
          }
        }

      }

    }

    private void UpdateDeviceStateInternals(long deviceId, DeviceStateEnum deviceState, INH_OP nhOpCtx)
    {
      var device = nhOpCtx.Device.Single(x => x.ID == deviceId);
      device.fk_DeviceStateID = (int)deviceState;
      device.UpdateUTC = DateTime.UtcNow;
      ResetDailyReport(nhOpCtx, device.ID, deviceState);
    }

    public void DeregisterDeviceState(long deviceId, DeviceStateEnum deviceState, DateTime deregesteredUTC)
    {
      if (deviceId <= 0)
        throw new InvalidOperationException("deviceId argument is not valid.");
      bool updated;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var device = opCtx.Device.Single(x => x.ID == deviceId);

        device.fk_DeviceStateID = (int)deviceState;
        device.UpdateUTC = DateTime.UtcNow;
        device.DeregisteredUTC = deregesteredUTC;

        ResetDailyReport(opCtx, device.ID, deviceState);

        updated = (opCtx.SaveChanges() > 0);
        if (updated && EnableNextGenSync)
        {
          var updateEvent = new
          {
            DeviceUID = (Guid)device.DeviceUID,
            DeregisteredUTC = deregesteredUTC,
            DeviceState = deviceState.ToString(),
            ActionUTC = DateTime.UtcNow
          };

          var result = API.DeviceService.UpdateDevice(updateEvent);
          if (!result)
          {
            log.IfInfoFormat("Error occurred while deregistering device state in VSP stack. DeviceId :{0}",
              deviceId);
          }
        }
      }
    }

    public void UpdateOpDeviceState(INH_OP opCtx, string gpsDeviceID, DeviceStateEnum deviceState, int deviceTypeID)
    {
      //if (deviceTypeID == (int)DeviceTypeEnum.MANUALDEVICE)
      //  throw new InvalidOperationException("Error updating device state for No Device Type");

      Device deviceInOP = (from d in opCtx.Device
                           where d.GpsDeviceID == gpsDeviceID
                           && d.fk_DeviceTypeID == deviceTypeID
                           select d).SingleOrDefault();

      if (deviceInOP != null && (int)deviceInOP.fk_DeviceStateID != (int)deviceState)
      {
        deviceInOP.fk_DeviceStateID = (int)deviceState;
        deviceInOP.UpdateUTC = DateTime.UtcNow;

        ResetDailyReport(opCtx, deviceInOP.ID, deviceState);

        if (opCtx.SaveChanges() < 1)
          throw new InvalidOperationException(string.Format("Error updating device state for  {0}", gpsDeviceID.ToString()));
        else
        {
          if (EnableNextGenSync)
          {
            var updateEvent = new
            {
              DeviceUID = (Guid)deviceInOP.DeviceUID,
              DeviceState = deviceState.ToString(),
              ActionUTC = DateTime.UtcNow
            };
            var result = API.DeviceService.UpdateDevice(updateEvent);
            if (!result)
            {
              log.IfInfoFormat("Error occurred while updating device state in VSP stack. GpsDeviceID :{0}",
                gpsDeviceID);
            }
          }
        }
      }
    }

    public void UpdateDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, DeviceConfigBase config, ObjectContextTransactionParams<Action> transactionParams = null, INH_OP nhOpCtx = null)
    {
      try
      {
        if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ConfigDataType) == ConfigDataType.TTConfigData.ToValString())
        {
          UpdateDeviceConfiguration<TTConfigData>(gpsDeviceID, deviceType, config);
          return;
        }

        if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ConfigDataType) == ConfigDataType.MTSConfigData.ToValString())
        {
          UpdateDeviceConfiguration<MTSConfigData>(gpsDeviceID, deviceType, config);
          return;
        }

        ////A5N2 devices do not (yet) send messages into Visionlink so we don't need this yet.
        if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ConfigDataType) == ConfigDataType.A5N2ConfigData.ToValString())
        {
          UpdateDeviceConfiguration<A5N2ConfigData>(gpsDeviceID, deviceType, config, transactionParams, nhOpCtx);
          return;
        }
        log.IfErrorFormat("Unable to update configuration for device {0}.", gpsDeviceID);
      }
      catch (Exception e)
      {
        log.IfErrorFormat(e, "Unexpected error updating device config for device SN {0}", gpsDeviceID);
        throw;
      }
    }

    private void UpdateDeviceConfiguration<TConfigData>(string gpsDeviceID, DeviceTypeEnum deviceType, DeviceConfigBase config, ObjectContextTransactionParams<Action> transactionParams = null, INH_OP nhOpCtx = null) where TConfigData : DeviceConfigData, new()
    {
      try
      {
        log.IfInfoFormat("Updating Device Config for gpsDeviceID: {0} Config: {1}", gpsDeviceID, config.ToXElement());
        if (string.IsNullOrEmpty(gpsDeviceID))
          throw new InvalidOperationException("GPS Device ID is null or empty.");

        using (nhOpCtx ?? (nhOpCtx = ObjectContextFactory.NewNHContext<INH_OP>()))
        {
          Asset asset = GetAsset(nhOpCtx, gpsDeviceID, deviceType);
          Device device = GetDevice(nhOpCtx, gpsDeviceID, deviceType);
          if (device != null)
          {
            TConfigData data = new TConfigData();
            if (!string.IsNullOrEmpty(device.DeviceDetailsXML))
              data.Parse(device.DeviceDetailsXML);

            data.Update(config);

            string detailXML = data.ToXElement().ToString();
            if (device.DeviceDetailsXML != detailXML)
            {
              device.DeviceDetailsXML = detailXML;
              device.UpdateUTC = DateTime.UtcNow;
              log.Info("Audit logging for config Changes");
              log.IfInfoFormat("Audit logging for Config: {0}", config.ToXElement());

              Action<TConfigData, INH_OP, Asset, DeviceConfigBase> updateDatabase = (innerData, innerCtx, innerAsset, innerConfig) =>
              {
                innerData.AuditConfigChanges(innerCtx, innerAsset, innerConfig);
                if (innerCtx.SaveChanges() < 1)
                {
                  log.IfWarn("No Changes to DeviceDetailsXML were saved");
                }
                else
                {
                  innerData.UpdateCurrentStatus(innerCtx, innerAsset, innerConfig);
                }
                if ((null != transactionParams) && (null != transactionParams.Callback))
                {
                  transactionParams.Callback();
                }
              };

              if ((null != transactionParams) && (null != transactionParams.Scope))
              {
                using (transactionParams.Scope.EnrollObjectContexts(nhOpCtx))
                {
                  updateDatabase(data, nhOpCtx, asset, config);
                  transactionParams.Scope.Commit();
                }
              }
              else
              {
                updateDatabase(data, nhOpCtx, asset, config);
              }
            }
            else
            {
              log.Info("DeviceDetails did Not Change");
            }
          }
          else
            log.IfInfoFormat("Could not find Device {0}, Device Type: {1}", gpsDeviceID, deviceType.ToString());
        }
      }
      catch (Exception e)
      {
        log.IfErrorFormat(e, "UpdateDeviceConfiguration, SN: {0} Error Updating Device Configuration", gpsDeviceID ?? string.Empty);
        throw;
      }
    }

    private Asset GetAsset(INH_OP ctx, string gpsDeviceID, DeviceTypeEnum type)
    {
      Asset asset = (from asst in ctx.Asset
                     where asst.Device.GpsDeviceID == gpsDeviceID &&
                           asst.Device.fk_DeviceTypeID == (int)type
                     select asst).FirstOrDefault<Asset>();

      if (asset != null)
        log.IfInfoFormat("Found Asset with ID: {0}", asset.AssetID);
      else
        log.IfInfoFormat("Could not find asset for GpsDeviceID: {0} and DeviceType {1}", gpsDeviceID, type.ToString());

      return asset;
    }

    private Device GetDevice(INH_OP ctx, string gpsDeviceID, DeviceTypeEnum type)
    {
      Device device = null;
      if (API.Device.IsProductLinkDevice(type))
      {
        //type is always == PL121 here..so we don't actually know if the device is PL121 or PL321
        List<Device> deviceList = (from d in ctx.Device
                                   where d.GpsDeviceID == gpsDeviceID
                                   && (d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL121 || d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL321)
                                   select d).ToList<Device>();

        if (deviceList.Count > 1) //pick the (hopefully only) 1 subscribed and if both, the most recently updated and subscribed, else, just the most recently updated
        {
          device =
            (from d in deviceList where d.fk_DeviceStateID == (int)DeviceStateEnum.Subscribed select d).
              OrderByDescending(x => x.UpdateUTC).First() ??
            (from d in deviceList orderby d.UpdateUTC select d).OrderByDescending(x => x.UpdateUTC).First();
        }
        else if (deviceList.Count > 0)
        {
          device = deviceList[0];
        }
      }
      else
      {
        int deviceType = (int)type;
        device = (from d in ctx.Device
                  where d.GpsDeviceID == gpsDeviceID
                     && d.fk_DeviceTypeID == deviceType
                  select d).FirstOrDefault<Device>();
      }

      if (device != null)
        log.IfInfoFormat("Found Device with ID: {0} for GpsDeviceID: {1} and DeviceType {2} (If type is PL121, it could also be a PL321)", device.ID, device.GpsDeviceID, type.ToString());
      else
        log.IfInfoFormat("Could not find device for GpsDeviceID: {0} and DeviceType {1} (If type is PL121 it could also be a PL321)", gpsDeviceID, type.ToString());

      return device;
    }

    #endregion

    private void DeactivateTrimTrac(INH_OP opCtx1, Device device)
    {
      string unitID = IMEI2UnitID(device.GpsDeviceID);
      string[] gpsDeviceIDs = new string[] { device.GpsDeviceID };
      int delayTimeout = 86400;//(24 hours)
      int idleTimeout = 999990;//(~11.5 days)
      int runtimeMeterLPAcountdown = 0;
      EMode_Z runtimeLPABased = EMode_Z.Disabled;
      int ignitionSenseOverride = 0;


      TTDevice ttDevice = (from tts in opCtx1.TTDevice where tts.UnitID == unitID select tts).SingleOrDefault();
      if (ttDevice != null)
      {
        ttDevice.UpdateUTC = DateTime.UtcNow;

        //Send a config message for cancelling
        //Trimtrac doesn't have a shutup message, hence adjusting the reporting interval
        API.TTOutbound.SetRateConfiguration(opCtx1, gpsDeviceIDs, delayTimeout, idleTimeout);
        API.TTOutbound.SetReportConfiguration(opCtx1, gpsDeviceIDs, runtimeMeterLPAcountdown, runtimeLPABased, ignitionSenseOverride);
      }
      opCtx1.SaveChanges();

    }

    #region TT Specific
    public bool CreateTTDevice(INH_OP opCtx1, string unitId, string imei, DeviceStateEnum deviceState, out TTDevice dev)
    {
      int deviceStateID = (int)deviceState;
      bool deviceExists = false;


      dev = (from tts in opCtx1.TTDevice where tts.UnitID == unitId select tts).SingleOrDefault();
      deviceExists = dev != null;

      if (!deviceExists)
      {
        dev = new TTDevice() { UnitID = unitId, UpdateUTC = DateTime.UtcNow, IMEI = imei };

        opCtx1.TTDevice.AddObject(dev);
      }
      else
      {
        if (dev.IMEI == null || dev.IMEI == "")//If it is null or empty it means already the row has got into the ttdevice table through some means and now the device is subscribed 
        {
          dev.IMEI = imei;
          dev.UpdateUTC = DateTime.UtcNow;
        }
      }
      opCtx1.SaveChanges();

      return deviceExists;
    }

    public void UpdateTTDeviceState(INH_OP opCtx, string gpsDeviceID, DeviceStateEnum deviceState)
    {
      UpdateOpDeviceState(opCtx, gpsDeviceID, deviceState, (int)DeviceTypeEnum.TrimTrac);
    }

    public string IMEI2UnitID(string IMEI)
    {
      if (IMEI.Length >= 14)
      {
        string serial = IMEI.Substring(8, 6);
        switch (IMEI.Substring(0, 8))
        {
          case "01030700": return string.Format("Y0{0}", serial);
          case "35323900": return string.Format("Y1{0}", serial);
          case "01107400": return string.Format("Y2{0}", serial);
          case "01127600": return string.Format("Y3{0}", serial);
          default:
            break;
        }
      }
      return null;
    }

    #endregion

    #region MTS500 Series

    public MTSDevice CreateMTSDevice(INH_OP opCtx1, string gpsDeviceID, TimeSpan bitPacketInterval, TimeSpan lowPowerInterval,
      TimeSpan samplingInterval, TimeSpan reportingInterval, DeviceTypeEnum deviceType)
    {
      int deviceTypeID = (int)deviceType;

      MTSDevice newDevice = null;
      int noDevice = (int)DeviceTypeEnum.MANUALDEVICE;



      //Check to see if the MTSDevice already exists
      MTSDevice existingDevice = (from mts in opCtx1.MTSDevice
                                  where mts.SerialNumber == gpsDeviceID
                                  && (mts.DeviceType == deviceTypeID || mts.DeviceType == noDevice)
                                  select mts).SingleOrDefault();

      //If does not exist, create it.
      if (existingDevice == null)
      {
        newDevice = new MTSDevice()
        {
          BitPacketRate = (int)bitPacketInterval.TotalSeconds,
          LowPowerRate = (int)lowPowerInterval.TotalSeconds,
          SampleRate = (int)samplingInterval.TotalSeconds,
          UpdateRate = (int)reportingInterval.TotalSeconds,
          IsTCP = true,
          UpdateUTC = DateTime.UtcNow,
          IpAddress = "0.0.0.0",
          SerialNumber = gpsDeviceID,
          DeviceType = deviceTypeID,
        };

        opCtx1.MTSDevice.AddObject(newDevice);
      }
      else         //Use the existing one.      
      {
        newDevice = existingDevice;
        newDevice.DeviceType = deviceTypeID;
        newDevice.UpdateUTC = DateTime.UtcNow;
      }

      if (opCtx1.SaveChanges() < 1)
        throw new InvalidOperationException("Error creating Device in INH_RAW");

      return newDevice;
    }

    #endregion

    #region PL
    public PLDevice CreatePLDevice(INH_OP opCtx1, string gpsDeviceID, DeviceTypeEnum deviceType, bool isReadOnly)
    {
      PLDevice newDevice = null;



      PLDevice existingDevice = (from pl in opCtx1.PLDevice
                                 where pl.ModuleCode == gpsDeviceID
                                 select pl).SingleOrDefault();

      if (existingDevice == null)
      {
        if (string.IsNullOrEmpty(gpsDeviceID) || gpsDeviceID.Length > 50)
        {
          log.IfWarnFormat("Failed to create device with invalid PL serial number '{0}'", gpsDeviceID);
          return null;
        }
        newDevice = new PLDevice() { ModuleCode = gpsDeviceID, UpdateUTC = DateTime.UtcNow, IsReadOnly = isReadOnly, InAmericas = true };
        opCtx1.PLDevice.AddObject(newDevice);

        if (opCtx1.SaveChanges() < 1)
          throw new InvalidOperationException("Error creating Device in INH_RAW");
      }
      else         //Use the existing one, if exists.      
      {
        newDevice = existingDevice;
      }

      return newDevice;
    }

    public bool IsProductLinkDevice(DeviceTypeEnum deviceType)
    {
      return (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.PLInOut.ToValString());
    }

    #endregion

    #region Implementation

    private bool ValidGpsDeviceID(string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      bool valid = false;
      switch (deviceType)
      {
        case DeviceTypeEnum.CrossCheck:
          int serialNumber;
          valid = int.TryParse(gpsDeviceID, out serialNumber);
          break;
        case DeviceTypeEnum.TrimTrac:
          Regex re = new Regex(@"^\d{1,15}$");
          valid = (re.IsMatch(gpsDeviceID) && IMEI2UnitID(gpsDeviceID) != null);
          break;
        case DeviceTypeEnum.MANUALDEVICE:
          valid = string.IsNullOrEmpty(gpsDeviceID);
          break;
        default://add validation for other device types as required
          valid = !string.IsNullOrEmpty(gpsDeviceID) || AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)deviceType), AppFeatureEnum.SupportBlankGPSDeviceID);
          break;
      }
      return valid;
    }

    private bool ValidateStoreGpsDeviceID(string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      bool valid = false;
      switch (deviceType)
      {
        case DeviceTypeEnum.CrossCheck:
          int serialNumber;
          valid = int.TryParse(gpsDeviceID, out serialNumber);
          break;
        case DeviceTypeEnum.TrimTrac:
          Regex re = new Regex(@"^\d{1,15}$");
          valid = (re.IsMatch(gpsDeviceID) && IMEI2UnitID(gpsDeviceID) != null);
          break;
        default://add validation for other device types as required
          valid = !string.IsNullOrEmpty(gpsDeviceID) || AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)deviceType), AppFeatureEnum.SupportBlankGPSDeviceID);
          break;
      }
      return valid;
    }

    #endregion

    #region BSS V2 methods

    public IList<DevicePersonality> CreateDevicePersonality(INH_OP opContext, long deviceID, string firmwareVersionID,
          string simSerialNumber, string partNumber, string cellularModemIMEA, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      var personalities = new List<DevicePersonality>();

      if (deviceType == DeviceTypeEnum.MANUALDEVICE || AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)deviceType), AppFeatureEnum.SupportBlankGPSDeviceID))
        return personalities;

      if (!string.IsNullOrWhiteSpace(firmwareVersionID))
        personalities.Add(CreateDevicePersonality(opContext, deviceID, PersonalityTypeEnum.Software, firmwareVersionID));

      if (!string.IsNullOrWhiteSpace(simSerialNumber))
        personalities.Add(CreateDevicePersonality(opContext, deviceID, PersonalityTypeEnum.SerialNumber, simSerialNumber));

      if (!string.IsNullOrWhiteSpace(partNumber))
        personalities.Add(CreateDevicePersonality(opContext, deviceID, PersonalityTypeEnum.Hardware, partNumber));

      if (!string.IsNullOrWhiteSpace(cellularModemIMEA))
        personalities.Add(CreateDevicePersonality(opContext, deviceID, PersonalityTypeEnum.SCID, cellularModemIMEA));

      if (deviceType == DeviceTypeEnum.TrimTrac)
        personalities.Add(CreateDevicePersonality(opContext, deviceID, PersonalityTypeEnum.UnitID, API.Device.IMEI2UnitID(gpsDeviceID)));

      if (personalities.Count > 0)
        if (opContext.SaveChanges() > 0)
          return personalities;

      return new List<DevicePersonality>();
    }

    private static DevicePersonality CreateDevicePersonality(INH_OP opContext, long deviceId, PersonalityTypeEnum personalityType, string value)
    {
      var personality = new DevicePersonality();
      personality.fk_DeviceID = deviceId;
      personality.fk_PersonalityTypeID = (int)personalityType;
      personality.Value = value;
      personality.UpdateUTC = DateTime.UtcNow;

      opContext.DevicePersonality.AddObject(personality);

      return personality;
    }

    #endregion

    #region Devices Daily Report Methods

    public bool UpdateDailyReportIsUserCustomized(INH_OP ctx, string gpsDeviceID, bool isUserCustomized, DateTime? dailyReportUTC, int deviceType)
    {
      DailyReport dailyReport = (from d in ctx.DailyReport
                                 from device in ctx.Device
                                 where device.GpsDeviceID == gpsDeviceID && device.fk_DeviceTypeID == deviceType
                                       && d.fk_DeviceID == device.ID
                                 select d).FirstOrDefault();
      if (dailyReport != null)
      {
        dailyReport.IsUserCustomized = isUserCustomized;
        dailyReport.LastDailyReportTZBias = null;
        dailyReport.LastDailyReportUTC = dailyReportUTC;
        dailyReport.NextCheckUTC = null;
        ctx.SaveChanges();
        return true;
      }

      return false;
    }

    private void ResetDailyReport(INH_OP ctx, long deviceID, DeviceStateEnum deviceState)
    {
      if (deviceState != DeviceStateEnum.Subscribed)
      {
        DailyReport dr = (from d in ctx.DailyReport
                          where d.fk_DeviceID == deviceID
                          select d).FirstOrDefault();
        if (dr == null)
        {
          dr = new DailyReport();
          dr.fk_DeviceID = deviceID;
          dr.IsUserCustomized = false;
          ctx.DailyReport.AddObject(dr);
        }
        else
        {
          dr.IsUserCustomized = false;
          dr.LastDailyReportTZBias = null;
          dr.LastDailyReportUTC = null;
          dr.NextCheckUTC = null;
        }
      }
    }
    #endregion

    public string GetDeviceTypeDescription(int deviceTypeID, string locale)
    {
      DeviceTypeEnum dt = (DeviceTypeEnum)deviceTypeID;

      switch (dt)
      {
        case DeviceTypeEnum.MANUALDEVICE: return VLResourceManager.GetString("devicetypenodevice", locale);
        case DeviceTypeEnum.Series521: return VLResourceManager.GetString("deviceTypeMTS521", locale);
        case DeviceTypeEnum.Series522: return VLResourceManager.GetString("deviceTypeMTS522", locale);
        case DeviceTypeEnum.Series523: return VLResourceManager.GetString("deviceTypeMTS523", locale);
        case DeviceTypeEnum.PL420: return VLResourceManager.GetString("deviceTypePL420", locale);
        case DeviceTypeEnum.TM3000: return VLResourceManager.GetString("deviceTypeTM3000", locale);
        case DeviceTypeEnum.TAP66: return VLResourceManager.GetString("deviceTypeTAP66", locale);
        case DeviceTypeEnum.THREEPDATA: return VLResourceManager.GetString("deviceTypeTHREEPDATA", locale);
        case DeviceTypeEnum.PLE641PLUSPL631: return VLResourceManager.GetString("deviceTypePLE641PLUSPL631", locale);
        case DeviceTypeEnum.MTGModularGatewayHYPHENElectricEngine: return VLResourceManager.GetString("deviceTypeMTGModularGatewayHYPHENElectricEngine", locale);
        case DeviceTypeEnum.MTGModularGatewayHYPHENMotorEngine: return VLResourceManager.GetString("deviceTypeMTGModularGatewayHYPHENMotorEngine", locale);
        case DeviceTypeEnum.MTHYPHEN10: return VLResourceManager.GetString("deviceTypeMTHYPHEN10", locale);
        case DeviceTypeEnum.MCHYPHEN3: return VLResourceManager.GetString("deviceTypeMCHYPHEN3", locale);
        default: return dt.ToString();
      }
    }

    #region Store
    public bool UpdateOwnerBSSID(long deviceId, Guid organizationIdentifier, INH_OP nhOpCtx)
    {

      var bssid = (from org in nhOpCtx.CustomerReadOnly
                   where org.CustomerUID == organizationIdentifier
                   select org.BSSID).SingleOrDefault();

      //This check is redundant as valid condition for device and organization is already handled in the interceptor.
      var device = nhOpCtx.Device.SingleOrDefault(x => x.ID == deviceId);
      if (device == null)
        return false;
      device.OwnerBSSID = bssid;
      device.UpdateUTC = DateTime.UtcNow;
      nhOpCtx.SaveChanges();
      return true;
    }

    public bool CancelOwnerBSSID(long deviceId, Guid organizationIdentifier, INH_OP nhOpCtx)
    {
      var device = (from d in nhOpCtx.Device
                    join c in nhOpCtx.CustomerReadOnly on d.OwnerBSSID equals c.BSSID
                    where d.ID == deviceId
                    && c.CustomerUID == organizationIdentifier
                    select d).SingleOrDefault();

      //This check is redundant as valid condition for device and organization is already handled in the interceptor.
      if (device == null)
        return false;

      var asset = nhOpCtx.AssetReadOnly.Where(x => x.fk_DeviceID == deviceId).Single();
      API.Equipment.CreateAssetDeviceHistory(nhOpCtx, asset.AssetID, deviceId, device.OwnerBSSID,
        asset.InsertUTC.GetValueOrDefault());

      device.OwnerBSSID = "";
      device.UpdateUTC = DateTime.UtcNow;
      nhOpCtx.SaveChanges();
      return true;
    }
    #endregion

  }
}
