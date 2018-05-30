using VSS.TRex.Designs.Storage;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Primary class responsible for computing profile information based on cell based production data. It constructs builder instances
  /// for the primary activities of collation of profile cells for a profile line, processing of those cells as a whole with respect to
  /// filtering and other parameters, and per-cell processing for layer analysis and other statistics
  /// to 
  /// </summary>
  public class ProfilerBuilder : IProfilerBuilder
  {
    private static IProfilerBuilderFactory factory = DI.DIContext.Obtain<IProfilerBuilderFactory>();

    /// <summary>
    /// Builder responsible fopr per-cell profile analysis
    /// </summary>
    public ICellLiftBuilder CellLiftBuilder { get; set; }

    /// <summary>
    /// Builder responsible for constructing cell vector from profile line
    /// </summary>
    public ICellProfileBuilder CellProfileBuilder { get; set; }

    /// <summary>
    /// Buidler responsibler from building overall profile informationk from cell vector
    /// </summary>
    public IProfileLiftBuilder ProfileLiftBuilder { get; set; }

    /// <summary>
    /// Constructs a new profile builder that provides the three core builders used in profiling: construction of cell vector from profile line,
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
    public ProfilerBuilder(ISiteModel siteModel,
      SubGridTreeBitMask productionDataExistenceMap,
      GridDataType gridDataType,
      CellPassAttributeFilter passFilter,
      CellSpatialFilter cellFilter,
      Design cutFillDesign,
      Design cellPassFilter_ElevationRangeDesign,
      FilteredValuePopulationControl PopulationControl,
      CellPassFastEventLookerUpper CellPassFastEventLookerUpper)    
    {
        CellLiftBuilder = factory.NewCellLiftBuilder(siteModel, gridDataType, PopulationControl, passFilter, CellPassFastEventLookerUpper);

        CellProfileBuilder = factory.NewCellProfileBuilder(siteModel, cellFilter, cutFillDesign);

        ProfileLiftBuilder = factory.NewProfileLiftBuilder(siteModel, productionDataExistenceMap, passFilter, cellFilter, cellPassFilter_ElevationRangeDesign, CellLiftBuilder);
    }
  }
}
