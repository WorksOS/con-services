using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfilerBuilder<T> where T: class, IProfileCellBase, new()
  {
    /// <summary>
    /// Builder responsible for per-cell profile analysis
    /// </summary>
    ICellLiftBuilder CellLiftBuilder { get; set; }

    /// <summary>
    /// Builder responsible for constructing cell vector from profile line
    /// </summary>
    ICellProfileBuilder<T> CellProfileBuilder { get; set; }

    /// <summary>
    /// Builder responsible from building overall profile information from cell vector
    /// </summary>
    IProfileLiftBuilder<T> ProfileLiftBuilder { get; set; }

    /// <summary>
    /// Configures a new profile builder that provides the three core builders used in profiling: construction of cell vector from profile line,
    /// profile analysis orchestration and per cell layer/statistics calculation
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="productionDataExistenceMap"></param>
    /// <param name="gridDataType"></param>
    /// <param name="passFilter"></param>
    /// <param name="cellFilter"></param>
    /// <param name="cutFillDesign"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="PopulationControl"></param>
    /// <param name="CellPassFastEventLookerUpper"></param>
    /// <param name="slicerToolUsed"></param>
    void Configure(ISiteModel siteModel,
      ISubGridTreeBitMask productionDataExistenceMap,
      GridDataType gridDataType,
      ICellPassAttributeFilter passFilter,
      ICellSpatialFilter cellFilter,
      IDesign cutFillDesign,
      IDesign cellPassFilter_ElevationRangeDesign,
      IFilteredValuePopulationControl PopulationControl,
      ICellPassFastEventLookerUpper CellPassFastEventLookerUpper,
      bool slicerToolUsed = true);
  }
}
