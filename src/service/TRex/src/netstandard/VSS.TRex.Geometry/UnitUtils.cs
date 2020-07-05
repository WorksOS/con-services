using System.Diagnostics;

namespace VSS.TRex.Geometry
{
  public static class UnitUtils
  {
    public static double DistToMeters(DistanceUnitsType distUnits)
    // Returns the conversion factor required to convert a distance value in the
    //    specified units to meters
    //  NOTE: Meters is the internal distance value unit used in SVO
    {
      const double USFeetToMeters = 0.304800609601;
      const double InternationalFeetToMeters = 0.3048;
      //const double WGS84Flattening = 298.257223563;
      //const double WGS84earthRadius = 6378137.0;

      switch (distUnits)
      {
        case DistanceUnitsType.Feet: return InternationalFeetToMeters;
        case DistanceUnitsType.US_feet: return USFeetToMeters;
        case DistanceUnitsType.Meters: return 1.0;
        case DistanceUnitsType.Chains: return InternationalFeetToMeters * 66;
        case DistanceUnitsType.Links: return InternationalFeetToMeters * 66 / 100;
        case DistanceUnitsType.Yards: return InternationalFeetToMeters * 3;
        case DistanceUnitsType.Millimeters: return 0.001;
        case DistanceUnitsType.Inches: return InternationalFeetToMeters / 12;
        case DistanceUnitsType.Kilometers: return 1000;
        case DistanceUnitsType.Miles: return InternationalFeetToMeters * 5280;
        case DistanceUnitsType.Centimeters: return 0.01;
        default:
          Debug.Assert(false);
          return Consts.NullDouble;
      }
    }
  }
}
