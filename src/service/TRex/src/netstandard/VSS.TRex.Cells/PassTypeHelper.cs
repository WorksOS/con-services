using System;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Cells
{
  /// <summary>
  /// Helper class that maps the two bites in the GPSMode byte to the four pass type values
  /// </summary>
  public static class PassTypeHelper
  {
    /// <summary>
    /// Sets the appropriate bits in the GPSModeStore corresponding to the desired pass type
    /// </summary>
    /// <param name="value"></param>
    /// <param name="passType"></param>
    /// <returns></returns>
    public static byte SetPassType(byte value, PassType passType)
    {
      byte result = value;

      switch (passType)
      {
        case PassType.Front: // val 0
          {
            result = BitFlagHelper.BitOff(result, (int)GPSFlagBits.GPSSBit6);
            result = BitFlagHelper.BitOff(result, (int)GPSFlagBits.GPSSBit7);
            break;
          }
        case PassType.Rear: // val 1
          {
            result = BitFlagHelper.BitOn(result, (int)GPSFlagBits.GPSSBit6);
            result = BitFlagHelper.BitOff(result, (int)GPSFlagBits.GPSSBit7);
            break;
          }
        case PassType.Track: // val 2
          {
            result = BitFlagHelper.BitOff(result, (int)GPSFlagBits.GPSSBit6);
            result = BitFlagHelper.BitOn(result, (int)GPSFlagBits.GPSSBit7);
            break;
          }
        case PassType.Wheel: // val 3
          {
            result = BitFlagHelper.BitOn(result, (int)GPSFlagBits.GPSSBit6);
            result = BitFlagHelper.BitOn(result, (int)GPSFlagBits.GPSSBit7);
            break;
          }
        default:
          {
            throw new ArgumentException($"Unknown pass type supplied to SetPassType {passType}", nameof(passType));
          }
      }

      return result;
    }

    /// <summary>
    /// Extracts the PassType enum value from the bi flags used to represent it
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PassType GetPassType(byte value)
    {
      byte testByte = 0;

      if ((value & (1 << (int)GPSFlagBits.GPSSBit6)) != 0)
      {
        testByte = 1;
      }
      if ((value & (1 << (int)GPSFlagBits.GPSSBit7)) != 0)
      {
        testByte += 2;
      }

      return (PassType)testByte;
    }

    /// <summary>
    /// Determines if a PassType encoded in the PassType enum is a member of the 
    /// PassTypeSet flag enum
    /// </summary>
    /// <param name="PassTypeSet"></param>
    /// <param name="PassType"></param>
    /// <returns></returns>
    public static bool PassTypeSetContains(PassTypeSet PassTypeSet, PassType PassType)
    {
      return ((int)PassTypeSet & (1 << (int)PassType)) != 0;
    }
  }
}
