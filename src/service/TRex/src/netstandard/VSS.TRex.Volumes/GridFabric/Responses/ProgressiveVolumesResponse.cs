using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.Responses
{
  public class ProgressiveVolumesResponse : SubGridRequestsResponse, IAggregateWith<ProgressiveVolumesResponse>
  {
    private const byte VERSION_NUMBER = 1;

    public ProgressiveVolumesResponse AggregateWith(ProgressiveVolumesResponse other)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// The bounding extent of the area covered by the volume computation expressed in the project site calibration/grid coordinate system
    /// </summary>
    public BoundingWorldExtent3D BoundingExtentGrid = BoundingWorldExtent3D.Null();

    /// <summary>
    /// The bounding extent of the area covered by the volume computation expressed in the WGS84 datum
    /// </summary>
    public BoundingWorldExtent3D BoundingExtentLLH = BoundingWorldExtent3D.Null();

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

/*      writer.WriteBoolean(Cut.HasValue);
      if (Cut.HasValue)
        writer.WriteDouble(Cut.Value);

      writer.WriteBoolean(Fill.HasValue);
      if (Fill.HasValue)
        writer.WriteDouble(Fill.Value);

      writer.WriteBoolean(TotalCoverageArea.HasValue);
      if (TotalCoverageArea.HasValue)
        writer.WriteDouble(TotalCoverageArea.Value);

      writer.WriteBoolean(CutArea.HasValue);
      if (CutArea.HasValue)
        writer.WriteDouble(CutArea.Value);

      writer.WriteBoolean(FillArea.HasValue);
      if (FillArea.HasValue)
        writer.WriteDouble(FillArea.Value);
 */

      BoundingExtentGrid.ToBinary(writer);
      BoundingExtentLLH.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      /*
      if (reader.ReadBoolean())
        Cut = reader.ReadDouble();

      if (reader.ReadBoolean())
        Fill = reader.ReadDouble();

      if (reader.ReadBoolean())
        TotalCoverageArea = reader.ReadDouble();

      if (reader.ReadBoolean())
        CutArea = reader.ReadDouble();

      if (reader.ReadBoolean())
        FillArea = reader.ReadDouble();
      */

      (BoundingExtentGrid ?? (BoundingExtentGrid = new BoundingWorldExtent3D())).FromBinary(reader);
      (BoundingExtentLLH ?? (BoundingExtentLLH = new BoundingWorldExtent3D())).FromBinary(reader);
    }
  }
}
