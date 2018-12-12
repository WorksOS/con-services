using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DIProfilingFixture : IDisposable
  {
    private static object Lock = new object();

    public DIProfilingFixture()
    {
      lock (Lock)
      {
        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())

          // Register the factory for the CellProfileAnalyzer for detailed call pass/lift cell profiles
          .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, ICellPassAttributeFilter, ICellSpatialFilter, IDesign, ICellLiftBuilder, ICellProfileAnalyzer<ProfileCell>>>(
            factory => (siteModel, pDExistenceMap, passFilter, cellFilter, cellPassFilter_ElevationRangeDesign, cellLiftBuilder) => new CellProfileAnalyzer(siteModel, pDExistenceMap, passFilter, cellFilter, cellPassFilter_ElevationRangeDesign, cellLiftBuilder)))

          // Register the factory for the CellProfileAnalyzer for summary volume cell profiles
          .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, ICellPassAttributeFilter, ICellSpatialFilter, IDesign, ICellLiftBuilder, ICellProfileAnalyzer<SummaryVolumeProfileCell>>>(
            factory => (siteModel, pDExistenceMap, passFilter, cellFilter, cellPassFilter_ElevationRangeDesign, cellLiftBuilder) => null))
           
          .Complete();
      }
    }

    public void Dispose()
    {
      DIBuilder.Continue().Eject();
    }
  }
}
