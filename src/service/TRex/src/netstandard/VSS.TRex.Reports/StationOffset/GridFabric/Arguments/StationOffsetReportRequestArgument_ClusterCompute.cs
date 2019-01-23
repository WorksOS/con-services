using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Types;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Arguments
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class StationOffsetReportRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgument, IEquatable<StationOffsetReportRequestArgument_ClusterCompute>
  {
    public bool ReportElevation { get; set; }

    public bool ReportCmv { get; set; }
 
    public bool ReportMdp { get; set; }

    public bool ReportPassCount { get; set; }

    public bool ReportTemperature { get; set; }

    public bool ReportCutFill { get; set; }


    /// <summary>
    /// Points in NEE derived from each station & offset
    /// </summary>
    /// 
    public List<StationOffsetPoint> Points { get; set; }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      
      writer.WriteBoolean(ReportElevation);
      writer.WriteBoolean(ReportCmv);
      writer.WriteBoolean(ReportMdp);
      writer.WriteBoolean(ReportPassCount);
      writer.WriteBoolean(ReportTemperature);
      writer.WriteBoolean(ReportCutFill);
      writer.WriteInt(Points.Count);
      foreach (var point in Points)
      {
        point.ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      ReportElevation = reader.ReadBoolean();
      ReportCmv = reader.ReadBoolean();
      ReportMdp = reader.ReadBoolean();
      ReportPassCount = reader.ReadBoolean();
      ReportTemperature = reader.ReadBoolean();
      ReportCutFill = reader.ReadBoolean();
      var pointCount = reader.ReadInt();
      for (int i = 0; i < pointCount; i++)
      {
        StationOffsetPoint point = null;
        (point = new StationOffsetPoint()).FromBinary(reader);
        Points.Add(point);
      }
    }

    public bool Equals(StationOffsetReportRequestArgument_ClusterCompute other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) &&
             ReportElevation.Equals(other.ReportElevation) &&
             ReportCmv.Equals(other.ReportCmv) &&
             ReportMdp.Equals(other.ReportMdp) &&
             ReportPassCount.Equals(other.ReportPassCount) &&
             ReportTemperature.Equals(other.ReportTemperature) &&
             ReportCutFill.Equals(other.ReportCutFill) &&
             Points.Equals(other.Points);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((StationOffsetReportRequestArgument_ClusterCompute) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = ReportElevation.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportCmv.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportMdp.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportPassCount.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportTemperature.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportCutFill.GetHashCode();
        hashCode = (hashCode * 397) ^ (Points != null ? Points.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}
