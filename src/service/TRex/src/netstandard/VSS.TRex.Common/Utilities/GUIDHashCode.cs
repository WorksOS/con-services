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
    /// Weird byte orders to deal with the funky sequence the bytes come out of the Guid.ToByteArray method
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static int Hash_Old(Guid g)
    {
      var bytes = g.ToByteArray();

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

      // ReSharper disable once IdentifierTypo
      var hilo = BitConverter.ToInt64(msb, 0) ^ BitConverter.ToInt64(lsb, 0);
      return (int)(hilo >> 32) ^ (int)hilo;
    }

    public static int Hash(Guid g)
    {
      // ReSharper disable once SuggestVarOrType_Elsewhere
      Span<byte> bytes = stackalloc byte[16];
      g.TryWriteBytes(bytes);
      
      ReadOnlySpan<byte> b1 = stackalloc byte[]
      {
        bytes[6],
        bytes[7],
        bytes[4],
        bytes[5],
        bytes[0],
        bytes[1],
        bytes[2],
        bytes[3]
      };

      ReadOnlySpan<byte> b2 = stackalloc byte[]
      {
        bytes[15],
        bytes[14],
        bytes[13],
        bytes[12],
        bytes[11],
        bytes[10],
        bytes[9],
        bytes[8]
      };

      // ReSharper disable once IdentifierTypo
      var hilo = BitConverter.ToInt64(b1) ^ BitConverter.ToInt64(b2);
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
