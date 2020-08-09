using System;
using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Rendering.GridFabric.Responses
{
  /// <summary>
  /// Contains the response bitmap for a tile request. Supports compositing of another bitmap with this one
  /// </summary>
  public class TileRenderResponse : SubGridsPipelinedResponseBase, IAggregateWith<TileRenderResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<TileRenderResponse>();

    private static byte VERSION_NUMBER = 1;

    public byte[] TileBitmapData { get; set; }

    public TileRenderResponse AggregateWith(TileRenderResponse other)
    {
      // Composite the bitmap held in this response with the bitmap held in 'other'
      // ...

      return null;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      try
      {
        base.ToBinary(writer);

        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

        writer.WriteBoolean(TileBitmapData != null);
        if (TileBitmapData != null)
        {
          writer.WriteByteArray(TileBitmapData);
        }
      }
      catch (TRexSerializationVersionException e)
      {
        _log.LogError(e, $"Serialization version exception in {nameof(TileRenderResponse)}.ToBinary()");
        throw; // Mostly for testing purposes...
      }
      catch (Exception e)
      {
        _log.LogCritical(e, $"Exception in {nameof(TileRenderResponse)}.ToBinary()");
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void FromBinary(IBinaryRawReader reader)
    {
      try
      {
        base.FromBinary(reader);

        VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

        if (reader.ReadBoolean())
        {
          TileBitmapData = reader.ReadByteArray();
        }
      }
      catch (TRexSerializationVersionException e)
      {
        _log.LogError(e, $"Serialization version exception in {nameof(TileRenderResponse)}.FromBinary()");
        throw; // Mostly for testing purposes...
      }
      catch (Exception e)
      {
        _log.LogCritical(e, $"Exception in {nameof(TileRenderResponse)}.FromBinary()");
      }
    }
  }
}
