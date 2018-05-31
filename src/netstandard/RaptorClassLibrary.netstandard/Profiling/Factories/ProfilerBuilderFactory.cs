using VSS.TRex.Designs.Storage;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Factories
{
  /// <summary>
  /// Factory responsible for creating builder elements use in construction of profile information based on cell production data
  /// </summary>
    public class ProfilerBuilderFactory : IProfilerBuilderFactory
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
      public ICellLiftBuilder NewCellLiftBuilder(ISiteModel siteModel, 
        GridDataType gridDataType, 
        FilteredValuePopulationControl populationControl, 
        CellPassAttributeFilter passFilter, 
        CellPassFastEventLookerUpper cellPassFastEventLookerUpper)
      {
         return new CellLiftBuilder(siteModel, gridDataType, populationControl, passFilter, cellPassFastEventLookerUpper);
      }

    /// <summary>
    /// Creates a new bulder responsible for determining a vector of cells that are cross by a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="cellFilter"></param>
    /// <param name="cutFillDesign"></param>
    /// <returns></returns>
    public ICellProfileBuilder NewCellProfileBuilder(ISiteModel siteModel, 
        CellSpatialFilter cellFilter, 
        Design cutFillDesign)
      {
        return new CellProfileBuilder(siteModel, cellFilter, cutFillDesign);
      }

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
      public IProfileLiftBuilder NewProfileLiftBuilder(ISiteModel siteModel,
        SubGridTreeBitMask pDExistenceMap,
        CellPassAttributeFilter passFilter,
        CellSpatialFilter cellFilter,
        Design cellPassFilter_ElevationRangeDesign,
        ICellLiftBuilder cellLiftBuilder)
      {
        return new ProfileLiftBuilder(siteModel, pDExistenceMap, passFilter, cellFilter, cellPassFilter_ElevationRangeDesign, cellLiftBuilder);
      }
  }
}
