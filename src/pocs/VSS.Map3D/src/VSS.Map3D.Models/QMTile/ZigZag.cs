namespace VSS.Map3D.Models.QMTile
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
