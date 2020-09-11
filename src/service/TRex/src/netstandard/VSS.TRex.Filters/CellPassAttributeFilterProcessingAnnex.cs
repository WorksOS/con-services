using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// CellPassAttributeFilter provides filtering support for grid data requested by the client
  /// </summary>
  public class CellPassAttributeFilterProcessingAnnex : ICellPassAttributeFilterProcessingAnnex
  {
    /// <summary>
    /// The top of the elevation range permitted for an individual cell being filtered against as
    /// elevation range filter.
    /// </summary>
    public double ElevationRangeTopElevationForCell { get; set; } = Consts.NullDouble;

    /// <summary>
    /// The bottom of the elevation range permitted for an individual cell being filtered against as
    /// elevation range filter.
    /// </summary>
    public double ElevationRangeBottomElevationForCell { get; set; } = Consts.NullDouble;

    /// <summary>
    /// A sub grid containing sampled elevations from a benchmark surface defining the bench surface for
    /// an elevation range filter.
    /// </summary>
    public float[,] ElevationRangeDesignElevations;

    /// <summary>
    /// Elevation parameters have been initialized in preparation for elevation range filtering, either
    /// by setting ElevationRangeBottomElevationForCell and ElevationRangeTopElevationForCell or by
    /// setting ElevationRangeDesignElevations top contain relevant benchmark elevations
    /// </summary>
    public bool ElevationRangeIsInitialized { get; set; }

    public void ClearElevationRangeFilterInitialization()
    {
      ElevationRangeIsInitialized = false;
      ElevationRangeDesignElevations = null;
    }

    public bool FilterPassUsingElevationRange(ref CellPass passValue)
    {
      if (!ElevationRangeIsInitialized)
        throw new TRexFilterProcessingException("Elevation range filter being used without the elevation range data being initialized");

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      return ElevationRangeBottomElevationForCell != Consts.NullDouble &&
             Range.InRange(passValue.Height, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    }

    public bool FiltersElevation(float elevation)
    {
      if (!ElevationRangeIsInitialized)
        throw new TRexFilterProcessingException("Elevation range filter being used without the elevation range data being initialized");

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      return ElevationRangeBottomElevationForCell != Consts.NullDouble &&
             Range.InRange(elevation, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    }

    public bool FiltersElevation(double elevation)
    {
      if (!ElevationRangeIsInitialized)
        throw new TRexFilterProcessingException("Elevation range filter being used without the elevation range data being initialized");

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      return ElevationRangeBottomElevationForCell != Consts.NullDouble &&
             Range.InRange(elevation, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    }

    public void InitializeElevationRangeFilter(ICellPassAttributeFilter attributeFilter, float[,] designElevations)
    {
      // If there is a design specified then initialize the filter using the design elevations
      // queried and supplied by the caller, otherwise the specified Elevation level, offset and thickness
      // are used to calculate an elevation bracket.

      var elevationRangeIsLevelAndThicknessOnly = designElevations == null;
      if (elevationRangeIsLevelAndThicknessOnly)
      {
        ElevationRangeTopElevationForCell = attributeFilter.ElevationRangeLevel + attributeFilter.ElevationRangeOffset;
        ElevationRangeBottomElevationForCell = ElevationRangeTopElevationForCell - attributeFilter.ElevationRangeThickness;
      }
      else
      {
        ElevationRangeDesignElevations = designElevations;
      }

      ElevationRangeIsInitialized = true;
    }

    public void InitializeFilteringForCell(ICellPassAttributeFilter attributeFilter, byte subGridCellX, byte subGridCellY)
    {
      if (!attributeFilter.HasElevationRangeFilter)
        return;

      if (ElevationRangeDesignElevations != null)
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (ElevationRangeDesignElevations[subGridCellX, subGridCellY] == Consts.NullHeight)
        {
          ElevationRangeTopElevationForCell = Consts.NullDouble;
          ElevationRangeBottomElevationForCell = Consts.NullDouble;
          return;
        }

        ElevationRangeTopElevationForCell = ElevationRangeDesignElevations[subGridCellX, subGridCellY] + attributeFilter.ElevationRangeOffset;
      }
      else
      {
        ElevationRangeTopElevationForCell = attributeFilter.ElevationRangeLevel + attributeFilter.ElevationRangeOffset;
      }

      ElevationRangeBottomElevationForCell = ElevationRangeTopElevationForCell - attributeFilter.ElevationRangeThickness;
    }
  }
}
