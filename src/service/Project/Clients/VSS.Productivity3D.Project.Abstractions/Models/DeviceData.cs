using System.Collections.Generic;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  /// <summary>
  ///   Describes C2S2 Device
  /// </summary>
  public class DeviceData  : BaseDataResult, IMasterDataModel
  {
    public string CustomerUID { get; set; }
    public string DeviceUID { get; set; }
    public string DeviceName { get; set; }
    public string SerialNumber { get; set; }

    public RelationStatusEnum RelationStatus { get; set; }

    public TCCDeviceStatusEnum TccDeviceStatus { get; set; }

    public long? ShortRaptorAssetId { get; set; }

    public DeviceData()
    { }

    public DeviceData(string customerUid, string deviceUid, string deviceName, string serialNumber, RelationStatusEnum relationStatus, TCCDeviceStatusEnum tccDeviceStatus, long? shortRaptorAssetId)
    {
      CustomerUID = customerUid;
      DeviceUID = deviceUid;
      DeviceName = deviceName;
      SerialNumber = serialNumber;
      RelationStatus = relationStatus;
      TccDeviceStatus = tccDeviceStatus;
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
             && otherDeviceData.RelationStatus == this.RelationStatus
             && otherDeviceData.TccDeviceStatus == this.TccDeviceStatus
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
