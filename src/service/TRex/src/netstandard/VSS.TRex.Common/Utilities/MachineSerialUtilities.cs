using System;
using VSS.TRex.Common.Types;

namespace VSS.TRex.Common.Utilities
{
  public static class MachineSerialUtilities
  {
    public static MachineControlPlatformType MapSerialToModel(string serial)
    {

      if (serial.EndsWith("SM"))
      {
        return MachineControlPlatformType.CB430;
      }
      else if (serial.EndsWith("SV"))
      {
        return MachineControlPlatformType.CB450;
      }
      else if (serial.EndsWith("SW"))
      {
        return MachineControlPlatformType.CB460;
      }
      else if (serial.EndsWith("YU"))
      {
        return MachineControlPlatformType.EC520;
      }
      throw new ArgumentException("No mapping exists for this serial number");
    }
  }
}
