using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class DesignSubGridFilterMaskArgument : DesignSubGridRequestArgumentBase
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The X origin location for the patch of elevations, or spot elevation, to be computed from
    /// </summary>
    public uint OriginX { get; set; }

    /// <summary>
    /// The Y origin location for the patch of elevations, or spot elevation, to be computed from
    /// </summary>
    public uint OriginY { get; set; }

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    public DesignSubGridFilterMaskArgument()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="designUid"></param>
    public DesignSubGridFilterMaskArgument(Guid siteModelID,
      uint originX,
      uint originY,
      Guid designUid,
      double cellSize) : base(siteModelID, designUid, 0)
    {
      OriginX = originX;
      OriginY = originY;
      CellSize = cellSize;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{ProjectID}, Origin:{OriginX}/{OriginY}, CellSize:{CellSize}, Design:{ReferenceDesignUID}";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)OriginX);
      writer.WriteInt((int)OriginY);
      writer.WriteDouble(CellSize);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      OriginX = (uint)reader.ReadInt();
      OriginY = (uint)reader.ReadInt();
      CellSize = reader.ReadDouble();
    }
  }
}
