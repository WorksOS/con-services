using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  /// <summary>
  ///   Describes C2S2 Device
  /// </summary>
  public class DeviceData  : IMasterDataModel
  {
    public string CustomerUID { get; set; }
    public string DeviceUID { get; set; }
    public string DeviceName { get; set; }
    public string SerialNumber { get; set; }
    
    // todoMaverick what will we get from WM Claimed/Registered/other?
    public string Status { get; set; }
    public long? ShortRaptorAssetId { get; set; }

    public DeviceData()
    { }

    public DeviceData(string customerUid, string deviceUid, string deviceName, string serialNumber, long? shortRaptorAssetId)
    {
      CustomerUID = customerUid;
      DeviceUID = deviceUid;
      DeviceName = deviceName;
      SerialNumber = serialNumber;
      ShortRaptorAssetId = shortRaptorAssetId;
    }

    public override bool Equals(object obj)
    {
      var otherDeviceData = obj as DeviceData;
      if (otherDeviceData == null) return false;
      return otherDeviceData.CustomerUID == this.CustomerUID
             && otherDeviceData.DeviceUID == this.DeviceUID
             && otherDeviceData.DeviceName == this.DeviceName
             && otherDeviceData.SerialNumber == this.SerialNumber
             && otherDeviceData.ShortRaptorAssetId == this.ShortRaptorAssetId
        ;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public List<string> GetIdentifiers() => new List<string>()
    {
      CustomerUID.ToString(),
      DeviceUID.ToString()
    };
  }
}
