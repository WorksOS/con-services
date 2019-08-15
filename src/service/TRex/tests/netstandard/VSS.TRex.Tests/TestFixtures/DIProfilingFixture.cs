using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
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
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, ICellLiftBuilder, IOverrideParameters, ILiftParameters, ICellProfileAnalyzer<ProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellLiftBuilder, overrides, liftParams) 
            => new CellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellLiftBuilder, overrides, liftParams)))

        // Register the factory for the CellProfileAnalyzer for summary volume cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesignWrapper, ICellLiftBuilder, VolumeComputationType, IOverrideParameters, ILiftParameters, ICellProfileAnalyzer<SummaryVolumeProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, referenceDesignWrapper, cellLiftBuilder, volumeComputationType, overrides, liftParams) 
            => new SummaryVolumesCellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, referenceDesignWrapper, cellLiftBuilder, volumeComputationType, overrides, liftParams)))

        .Complete();
    }
  }
}
