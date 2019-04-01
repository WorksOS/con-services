﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Common;

namespace VSS.TRex.Volumes.GridFabric.Arguments
{
  /// <summary>
  /// The argument passed to simple volumes requests
  /// </summary>
  public class SimpleVolumesRequestArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

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
    public ICombinedFilter BaseFilter { get; set; }
    public ICombinedFilter TopFilter { get; set; }

    public Guid BaseDesignID { get; set; } = Guid.Empty;
    public Guid TopDesignID { get; set; } = Guid.Empty;

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

    private void WriteFilter(IBinaryRawWriter writer, ICombinedFilter filter)
    {
      writer.WriteBoolean(filter != null);
      filter?.ToBinary(writer);
    }

    private ICombinedFilter ReadFilter(IBinaryRawReader reader)
    {
      ICombinedFilter filter = null;

      if (reader.ReadBoolean())
      {
        filter = new CombinedFilter();
        filter.FromBinary(reader);
      }

      return filter;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteInt((int)VolumeType);

      WriteFilter(writer, BaseFilter);
      WriteFilter(writer, TopFilter);

      writer.WriteGuid(BaseDesignID);
      writer.WriteGuid(TopDesignID);

      WriteFilter(writer, AdditionalSpatialFilter);

      writer.WriteDouble(CutTolerance);
      writer.WriteDouble(FillTolerance);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      VolumeType = (VolumeComputationType)reader.ReadInt();

      BaseFilter = ReadFilter(reader);
      TopFilter = ReadFilter(reader);

      BaseDesignID = reader.ReadGuid() ?? Guid.Empty;
      TopDesignID = reader.ReadGuid() ?? Guid.Empty;

      AdditionalSpatialFilter = ReadFilter(reader);

      CutTolerance = reader.ReadDouble();
      FillTolerance = reader.ReadDouble();
    }
  }
}
