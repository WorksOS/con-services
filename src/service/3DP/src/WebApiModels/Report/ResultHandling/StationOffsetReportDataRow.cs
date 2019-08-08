using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  public class StationOffsetReportDataRow
  {
    public double Station { get; set; }

    public List<StationOffsetDataRow> Offsets { get; set; }

    public GriddedReportDataRowBase Minimum { get; set; }
    public GriddedReportDataRowBase Maximum { get; set; }
    public GriddedReportDataRowBase Average { get; set; }

    public StationOffsetReportDataRow()
    {
      SetValues();
    }

    public StationOffsetReportDataRow(
      double station, List<StationOffsetDataRow> offsets)
    {
      SetValues(station, offsets);
    }

    private void SetValues(double station = double.MinValue, List<StationOffsetDataRow> offsets = null)
    {
      Station = station;
      Offsets = new List<StationOffsetDataRow>();
      Minimum = new GriddedReportDataRowBase();
      Maximum = new GriddedReportDataRowBase();
      Average = new GriddedReportDataRowBase();
      if (offsets != null)
      {
        Offsets.AddRange(offsets);
        if (Offsets.Count > 0)
        {
          Minimum = CalculateMinimum(Offsets);
          Maximum = CalculateMaximum(Offsets);
          Average = CalculateAverage(Offsets);
        }
      }
    }

    private GriddedReportDataRowBase CalculateMinimum(List<StationOffsetDataRow> offsets)
    {
      return new GriddedReportDataRowBase()
      {
        Northing = 0,
        Easting = 0,
        Elevation = (from x in offsets where x.Elevation != Consts.NullHeight select x.Elevation).DefaultIfEmpty(Consts.NullHeight).Min(),
        CutFill = (from x in offsets where x.CutFill != Consts.NullHeight select x.CutFill).DefaultIfEmpty(Consts.NullHeight).Min(),
        Cmv = (from x in offsets where x.Cmv != CellPassConsts.NullCCV select x.Cmv).DefaultIfEmpty(CellPassConsts.NullCCV).Min(),
        Mdp = (from x in offsets where x.Mdp != CellPassConsts.NullMDP select x.Mdp).DefaultIfEmpty(CellPassConsts.NullMDP).Min(),
        PassCount = (from x in offsets where x.PassCount != CellPassConsts.NullPassCountValue select x.PassCount).DefaultIfEmpty((short)CellPassConsts.NullPassCountValue).Min(),
        Temperature = (from x in offsets where x.Temperature != CellPassConsts.NullMaterialTemperatureValue select x.Temperature).DefaultIfEmpty((short)CellPassConsts.NullMaterialTemperatureValue).Min()
      };
    }

    private GriddedReportDataRowBase CalculateMaximum(List<StationOffsetDataRow> offsets)
    {
      return new GriddedReportDataRowBase()
      {
        Northing = 0,
        Easting = 0,
        Elevation = (from x in offsets where x.Elevation != Consts.NullHeight select x.Elevation).DefaultIfEmpty(Consts.NullHeight).Max(),
        CutFill = (from x in offsets where x.CutFill != Consts.NullHeight select x.CutFill).DefaultIfEmpty(Consts.NullHeight).Max(),
        Cmv = (from x in offsets where x.Cmv != CellPassConsts.NullCCV select x.Cmv).DefaultIfEmpty(CellPassConsts.NullCCV).Max(),
        Mdp = (from x in offsets where x.Mdp != CellPassConsts.NullMDP select x.Mdp).DefaultIfEmpty(CellPassConsts.NullMDP).Max(),
        PassCount = (from x in offsets where x.PassCount != CellPassConsts.NullPassCountValue select x.PassCount).DefaultIfEmpty((short)CellPassConsts.NullPassCountValue).Max(),
        Temperature = (from x in offsets where x.Temperature != CellPassConsts.NullMaterialTemperatureValue select x.Temperature).DefaultIfEmpty((short)CellPassConsts.NullMaterialTemperatureValue).Max()
      };
    }

    private GriddedReportDataRowBase CalculateAverage(List<StationOffsetDataRow> offsets)
    {
      return new GriddedReportDataRowBase()
      {
        Northing = 0,
        Easting = 0,
        Elevation = (from x in offsets where x.Elevation != Consts.NullHeight select x.Elevation).DefaultIfEmpty(Consts.NullHeight).Average(),
        CutFill = (from x in offsets where x.CutFill != Consts.NullHeight select x.CutFill).DefaultIfEmpty(Consts.NullHeight).Average(),
        Cmv = (short) (from x in offsets where x.Cmv != CellPassConsts.NullCCV select x.Cmv).DefaultIfEmpty(CellPassConsts.NullCCV).Average(x => x),
        Mdp = (short) (from x in offsets where x.Mdp != CellPassConsts.NullMDP select x.Mdp).DefaultIfEmpty(CellPassConsts.NullMDP).Average(x => x),
        PassCount = (short) (from x in offsets where x.PassCount != CellPassConsts.NullPassCountValue select x.PassCount).DefaultIfEmpty((short) CellPassConsts.NullPassCountValue).Average(x => x),
        Temperature = (short) (from x in offsets where x.Temperature != CellPassConsts.NullMaterialTemperatureValue select x.Temperature).DefaultIfEmpty((short) CellPassConsts.NullMaterialTemperatureValue).Average(x => x)
      };
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Station);
      writer.Write(Offsets.Count);
      foreach (var offset in Offsets)
      {
        offset.Write(writer);
      }
      Minimum.Write(writer);
      Maximum.Write(writer);
      Average.Write(writer);
    }

    public void Read(BinaryReader reader)
    {
      Station = reader.ReadDouble();
      var offsetsCount = reader.ReadInt32();
      Offsets = new List<StationOffsetDataRow>();
      for (var i = 0; i < offsetsCount; i++)
      {
        var offset = new StationOffsetDataRow();
        offset.Read(reader);
        Offsets.Add(offset);
      }

      Minimum.Read(reader);
      Maximum.Read(reader);
      Average.Read(reader);
    }
  }
}
