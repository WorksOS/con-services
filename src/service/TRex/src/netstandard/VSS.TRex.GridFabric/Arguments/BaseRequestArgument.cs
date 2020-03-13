using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.GridFabric.Arguments
{
  /// <summary>
  /// Defines a base class implementation of the Ignite binarizable serialization. It also defines a separate
  /// set of methods from IFromToBinary based on the Ignite raw read/write serialization as the preferred performant serialization
  /// approach
  /// </summary>
  public class BaseRequestArgument : IBinarizable, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

    public Guid OriginatingIgniteNodeId { get; set; } = Guid.Empty;

    /// <summary>
    /// A common descriptor that may be supplied by the argument consumer to hold an
    /// externally provided Guid identifier for the request
    /// </summary>
    public Guid ExternalDescriptor { get; set; } = Guid.Empty;

    public virtual void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(OriginatingIgniteNodeId);
      writer.WriteGuid(ExternalDescriptor);
    }

    public virtual void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      OriginatingIgniteNodeId = reader.ReadGuid() ?? Guid.Empty;
      ExternalDescriptor = reader.ReadGuid() ?? Guid.Empty;
    }

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialize this object.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialize this object.
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}
