using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Primary class responsible for computing profile information based on cell based production data. It constructs builder instances
  /// for the primary activities of collation of profile cells for a profile line, processing of those cells as a whole with respect to
  /// filtering and other parameters, and per-cell processing for layer analysis and other statistics
  /// </summary>
  public class ProfilerBuilder<T> : IProfilerBuilder<T> where T: class, IProfileCellBase, new()
  {
    private static readonly IProfilerBuilderFactory<T> factory = DI.DIContext.Obtain<IProfilerBuilderFactory<T>>();

    /// <summary>
    /// Builder responsible for constructing cell vector from profile line
    /// </summary>
    public ICellProfileBuilder<T> CellProfileBuilder { get; set; }

    /// <summary>
    /// Builder responsible from building overall profile information from cell vector
    /// </summary>
    public ICellProfileAnalyzer<T> CellProfileAnalyzer { get; set; }

    /// <summary>
    /// Builder responsible for per-cell profile analysis
    /// </summary>
    public ICellLiftBuilder CellLiftBuilder { get; set; }

    public ProfilerBuilder()
    {
    }

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
    public void Configure(ISiteModel siteModel,
      ISubGridTreeBitMask productionDataExistenceMap,
      GridDataType gridDataType,
      ICellPassAttributeFilter passFilter,
      ICellSpatialFilter cellFilter,
      IDesign cutFillDesign,
      IDesign cellPassFilter_ElevationRangeDesign,
      IFilteredValuePopulationControl PopulationControl,
      ICellPassFastEventLookerUpper CellPassFastEventLookerUpper,
      bool slicerToolUsed = true)    
    {
        CellLiftBuilder = factory.NewCellLiftBuilder(siteModel, gridDataType, PopulationControl, passFilter, CellPassFastEventLookerUpper);

        CellProfileBuilder = factory.NewCellProfileBuilder(siteModel, cellFilter, cutFillDesign, slicerToolUsed);

        CellProfileAnalyzer = factory.NewCellProfileAnalyzer(siteModel, productionDataExistenceMap, passFilter, cellFilter, cellPassFilter_ElevationRangeDesign, CellLiftBuilder);
    }
  }
}
