using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignElevationPatchArgument : BaseApplicationServiceRequestArgument, IEquatable<BaseApplicationServiceRequestArgument>
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
    /// The descriptor of the design file the elevations are to be interpolated from
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; set; }

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
    /// <param name="designDescriptor"></param>
    // /// <param name="processingMap"></param>
    public CalculateDesignElevationPatchArgument(Guid siteModelID,
                                     uint originX,
                                     uint originY,
                                     double cellSize,
                                     DesignDescriptor designDescriptor/*,
                                         SubGridTreeBitmapSubGridBits processingMap*/) : this()
    {
      ProjectID = siteModelID;
      OriginX = originX;
      OriginY = originY;
      CellSize = cellSize;
      DesignDescriptor = designDescriptor;
      //            ProcessingMap = processingMap;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{ProjectID}, Origin:{OriginX}/{OriginY}, CellSize:{CellSize}, Design:{DesignDescriptor}";
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteInt((int)OriginX);
      writer.WriteInt((int)OriginY);
      writer.WriteDouble(CellSize);

      DesignDescriptor.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      OriginX = (uint)reader.ReadInt();
      OriginY = (uint)reader.ReadInt();
      CellSize = reader.ReadDouble();

      DesignDescriptor.FromBinary(reader);
    }

    protected bool Equals(CalculateDesignElevationPatchArgument other)
    {
      return base.Equals(other) && 
             OriginX == other.OriginX && 
             OriginY == other.OriginY && 
             CellSize.Equals(other.CellSize) && 
             DesignDescriptor.Equals(other.DesignDescriptor);
    }

    public new bool Equals(BaseApplicationServiceRequestArgument other)
    {
      return Equals(other as CalculateDesignElevationPatchArgument);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CalculateDesignElevationPatchArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ (int) OriginX;
        hashCode = (hashCode * 397) ^ (int) OriginY;
        hashCode = (hashCode * 397) ^ CellSize.GetHashCode();
        hashCode = (hashCode * 397) ^ DesignDescriptor.GetHashCode();
        return hashCode;
      }
    }
  }
}
