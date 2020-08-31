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
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(TileBitmapData != null);
      if (TileBitmapData != null)
      {
        writer.WriteByteArray(TileBitmapData);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        if (reader.ReadBoolean())
        {
          TileBitmapData = reader.ReadByteArray();
        }
      }
    }
  }
}
