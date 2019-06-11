namespace VSS.TRex.Common.Utilities
{
  public static class PassCountSize
  {
    public const byte ONE_BYTE = 1;
    public const byte TWO_BYTES = 2;
    public const byte FOUR_BYTES = 3;

    /// <summary>
    /// Determine the sizing increment needed to store pass counts (1 for less than 256, 2 for less than 32768, 3 otherwise
    /// </summary>
    /// <param name="MaxPassCounts"></param>
    /// <returns></returns>
    public static int Calculate(int MaxPassCounts)
    {
      return MaxPassCounts < (1 << 8) ? ONE_BYTE : MaxPassCounts < (1 << 15) ? TWO_BYTES : FOUR_BYTES;
    }
  }
}
