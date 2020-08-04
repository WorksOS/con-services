using System;
using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Extensions;
using VSS.TRex.Common;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.GridFabric.Responses;
using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses
{
  public class TileRenderResponse_Core2 : TileRenderResponse
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<TileRenderResponse_Core2>();

    private static byte VERSION_NUMBER = 1;

    public byte[] TileBitmapData { get; set; }

    public override ITileRenderResponse AggregateWith(ITileRenderResponse other)
    {
      // Composite the bitmap held in this response with the bitmap held in 'other'
      // ...

      return null;
    }

    public override void SetBitmap(object bitmap)
    {
      TileBitmapData = ((Draw.Bitmap)bitmap)?.BitmapToByteArray();
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      try
      {
        base.ToBinary(writer);

        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

        writer.WriteBoolean(TileBitmapData != null);
        writer.WriteByteArray(TileBitmapData);
      }
      catch (Exception e)
      {
        _log.LogCritical(e, $"Exception in {nameof(TileRenderResponse_Core2)}.ToBinary()");
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      try
      {
        base.FromBinary(reader);

        VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

        if (reader.ReadBoolean())
          TileBitmapData = reader.ReadByteArray();
      }
      catch (Exception e)
      {
        _log.LogCritical(e, $"Exception in {nameof(TileRenderResponse_Core2)}.FromBinary()");
      }
    }
  }
}
