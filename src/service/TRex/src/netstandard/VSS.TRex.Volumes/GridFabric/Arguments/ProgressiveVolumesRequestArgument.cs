using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Volumes.GridFabric.Arguments
{
  public class ProgressiveVolumesRequestArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The volume computation method to use when calculating volume information
    /// </summary>
    public VolumeComputationType VolumeType = VolumeComputationType.None;

    /// <summary>
    /// Filter controlling selection of cells and cell passes for progressive volumes.
    /// </summary>
    public ICombinedFilter Filter { get; set; }

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
    /// Default no-arg constructor
    /// </summary>
    public ProgressiveVolumesRequestArgument()
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

      WriteFilter(writer, Filter);

      writer.WriteBoolean(BaseDesign != null);
      BaseDesign?.ToBinary(writer);
      writer.WriteBoolean(TopDesign != null);
      TopDesign?.ToBinary(writer);

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

      Filter = ReadFilter(reader);

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
    }
  }
}
