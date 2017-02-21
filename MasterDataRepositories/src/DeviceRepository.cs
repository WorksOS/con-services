using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using VSS.Masterdata.Service.Repositories;
using System.Collections.Generic;

namespace VSS.Device.Data
{
  public class DeviceRepository : RepositoryBase, IRepository<IDeviceEvent>
  {
    private readonly ILogger log;

    public DeviceRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString)
    {
      log = logger.CreateLogger<DeviceRepository>();
    }

    public async Task<int> StoreEvent(IDeviceEvent evt)
    {
      var upsertedCount = 0;
      string eventType = "Unknown";
      if (evt is CreateDeviceEvent)
      {
        var device = new VSS.Device.Data.Models.Device();
        var deviceEvent = (CreateDeviceEvent)evt;
        device.DeviceUID = deviceEvent.DeviceUID.ToString();
        device.DeviceSerialNumber = deviceEvent.DeviceSerialNumber;
        device.DeviceType = deviceEvent.DeviceType;
        device.DeviceState = deviceEvent.DeviceState;
        device.DeregisteredUTC = deviceEvent.DeregisteredUTC;
        device.ModuleType = deviceEvent.ModuleType;
        device.MainboardSoftwareVersion = deviceEvent.MainboardSoftwareVersion;
        device.RadioFirmwarePartNumber = deviceEvent.RadioFirmwarePartNumber;
        device.GatewayFirmwarePartNumber = deviceEvent.GatewayFirmwarePartNumber;
        device.DataLinkType = deviceEvent.DataLinkType;
        device.LastActionedUtc = deviceEvent.ActionUTC;
        eventType = "CreatedDeviceEvent";
        upsertedCount = await UpsertDeviceDetail(device, eventType);
      }
      else if (evt is UpdateDeviceEvent)
      {
        var device = new VSS.Device.Data.Models.Device();
        var deviceEvent = (UpdateDeviceEvent)evt;
        device.DeviceUID = deviceEvent.DeviceUID.ToString();

        // I don't think the following 2 can/should be altered todo?
        device.DeviceSerialNumber = deviceEvent.DeviceSerialNumber;
        device.DeviceType = deviceEvent.DeviceType;

        device.DeviceState = deviceEvent.DeviceState;
        device.DeregisteredUTC = deviceEvent.DeregisteredUTC;
        device.MainboardSoftwareVersion = deviceEvent.MainboardSoftwareVersion;
        device.RadioFirmwarePartNumber = deviceEvent.RadioFirmwarePartNumber;
        device.GatewayFirmwarePartNumber = deviceEvent.GatewayFirmwarePartNumber;
        device.DataLinkType = deviceEvent.DataLinkType;
        device.LastActionedUtc = deviceEvent.ActionUTC;
        eventType = "UpdateDeviceEvent";
        upsertedCount = await UpsertDeviceDetail(device, eventType);
      }
      else if (evt is AssociateDeviceAssetEvent)
      {
        var deviceAsset = new AssetDevice.Data.Models.AssetDevice();
        var deviceEvent = (AssociateDeviceAssetEvent)evt;
        deviceAsset.DeviceUID = deviceEvent.DeviceUID.ToString();
        deviceAsset.AssetUID = deviceEvent.AssetUID.ToString();
        deviceAsset.LastActionedUtc = deviceEvent.ActionUTC;
        eventType = "AssociateDeviceAssetEvent";
        upsertedCount = await UpsertDeviceAssetDetail(deviceAsset, eventType);
      }
      else if (evt is DissociateDeviceAssetEvent)
      {
        var deviceAsset = new AssetDevice.Data.Models.AssetDevice();
        var deviceEvent = (DissociateDeviceAssetEvent)evt;
        deviceAsset.DeviceUID = deviceEvent.DeviceUID.ToString();
        deviceAsset.AssetUID = deviceEvent.AssetUID.ToString();
        deviceAsset.LastActionedUtc = deviceEvent.ActionUTC;
        eventType = "DissociateDeviceAssetEvent";
        upsertedCount = await UpsertDeviceAssetDetail(deviceAsset, eventType);
      }

      return upsertedCount;
    }

    
    #region device
    /// <summary>
    /// All detail-related columns can be inserted, 
    ///    but only certain columns can be updated.
    ///    on deletion, a flag will be set.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertDeviceDetail(Models.Device device, string eventType)
    {
      try
      {
        {
          await PerhapsOpenConnection();
          log.LogDebug("DeviceRepository: Upserting eventType{0} deviceUid={1}", eventType, device.DeviceUID);
          var upsertedCount = 0;

          var existing = await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return (await Connection.QueryAsync<Models.Device>
                      (@"SELECT 
                            DeviceUID, DeviceSerialNumber, DeviceType, DeviceState, DeregisteredUTC, ModuleType, MainboardSoftwareVersion, RadioFirmwarePartNumber, GatewayFirmwarePartNumber, DataLinkType,
                            LastActionedUTC AS LastActionedUtc
                          FROM Device
                          WHERE DeviceUID = @deviceUid"
                          , new { deviceUid = device.DeviceUID }
                      )).FirstOrDefault();
          });

          if (existing == null)
          {
            if (eventType == "CreateDeviceEvent")
            {
              upsertedCount = await CreateDevice(device, existing);
            }

            if (eventType == "UpdatedeviceEvent")
            {
              upsertedCount = await UpdateDevice(device, existing);
            }
          }
          log.LogDebug("DeviceRepository: upserted {0} rows", upsertedCount);
          log.LogInformation("Event stored SUCCESS: {0}, {1}", eventType, JsonConvert.SerializeObject(device));
          return upsertedCount;
        }
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }

    private async Task<int> CreateDevice(Models.Device device, Models.Device existing)
    {
      try
      {
        await PerhapsOpenConnection();
        if (existing == null)
        {
          const string upsert =
              @"INSERT Device
                    (DeviceUID,  DeviceSerialNumber,  DeviceType,   DeviceState,  DeregisteredUTC,  ModuleType,  MainboardSoftwareVersion,  RadioFirmwarePartNumber,  GatewayFirmwarePartNumber, DataLinkType, LastActionedUTC )
                  VALUES
                   (@DeviceUID, @DeviceSerialNumber, @LDeviceType, @DeviceState, @DeregisteredUTC, @ModuleType, @MainboardSoftwareVersion, @RadioFirmwarePartNumber, @GatewayFirmwarePartNumber, @DataLinkType, @LastActionedUtc)
              ";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return await Connection.ExecuteAsync(upsert, device);
          });
        }
        else if (device.LastActionedUtc >= existing.LastActionedUtc)
        {
          const string update =
              @"UPDATE Device                
                  SET DeviceSerialNumber = @DeviceSerialNumber,
                      DeviceType = @DeviceType,
                      DeviceState = @DeviceState,
                      DeregisteredUTC = @DeregisteredUTC,
                      ModuleType = @ModuleType,
                      MainboardSoftwareVersion = @MainboardSoftwareVersion,
                      RadioFirmwarePartNumber = @RadioFirmwarePartNumber,      
                      GatewayFirmwarePartNumber = @GatewayFirmwarePartNumber,  
                      DataLinkType = @DataLinkType,     
                      LastActionedUTC = @LastActionedUtc
                WHERE DeviceUID = @deviceUid";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return await Connection.ExecuteAsync(update, device);
          });
        }
        // Create received after Update
        //     not required for Device, as everything (strangley) in an Update is also in the Create, so nothing to fill in
        
        return await Task.FromResult(0);
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }

    private async Task<int> UpdateDevice(Models.Device device, Models.Device existing)
    {
      try
      {
        await PerhapsOpenConnection();
        if (existing != null)
        {
          if (device.LastActionedUtc >= existing.LastActionedUtc)
          {
            const string update =
              @"UPDATE Device                
                  SET DeviceSerialNumber = @DeviceSerialNumber,
                      DeviceType = @DeviceType,
                      DeviceState = @DeviceState,
                      DeregisteredUTC = @DeregisteredUTC,
                      ModuleType = @ModuleType,
                      MainboardSoftwareVersion = @MainboardSoftwareVersion,
                      RadioFirmwarePartNumber = @RadioFirmwarePartNumber,      
                      GatewayFirmwarePartNumber = @GatewayFirmwarePartNumber,  
                      DataLinkType = @DataLinkType,     
                      LastActionedUTC = @LastActionedUtc
                WHERE DeviceUID = @deviceUid";
            return await dbAsyncPolicy.ExecuteAsync(async () =>
            {
              return await Connection.ExecuteAsync(update, device);
            });
          }
          else
          {
            log.LogDebug(
                "DeviceRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
                existing.LastActionedUtc, device.LastActionedUtc);
          }
        }
        else // doesn't exist already
        {
          log.LogDebug("DeviceRepository: Inserted an UpdateDeviceEvent as none existed.  newActionedUTC{0}",
              device.LastActionedUtc);

          const string upsert =
              @"INSERT Device
                    (DeviceUID,  DeviceSerialNumber,  DeviceType,   DeviceState,  DeregisteredUTC,  ModuleType,  MainboardSoftwareVersion,  RadioFirmwarePartNumber,  GatewayFirmwarePartNumber, DataLinkType, LastActionedUTC )
                  VALUES
                   (@DeviceUID, @DeviceSerialNumber, @LDeviceType, @DeviceState, @DeregisteredUTC, @ModuleType, @MainboardSoftwareVersion, @RadioFirmwarePartNumber, @GatewayFirmwarePartNumber, @DataLinkType, @LastActionedUtc)
              ";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return await Connection.ExecuteAsync(upsert, device);
          });
        }
        return await Task.FromResult(0);
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }
    #endregion device


    #region AssociateDeviceAsset
    /// <summary>
    /// All detail-related columns can be inserted, 
    ///    but only certain columns can be updated.
    ///    on deletion, a flag will be set.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertDeviceAssetDetail(AssetDevice.Data.Models.AssetDevice assetDevice, string eventType)
    {
      try
      {
        {
          await PerhapsOpenConnection();
          log.LogDebug("DeviceRepository: Upserting eventType{0} deviceUid={1}", eventType, assetDevice.DeviceUID);
          var upsertedCount = 0;

          var existing = await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return (await Connection.QueryAsync<AssetDevice.Data.Models.AssetDevice>
                      (@"SELECT DeviceUID, AssetUID, LastActionedUTC
                          FROM AssetDevice
                          WHERE DeviceUID = @deviceUid"
                          , new { deviceUid = assetDevice.DeviceUID }
                      )).FirstOrDefault();
          });

          if (existing == null)
          {
            if (eventType == "AssociateDeviceAssetEvent")
            {
              upsertedCount = await AssociateDeviceAsset(assetDevice, existing);
            }

            if (eventType == "DissociateDeviceAssetEvent")
            {
              upsertedCount = await DissociateDeviceAsset(assetDevice, existing);
            }
          }
          log.LogDebug("DeviceRepository: upserted {0} rows", upsertedCount);
          log.LogInformation("Event stored SUCCESS: {0}, {1}", eventType, JsonConvert.SerializeObject(assetDevice));
          return upsertedCount;
        }
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }

    private async Task<int> AssociateDeviceAsset(AssetDevice.Data.Models.AssetDevice assetDevice, AssetDevice.Data.Models.AssetDevice existing)
    {
      try
      {
        await PerhapsOpenConnection();
        if (existing == null)
        {
          const string upsert =
              @"INSERT AssetDevice
                    (DeviceUID, AssetUID, LastActionedUTC )
                  VALUES
                   (@DeviceUID, @AssetUID, @LastActionedUtc)
              ";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return await Connection.ExecuteAsync(upsert, assetDevice);
          });
        }
        else if (assetDevice.LastActionedUtc >= existing.LastActionedUtc)
        {
          const string update =
              @"UPDATE AssetDevice                
                  SET AssetUID = @assetUid
                WHERE DeviceUID = @deviceUid";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return await Connection.ExecuteAsync(update, assetDevice);
          });
        }
        // Create received after Update
        //     not required for assetDevice, as everything in an Update is also in the Create, so nothing to fill in

        return await Task.FromResult(0);
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }

    private async Task<int> DissociateDeviceAsset(AssetDevice.Data.Models.AssetDevice assetDevice, AssetDevice.Data.Models.AssetDevice existing)
    {
      // this is disastrous for the timing: Associate then Dissociate, then Associate received again - as it will be left as associated.
      try
      {
        await PerhapsOpenConnection();
        if (existing != null)
        {
          if (assetDevice.LastActionedUtc >= existing.LastActionedUtc)
          {
            const string update =
              @"DELETE AssetDevice                                 
                WHERE DeviceUID = @deviceUid
                  AND AssetUID = @assetUid";
            return await dbAsyncPolicy.ExecuteAsync(async () =>
            {
              return await Connection.ExecuteAsync(update, assetDevice);
            });
          }
          else
          {
            log.LogDebug(
                "DeviceRepository: old delete event ignored as a newer one exists currentActionedUTC{0} newActionedUTC{1}",
                existing.LastActionedUtc, assetDevice.LastActionedUtc);
          }
        }
        // else doesn't exist already, do nothing
        
        return await Task.FromResult(0);
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }
    #endregion AssociateDeviceAsset


    #region getters

    public async Task<Models.Device> GetDevice(string deviceUid)
    {
      try
      {
        await PerhapsOpenConnection();
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          return (await Connection.QueryAsync<Models.Device>
                      (@"SELECT 
                            DeviceUID, DeviceSerialNumber, DeviceType, DeviceState, DeregisteredUTC, ModuleType, MainboardSoftwareVersion, RadioFirmwarePartNumber, GatewayFirmwarePartNumber, DataLinkType,
                            LastActionedUTC AS LastActionedUtc
                          FROM Device
                          WHERE DeviceUID = @deviceUid"
                      , new { deviceUid }
                  )).FirstOrDefault();
        });
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }
    #endregion
  }
}