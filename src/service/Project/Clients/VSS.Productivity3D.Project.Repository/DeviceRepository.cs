using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Repository
{
  public class DeviceRepository : RepositoryBase, IRepository<IDeviceEvent>, IDeviceRepository
  {
    public DeviceRepository(IConfigurationStore connectionString, ILoggerFactory logger) : base(connectionString,
      logger)
    {
      Log = logger.CreateLogger<DeviceRepository>();
    }

    public async Task<int> StoreEvent(IDeviceEvent evt)
    {
      var upsertedCount = 0;
      var eventType = "Unknown";
      if (evt == null)
      {
        Log.LogWarning($"Unsupported event type");
        return 0;
      }

      Log.LogDebug($"Event type is {evt.GetType()}");
      if (evt is CreateDeviceEvent)
      {
        var device = new Device();
        var deviceEvent = (CreateDeviceEvent)evt;
        device.DeviceUID = deviceEvent.DeviceUID.ToString();
        device.ShortRaptorAssetID = -1;
        device.LastActionedUTC = deviceEvent.ActionUTC;
        eventType = "CreateDeviceEvent";
        upsertedCount = await UpsertDeviceDetail(device, eventType);
      }
      return upsertedCount;
    }


    #region device

    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertDeviceDetail(Device device, string eventType)
    {
      Log.LogDebug("DeviceRepository: Upserting eventType{0} deviceUid={1}", eventType, device.DeviceUID);
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<Device>
      (@"SELECT 
            DeviceUID, ShortRaptorAssetID, LastActionedUTC
          FROM Device
          WHERE DeviceUID = @DeviceUID"
        , new { device.DeviceUID }
      )).FirstOrDefault();

      if (eventType == "CreateDeviceEvent")
        upsertedCount = await CreateDevice(device, existing);

      //if (eventType == "UpdateDeviceEvent")
      //  upsertedCount = await UpdateDevice(device, existing);

      Log.LogDebug("DeviceRepository: upserted {0} rows", upsertedCount);
      Log.LogInformation("Event storage {0}, {1}, success: {2}", eventType, JsonConvert.SerializeObject(device),
        upsertedCount);
      return upsertedCount;
    }

    private async Task<int> CreateDevice(Device device, Device existing)
    {
      if (existing == null)
      {
        // the DB will generate the ShortRaptorAssetId
        const string upsert =
          @"INSERT Device
                (DeviceUID, LastActionedUTC )
              VALUES
                (@DeviceUID, @LastActionedUTC)";
        return await ExecuteWithAsyncPolicy(upsert, device);
      }

      return await Task.FromResult(0);
    }

    #endregion device


    #region getters

    public async Task<Device> GetDevice(string deviceUid)
    {
      return (await QueryWithAsyncPolicy<Device>
      (@"SELECT 
              DeviceUID, ShortRaptorAssetID,  LastActionedUTC
            FROM Device
            WHERE DeviceUID = @DeviceUID"
        , new { DeviceUID = deviceUid }
      )).FirstOrDefault();
    }

    public async Task<Device> GetDevice(int shortRaptorAssetId)
    {
      return (await QueryWithAsyncPolicy<Device>
      (@"SELECT 
              DeviceUID, ShortRaptorAssetId,  LastActionedUTC AS LastActionedUtc
            FROM Device
            WHERE ShortRaptorAssetId = @shortRaptorAssetId"
        , new { shortRaptorAssetId }
      )).FirstOrDefault();
    }

    #endregion
  }
}
