using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
    /// <returns></returns>
    Task<(double spotHeight, DesignProfilerRequestResult errorCode)> GetDesignSpotHeight(Guid siteModelID, double offset,double spotX, double spotY);

    /// <summary>
    /// Calculates an elevation sub grid for a design sub grid on this design
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="offset"></param>
    /// <param name="originCellAddress"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    Task<(IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode)> GetDesignHeights(Guid siteModelID, double offset, SubGridCellAddress originCellAddress, double cellSize);

    /// <summary>
    /// Calculates a filter mask for a designated sub grid on this design
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="originCellAddress"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    Task<(SubGridTreeBitmapSubGridBits filterMask, DesignProfilerRequestResult errorCode)> GetFilterMask(Guid siteModelID, SubGridCellAddress originCellAddress, double cellSize);

    DesignDescriptor DesignDescriptor { get; }

    Task<(List<XYZS> profile, DesignProfilerRequestResult errorCode)> ComputeProfile(Guid projectUID, XYZ[] profilePath, double cellSize, double offset);
  }
}
