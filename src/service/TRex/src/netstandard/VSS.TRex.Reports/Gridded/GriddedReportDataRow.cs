using System;
using System.IO;

namespace VSS.TRex.Reports.Gridded
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class GriddedReportDataRow : IEquatable<GriddedReportDataRow>
  {
    public double Northing { get; set; }
    public double Easting { get; set; }
    public double Elevation { get; set; }
    public double CutFill { get; set; }
    public short Cmv { get; set; }
    public short Mdp { get; set; }
    public short PassCount { get; set; }
    public short Temperature { get; set; }

    public GriddedReportDataRow()
    {
    }
    public GriddedReportDataRow(
      double northing, double easting, double elevation,
      double cutFill, short cmv, short mdp, short passCount, short temperature)
    {
      Northing = northing;
      Easting = easting;
      Elevation = elevation;
      CutFill = cutFill;
      Cmv = cmv;
      Mdp = mdp;
      PassCount = passCount;
      Temperature = temperature;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Northing);
      writer.Write(Easting);
      writer.Write(Elevation);
      writer.Write(CutFill);
      writer.Write(Cmv);
      writer.Write(Mdp);
      writer.Write(PassCount);
      writer.Write(Temperature);
    }

    public void Read(BinaryReader reader)
    {
      Northing = reader.ReadDouble();
      Easting = reader.ReadDouble();
      Elevation = reader.ReadDouble();
      CutFill = reader.ReadDouble();
      Cmv = reader.ReadInt16();
      Mdp = reader.ReadInt16();
      PassCount = reader.ReadInt16();
      Temperature = reader.ReadInt16();
    }
    
    public bool Equals(GriddedReportDataRow other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Northing.Equals(other.Northing) &&
             Easting.Equals(other.Easting) &&
             Elevation.Equals(other.Elevation) &&
             CutFill.Equals(other.CutFill) &&
             Cmv.Equals(other.Cmv) &&
             Mdp.Equals(other.Mdp) &&
             PassCount.Equals(other.PassCount) &&
             Temperature.Equals(other.Temperature);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((GriddedReportDataRow)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ Northing.GetHashCode();
        hashCode = (hashCode * 397) ^ Easting.GetHashCode();
        hashCode = (hashCode * 397) ^ Elevation.GetHashCode();
        hashCode = (hashCode * 397) ^ CutFill.GetHashCode();
        hashCode = (hashCode * 397) ^ Cmv.GetHashCode();
        hashCode = (hashCode * 397) ^ Mdp.GetHashCode();
        hashCode = (hashCode * 397) ^ PassCount.GetHashCode();
        hashCode = (hashCode * 397) ^ Temperature.GetHashCode();
        return hashCode;
      }
    }
  }
}
