using System;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Filters.Interfaces
{
  public interface ICellSpatialFilter : IFromToBinary
  {
    /// <summary>
    /// The fence used for polygon based spatial filtering
    /// </summary>
    Fence Fence { get; set; }

    /// <summary>
    /// The design used as an alignment mask spatial filter
    /// </summary>
    Guid AlignmentMaskDesignUID { get; set; }

    /// <summary>
    /// The fence used to represent the spatial restriction derived from an alignment filter expressed as a 
    /// station and offset range with respect tot he alignment centerline geometry expressed as a polygon
    /// </summary>
    Fence AlignmentFence { get; set; } // contains alignment boundary to help speed up filtering on alignment files

    /// <summary>
    /// The X ordinate of the positional spatial filter
    /// </summary>
    double PositionX { get; set; }

    /// <summary>
    /// The Y ordinate of the positional spatial filter
    /// </summary>
    double PositionY { get; set; }

    /// <summary>
    /// The radius of the positional spatial filter for point-radius positional filters
    /// </summary>
    double PositionRadius { get; set; }

    /// <summary>
    /// Determines if the point-radius shoudl be applied as a square rather than a circle
    /// </summary>
    bool IsSquare { get; set; }

    /// <summary>
    /// OverrideSpatialCellRestriction provides a rectangular, cell address based,
    /// restrictive boundary that overrides all other cell selection filter considerations
    /// in that it is always evaluated first. This is useful in contexts such as Web Map Service
    /// tile generation where the tile region itself is an overriding constraint
    /// on the data that needs to be queried
    /// </summary>
    BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; }

    /// <summary>
    /// The starting station of the parametrically defined alignment spatial filter
    /// </summary>
    double? StartStation { get; set; }

    /// <summary>
    /// The ending station of the parametrically defined alignment spatial filter
    /// </summary>
    double? EndStation { get; set; }

    /// <summary>
    /// The left offset of the parametrically defined alignment spatial filter
    /// </summary>
    double? LeftOffset { get; set; }

    /// <summary>
    /// The right offset of the parametrically defined alignment spatial filter
    /// </summary>
    double? RightOffset { get; set; }

    /// <summary>
    /// CoordsAreGrid controls whether the plan (XY/NE) coordinates in the spatial filters are to 
    /// be interpreted as rectangular cartesian coordinates or as WGS84 latitude/longitude coordinates
    /// </summary>
    bool CoordsAreGrid { get; set; }

    /// <summary>
    /// Restricts cells to spatial fence
    /// </summary>
    bool IsSpatial { get; set; }

    /// <summary>
    /// Restricts cells to spatial fence
    /// </summary>
    bool IsPositional { get; set; }

    /// <summary>
    /// Using a loaded surface design to 'mask' the cells that should be included in the filter
    /// </summary>
    bool IsDesignMask { get; set; }

    /// <summary>
    /// A design that acts as a spatial filter for cell selection. Only cells that have center locations that lie over the 
    /// design recorded in DesignMask will be included
    /// </summary>
    Guid SurfaceDesignMaskDesignUid { get; set; }

    /// <summary>
    /// Using a load alignment design to 'mask' the cells that should be included in the filter
    /// </summary>
    bool IsAlignmentMask { get; set; }

    /// <summary>
    /// Determines if the type of the spatial filter is Spatial or Positional
    /// </summary>
    bool HasSpatialOrPostionalFilters { get; }

    /// <summary>
    /// Return a formatted string indicating the state of the filter flags
    /// </summary>
    /// <returns></returns>
    string ActiveFiltersString();

    /// <summary>
    /// Clears all filter state to a state that will pass (accept) all cells
    /// </summary>
    void Clear();

    /// <summary>
    /// Removes all state related to an alignment mask filter and sets the alignment mask type to off
    /// </summary>
    void ClearAlignmentMask();

    /// <summary>
    /// Removes all state related to an design mask filter and sets the design mask type to off
    /// </summary>
    void ClearDesignMask();

    /// <summary>
    /// Removes all state related to a positional filter and sets the positional mask type to off
    /// </summary>
    void ClearPositional();

    /// <summary>
    /// Removes all state related to a polygonal filter and sets the spatial mask type to off
    /// </summary>
    void ClearSpatial();

    /// <summary>
    /// Determines if the filter contains sufficient information to adequately describe an active alignment
    /// spatial filter
    /// </summary>
    /// <returns></returns>
    bool HasAlignmentDesignMask();

    /// <summary>
    /// Determines if a cell given by it's central location is included in the spatial filter
    /// </summary>
    /// <param name="CellCenterX"></param>
    /// <param name="CellCenterY"></param>
    /// <returns></returns>
    bool IsCellInSelection(double CellCenterX, double CellCenterY);

    /// <summary>
    /// Determines if an arbitrary location is included in the spatial filter.
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <returns></returns>
    bool IsPositionInSelection(double X, double Y);

    /// <summary>
    /// Calculate a bounding extent of this spatial filter with a given external bounding extent
    /// </summary>
    /// <param name="Extents"></param>
    /// <returns></returns>
    BoundingWorldExtent3D CalculateIntersectionWithExtents(BoundingWorldExtent3D Extents);
  }
}
