using System.Runtime.CompilerServices;

namespace VSS.TRex.IO
{
  public static class Utilities
  {
    /// <summary>
    /// Fast determination of whole number binary log less than or equal to whole log2 of an integer number
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Log2(int n)
    {
      int bits = 0;
      if (n > 32767)
      {
        n >>= 16;
        bits += 16;
      }
      if (n > 127)
      {
        n >>= 8;
        bits += 8;
      }
      if (n > 7)
      {
        n >>= 4;
        bits += 4;
      }
      if (n > 1)
      {
        n >>= 2;
        bits += 2;
      }
      if (n > 0)
      {
        bits++;
      }

      return bits;
    }
  }
}
