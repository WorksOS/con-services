using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Geometry;

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

    public static double DXFDistance(double value, DistanceUnitsType outputUnits)
    {
      // Takes a value in SI units (ie: meters) and converts it to the project units for writing out to a DXF file
      return value / UnitUtils.DistToMeters(outputUnits);
    }
  }
}
