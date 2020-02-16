using System;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers
{
  public class UnknownDeviceException : Exception
  {
    public UnknownDeviceException(string message)
      : base(message)
    { }
  }
}
