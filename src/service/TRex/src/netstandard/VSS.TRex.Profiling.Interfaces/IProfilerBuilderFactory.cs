using VSS.TRex.Common;
using VSS.TRex.Common.Models;
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
    ICellLiftBuilder NewCellLiftBuilder(ISiteModel siteModel,
      GridDataType gridDataType,
      IFilteredValuePopulationControl populationControl,
      IFilterSet filterSet,
      ICellPassFastEventLookerUpper cellPassFastEventLookerUpper);

    /// <summary>
    /// Creates a new builder responsible for determining a vector of cells that are cross by a profile line
    /// </summary>
    ICellProfileBuilder<T> NewCellProfileBuilder(ISiteModel siteModel,
      IFilterSet filterSet,
      IDesignWrapper cutFillDesignWrapper,
      bool slicerToolUsed);

    /// <summary>
    /// Creates a new builder responsible for analyzing profile information for a cell of cells identified along a profile line
    /// </summary>
    ICellProfileAnalyzer<T> NewCellProfileAnalyzer(
      ProfileStyle profileStyle,
      ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesignWrapper referenceDesignWrapper,
      ICellLiftBuilder cellLiftBuilder,
      VolumeComputationType volumeComputationType,
      IOverrideParameters overrides,
      ILiftParameters liftParams);
  }
}
