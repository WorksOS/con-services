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

    private const int TPAAS_REQUEST_TIMEOUT_SECONDS = 60;

    /// <summary>
    /// The time the request was emitted from the service platform context acting as a client to TRex.
    /// Any request arriving at an endpoint in TRex that may have this time examined to determined if it is too
    /// old to be considered. The prime motivation for this is the TPaaS request timeout (currently 60 seconds
    /// at the time of writing)
    /// </summary>
    public DateTime RequestEmissionDateUtc = DateTime.UtcNow;

    /// <summary>
    /// Indicates this request was emitted outside of the TPaaS request timeout
    /// </summary>
    public bool IsOutsideTPaaSTimeout { get; private set; }

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
      writer.WriteLong(RequestEmissionDateUtc.ToBinary());
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        OriginatingIgniteNodeId = reader.ReadGuid() ?? Guid.Empty;
        ExternalDescriptor = reader.ReadGuid() ?? Guid.Empty;
        RequestEmissionDateUtc = DateTime.FromBinary(reader.ReadLong());

        IsOutsideTPaaSTimeout = RequestEmissionDateUtc.AddSeconds(TPAAS_REQUEST_TIMEOUT_SECONDS) < DateTime.UtcNow;
      }
    }
  }
}
