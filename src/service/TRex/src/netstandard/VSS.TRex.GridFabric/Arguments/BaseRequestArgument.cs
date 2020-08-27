using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.GridFabric.Arguments
{
  /// <summary>
  /// Defines a base class implementation of the Ignite binarizable serialization. It also defines a separate
  /// set of methods from IFromToBinary based on the Ignite raw read/write serialization as the preferred performant serialization
  /// approach
  /// </summary>
  public class BaseRequestArgument : VersionCheckedBinarizableSerializationBase 
  {
    private const byte VERSION_NUMBER = 1;

    public Guid OriginatingIgniteNodeId { get; set; } = Guid.Empty;

    /// <summary>
    /// A common descriptor that may be supplied by the argument consumer to hold an
    /// externally provided Guid identifier for the request
    /// </summary>
    public Guid ExternalDescriptor { get; set; } = Guid.Empty;

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(OriginatingIgniteNodeId);
      writer.WriteGuid(ExternalDescriptor);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        OriginatingIgniteNodeId = reader.ReadGuid() ?? Guid.Empty;
        ExternalDescriptor = reader.ReadGuid() ?? Guid.Empty;
      }
    }
  }
}
