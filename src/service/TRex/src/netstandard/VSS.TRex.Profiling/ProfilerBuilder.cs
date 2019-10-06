using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
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
    private readonly IProfilerBuilderFactory<T> factory = DI.DIContext.Obtain<IProfilerBuilderFactory<T>>();

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
    public void Configure(ProfileStyle profileStyle,
      ISiteModel siteModel,
      ISubGridTreeBitMask productionDataExistenceMap,
      GridDataType gridDataType,
      IFilterSet filterSet,
      IDesignWrapper referenceDesignWrapper,
      IFilteredValuePopulationControl PopulationControl,
      ICellPassFastEventLookerUpper CellPassFastEventLookerUpper,
      VolumeComputationType volumeType, 
      IOverrideParameters overrides,
      ILiftParameters liftParams,
      bool slicerToolUsed = true)    
    {
        CellLiftBuilder = factory.NewCellLiftBuilder(siteModel, gridDataType, PopulationControl, filterSet, CellPassFastEventLookerUpper);

        CellProfileBuilder = factory.NewCellProfileBuilder(siteModel, filterSet, referenceDesignWrapper, slicerToolUsed);

        CellProfileAnalyzer = factory.NewCellProfileAnalyzer(
          profileStyle, siteModel, productionDataExistenceMap, filterSet, 
          referenceDesignWrapper, CellLiftBuilder, volumeType, overrides, liftParams);
    }
  }
}
