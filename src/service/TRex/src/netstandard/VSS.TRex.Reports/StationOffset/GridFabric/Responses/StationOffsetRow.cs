using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  public class StationOffsetRow : GriddedReportDataRow
  {
    public double Station { get; set; }

    public double Offset { get; set; }

    public StationOffsetRow()
    {
    }

    public StationOffsetRow
    (double station, double offset, double northing, double easting, double elevation,
      double cutFill, short cmv, short mdp, short passCount, short temperature)
    {
      SetValues(station, offset, northing, easting, elevation, cutFill, cmv, mdp, passCount, temperature);
    }

    public StationOffsetRow(double station, double offset, double northing, double easting)
    {
      SetValues(station, offset, northing, easting, Consts.NullHeight, Consts.NullHeight, CellPassConsts.NullCCV, CellPassConsts.NullMDP, (short) CellPassConsts.NullPassCountValue, (short) CellPassConsts.NullMaterialTemperatureValue);
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

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public new void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteDouble(Station);
      writer.WriteDouble(Offset);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public new void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      Station = reader.ReadDouble();
      Offset = reader.ReadDouble();
    }
    
    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ Station.GetHashCode();
        hashCode = (hashCode * 397) ^ Offset.GetHashCode();
        return hashCode;
      }
    }
  }
}
