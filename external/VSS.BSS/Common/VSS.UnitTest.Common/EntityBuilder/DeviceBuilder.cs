using System;
using System.Collections.Generic;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class DeviceBuilder 
  {
    #region Defaults

    private int _id = IdGen.GetId();
    private string _gpsDeviceId = "GPS_DEVICE_ID";
    private DeviceTypeEnum _deviceType = DeviceTypeEnum.MANUALDEVICE;
    private string _ibKey = "IB_KEY_" + IdGen.GetId();
    private string _ownerBssId = "OWNER_BSS_ID_" + IdGen.GetId();
    private DeviceStateEnum _deviceState = DeviceStateEnum.Provisioned;
    private DateTime _updateUtc = DateTime.UtcNow;
    private DateTime? _deregisteredUtc = null;
    private string _deviceDetailsXML;

    private IList<Service> _services = new List<Service>();
    private bool _syncWithNhRaw;
    private bool _addOnlyToNhRaw = false;

    #endregion

    public DeviceBuilder(DeviceTypeEnum deviceType)
    {
      _deviceType = deviceType;
      _gpsDeviceId = string.Format("{0}_{1}", _gpsDeviceId, _id);
    }
    public virtual DeviceBuilder GpsDeviceId(string gpsDeviceId) 
    {
      _gpsDeviceId = gpsDeviceId;
      return this;
    }
    public virtual DeviceBuilder IbKey(string ibKey)
    {
      _ibKey = ibKey;
      return this;
    }
    public virtual DeviceBuilder OwnerBssId(string ownerBssId)
    {
      _ownerBssId = ownerBssId;
      return this;
    }
    public virtual DeviceBuilder DeviceState(DeviceStateEnum deviceState)
    {
      _deviceState = deviceState;
      return this;
    }
    public virtual DeviceBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public virtual DeviceBuilder DeregisteredUTC(DateTime? deregisteredUtc)
    {
      _deregisteredUtc = deregisteredUtc;
      return this;
    }
    public virtual DeviceBuilder DeviceDetailsXML(string deviceDetailsXML)
    {
      _deviceDetailsXML = deviceDetailsXML;
      return this;
    }
    public virtual DeviceBuilder WithService(Service service)
    {
      _services.Add(service);
      return this;
    }
    public virtual DeviceBuilder SyncWithNhRaw()
    {
      _syncWithNhRaw = true;
      return this;
    }
    public virtual DeviceBuilder AddOnlyToNhRaw()
    {
      _addOnlyToNhRaw = true;
      return this;
    }
    public Device Build()
    {
      CheckForDuplicateDevice(ContextContainer.Current.OpContext, _gpsDeviceId, _deviceType);
      
      Device device = new Device();

      device.ID = _id;
      device.GpsDeviceID = _gpsDeviceId;
      device.IBKey = _ibKey;
      device.OwnerBSSID = _ownerBssId;
      device.UpdateUTC = _updateUtc;
      device.DeregisteredUTC = _deregisteredUtc;
      device.DeviceDetailsXML =_deviceDetailsXML;
      device.DeviceUID = Guid.NewGuid();
      device.fk_DeviceTypeID = (int)_deviceType;
      device.fk_DeviceStateID = (int)_deviceState;

      foreach (var service in _services)
      {
        device.Service.Add(service);
      }

      return device;
    }
    public virtual Device Save()
    {
      Device device = Build();

      if (_addOnlyToNhRaw)
      {
        Helpers.NHRaw.AddDeviceToRawDevice(device);
        return device;
      }

      ContextContainer.Current.OpContext.Device.AddObject(device);
      ContextContainer.Current.OpContext.SaveChanges();
     
      if(_syncWithNhRaw)
        Helpers.NHRaw.AddDeviceToRawDevice(device);

      return device;
    }

    public static void CheckForDuplicateDevice(INH_OP context, string gpsDeviceId, DeviceTypeEnum deviceType)
    {
      var deviceTypeId = (int) deviceType;

      var deviceExists = (from d in context.Device
                          where d.GpsDeviceID == gpsDeviceId 
                            && d.fk_DeviceTypeID == deviceTypeId
                          select 1).Count() > 0;

      if(deviceExists)
      {
        throw new InvalidOperationException(String.Format("Device exists for GPSDeviceID {0} Type {1}", gpsDeviceId, deviceType));
      }
    }

    public static string IMEI2UnitID(string IMEI) 
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
        }
      }
      return null;
    }
  }
}