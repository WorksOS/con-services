using System;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Geometry;

namespace VSS.TRex.Filters.Interfaces
{
  public interface ICellSpatialFilterModel : IFromToBinary
  {
    /// <summary>
    /// The fence used for polygon based spatial filtering
    /// </summary>
    Fence Fence { get; set; }

    /// <summary>
    /// The fence used to represent the spatial restriction derived from an alignment filter expressed as a 
    /// station and offset range with respect tot he alignment center line geometry expressed as a polygon
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
    /// Determines if the point-radius should be applied as a square rather than a circle
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
    /// The design used as an alignment mask spatial filter
    /// </summary>
    Guid AlignmentDesignMaskDesignUID { get; set; }
  }
}
