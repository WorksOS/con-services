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
  public class SimpleVolumesRequestArgument : BaseApplicationServiceRequestArgument
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
    /// meaningful for a filter to have a spatial extent, and to denote aa
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
  }
}
