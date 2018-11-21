using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Rendering.GridFabric.Arguments
{
  public class TileRenderRequestArgument : BaseApplicationServiceRequestArgument, IEquatable<BaseApplicationServiceRequestArgument>
  {
    private const byte VERSION_NUMBER = 1;

    public DisplayMode Mode { get; set; } = DisplayMode.Height;

    public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted();

    public bool CoordsAreGrid { get; set; }

    public ushort PixelsX { get; set; } = 256;
    public ushort PixelsY { get; set; } = 256;

    public ICombinedFilter Filter1 { get; set; }
    public ICombinedFilter Filter2 { get; set; }

    public TileRenderRequestArgument()
    { }

    public TileRenderRequestArgument(Guid siteModelID,
                                     DisplayMode mode,
                                     BoundingWorldExtent3D extents,
                                     bool coordsAreGrid,
                                     ushort pixelsX,
                                     ushort pixelsY,
                                     ICombinedFilter filter1,
                                     ICombinedFilter filter2,
                                     Guid referenceDesignId)
    {
      ProjectID = siteModelID;
      Mode = mode;
      Extents = extents;
      CoordsAreGrid = coordsAreGrid;
      PixelsX = pixelsX;
      PixelsY = pixelsY;
      Filter1 = filter1;
      Filter2 = filter2;
      ReferenceDesignID = referenceDesignId;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteByte(VERSION_NUMBER);

      writer.WriteInt((int)Mode);

      writer.WriteBoolean(Extents != null);
      Extents.ToBinary(writer);

      writer.WriteBoolean(CoordsAreGrid);
      writer.WriteInt(PixelsX);
      writer.WriteInt(PixelsY);

      writer.WriteBoolean(Filter1 != null);
      Filter1?.ToBinary(writer);

      writer.WriteBoolean(Filter2 != null);
      Filter2?.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      var version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      Mode = (DisplayMode)reader.ReadInt();

      if (reader.ReadBoolean())
      {
        Extents = new BoundingWorldExtent3D();
        Extents.FromBinary(reader);
      }

      CoordsAreGrid = reader.ReadBoolean();
      PixelsX = (ushort)reader.ReadInt();
      PixelsY = (ushort)reader.ReadInt();

      if (reader.ReadBoolean())
      {
        Filter1 = new CombinedFilter();
        Filter1.FromBinary(reader);
      }

      if (reader.ReadBoolean())
      {
        Filter2 = new CombinedFilter();
        Filter2.FromBinary(reader);
      }
    }

    protected bool Equals(TileRenderRequestArgument other)
    {
      return base.Equals(other) && 
             Equals(Extents, other.Extents) && 
             Mode == other.Mode && 
             CoordsAreGrid == other.CoordsAreGrid && 
             PixelsX == other.PixelsX && 
             PixelsY == other.PixelsY && 
             Equals(Filter1, other.Filter1) && 
             Equals(Filter2, other.Filter2);
    }

    public new bool Equals(BaseApplicationServiceRequestArgument other)
    {
      return Equals(other as TileRenderRequestArgument);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((TileRenderRequestArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ (Extents != null ? Extents.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (int) Mode;
        hashCode = (hashCode * 397) ^ CoordsAreGrid.GetHashCode();
        hashCode = (hashCode * 397) ^ PixelsX.GetHashCode();
        hashCode = (hashCode * 397) ^ PixelsY.GetHashCode();
        hashCode = (hashCode * 397) ^ (Filter1 != null ? Filter1.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Filter2 != null ? Filter2.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}
