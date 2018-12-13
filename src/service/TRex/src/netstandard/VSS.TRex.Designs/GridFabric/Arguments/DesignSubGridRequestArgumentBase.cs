using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class DesignSubGridRequestArgumentBase : BaseApplicationServiceRequestArgument
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
    /// Default no-arg constructor
    /// </summary>
    public DesignSubGridRequestArgumentBase()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="cellSize"></param>
    /// <param name="referenceDesignUID"></param>
    // /// <param name="processingMap"></param>
    public DesignSubGridRequestArgumentBase(Guid siteModelID,
                                     uint originX,
                                     uint originY,
                                     double cellSize,
                                     Guid referenceDesignUID) : this()
    {
      ProjectID = siteModelID;
      OriginX = originX;
      OriginY = originY;
      CellSize = cellSize;
      ReferenceDesignUID = referenceDesignUID;
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

      writer.WriteInt((int)OriginX);
      writer.WriteInt((int)OriginY);
      writer.WriteDouble(CellSize);

      writer.WriteGuid(ReferenceDesignUID);
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

      ReferenceDesignUID = reader.ReadGuid() ?? Guid.Empty;
    }
  }
}
