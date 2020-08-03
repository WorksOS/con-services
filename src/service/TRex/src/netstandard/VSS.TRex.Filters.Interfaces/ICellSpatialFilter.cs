using VSS.TRex.Geometry;

namespace VSS.TRex.Filters.Interfaces
{
  public interface ICellSpatialFilter : ICellSpatialFilterModel
  {
 
    /// <summary>
    /// Determines if the type of the spatial filter is Spatial or Positional
    /// </summary>
    bool HasSpatialOrPositionalFilters { get; }

    /// <summary>
    /// Return a formatted string indicating the state of the filter flags
    /// </summary>
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
    bool HasAlignmentDesignMask();

    /// <summary>
    /// Determines if the filter contains sufficient information to adequately describe an active design mask spatial filter
    /// </summary>
    bool HasSurfaceDesignMask();

    /// <summary>
    /// Determines if a cell given by it's central location is included in the spatial filter
    /// </summary>
    bool IsCellInSelection(double CellCenterX, double CellCenterY);

    /// <summary>
    /// Determines if an arbitrary location is included in the spatial filter.
    /// </summary>
    bool IsPositionInSelection(double X, double Y);

    /// <summary>
    /// Calculate a bounding extent of this spatial filter with a given external bounding extent
    /// </summary>
    void CalculateIntersectionWithExtents(BoundingWorldExtent3D Extents);

    void Assign(ICellSpatialFilter source);
  }
}
