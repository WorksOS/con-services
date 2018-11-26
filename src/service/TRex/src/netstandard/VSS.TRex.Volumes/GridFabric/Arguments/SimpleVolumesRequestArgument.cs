using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Volumes.GridFabric.Arguments
{
  /// <summary>
  /// The argument passed to simple volumes requests
  /// </summary>
  public class SimpleVolumesRequestArgument : BaseApplicationServiceRequestArgument, IEquatable<SimpleVolumesRequestArgument>
  {
    //ExternalDescriptor : TASNodeRequestDescriptor;

    /// <summary>
    /// The volume computation method to use when calculating volume information
    /// </summary>
    public VolumeComputationType VolumeType = VolumeComputationType.None;

    // FLiftBuildSettings : TICLiftBuildSettings;

    /// <summary>
    /// BaseFilter and TopFilter reference two sets of filter settings
    /// between which we may calculate volumes. At the current time, it is
    /// meaingful for a filter to have a spatial extent, and to denote aa
    /// 'as-at' time only.
    /// </summary>
    public ICombinedFilter BaseFilter = null;
    public ICombinedFilter TopFilter = null;

    public Guid BaseDesignID = Guid.Empty;
    public Guid TopDesignID = Guid.Empty;

    /// <summary>
    /// AdditionalSpatialFilter is an additional boundary specified by the user to bound the result of the query
    /// </summary>
    public ICombinedFilter AdditionalSpatialFilter;

    /// <summary>
    /// CutTolerance determines the tolerance (in meters) that the 'From' surface
    /// needs to be above the 'To' surface before the two surfaces are not
    /// considered to be equivalent, or 'on-grade', and hence there is material still remaining to
    /// be cut
    /// </summary>
    public double CutTolerance = VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE;

    /// <summary>
    /// FillTolerance determines the tolerance (in meters) that the 'To' surface
    /// needs to be above the 'From' surface before the two surfaces are not
    /// considered to be equivalent, or 'on-grade', and hence there is material still remaining to
    /// be filled
    /// </summary>
    public double FillTolerance = VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SimpleVolumesRequestArgument()
    {
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteGuid(ProjectID);
      writer.WriteInt((int)VolumeType);

      writer.WriteBoolean(BaseFilter != null);
      BaseFilter?.ToBinary(writer);

      writer.WriteBoolean(TopFilter != null);
      TopFilter?.ToBinary(writer);

      writer.WriteGuid(BaseDesignID);
      writer.WriteGuid(TopDesignID);

      writer.WriteBoolean(AdditionalSpatialFilter != null);
      AdditionalSpatialFilter?.ToBinary(writer);

      writer.WriteDouble(CutTolerance);
      writer.WriteDouble(FillTolerance);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      VolumeType = (VolumeComputationType)reader.ReadInt();

      if (reader.ReadBoolean())
      {
        BaseFilter = new CombinedFilter();
        BaseFilter.FromBinary(reader);
      }

      if (reader.ReadBoolean())
      {
        TopFilter = new CombinedFilter();
        TopFilter.FromBinary(reader);
      }

      BaseDesignID = reader.ReadGuid() ?? Guid.Empty;
      TopDesignID = reader.ReadGuid() ?? Guid.Empty;

      if (reader.ReadBoolean())
      {
        AdditionalSpatialFilter = new CombinedFilter();
        AdditionalSpatialFilter.FromBinary(reader);
      }

      CutTolerance = reader.ReadDouble();
      FillTolerance = reader.ReadDouble();
    }

    public bool Equals(SimpleVolumesRequestArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && 
             VolumeType == other.VolumeType && 
             Equals(BaseFilter, other.BaseFilter) && 
             Equals(TopFilter, other.TopFilter) && 
             BaseDesignID.Equals(other.BaseDesignID) && 
             TopDesignID.Equals(other.TopDesignID) && 
             Equals(AdditionalSpatialFilter, other.AdditionalSpatialFilter) && 
             CutTolerance.Equals(other.CutTolerance) && 
             FillTolerance.Equals(other.FillTolerance);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SimpleVolumesRequestArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ (int) VolumeType;
        hashCode = (hashCode * 397) ^ (BaseFilter != null ? BaseFilter.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (TopFilter != null ? TopFilter.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ BaseDesignID.GetHashCode();
        hashCode = (hashCode * 397) ^ TopDesignID.GetHashCode();
        hashCode = (hashCode * 397) ^ (AdditionalSpatialFilter != null ? AdditionalSpatialFilter.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ CutTolerance.GetHashCode();
        hashCode = (hashCode * 397) ^ FillTolerance.GetHashCode();
        return hashCode;
      }
    }
  }
}
