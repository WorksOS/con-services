using System;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Factories
{
  /// <summary>
  /// Factory responsible for creating builder elements used in construction of profile information based on cell production data
  /// </summary>
  public class ProfilerBuilderFactory<T> : IProfilerBuilderFactory<T> where T : class, IProfileCellBase, new()
  {
    /// <summary>
    /// Creates a new builder responsible for determining a vector of cells that are crossed by a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="filterSet"></param>
    /// <param name="cutFillDesignWrapper"></param>
    /// <param name="slicerToolUsed"></param>
    /// <returns></returns>
    public ICellProfileBuilder<T> NewCellProfileBuilder(ISiteModel siteModel,
      IFilterSet filterSet,
      IDesignWrapper cutFillDesignWrapper,
      bool slicerToolUsed)
    {
      return new CellProfileBuilder<T>(siteModel, filterSet, cutFillDesignWrapper, slicerToolUsed);
    }

    /// <summary>
    /// Creates a new builder responsible for analyzing profile information for a cell or cells identified along a profile line
    /// </summary>
    public ICellProfileAnalyzer<T> NewCellProfileAnalyzer(ProfileStyle profileStyle,
      ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesignWrapper cellPassFilter_ElevationRangeDesignWrapper,
      IDesignWrapper referenceDesignWrapper,
      ICellLiftBuilder cellLiftBuilder,
      VolumeComputationType volumeComputationType,
      IOverrideParameters overrides,
      ILiftParameters liftParams)
    {
      switch (profileStyle)
      {
        case ProfileStyle.CellPasses:
          return DIContext.Obtain<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesignWrapper, ICellLiftBuilder, IOverrideParameters, ILiftParameters, ICellProfileAnalyzer<T>>>()
            (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesignWrapper, cellLiftBuilder, overrides, liftParams);

        case ProfileStyle.SummaryVolume:
          return DIContext.Obtain<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesignWrapper, IDesignWrapper, ICellLiftBuilder, VolumeComputationType, ICellProfileAnalyzer<T>>>()
            (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesignWrapper, referenceDesignWrapper, cellLiftBuilder, volumeComputationType);

        default:
          throw new ArgumentOutOfRangeException(nameof(profileStyle), profileStyle, null);
      }
    }

    /// <summary>
    /// Creates a new builder responsible for processing cell pass, layer and other information for single cells in a profile
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="gridDataType"></param>
    /// <param name="populationControl"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFastEventLookerUpper"></param>
    /// <returns></returns>
    public ICellLiftBuilder NewCellLiftBuilder(ISiteModel siteModel,
      GridDataType gridDataType,
      IFilteredValuePopulationControl populationControl,
      IFilterSet filterSet,
      ICellPassFastEventLookerUpper cellPassFastEventLookerUpper)
    {
      return new CellLiftBuilder(siteModel, gridDataType, populationControl, filterSet, cellPassFastEventLookerUpper);
    }
  }
}
