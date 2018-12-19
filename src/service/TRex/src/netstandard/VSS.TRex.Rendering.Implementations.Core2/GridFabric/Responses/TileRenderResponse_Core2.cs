using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Extensions;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.GridFabric.Responses;
using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses
{
  public class TileRenderResponse_Core2 : TileRenderResponse
  {
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
      base.ToBinary(writer);

      writer.WriteBoolean(TileBitmapData != null);
      writer.WriteByteArray(TileBitmapData);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      if (reader.ReadBoolean())
        TileBitmapData = reader.ReadByteArray();
    }
  }
}
