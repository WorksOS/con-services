using System;
using System.IO;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesign
  {
    /// <summary>
    /// Binary serialization logic
    /// </summary>
    /// <param name="writer"></param>
    void Write(BinaryWriter writer);

    /// <summary>
    /// Binary serialization logic
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    void Write(BinaryWriter writer, byte[] buffer);

    /// <summary>
    /// Binary deserialization logic
    /// </summary>
    /// <param name="reader"></param>
    void Read(BinaryReader reader);

    /// <summary>
    /// The intenal identifier of the design
    /// </summary>
    Guid ID { get; }

    /// <summary>
    /// Returns the real world 3D enclosing extents for the surveyed surface topology, including any configured vertical offset
    /// </summary>
    BoundingWorldExtent3D Extents { get; }

    /// <summary>
    /// Produces a deep clone of the design
    /// </summary>
    /// <returns></returns>
    IDesign Clone();

    /// <summary>
    /// ToString() for Design
    /// </summary>
    /// <returns></returns>
    string ToString();

    /// <summary>
    /// Determine if two designs are equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool Equals(IDesign other);

    /// <summary>
    /// Calculates an elevation subgrid for a design subgrid on this design
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="originCellAddress"></param>
    /// <param name="cellSize"></param>
    /// <param name="designHeights"></param>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    bool GetDesignHeights(Guid siteModelID,
      ISubGridCellAddress originCellAddress,
      double cellSize,
      out IClientHeightLeafSubGrid designHeights,
      out DesignProfilerRequestResult errorCode);

    DesignDescriptor Get_DesignDescriptor();
  }
}
