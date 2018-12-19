using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignElevationPatchArgument : DesignSubGridRequestArgumentBase
  {
    /// <summary>
    /// The offset to be applied to computed elevations
    /// </summary>
    public double Offset { get; set; }

    /// <summary>
    /// A map of the cells within the subgrid patch to be computed
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
      /*SubGridTreeBitmapSubGridBits processingMap*/) : base(siteModelID, originX, originY, cellSize, designUid)
    {
      Offset = offset;
      //            ProcessingMap = processingMap;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $", Offset{Offset}";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteDouble(Offset);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      Offset = reader.ReadDouble();
    }
  }
}
