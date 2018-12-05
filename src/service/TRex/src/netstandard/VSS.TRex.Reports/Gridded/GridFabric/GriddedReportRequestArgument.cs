using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Reports.Gridded.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class GriddedReportRequestArgument : BaseApplicationServiceRequestArgument, IEquatable<GriddedReportRequestArgument>
  {

    /// <summary>
    /// Include the measured elevation at the sampled location
    /// </summary>
    /// 
    public bool ReportElevation { get; set; }

    /// <summary>
    /// Include the measured CMV at the sampled location
    /// </summary
    /// >
    public bool ReportCMV { get; set; }

    /// <summary>
    /// Include the measured MDP at the sampled location
    /// </summary>
    /// 
    public bool ReportMDP { get; set; }

    /// <summary>
    /// Include the calculated pass count at the sampled location
    /// </summary>
    /// 
    public bool ReportPassCount { get; set; }

    /// <summary>
    /// Include the measured temperature at the sampled location
    /// </summary>
    /// 
    public bool ReportTemperature { get; set; }

    /// <summary>
    /// Include the calculated cut-fill between the elevation at the sampled location and the design elevation at the same location
    /// </summary>
    /// 
    public bool ReportCutFill { get; set; }

    /// <summary>
    /// Grid report option. Whether it is defined automatically or by user specified parameters.
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
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteBoolean(ReportElevation);
      writer.WriteBoolean(ReportCMV);
      writer.WriteBoolean(ReportMDP);
      writer.WriteBoolean(ReportPassCount);
      writer.WriteBoolean(ReportTemperature);
      writer.WriteBoolean(ReportCutFill);
      writer.WriteDouble(GridInterval);
      writer.WriteInt((int) GridReportOption);
      writer.WriteDouble(StartNorthing);
      writer.WriteDouble(StartEasting);
      writer.WriteDouble(EndNorthing);
      writer.WriteDouble(EndEasting);
      writer.WriteDouble(Azimuth);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      ReportElevation = reader.ReadBoolean();
      ReportCMV = reader.ReadBoolean();
      ReportMDP = reader.ReadBoolean();
      ReportPassCount = reader.ReadBoolean();
      ReportTemperature = reader.ReadBoolean();
      ReportCutFill = reader.ReadBoolean();
      GridInterval = reader.ReadDouble();
      GridReportOption = (GridReportOption) reader.ReadInt();
      StartNorthing = reader.ReadDouble();
      StartEasting = reader.ReadDouble();
      EndNorthing = reader.ReadDouble();
      EndEasting = reader.ReadDouble();
      Azimuth = reader.ReadDouble();
    }

    public bool Equals(GriddedReportRequestArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) &&
             ReportElevation.Equals(other.ReportElevation) &&
             ReportCMV.Equals(other.ReportCMV) &&
             ReportMDP.Equals(other.ReportMDP) &&
             ReportPassCount.Equals(other.ReportPassCount) &&
             ReportTemperature.Equals(other.ReportTemperature) &&
             ReportCutFill.Equals(other.ReportCutFill) &&
             GridInterval.Equals(other.GridInterval) &&
             GridReportOption.Equals(other.GridReportOption) &&
             StartNorthing.Equals(other.StartNorthing) &&
             StartEasting.Equals(other.StartEasting) &&
             EndNorthing.Equals(other.EndNorthing) &&
             EndEasting.Equals(other.EndEasting) &&
             Azimuth.Equals(other.Azimuth);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((GriddedReportRequestArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportElevation.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportCMV.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportMDP.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportPassCount.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportTemperature.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportCutFill.GetHashCode();
        hashCode = (hashCode * 397) ^ GridInterval.GetHashCode();
        hashCode = (hashCode * 397) ^ GridReportOption.GetHashCode();
        hashCode = (hashCode * 397) ^ StartNorthing.GetHashCode();
        hashCode = (hashCode * 397) ^ StartEasting.GetHashCode();
        hashCode = (hashCode * 397) ^ EndNorthing.GetHashCode();
        hashCode = (hashCode * 397) ^ EndEasting.GetHashCode();
        hashCode = (hashCode * 397) ^ Azimuth.GetHashCode();
        return hashCode;
      }
    }
  }
}
