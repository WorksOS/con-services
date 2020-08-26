using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignElevationPatchArgument : DesignSubGridRequestArgumentBase
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The X origin location for the patch of elevations, or spot elevation, to be computed from
    /// </summary>
    public int OriginX { get; set; }

    /// <summary>
    /// The Y origin location for the patch of elevations, or spot elevation, to be computed from
    /// </summary>
    public int OriginY { get; set; }

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// A map of the cells within the sub grid patch to be computed
    /// </summary>
    ///        public SubGridTreeBitmapSubGridBits ProcessingMap { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateDesignElevationPatchArgument()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    public CalculateDesignElevationPatchArgument(Guid siteModelID,
      int originX,
      int originY,
      double cellSize,
      DesignOffset referenceDesign) : base(siteModelID, referenceDesign)
    {
      OriginX = originX;
      OriginY = originY;
      CellSize = cellSize;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{ProjectID}, Origin:{OriginX}/{OriginY}, CellSize:{CellSize}, Design:{ReferenceDesign?.DesignID}, Offset:{ReferenceDesign?.Offset}";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(OriginX);
      writer.WriteInt(OriginY);
      writer.WriteDouble(CellSize);
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
        OriginX = reader.ReadInt();
        OriginY = reader.ReadInt();
        CellSize = reader.ReadDouble();
      }
    }
  }
}
