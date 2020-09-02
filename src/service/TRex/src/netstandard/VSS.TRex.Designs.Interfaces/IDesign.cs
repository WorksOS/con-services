using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Interfaces.Interfaces;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesign
  {
    /// <summary>
    /// Binary serialization logic
    /// </summary>
    void Write(BinaryWriter writer);

    /// <summary>
    /// Binary deserialization logic
    /// </summary>
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
    IDesign Clone();

    /// <summary>
    /// ToString() for Design
    /// </summary>
    string ToString();

    /// <summary>
    /// Determine if two designs are equal
    /// </summary>
    bool Equals(IDesign other);

    /// <summary>
    /// Calculates a spot elevation designated location on this design
    /// </summary>
    Task<(double spotHeight, DesignProfilerRequestResult errorCode)> GetDesignSpotHeight(Guid siteModelID, double offset,double spotX, double spotY);

    /// <summary>
    /// Calculates an elevation sub grid for a design sub grid on this design by making a request to the design elevation service
    /// </summary>
    Task<(IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode)> GetDesignHeightsViaDesignElevationService(Guid siteModelID, double offset, SubGridCellAddress originCellAddress, double cellSize);

    /// <summary>
    /// Calculates an elevation sub grid for a design sub grid on this design by performing the design elevation query locally in the same process
    /// </summary>
    (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode) GetDesignHeightsViaLocalCompute(ISiteModelBase siteModel, double offset, SubGridCellAddress originCellAddress, double cellSize);
    
    /// <summary>
    /// Calculates a filter mask for a designated sub grid on this design
    /// </summary>
    Task<(SubGridTreeBitmapSubGridBits filterMask, DesignProfilerRequestResult errorCode)> GetFilterMaskViaDesignElevationService(Guid siteModelID, SubGridCellAddress originCellAddress, double cellSize);

    /// <summary>
    /// Calculates a filter mask for a designated sub grid on this design
    /// </summary>
    (SubGridTreeBitmapSubGridBits filterMask, DesignProfilerRequestResult errorCode) GetFilterMaskViaLocalCompute(ISiteModelBase siteModel, SubGridCellAddress originCellAddress, double cellSize);

    DesignDescriptor DesignDescriptor { get; }

    Task<(List<XYZS> profile, DesignProfilerRequestResult errorCode)> ComputeProfile(Guid projectUid, WGS84Point startPoint, WGS84Point endPoint, double cellSize, double offset, bool arePositionsGrid);
  }
}
