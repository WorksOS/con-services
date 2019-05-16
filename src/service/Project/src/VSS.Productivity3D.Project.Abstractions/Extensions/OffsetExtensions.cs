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
      const double ONE_MM = 0.001;
      return Math.Abs(offset1 - offset2) < ONE_MM;
    }
  }
}
