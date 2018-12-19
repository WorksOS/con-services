using System;

namespace VSS.TRex.Common.Utilities
{
  /// <summary>
  /// Implements a hashing function for GUIDs for TRex to use as a partitioning function for Ignite
  /// which is not affected by underlying hashing implementation changes in the .Net platform
  /// </summary>
  public static class GuidHashCode
  {
    /// <summary>
    /// Performs a byte-wise hash of the content of the GUID. Unfortunately this is not as fast as the .Net
    /// implementation which uses an unsafe context XOR of the four internal integers contained in the GUID
    /// (which is in itself may not not stable due to hardware big/little Endianness)
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    public static int Hash(Guid g)
    {
      byte[] b = g.ToByteArray();

      return ((b[0]  << 24) | (b[1]  << 16) | (b[2]  << 8) | b[3]) ^
             ((b[4]  << 24) | (b[5]  << 16) | (b[6]  << 8) | b[7]) ^
             ((b[8]  << 24) | (b[9]  << 16) | (b[10] << 8) | b[11]) ^
             ((b[12] << 24) | (b[13] << 16) | (b[14] << 8) | b[15]);
    }
  }
}
