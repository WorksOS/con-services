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
    ICellProfileAnalyzer<T> CellProfileAnalyzer { get; set; }

    /// <summary>
    /// Configures a new profile builder that provides the three core builders used in profiling: construction of cell vector from profile line,
    /// profile analysis orchestration and per cell layer/statistics calculation
    /// </summary>
    void Configure(ProfileStyle profileStyle,
      ISiteModel siteModel,
      ISubGridTreeBitMask productionDataExistenceMap,
      GridDataType gridDataType,
      IFilterSet filterSet,
      IDesignWrapper referenceDesignWrapper,
      IDesignWrapper cellPassFilter_ElevationRangeDesignWrapper,
      IFilteredValuePopulationControl PopulationControl,
      ICellPassFastEventLookerUpper CellPassFastEventLookerUpper,
      VolumeComputationType VolumeType,
      IOverrideParameters overrides,
      ILiftParameters liftParams,
      bool slicerToolUsed = true);
  }
}
