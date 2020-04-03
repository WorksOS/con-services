using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;

namespace VSS.TRex.Volumes.GridFabric.Arguments
{
  public class ProgressiveVolumesRequestArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The volume computation method to use when calculating volume information
    /// </summary>
    public VolumeComputationType VolumeType = VolumeComputationType.None;

    public DesignOffset BaseDesign { get; set; } = new DesignOffset();
    public DesignOffset TopDesign { get; set; } = new DesignOffset();

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
    /// The date/time at which to start calculating progressive volumes.
    /// The first progressive volume will be calculated at this date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The date/time at which to stop calculating progressive volumes.
    /// The last progressive volume will be calculated at or before this date according
    /// to the progressive volumes interval specified
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// The time interval between calculated progressive volumes
    /// </summary>
    public TimeSpan Interval { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public ProgressiveVolumesRequestArgument()
    {
    }

    private static void WriteFilter(IBinaryRawWriter writer, ICombinedFilter filter)
    {
      writer.WriteBoolean(filter != null);
      filter?.ToBinary(writer);
    }

    private static ICombinedFilter ReadFilter(IBinaryRawReader reader)
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

      writer.WriteBoolean(BaseDesign != null);
      BaseDesign?.ToBinary(writer);
      writer.WriteBoolean(TopDesign != null);
      TopDesign?.ToBinary(writer);

      WriteFilter(writer, AdditionalSpatialFilter);

      writer.WriteDouble(CutTolerance);
      writer.WriteDouble(FillTolerance);

      writer.WriteLong(StartDate.ToBinary());
      writer.WriteLong(EndDate.ToBinary());
      writer.WriteLong(Interval.Ticks);
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

      if (reader.ReadBoolean())
      {
        BaseDesign = new DesignOffset();
        BaseDesign.FromBinary(reader);
      }
      if (reader.ReadBoolean())
      {
        TopDesign = new DesignOffset();
        TopDesign.FromBinary(reader);
      }

      AdditionalSpatialFilter = ReadFilter(reader);

      CutTolerance = reader.ReadDouble();
      FillTolerance = reader.ReadDouble();

      StartDate = DateTime.FromBinary(reader.ReadLong());
      EndDate = DateTime.FromBinary(reader.ReadLong());
      Interval = TimeSpan.FromTicks(reader.ReadLong());
    }
  }
}
