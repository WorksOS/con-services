using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments 
{
  public class AlignmentDesignGeometryArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The project the request is relevant to
    /// </summary>
    public Guid ProjectID { get; set; }

    /// <summary>
    /// The alignment design the request is relevant to
    /// </summary>
    public Guid AlignmentDesignID { get; set; }

    /// <summary>
    /// Notes whether arcs elements should be expressed as poly lines (chorded arcs), or as geometric arcs.
    /// </summary>
    public bool ConvertArcsToPolyLines { get; set; }

    /// <summary>
    /// The maximum error between the arc a chorded poly line an arc should be converted into.
    /// This value is expressed in meters and defaults to 1 meter
    /// </summary>
    public double ArcChordTolerance { get; set; } = 1.0;

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteGuid(AlignmentDesignID);
      writer.WriteBoolean(ConvertArcsToPolyLines);
      writer.WriteDouble(ArcChordTolerance);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ProjectID = reader.ReadGuid() ?? Guid.Empty;
        AlignmentDesignID = reader.ReadGuid() ?? Guid.Empty;
        ConvertArcsToPolyLines = reader.ReadBoolean();
        ArcChordTolerance = reader.ReadDouble();
      }
    }
  }
}
