using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Alignments.GridFabric.Arguments
{
  /// <summary>
  /// Contains the parameters for addition and modification of alignments in a project
  /// </summary>
  public class AddAlignmentArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The project the request is relevant to
    /// </summary>
    public Guid ProjectID { get; set; }

    /// <summary>
    /// The descriptor of the design being added or modified
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; set; }

    /// <summary>
    /// The bounding rectangle computed for the design
    /// </summary>
    public BoundingWorldExtent3D Extents { get; set; }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteBoolean(DesignDescriptor != null);
      DesignDescriptor?.ToBinary(writer);

      writer.WriteBoolean(Extents != null);
      Extents?.ToBinary(writer);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ProjectID = reader.ReadGuid() ?? Guid.Empty;

        if (reader.ReadBoolean())
          (DesignDescriptor = new DesignDescriptor()).FromBinary(reader);

        if (reader.ReadBoolean())
          (Extents = new BoundingWorldExtent3D()).FromBinary(reader);
      }
    }
  }
}
