using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.SubGrids
{
  public class RequestorUtilitiesTestsLoggingFixture : DILoggingFixture, IDisposable
  {
    public static ISurfaceElevationPatchRequest SurfaceElevationPatchRequest;
    public static ITRexSpatialMemoryCacheContext TRexSpatialMemoryCacheContext;

    public RequestorUtilitiesTestsLoggingFixture()
    {
      // Provide the surveyed surface request mock
      var surfaceElevationPatchRequest = new Mock<ISurfaceElevationPatchRequest>();
      surfaceElevationPatchRequest.Setup(x => x.ExecuteAsync(It.IsAny<ISurfaceElevationPatchArgument>())).Returns(Task.FromResult(new ClientHeightAndTimeLeafSubGrid() as IClientLeafSubGrid));
      SurfaceElevationPatchRequest = surfaceElevationPatchRequest.Object;

      // Provide the mocks for spatial caching
      var tRexSpatialMemoryCacheContext = new Mock<ITRexSpatialMemoryCacheContext>();
      TRexSpatialMemoryCacheContext = tRexSpatialMemoryCacheContext.Object;

      var tRexSpatialMemoryCache = new Mock<ITRexSpatialMemoryCache>();
      tRexSpatialMemoryCache.Setup(x => x.LocateOrCreateContext(It.IsAny<Guid>(), It.IsAny<string>())).Returns(TRexSpatialMemoryCacheContext);
      tRexSpatialMemoryCache.Setup(x => x.LocateOrCreateContext(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).Returns(TRexSpatialMemoryCacheContext);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))

        // Register the mock factory for surface elevation requests
        .Add(x => x.AddSingleton<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>((cache, context) => SurfaceElevationPatchRequest))

        .Add(x => x.AddSingleton<ITRexSpatialMemoryCache>(tRexSpatialMemoryCache.Object))

        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new TRex.SurveyedSurfaces.SurveyedSurfaces()))

        .Add(x => x.AddSingleton<ISiteModels>(new TRex.SiteModels.SiteModels(TRex.Storage.Models.StorageMutability.Immutable)))
        .Add(x => x.AddSingleton<Func<ISubGridRequestor>>(factory => () => new SubGridRequestor()))

        .Add(x => x.AddSingleton<ISubGridRetrieverFactory>(new SubGridRetrieverFactory()))

        .Complete();
    }

    public new void Dispose()
    {
      base.Dispose();

      SurfaceElevationPatchRequest = null;
      TRexSpatialMemoryCacheContext = null;
    }
  }
}
