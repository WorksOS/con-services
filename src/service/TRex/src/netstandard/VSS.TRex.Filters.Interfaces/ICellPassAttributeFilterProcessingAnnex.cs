using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Filters.Interfaces
{
  public interface ICellPassAttributeFilterProcessingAnnex
  {
    /// <summary>
    /// The top of the elevation range permitted for an individual cell being filtered against as
    /// elevation range filter.
    /// </summary>
    double ElevationRangeTopElevationForCell { get; set; }

    /// <summary>
    /// The bottom of the elevation range permitted for an individual cell being filtered against as
    /// elevation range filter.
    /// </summary>
    double ElevationRangeBottomElevationForCell { get; set; }

    /// <summary>
    /// Elevation parameters have been initialized in preparation for elevation range filtering, either
    /// by setting ElevationRangeBottomElevationForCell and ElevationRangeTopElevationForCell or by
    /// setting ElevationRangeDesignElevations top contain relevant benchmark elevations
    /// </summary>
    bool ElevationRangeIsInitialized { get; set; }

    void ClearElevationRangeFilterInitialization();
    bool FilterPassUsingElevationRange(ref CellPass PassValue);
    bool FiltersElevation(float Elevation);
    bool FiltersElevation(double Elevation);
    void InitializeElevationRangeFilter(ICellPassAttributeFilter attributeFilter, IClientHeightLeafSubGrid DesignElevations);
    void InitializeFilteringForCell(ICellPassAttributeFilter attributeFilter, byte ASubGridCellX, byte ASubGridCellY);
  }
}
