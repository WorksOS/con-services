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
    /// Not as fast at the .Net implemnentation but is consistant with the java implementation.
    /// Weird byte orders to deal with the funky sequence the bytes come out of the the Guid.ToByteArray method
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    public static int Hash(Guid g)
    {
      byte[] bytes = g.ToByteArray();

      var msb = new byte[] {
        bytes[6],
        bytes[7],
        bytes[4],
        bytes[5],
        bytes[0],
        bytes[1],
        bytes[2],
        bytes[3] };

      var lsb = new byte[] {
        bytes[15],
        bytes[14],
        bytes[13],
        bytes[12],
        bytes[11],
        bytes[10],
        bytes[9],
        bytes[8]
        };

      long hilo = BitConverter.ToInt64(msb, 0) ^ BitConverter.ToInt64(lsb, 0);
      return ((int)(hilo >> 32)) ^ (int)hilo;
    }
  }
}
