using System;

namespace VSS.Productivity3D.Project.Abstractions.Extensions
{
  public static class OffsetExtensions
  {
    public static bool EqualsToNearestMillimeter(this double? offset1, double? offset2)
    {
      return (offset1 ?? 0).EqualsToNearestMillimeter(offset2 ?? 0);
    }

    public static bool EqualsToNearestMillimeter(this double? offset1, double offset2)
    {
      return (offset1 ?? 0).EqualsToNearestMillimeter(offset2);
    }

    public static bool EqualsToNearestMillimeter(this double offset1, double? offset2)
    {
      return offset1.EqualsToNearestMillimeter(offset2 ?? 0);
    }

    public static bool EqualsToNearestMillimeter(this double offset1, double offset2)
    {
      return Math.Round(offset1, 3) == Math.Round(offset2, 3);
    }
  }
}
