using System;
using VSS.Common.Abstractions.Clients.CWS.Enums;

namespace VSS.TRex.Common.Utilities
{
  public static class MachineSerialUtilities
  {
    public static CWSDeviceTypeEnum MapSerialToModel(string serial)
    {
      if (serial.EndsWith("SM"))
      {
        return CWSDeviceTypeEnum.CB430;
      }
      else if (serial.EndsWith("SV"))
      {
        return CWSDeviceTypeEnum.CB450;
      }
      else if (serial.EndsWith("SW"))
      {
        return CWSDeviceTypeEnum.CB460;
      }
      else if (serial.EndsWith("YU"))
      {
        // Earthworks serialNumber ends YU
        // 1234J501YU = EC520   (Non Wi-Fi version)
        // 1234J001YU = EC520-W (Wi-Fi version)
        var theJIndex = serial.IndexOf('J');
        if (theJIndex > -1 && char.IsNumber(serial[theJIndex + 1]))
          if ((serial[theJIndex + 1] - '0') >= 5)
             return CWSDeviceTypeEnum.EC520;
          else
             return CWSDeviceTypeEnum.EC520W;

        return CWSDeviceTypeEnum.Unknown;
      }
      else if (!string.IsNullOrEmpty(serial))
      {
        return CWSDeviceTypeEnum.Unknown;
      }
      throw new ArgumentException("No mapping exists for this serial number");
    }
  }
}
