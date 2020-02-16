using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssDeviceService : IBssDeviceService
  {
    const int gpsIDLength = 8;
    public ExistingDeviceDto GetDeviceByIbKey(string ibKey)
    {
      var deviceInfo = (from device in Data.Context.OP.DeviceReadOnly
                        let owner =(from o in Data.Context.OP.CustomerReadOnly where o.BSSID == device.OwnerBSSID select o).FirstOrDefault()
                        let asset = (from a in Data.Context.OP.AssetReadOnly where a.fk_DeviceID == device.ID select a).FirstOrDefault()
                        where device.IBKey == ibKey
                        select new
                                 {
                                   Device = device,
                                   Owner = owner,
                                   Asset = asset
                                 }).SingleOrDefault();

      var existingDevice = new ExistingDeviceDto();

      if(deviceInfo == null || deviceInfo.Device == null)
        return existingDevice;

      existingDevice.Id = deviceInfo.Device.ID;
      existingDevice.MapDevice(deviceInfo.Device);
      
      if(deviceInfo.Asset != null)
      {
        existingDevice.AssetId = deviceInfo.Asset.AssetID;
        existingDevice.Asset.InsertUtc = deviceInfo.Asset.InsertUTC;
        existingDevice.Asset.MapAsset(deviceInfo.Asset);
      }

      if(deviceInfo.Owner == null)
        return existingDevice;

      existingDevice.OwnerId = deviceInfo.Owner.ID;
      existingDevice.Owner.MapOwner(deviceInfo.Owner);

      if(existingDevice.Owner.Type == CustomerTypeEnum.Dealer)
      {
        existingDevice.Owner.RegisteredDealerId = deviceInfo.Owner.ID;
        existingDevice.Owner.RegisteredDealerNetwork = (DealerNetworkEnum) deviceInfo.Owner.fk_DealerNetworkID;
        return existingDevice;
      }

      var registeredDealer = Services.Customers().GetParentDealerByChildCustomerId(deviceInfo.Owner.ID);

      if(registeredDealer != null && registeredDealer.Item1 != null && registeredDealer.Item1.IsActivated)
      {
        existingDevice.Owner.RegisteredDealerId = registeredDealer.Item1.ID;
        existingDevice.Owner.RegisteredDealerNetwork = (DealerNetworkEnum) registeredDealer.Item1.fk_DealerNetworkID;
      }

      return existingDevice;
    }

    public Device GetDeviceByGpsDeviceId(string gpsDeviceId, DeviceTypeEnum? type)
    {
      if(type == null)
        return null;

      gpsDeviceId = TrucateLeadingZeroForXCheckDeviceID(gpsDeviceId, type.Value);

      int deviceTypeId = (int) type;
      var device = (from d in Data.Context.OP.DeviceReadOnly 
                    where d.GpsDeviceID == gpsDeviceId && 
                      d.fk_DeviceTypeID == deviceTypeId 
                    select d).FirstOrDefault();
      return device;
    }

    public DeviceTypeEnum? GetDeviceTypeByPartNumber(string partNumber)
    {
      var devicePartNumber = (from pn in Data.Context.OP.DevicePartNumberReadOnly
                              where pn.BSSPartNumber == partNumber
                              select new {deviceTypeId = pn.fk_DeviceTypeID}).FirstOrDefault();

      if (devicePartNumber == null)
        return null;

      return (DeviceTypeEnum) devicePartNumber.deviceTypeId;
    }

    public Device CreateDevice(DeviceDto ibDevice)
    {
      Require.IsNotNull(ibDevice, "DeviceDto");
      
      if(!ibDevice.Type.HasValue)
        throw new ArgumentException("Device Type is null.");

      ibDevice.GpsDeviceId = TrucateLeadingZeroForXCheckDeviceID(ibDevice.GpsDeviceId, ibDevice.Type.Value);

      return API.Device.CreateDevice(
            Data.Context.OP,
            ibDevice.IbKey,
            ibDevice.OwnerBssId,
            ibDevice.GpsDeviceId,
            ibDevice.Type.Value,
            samplingInterval: ServiceType.DefaultSamplingInterval,
            reportingInterval: ServiceType.DefaultReportingInterval,
            lowPowerInterval: ServiceType.DefaultLowPowerInterval,
            bitReportInterval: ServiceType.DefaultBitPacketInterval,
            isReadOnly: IsDeviceReadOnly(ibDevice.Type.Value, ibDevice.IbKey)
        );
    }

    public IList<DevicePersonality> CreateDevicePersonality(AssetDeviceContext context)
    {
      Require.IsNotNull(context.IBDevice, "DeviceContext.IBDevice");

      return API.Device.CreateDevicePersonality(
            Data.Context.OP,
            context.Device.Id,
            context.IBDevice.FirmwareVersionId,
            context.IBDevice.SIMSerialNumber,
            context.IBDevice.PartNumber,
            context.IBDevice.CellularModemIMEA,
            context.IBDevice.GpsDeviceId,
            context.IBDevice.Type.Value);
    }

    public bool TransferOwnership(long deviceId, string newOwnerBssId)
    {
      if(deviceId == default(long))
        throw new ArgumentException("deviceId must have value.", "deviceId");

      if(string.IsNullOrWhiteSpace(newOwnerBssId))
        throw new ArgumentException("newOwnerBssId must have value.", "newOwnerBssId");

      var deviceToTransfer = (from d in Data.Context.OP.Device where d.ID == deviceId select d).SingleOrDefault();

      if(deviceToTransfer.OwnerBSSID == newOwnerBssId)
        throw new InvalidOperationException("Current OwnerBSSID and New OwnerBSSID are the same.");

      var newOwnerParam = new Param {Name = "OwnerBSSID", Value = newOwnerBssId};
      var transferedDevice = API.Update(Data.Context.OP, deviceToTransfer, new List<Param> {newOwnerParam});
      return transferedDevice.OwnerBSSID == newOwnerBssId;
    }

    /// <summary>
    /// Updates the device state to Register thru DeviceRegistration message
    /// </summary>
    /// <param name="deviceID">Device for which the state is being changed</param>
    public void RegisterDevice(long deviceID)
    {
      DeviceStateEnum deviceState;

      int keyDateNow = DateTime.UtcNow.KeyDate();
      var deviceHasActiveCoreService = (from s in Data.Context.OP.ServiceReadOnly
                                        join st in Data.Context.OP.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                        where s.fk_DeviceID == deviceID
                                        && s.ActivationKeyDate <= keyDateNow
                                        && s.CancellationKeyDate == DotNetExtensions.NullKeyDate
                                        && st.IsCore == true
                                        select s).Any();
      if (deviceHasActiveCoreService)
        deviceState = DeviceStateEnum.Subscribed;
      else
        deviceState = DeviceStateEnum.Provisioned;
      API.Device.UpdateDeviceState(deviceID, deviceState);
    }

    public void ReconfigureDevice(long oldDeviceID, string oldGPSDeviceID, DeviceTypeEnum oldDeviceType, long newDeviceID, string newGPSDeviceID, DeviceTypeEnum newDeviceType, DateTime actionUtc)
    {
      var keyDate = DateTime.UtcNow.KeyDate();

      var servicesToTransfer = (from s in Data.Context.OP.Service
                                join st in Data.Context.OP.ServiceType on s.fk_ServiceTypeID equals st.ID
                                where s.fk_DeviceID == newDeviceID &&
                                s.ActivationKeyDate <= keyDate &&
                                s.CancellationKeyDate == DotNetExtensions.NullKeyDate
                                //sort the services in the assending order so that the device is configured for 
                                //the service plans from low to high instead of random order
                                orderby st.ID
                                select new { Service = s, IsCore = st.IsCore }).ToList();

      foreach(var service in servicesToTransfer.Select(x => x.Service))
      {
        //terminate the services for the old gpsdeviceid in nh_raw
        DeviceConfig.ConfigureDeviceForServicePlan(
        Data.Context.OP,
        oldGPSDeviceID,
        oldDeviceType,
        false,
        (ServiceTypeEnum)service.fk_ServiceTypeID,
        GetServicePlanIDs(null));

        //set up the services for the new gpsdeviceid in nh_raw
        DeviceConfig.ConfigureDeviceForServicePlan(
        Data.Context.OP,
        newGPSDeviceID,
        newDeviceType,
        true,
        (ServiceTypeEnum)service.fk_ServiceTypeID,
        GetServicePlanIDs(servicesToTransfer.Select(t => new Tuple<long, bool>(t.Service.fk_ServiceTypeID, t.IsCore)).ToList()));
      }
    }

    #region privates
    public bool IsDeviceReadOnly(DeviceTypeEnum deviceType, string ibKey)
    {
      if (API.Device.IsProductLinkDevice(deviceType) &&
          !ibKey.StartsWith("-") &&
          DeviceConfig.IsEnvironmentProd())
        return false;

      return true;
    }

    private List<DeviceConfig.ServicePlanIDs> GetServicePlanIDs(List<Tuple<long, bool>> servicePlanIDs)
    {
      List<DeviceConfig.ServicePlanIDs> IDs = new List<DeviceConfig.ServicePlanIDs>();

      if (servicePlanIDs == null)
        return IDs;

      foreach (var svc in servicePlanIDs)
        IDs.Add(new DeviceConfig.ServicePlanIDs() { PlanID = svc.Item1, IsCore = svc.Item2 });

      return IDs;
    }

    /// <summary>
    /// In crosscheck devices, there are situations where the IB message may contain a leading zero if the serial number is only 7 digits(it becomes 8 digits with zero).
    /// The device wouldn't send this leading zero when it sends messages, so there will be a disconnect between the gpsdeviceid in VL and the gpsdeviceid that the device reports
    /// To resolve this, we are removing the leading zero from crosscheck gpsdeviceid.
    /// </summary>
    /// <param name="gpsDeviceID"></param>
    /// <param name="deviceType"></param>
    /// <returns></returns>
    private string TrucateLeadingZeroForXCheckDeviceID(string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      return (DeviceTypeEnum.CrossCheck == deviceType && gpsDeviceID.StartsWith("0") && gpsIDLength == gpsDeviceID.Length) ? gpsDeviceID.Remove(0, 1) : gpsDeviceID;
    }

    #endregion
    
    public void UpdateDeviceState(long deviceId, VSS.Hosted.VLCommon.DeviceStateEnum deviceState)
    {
      API.Device.UpdateDeviceState(deviceId, deviceState);
    }

    public void DeregisterDeviceState(long deviceId, DeviceStateEnum deviceState, DateTime deregesteredUTC)
    {
      API.Device.DeregisterDeviceState(deviceId, deviceState, deregesteredUTC);
    }

    public void UpdateDeviceOwnerBssIds(string oldBssId, string newBssId)
    {
      var devices = (from d in Data.Context.OP.Device
                     where d.OwnerBSSID == oldBssId
                     select d).ToList();

      foreach (var device in devices)
      {
        var modifiedProperties = new List<Param> { new Param { Name = "OwnerBSSID", Value = newBssId } };
        API.Update(Data.Context.OP, device, modifiedProperties);
      }
    }
  }
}
