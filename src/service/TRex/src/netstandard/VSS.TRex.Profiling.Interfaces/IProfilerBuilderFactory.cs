using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfilerBuilderFactory
  {
    /// <summary>
    /// Creates a new builder responsible for processig layer and other information for single cells in a profile
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="gridDataType"></param>
    /// <param name="populationControl"></param>
    /// <param name="passFilter"></param>
    /// <param name="cellPassFastEventLookerUpper"></param>
    /// <returns></returns>
    ICellLiftBuilder NewCellLiftBuilder(ISiteModel siteModel,
      GridDataType gridDataType,
      IFilteredValuePopulationControl populationControl,
      ICellPassAttributeFilter passFilter,
      ICellPassFastEventLookerUpper cellPassFastEventLookerUpper);

    /// <summary>
    /// Creates a new bulder responsible for determining a vector of cells that are cross by a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="cellFilter"></param>
    /// <param name="cutFillDesign"></param>
    /// <param name="slicerToolUsed"></param>
    /// <returns></returns>
    ICellProfileBuilder NewCellProfileBuilder(ISiteModel siteModel,
      ICellSpatialFilter cellFilter,
      IDesign cutFillDesign,
      bool slicerToolUsed);

    /// <summary>
    /// Creates a new builder responsible for analysing profile information for a cell of cells idenfitied along a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="passFilter"></param>
    /// <param name="cellFilter"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="cellLiftBuilder"></param>
    /// <returns></returns>
    IProfileLiftBuilder NewProfileLiftBuilder(ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      ICellPassAttributeFilter passFilter,
      ICellSpatialFilter cellFilter,
      IDesign cellPassFilter_ElevationRangeDesign,
      ICellLiftBuilder cellLiftBuilder);
  }
}
