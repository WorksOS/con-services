namespace VSS.TRex.Common.Extensions
{
  public static class IntArrayExtensions
  {
    public static short MinValue(this short[] values)
    {
      short result = short.MaxValue;

      for (int i = 0, length = values.Length; i < length; i++)
        if (values[i] < result)
          result = values[i];

      return result;
    }

    public static short MaxValue(this short[] values)
    {
      short result = short.MinValue;

      for (int i = 0, length = values.Length; i < length; i++)
        if (values[i] > result)
          result = values[i];

      return result;
    }

    public static int MinValue(this int[] values)
    {
      int result = int.MaxValue;

      for (int i = 0, length = values.Length; i < length; i++)
        if (values[i] < result)
          result = values[i];

      return result;
    }

    public static int MaxValue(this int[] values)
    {
      int result = int.MinValue;

      for (int i = 0, length = values.Length; i < length; i++)
        if (values[i] > result)
          result = values[i];

      return result;
    }
  }
}
