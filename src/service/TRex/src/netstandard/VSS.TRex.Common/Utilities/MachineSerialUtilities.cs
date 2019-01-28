using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Common.Utilities
{
  public static class MachineSerialUtilities
  {
    public static string MapSerialToModel(string serial)
    {

      if (serial.EndsWith("SM"))
      {
        return "CB430";
      }
      else if (serial.EndsWith("SV"))
      {
        return "CB450";
      }
      else if (serial.EndsWith("SW"))
      {
        return "CB460";
      }
      else if (serial.EndsWith("YU"))
      {
        return "EC520";
      }
      throw new ArgumentException("No mapping exists for this serial number");
    }
  }
}
