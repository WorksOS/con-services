using System.Diagnostics;
using System.IO;
using System.Linq;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.SVL.DXF
{
  public static class DXFUtils
  {
    public static string FormatDXFRecNum(int RecNum) => $"{RecNum:3D}";

    public static void WriteDXFRecord(StreamWriter writer,
      int RecNum,
      string RecValue)
    {
      writer.WriteLine(FormatDXFRecNum(RecNum));
      writer.WriteLine(RecValue);
    }

    public static void WriteXYZToDXF(StreamWriter writer,
      int Offset,
      double X, double Y, double Z,
      distance_units_type OutputUnits)
    {
      WriteDXFRecord(writer, 10 + Offset, NoLocaleFloatToStrF(DXFDistance(X, OutputUnits), 6));
      WriteDXFRecord(writer, 20 + Offset, NoLocaleFloatToStrF(DXFDistance(Y, OutputUnits), 6));
      if (Z != Consts.NullDouble)
        WriteDXFRecord(writer, 30 + Offset, NoLocaleFloatToStrF(DXFDistance(Z, OutputUnits), 6));
    }

    public static void WriteDXFAngle(StreamWriter writer, int RecNum, double Angle) // Angle is a mathematical angle in degrees
    {
      WriteDXFRecord(writer, RecNum, NoLocaleFloatToStrF(Angle, 9));
    }

    public static string DXFiseLayerName(string LayerName)
    {
      const int MaxDXFNameLength = 31;
      const string DXFNameCharSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-$";

      var Result = LayerName.Substring(0, MaxDXFNameLength).ToCharArray();

      for (int index = 0; index < Result.Length; index++)
        if (!DXFNameCharSet.Contains(Result[index]))
          Result[index] = '_';

      return new string(Result);
    }

    public static string NoLocaleFloatToStrF(double value, int dp)
    {
      return value.ToString($"F{dp}", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double FeetToMetres => 0.3048; // Todo: International feet only

    public static double DistToMetres(distance_units_type dist_units)
      // Returns the conversion factor required to convert a distance value in the
      //    specified units to meters
      //  NOTE: Meters is the internal distance value unit used in SVO
    {
      const double USFeetToMetres = 0.304800609601;
      const double ImperialFeetToMetres = 0.3048;
      //const double WGS84Flattening = 298.257223563;
      //const double WGS84earthRadius = 6378137.0;

      switch (dist_units)
      {
        case distance_units_type.feet: return ImperialFeetToMetres;
        case distance_units_type.US_feet: return USFeetToMetres;
        case distance_units_type.metres: return 1.0;
        case distance_units_type.chains: return FeetToMetres * 66;
        case distance_units_type.links: return FeetToMetres * 66 / 100;
        case distance_units_type.yards: return FeetToMetres * 3;
        case distance_units_type.millimetres: return 0.001;
        case distance_units_type.inches: return FeetToMetres / 12;
        case distance_units_type.kilometres: return 1000;
        case distance_units_type.miles: return FeetToMetres * 5280;
        case distance_units_type.centimetres: return 0.01;
        default:
          Debug.Assert(false);
          return Consts.NullDouble;
      }
    }

    public static double DXFDistance(double value, distance_units_type OutputUnits)
    {
      // Takes a value in SI units (ie: meters) and converts it to the project units for
// writing out to a DXF file
      return value / DistToMetres(OutputUnits);
    }
  }
}