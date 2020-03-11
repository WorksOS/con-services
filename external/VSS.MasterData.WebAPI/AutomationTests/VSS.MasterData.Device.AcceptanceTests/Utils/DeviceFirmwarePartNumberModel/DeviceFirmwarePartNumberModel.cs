using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Device.AcceptanceTests.Utils.DeviceFirmwarePartNumberModel
{
  public class Asset
  {
    public string AssetUid { get; set; }
    public string MakeCode { get; set; }
    public string SerialNumberVin { get; set; }
  }

  public class Device
  {
    public string DeviceUID { get; set; }
    public string DeviceId { get; set; }
    public string DeviceType { get; set; }
  }

  public class Timestamp
  {
    public DateTime EventUtc { get; set; }
    public DateTime ReceivedUtc { get; set; }
  }

  public class DeviceFirmwarePartNumberModel
  {
    public string Description { get; set; }
    public string Value { get; set; }
    public string MessageHash { get; set; }
    public Asset Asset { get; set; }
    public Device Device { get; set; }
    public Timestamp Timestamp { get; set; }
  }


}
