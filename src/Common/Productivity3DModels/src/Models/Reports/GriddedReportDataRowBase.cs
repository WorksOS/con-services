using System.IO;

namespace VSS.Productivity3D.Models.Models.Reports
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class GriddedReportDataRowBase
  {
    public double Northing { get; set; }
    public double Easting { get; set; }
    public double Elevation { get; set; }
    public double CutFill { get; set; }
    public short Cmv { get; set; }
    public short Mdp { get; set; }
    public short PassCount { get; set; }
    public short Temperature { get; set; }

    public GriddedReportDataRowBase()
    {
    }

    public GriddedReportDataRowBase(
      double northing, double easting, double elevation,
      double cutFill, short cmv, short mdp, short passCount, short temperature)
    {
      SetValues(northing, easting, elevation, cutFill, cmv, mdp, passCount, temperature);
    }

    public void SetValues(
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
  }
}
