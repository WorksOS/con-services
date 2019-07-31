using System.Diagnostics;
using System.IO;
using System.Linq;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.SVL.DXF
{
  public static class DXFUtils
  {
    public static string FormatDXFRecNum(int recNum) => $"{recNum,3}";

    public static void WriteDXFRecord(StreamWriter writer,
      int recNum,
      string recValue)
    {
      writer.WriteLine(FormatDXFRecNum(recNum));
      writer.WriteLine(recValue);
    }

    public static void WriteXYZToDXF(StreamWriter writer,
      int offset,
      double x, double y, double z,
      DistanceUnitsType OutputUnits)
    {
      WriteDXFRecord(writer, 10 + offset, NoLocaleFloatToStrF(DXFDistance(x, OutputUnits), 6));
      WriteDXFRecord(writer, 20 + offset, NoLocaleFloatToStrF(DXFDistance(y, OutputUnits), 6));
      if (z != Consts.NullDouble)
        WriteDXFRecord(writer, 30 + offset, NoLocaleFloatToStrF(DXFDistance(z, OutputUnits), 6));
    }

    public static void WriteDXFAngle(StreamWriter writer, int recNum, double angle) // angle is a mathematical angle in degrees
    {
      WriteDXFRecord(writer, recNum, NoLocaleFloatToStrF(angle, 9));
    }

    public static string DXFiseLayerName(string layerName)
    {
      const int MaxDXFNameLength = 31;
      const string DXFNameCharSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-$";

      var result = (layerName.Length < MaxDXFNameLength ? layerName : layerName.Substring(0, MaxDXFNameLength)).ToCharArray();

      for (int index = 0; index < result.Length; index++)
        if (!DXFNameCharSet.Contains(result[index]))
          result[index] = '_';

      return new string(result);
    }

    public static string NoLocaleFloatToStrF(double value, int dp)
    {
      return value.ToString($"F{dp}", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double FeetToMeters => 0.3048; // Todo: International Feet only

    public static double DistToMeters(DistanceUnitsType dist_units)
      // Returns the conversion factor required to convert a distance value in the
      //    specified units to meters
      //  NOTE: Meters is the internal distance value unit used in SVO
    {
      const double USFeetToMeters = 0.304800609601;
      const double ImperialFeetToMeters = 0.3048;
      //const double WGS84Flattening = 298.257223563;
      //const double WGS84earthRadius = 6378137.0;

      switch (dist_units)
      {
        case DistanceUnitsType.Feet: return ImperialFeetToMeters;
        case DistanceUnitsType.US_feet: return USFeetToMeters;
        case DistanceUnitsType.Meters: return 1.0;
        case DistanceUnitsType.Chains: return FeetToMeters * 66;
        case DistanceUnitsType.Links: return FeetToMeters * 66 / 100;
        case DistanceUnitsType.Yards: return FeetToMeters * 3;
        case DistanceUnitsType.Millimeters: return 0.001;
        case DistanceUnitsType.Inches: return FeetToMeters / 12;
        case DistanceUnitsType.Kilometers: return 1000;
        case DistanceUnitsType.Miles: return FeetToMeters * 5280;
        case DistanceUnitsType.Centimeters: return 0.01;
        default:
          Debug.Assert(false);
          return Consts.NullDouble;
      }
    }

    public static double DXFDistance(double value, DistanceUnitsType outputUnits)
    {
      // Takes a value in SI units (ie: meters) and converts it to the project units for
// writing out to a DXF file
      return value / DistToMeters(outputUnits);
    }
  }
}
