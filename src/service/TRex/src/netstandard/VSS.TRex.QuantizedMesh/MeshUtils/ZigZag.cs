using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  /// <summary>
  /// encryption that helps compression when gzipping
  /// </summary>
  public class ZigZag
  {
    public static long Decode(long n)
    {
      return (n >> 1) ^ (-(n & 1));
    }

    public static long Encode(long n)
    {
      return (n << 1) ^ (n >> 31);
    }

  }
}
