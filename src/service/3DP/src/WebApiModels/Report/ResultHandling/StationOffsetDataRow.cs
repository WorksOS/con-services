using System.IO;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  public class StationOffsetDataRow : GriddedReportDataRowBase
  {
    public double Station { get; set; }

    public double Offset { get; set; }

    public StationOffsetDataRow()
    {
    }

    public StationOffsetDataRow
    (double station, double offset, double northing, double easting, double elevation,
      double cutFill, short cmv, short mdp, short passCount, short temperature)
    {
      SetValues(station, offset, northing, easting, elevation, cutFill, cmv, mdp, passCount, temperature);
    }

    public StationOffsetDataRow(double station, double offset, double northing, double easting)
    {
      SetValues(station, offset, northing, easting, Consts.NullHeight, Consts.NullHeight, CellPassConsts.NullCCV, CellPassConsts.NullMDP, (short)CellPassConsts.NullPassCountValue, (short)CellPassConsts.NullMaterialTemperatureValue);
    }

    public void SetValues(double station, double offset, double northing, double easting, double elevation,
      double cutFill, short cmv, short mdp, short passCount, short temperature)
    {
      Station = station;
      Offset = offset;
      base.SetValues(northing, easting, elevation, cutFill, cmv, mdp, passCount, temperature);
    }

    public new void Write(BinaryWriter writer)
    {
      base.Write(writer);
      writer.Write(Station);
      writer.Write(Offset);
    }

    public new void Read(BinaryReader reader)
    {
      base.Read(reader);
      Station = reader.ReadDouble();
      Offset = reader.ReadDouble();
    }
  }
}
