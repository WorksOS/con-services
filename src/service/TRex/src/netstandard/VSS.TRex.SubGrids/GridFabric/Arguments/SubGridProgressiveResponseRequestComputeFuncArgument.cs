using System;
using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;

namespace VSS.TRex.SubGrids.GridFabric.Arguments
{
  public class SubGridProgressiveResponseRequestComputeFuncArgument : BaseRequestArgument, ISubGridProgressiveResponseRequestComputeFuncArgument
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridProgressiveResponseRequestComputeFuncArgument>();

    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The ID of the TRex Ignite node originating the request the processed sub grids are destined for.
    /// </summary>
    public Guid NodeId { get; set; } = Guid.Empty;

    /// <summary>
    /// A common descriptor that may be supplied by the argument consumer to hold an
    /// externally provided Guid identifier for the request
    /// </summary>
    public Guid RequestDescriptor { get; set; } = Guid.Empty;

    /// <summary>
    /// The payload containing a set of processed sub grids
    /// </summary>
    public ISerialisedByteArrayWrapper Payload { get; set; }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      try
      {
        base.ToBinary(writer);

        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

        writer.WriteGuid(NodeId);
        writer.WriteGuid(RequestDescriptor);

        writer.WriteBoolean(Payload?.Bytes != null);
        if (Payload?.Bytes != null)
        {
          writer.WriteByteArray(Payload.Bytes);
        }
      }
      catch (TRexSerializationVersionException e)
      {
        _log.LogError(e, $"Serialization version exception in {nameof(SubGridProgressiveResponseRequestComputeFuncArgument)}.ToBinary()");
        throw; // Mostly for testing purposes...
      }
      catch (Exception e)
      {
        _log.LogCritical(e, $"Exception in {nameof(SubGridProgressiveResponseRequestComputeFuncArgument)}.ToBinary()");
      }
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      try
      {
        base.FromBinary(reader);

        VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

        NodeId = reader.ReadGuid() ?? Guid.Empty;
        RequestDescriptor = reader.ReadGuid() ?? Guid.Empty;

        if (reader.ReadBoolean())
        {
          Payload = new SerialisedByteArrayWrapper(reader.ReadByteArray());
        }
      }
      catch (TRexSerializationVersionException e)
      {
        _log.LogError(e, $"Serialization version exception in {nameof(SubGridProgressiveResponseRequestComputeFuncArgument)}.FromBinary()");
        throw; // Mostly for testing purposes...
      }
      catch (Exception e)
      {
        _log.LogCritical(e, $"Exception in {nameof(SubGridProgressiveResponseRequestComputeFuncArgument)}.FromBinary()");
      }
    }
  }
}
