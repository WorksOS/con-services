using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DIProfilingFixture : DITAGFileAndSubGridRequestsFixture
  {
    public DIProfilingFixture()
    {
      DIBuilder
        .Continue()

        // Register the factory for the CellProfileAnalyzer for detailed call pass/lift cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesign, double, ICellLiftBuilder, ICellProfileAnalyzer<ProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellPassFilter_ElevationRangeDesignOffset, cellLiftBuilder) 
            => new CellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellPassFilter_ElevationRangeDesignOffset, cellLiftBuilder)))

        // Register the factory for the CellProfileAnalyzer for summary volume cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesign, double, IDesign, double, ICellLiftBuilder, ICellProfileAnalyzer<SummaryVolumeProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellPassFilter_ElevationRangeDesignOffset, referenceDesign, referenceDesignOffset, cellLiftBuilder) 
            => new SummaryVolumesCellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellPassFilter_ElevationRangeDesignOffset, referenceDesign, referenceDesignOffset, cellLiftBuilder)))

        .Complete();
    }
  }
}
