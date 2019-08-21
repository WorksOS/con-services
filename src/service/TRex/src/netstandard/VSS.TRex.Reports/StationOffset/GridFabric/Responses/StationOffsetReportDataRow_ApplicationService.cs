using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class StationOffsetReportDataRow_ApplicationService
  {
    public double Station { get; set; }

    public List<StationOffsetRow> Offsets { get; set; }

    public OffsetStatistics_ApplicationService Minimum { get; set; }
    public OffsetStatistics_ApplicationService Maximum { get; set; }
    public OffsetStatistics_ApplicationService Average { get; set; }

    public StationOffsetReportDataRow_ApplicationService()
    {
      SetValues();
    }

    public StationOffsetReportDataRow_ApplicationService(
      double station, List<StationOffsetRow> offsets)
    {
      SetValues(station, offsets);
    }

    private void SetValues(double station = double.MinValue, List<StationOffsetRow> offsets = null)
    {
      Station = station;
      Offsets = new List<StationOffsetRow>();
      Minimum = new OffsetStatistics_ApplicationService();
      Maximum = new OffsetStatistics_ApplicationService();
      Average = new OffsetStatistics_ApplicationService();
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

    private OffsetStatistics_ApplicationService CalculateMinimum(List<StationOffsetRow> offsets)
    {
      return new OffsetStatistics_ApplicationService()
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

    private OffsetStatistics_ApplicationService CalculateMaximum(List<StationOffsetRow> offsets)
    {
      return new OffsetStatistics_ApplicationService()
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

    private OffsetStatistics_ApplicationService CalculateAverage(List<StationOffsetRow> offsets)
    {
      return new OffsetStatistics_ApplicationService()
      {
        Northing = 0,
        Easting = 0,
        Elevation = (from x in offsets where x.Elevation != Consts.NullHeight select x.Elevation).DefaultIfEmpty(Consts.NullHeight).Average(),
        CutFill = (from x in offsets where x.CutFill != Consts.NullHeight select x.CutFill).DefaultIfEmpty(Consts.NullHeight).Average(),
        Cmv = (short) (from x in offsets where x.Cmv != CellPassConsts.NullCCV select x.Cmv).DefaultIfEmpty(CellPassConsts.NullCCV).Average(x => x),
        Mdp = (short) (from x in offsets where x.Mdp != CellPassConsts.NullMDP select x.Mdp).DefaultIfEmpty(CellPassConsts.NullMDP).Average(x => x),
        PassCount = (short) (from x in offsets where x.PassCount != CellPassConsts.NullPassCountValue select x.PassCount).DefaultIfEmpty((short)CellPassConsts.NullPassCountValue).Average(x => x),
        Temperature = (short) (from x in offsets where x.Temperature != CellPassConsts.NullMaterialTemperatureValue select x.Temperature).DefaultIfEmpty((short)CellPassConsts.NullMaterialTemperatureValue).Average(x => x)
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
      Offsets = new List<StationOffsetRow>();
      for (var i = 0; i < offsetsCount; i++)
      {
        var offset = new StationOffsetRow();
        offset.Read(reader);
        Offsets.Add(offset);
      }

      Minimum.Read(reader);
      Maximum.Read(reader);
      Average.Read(reader);
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Station);
      writer.WriteInt(Offsets.Count);
      foreach (var offset in Offsets)
      {
        offset.ToBinary(writer);
      }
      Minimum.ToBinary(writer);
      Maximum.ToBinary(writer);
      Average.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Station = reader.ReadDouble();
      var offsetCount = reader.ReadInt();
      Offsets = new List<StationOffsetRow>();
      for (var i = 0; i < offsetCount; i++)
      {
        var offset = new StationOffsetRow();
        offset.FromBinary(reader);
        Offsets.Add(offset);
      }
      Minimum.FromBinary(reader);
      Maximum.FromBinary(reader);
      Average.FromBinary(reader);
    }
  }
}
