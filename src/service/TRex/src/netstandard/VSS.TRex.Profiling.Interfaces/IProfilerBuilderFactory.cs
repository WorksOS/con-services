using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfilerBuilderFactory<T> where T : class, IProfileCellBase, new()
  {
    /// <summary>
    /// Creates a new builder responsible for processing layer and other information for single cells in a profile
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="gridDataType"></param>
    /// <param name="populationControl"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFastEventLookerUpper"></param>
    /// <returns></returns>
    ICellLiftBuilder NewCellLiftBuilder(ISiteModel siteModel,
      GridDataType gridDataType,
      IFilteredValuePopulationControl populationControl,
      IFilterSet filterSet,
      ICellPassFastEventLookerUpper cellPassFastEventLookerUpper);

    /// <summary>
    /// Creates a new builder responsible for determining a vector of cells that are cross by a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="filterSet"></param>
    /// <param name="cutFillDesign"></param>
    /// <param name="slicerToolUsed"></param>
    /// <returns></returns>
    ICellProfileBuilder<T> NewCellProfileBuilder(ISiteModel siteModel,
      IFilterSet filterSet,
      IDesign cutFillDesign,
      bool slicerToolUsed);

    /// <summary>
    /// Creates a new builder responsible for analyzing profile information for a cell of cells identified along a profile line
    /// </summary>
    /// <param name="profileStyle"></param>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="cellLiftBuilder"></param>
    /// <returns></returns>
    ICellProfileAnalyzer<T> NewCellProfileAnalyzer(ProfileStyle profileStyle,
      ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesign cellPassFilter_ElevationRangeDesign,
      ICellLiftBuilder cellLiftBuilder);
  }
}
