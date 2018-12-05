using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignElevationPatchArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// The X origin location for the patch of elevations to be computed from
    /// </summary>
    public uint OriginX { get; set; }

    /// <summary>
    /// The Y origin location for the patch of elevations to be computed from
    /// </summary>
    public uint OriginY { get; set; }

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// The ID of the design file the elevations are to be interpolated from
    /// </summary>
    public Guid DesignUid { get; set; }

    /// <summary>
    /// The offset to be applied to computed elevations
    /// </summary>
    public double Offset { get; set; }

    /// <summary>
    /// A map of the cells within the subgrid patch to be computed
    /// </summary>
    //        public SubGridTreeBitmapSubGridBits ProcessingMap { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateDesignElevationPatchArgument()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="cellSize"></param>
    /// <param name="designUid"></param>
    /// <param name="offset"></param>
    // /// <param name="processingMap"></param>
    public CalculateDesignElevationPatchArgument(Guid siteModelID,
                                     uint originX,
                                     uint originY,
                                     double cellSize,
                                     Guid designUid,
                                     double offset
                                        /*SubGridTreeBitmapSubGridBits processingMap*/) : this()
    {
      ProjectID = siteModelID;
      OriginX = originX;
      OriginY = originY;
      CellSize = cellSize;
      DesignUid = designUid;
      Offset = offset;
      //            ProcessingMap = processingMap;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{ProjectID}, Origin:{OriginX}/{OriginY}, CellSize:{CellSize}, Design:{DesignUid}";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteInt((int)OriginX);
      writer.WriteInt((int)OriginY);
      writer.WriteDouble(CellSize);

      writer.WriteGuid(DesignUid);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      OriginX = (uint)reader.ReadInt();
      OriginY = (uint)reader.ReadInt();
      CellSize = reader.ReadDouble();

      DesignUid = reader.ReadGuid() ?? Guid.Empty;
    }
  }
}
