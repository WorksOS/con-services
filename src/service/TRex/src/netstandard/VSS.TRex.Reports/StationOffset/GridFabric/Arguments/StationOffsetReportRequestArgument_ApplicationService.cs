using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Arguments
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class StationOffsetReportRequestArgument_ApplicationService : BaseApplicationServiceRequestArgument, IEquatable<StationOffsetReportRequestArgument_ApplicationService>
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
    public bool ReportCmv { get; set; }

    /// <summary>
    /// Include the measured MDP at the sampled location
    /// </summary>
    /// 
    public bool ReportMdp { get; set; }

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
    public bool ReportCutFill { get; set; }

    /// <summary>
    /// This design contains the center line which will be sampled along
    /// </summary>
    public Guid AlignmentDesignUid { get; set; }

    /// <summary>
    /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
    /// </summary>
    /// 
    public double CrossSectionInterval { get; set; }

    /// <summary>
    /// Start point along the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double StartStation { get; set; }

    /// <summary>
    /// End point along the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double EndStation { get; set; }

    /// <summary>
    /// Offsets left and right (or on) the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double[] Offsets { get; set; } = new double[0];

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
      writer.WriteGuid(AlignmentDesignUid);
      writer.WriteDouble(CrossSectionInterval);
      writer.WriteDouble(StartStation);
      writer.WriteDouble(EndStation);
      writer.WriteDoubleArray(Offsets);
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
      AlignmentDesignUid = reader.ReadGuid() ?? Guid.Empty;
      CrossSectionInterval = reader.ReadDouble();
      StartStation = reader.ReadDouble();
      EndStation = reader.ReadDouble();
      Offsets = reader.ReadDoubleArray();
    }

    public bool Equals(StationOffsetReportRequestArgument_ApplicationService other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      if ((Offsets == null && other.Offsets != null) ||
          (Offsets != null && other.Offsets == null))
        return false;

      if (Offsets != null && other.Offsets != null)
      {
        if (!Offsets.Length.Equals(other.Offsets.Length))
          return false;
        for (int i = 0; i < Offsets.Length; i++)
        {
          if (!Offsets[i].Equals(other.Offsets[i]))
            return false;
        }
      }

      return ReportElevation.Equals(other.ReportElevation) &&
             ReportCmv.Equals(other.ReportCmv) &&
             ReportMdp.Equals(other.ReportMdp) &&
             ReportPassCount.Equals(other.ReportPassCount) &&
             ReportTemperature.Equals(other.ReportTemperature) &&
             ReportCutFill.Equals(other.ReportCutFill) &&
             AlignmentDesignUid.Equals(other.AlignmentDesignUid) &&
             CrossSectionInterval.Equals(other.CrossSectionInterval) &&
             StartStation.Equals(other.StartStation) &&
             EndStation.Equals(other.EndStation);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((StationOffsetReportRequestArgument_ApplicationService) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportElevation.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportCmv.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportMdp.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportPassCount.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportTemperature.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportCutFill.GetHashCode();
        hashCode = (hashCode * 397) ^ AlignmentDesignUid.GetHashCode();
        hashCode = (hashCode * 397) ^ CrossSectionInterval.GetHashCode();
        hashCode = (hashCode * 397) ^ StartStation.GetHashCode();
        hashCode = (hashCode * 397) ^ EndStation.GetHashCode();
        hashCode = (hashCode * 397) ^ Offsets.GetHashCode();
        return hashCode;
      }
    }
  }
}
