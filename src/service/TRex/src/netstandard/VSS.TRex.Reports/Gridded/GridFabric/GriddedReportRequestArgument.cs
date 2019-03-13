using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.Common;

namespace VSS.TRex.Reports.Gridded.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class GriddedReportRequestArgument : BaseApplicationServiceRequestArgumentReport
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
    /// </summary>
    /// 
    public double GridInterval { get; set; }

    /// <summary>
    /// Grid report option. Whether it is defined automatically or by user specified parameters.
    /// </summary>
    /// 
    public GridReportOption GridReportOption { get; set; }

    /// <summary>
    /// The Northing ordinate of the location to start gridding from
    /// </summary>
    public double StartNorthing { get; set; }

    /// <summary>
    /// The Easting ordinate of the location to start gridding from
    /// </summary>
    public double StartEasting { get; set; }

    /// <summary>
    /// The Northing ordinate of the location to end gridding at
    /// </summary>
    public double EndNorthing { get; set; }

    /// <summary>
    /// The Easting ordinate of the location to end gridding at
    /// </summary>
    public double EndEasting { get; set; }

    /// <summary>
    /// The orientation of the grid, expressed in radians
    /// </summary>
    public double Azimuth { get; set; }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(GridInterval);
      writer.WriteInt((int) GridReportOption);
      writer.WriteDouble(StartNorthing);
      writer.WriteDouble(StartEasting);
      writer.WriteDouble(EndNorthing);
      writer.WriteDouble(EndEasting);
      writer.WriteDouble(Azimuth);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      GridInterval = reader.ReadDouble();
      GridReportOption = (GridReportOption) reader.ReadInt();
      StartNorthing = reader.ReadDouble();
      StartEasting = reader.ReadDouble();
      EndNorthing = reader.ReadDouble();
      EndEasting = reader.ReadDouble();
      Azimuth = reader.ReadDouble();
    }
  }
}
