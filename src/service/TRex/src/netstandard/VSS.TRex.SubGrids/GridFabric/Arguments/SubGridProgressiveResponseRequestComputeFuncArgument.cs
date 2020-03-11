using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;

namespace VSS.TRex.SubGrids.GridFabric.Arguments
{
  public class SubGridProgressiveResponseRequestComputeFuncArgument : BaseRequestArgument, ISubGridProgressiveResponseRequestComputeFuncArgument
  {
    private const byte VERSION_NUMBER = 1;

    public Guid NodeId { get; set; } = Guid.Empty;

    /// <summary>
    /// A common descriptor that may be supplied by the argument consumer to hold an
    /// externally provided Guid identifier for the request
    /// </summary>
    public Guid RequestDescriptor { get; set; } = Guid.Empty;

    public ISerialisedByteArrayWrapper Payload { get; set; }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(NodeId);
      writer.WriteGuid(RequestDescriptor);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      NodeId = reader.ReadGuid() ?? Guid.Empty;
      RequestDescriptor = reader.ReadGuid() ?? Guid.Empty;
    }
  }
}
