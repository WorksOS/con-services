using System;
using System.Diagnostics.CodeAnalysis;

namespace VSS.TRex.Common.Utilities
{
  /// <summary>
  /// Implements a hashing function for GUIDs for TRex to use as a partitioning function for Ignite
  /// which is not affected by underlying hashing implementation changes in the .Net platform
  /// </summary>
  public static class GuidHashCode
  {
    /// <summary>
    /// Not as fast at the .Net implementation but is consistent with the java implementation.
    /// Weird byte orders to deal with the funky sequence the bytes come out of the the Guid.ToByteArray method
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    public static int Hash_Old(Guid g)
    {
      byte[] bytes = g.ToByteArray();

      var msb = new [] {
        bytes[6],
        bytes[7],
        bytes[4],
        bytes[5],
        bytes[0],
        bytes[1],
        bytes[2],
        bytes[3] };

      var lsb = new [] {
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
      return (int)(hilo >> 32) ^ (int)hilo;
    }

    public static int Hash(Guid g)
    {
      byte[] bytes = g.ToByteArray();

      var b = new[] // TODO: Declare this as stackalloc'ed when we move to .Net Core
      {
        bytes[6],
        bytes[7],
        bytes[4],
        bytes[5],
        bytes[0],
        bytes[1],
        bytes[2],
        bytes[3],
        bytes[15],
        bytes[14],
        bytes[13],
        bytes[12],
        bytes[11],
        bytes[10],
        bytes[9],
        bytes[8]
      };

      long hilo = BitConverter.ToInt64(b, 0) ^ BitConverter.ToInt64(b, 8);
      return (int) (hilo >> 32) ^ (int) hilo;
    }    

    /* This unsafe implementation has essentially identical performance to Hash()
    public static unsafe int HashExUnsafe(Guid g)
    {
      byte[] bytes = g.ToByteArray();

      var b = new[]
      {
        bytes[6],
        bytes[7],
        bytes[4],
        bytes[5],
        bytes[0],
        bytes[1],
        bytes[2],
        bytes[3],
        bytes[15],
        bytes[14],
        bytes[13],
        bytes[12],
        bytes[11],
        bytes[10],
        bytes[9],
        bytes[8]
      };

      fixed (byte* pbyte = &b[0])
      {
          long hilo = *((long*) pbyte) ^ *((long*) (pbyte + 8));
          return (int)(hilo >> 32) ^ (int)hilo;
      }
    }
   */
  }
}
