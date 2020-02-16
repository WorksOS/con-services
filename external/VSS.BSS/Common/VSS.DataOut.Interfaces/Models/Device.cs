using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models
{
  [Serializable]
  public class Device
  {
    public string DeviceId { get; set; }

    public string DeviceType { get; set; }
  }
}
