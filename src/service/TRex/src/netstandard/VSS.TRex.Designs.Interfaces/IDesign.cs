using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees;

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
    /// The internal identifier of the design
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
    /// Calculates a spot elevation designated location on this design
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="offset"></param>
    /// <param name="spotX"></param>
    /// <param name="spotY"></param>
    /// <param name="spotHeight"></param>
    /// <param name="errorCode"></param>
    void GetDesignSpotHeight(Guid siteModelID, double offset,
      double spotX, double spotY,
      out double spotHeight,
      out DesignProfilerRequestResult errorCode);

    /// <summary>
    /// Calculates an elevation sub grid for a design sub grid on this design
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="originCellAddress"></param>
    /// <param name="cellSize"></param>
    /// <param name="designHeights"></param>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    void GetDesignHeights(Guid siteModelID,
      SubGridCellAddress originCellAddress,
      double cellSize,
      out IClientHeightLeafSubGrid designHeights,
      out DesignProfilerRequestResult errorCode);

    /// <summary>
    /// Calculates a filter mask for a designated sub grid on this design
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="originCellAddress"></param>
    /// <param name="cellSize"></param>
    /// <param name="filterMask"></param>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    void GetFilterMask(Guid siteModelID,
      SubGridCellAddress originCellAddress,
      double cellSize,
      out SubGridTreeBitmapSubGridBits filterMask,
      out DesignProfilerRequestResult errorCode);

    DesignDescriptor DesignDescriptor { get; }

    List<XYZS> ComputeProfile(Guid projectUID, XYZ[] profilePath, double cellSize, double offset, out DesignProfilerRequestResult errorCode);
  }
}
