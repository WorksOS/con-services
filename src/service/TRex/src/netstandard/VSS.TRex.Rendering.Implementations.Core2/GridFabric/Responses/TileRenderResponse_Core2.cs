using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Extensions;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.GridFabric.Responses;
using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses
{
  public class TileRenderResponse_Core2 : TileRenderResponse, IEquatable<TileRenderResponse_Core2>
  {
    public byte[] TileBitmapData { get; set; }

    public override ITileRenderResponse AggregateWith(ITileRenderResponse other)
    {
      // Composite the bitmap held in this response with the bitmap held in 'other'

      // throw new NotImplementedException("Bitmap compositing not implemented");

      return null;
    }

    public override void SetBitmap(object bitmap)
    {
      TileBitmapData = ((Draw.Bitmap)bitmap)?.BitmapToByteArray();
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteBoolean(TileBitmapData != null);
      writer.WriteByteArray(TileBitmapData);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      if (reader.ReadBoolean())
        TileBitmapData = reader.ReadByteArray();
    }

    public bool Equals(TileRenderResponse_Core2 other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      return base.Equals(other) &&

             (Equals(TileBitmapData, other.TileBitmapData) ||
              (TileBitmapData != null && other.TileBitmapData != null && TileBitmapData.Length == other.TileBitmapData.Length && TileBitmapData.SequenceEqual(other.TileBitmapData)));
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((TileRenderResponse_Core2) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (base.GetHashCode() * 397) ^ (TileBitmapData != null ? TileBitmapData.GetHashCode() : 0);
      }
    }
  }
}
