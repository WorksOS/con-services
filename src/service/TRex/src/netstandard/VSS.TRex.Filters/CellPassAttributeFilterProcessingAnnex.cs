using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Client.Interfaces;
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
    public IClientHeightLeafSubGrid ElevationRangeDesignElevations;

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

    public bool FilterPassUsingElevationRange(ref CellPass PassValue)
    {
      if (!ElevationRangeIsInitialized)
        throw new TRexFilterProcessingException("Elevation range filter being used without the elevation range data being initialized");

      return (ElevationRangeBottomElevationForCell != Consts.NullDouble) &&
             Range.InRange(PassValue.Height, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    }

    public bool FiltersElevation(float Elevation)
    {
      if (!ElevationRangeIsInitialized)
        throw new TRexFilterProcessingException("Elevation range filter being used without the elevation range data being initialized");

      return ElevationRangeBottomElevationForCell != Consts.NullDouble &&
             Range.InRange(Elevation, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    }

    public bool FiltersElevation(double Elevation)
    {
      if (!ElevationRangeIsInitialized)
        throw new TRexFilterProcessingException("Elevation range filter being used without the elevation range data being initialized");

      return ElevationRangeBottomElevationForCell != Consts.NullDouble &&
             Range.InRange(Elevation, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    } 

    public void InitializeElevationRangeFilter(ICellPassAttributeFilter attributeFilter, IClientHeightLeafSubGrid DesignElevations)
    {
      // If there is a design specified then initialize the filter using the design elevations
      // queried and supplied by the caller, otherwise the specified Elevation level, offset and thickness
      // are used to calculate an elevation bracket.

      bool ElevationRangeIsLevelAndThicknessOnly = DesignElevations == null;
      if (ElevationRangeIsLevelAndThicknessOnly)
      {
        ElevationRangeTopElevationForCell = attributeFilter.ElevationRangeLevel + attributeFilter.ElevationRangeOffset;
        ElevationRangeBottomElevationForCell = ElevationRangeTopElevationForCell - attributeFilter.ElevationRangeThickness;
      }
      else
      {
        ElevationRangeDesignElevations = DesignElevations;
      }

      ElevationRangeIsInitialized = true;
    }

    public void InitializeFilteringForCell(ICellPassAttributeFilter attributeFilter, byte ASubGridCellX, byte ASubGridCellY)
    {
      if (!attributeFilter.HasElevationRangeFilter)
        return;

      if (ElevationRangeDesignElevations != null)
      {
        if (ElevationRangeDesignElevations.Cells[ASubGridCellX, ASubGridCellY] == Consts.NullHeight)
        {
          ElevationRangeTopElevationForCell = Consts.NullDouble;
          ElevationRangeBottomElevationForCell = Consts.NullDouble;
          return;
        }

        ElevationRangeTopElevationForCell = ElevationRangeDesignElevations.Cells[ASubGridCellX, ASubGridCellY] + attributeFilter.ElevationRangeOffset;
      }
      else
      {
        ElevationRangeTopElevationForCell = attributeFilter.ElevationRangeLevel + attributeFilter.ElevationRangeOffset;
      }

      ElevationRangeBottomElevationForCell = ElevationRangeTopElevationForCell - attributeFilter.ElevationRangeThickness;
    }
  }
}
