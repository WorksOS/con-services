using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  /// <summary>
  ///   Describes C2S2 Device
  /// </summary>
  public class DeviceData  : IMasterDataModel
  {
    public string AccountTrn { get; set; }
    public string DeviceTrn { get; set; }
    public string DeviceName { get; set; }
    public long ShortRaptorAssetId { get; set; }
    
    public string SerialNumber { get; set; }
    
    public DeviceData()
    { }

    public DeviceData(string accountTrn, string deviceTrn, string deviceName, long shortRaptorAssetId,
        string serialNumber)
    {
      AccountTrn = accountTrn;
      DeviceTrn = deviceTrn;
      DeviceName = deviceName;
      ShortRaptorAssetId = shortRaptorAssetId;
      SerialNumber = serialNumber;
    }

    public override bool Equals(object obj)
    {
      var otherDeviceData = obj as DeviceData;
      if (otherDeviceData == null) return false;
      return otherDeviceData.AccountTrn == this.AccountTrn
             && otherDeviceData.DeviceTrn == this.DeviceTrn
             && otherDeviceData.DeviceName == this.DeviceName
             && otherDeviceData.ShortRaptorAssetId == this.ShortRaptorAssetId
             && otherDeviceData.SerialNumber == this.SerialNumber
        ;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public List<string> GetIdentifiers() => new List<string>()
    {
      AccountTrn.ToString(),
      DeviceTrn.ToString()
    };
  }
}
