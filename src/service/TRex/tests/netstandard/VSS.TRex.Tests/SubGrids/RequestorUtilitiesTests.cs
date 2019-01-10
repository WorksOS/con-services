using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using Xunit;
using Moq;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.SubGrids
{
  public class RequestorUtilitiesTestsLoggingFixture : IDisposable
  {
    public static ISurfaceElevationPatchRequest SurfaceElevationPatchRequest = null;

    public RequestorUtilitiesTestsLoggingFixture()
    {
      // Provide the surveyed surface request mock
      Mock<ISurfaceElevationPatchRequest> surfaceElevationPatchRequest = new Mock<ISurfaceElevationPatchRequest>();
      surfaceElevationPatchRequest.Setup(x => x.Execute(It.IsAny<ISurfaceElevationPatchArgument>())).Returns(new ClientHeightAndTimeLeafSubGrid());
      SurfaceElevationPatchRequest = surfaceElevationPatchRequest.Object;

      // Provide the mocks for spatial caching
      Mock<ITRexSpatialMemoryCacheContext> tRexSpatialMemoryCacheContext = new Mock<ITRexSpatialMemoryCacheContext>();
      Mock<ITRexSpatialMemoryCache> tRexSpatialMemoryCache = new Mock<ITRexSpatialMemoryCache>();
      tRexSpatialMemoryCache.Setup(x => x.LocateOrCreateContext(It.IsAny<Guid>(), It.IsAny<string>())).Returns(tRexSpatialMemoryCacheContext.Object);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))

        // Register the mock factory for surface elevation requests
        .Add(x => x.AddSingleton<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>((cache, context) => surfaceElevationPatchRequest.Object))

        .Add(x => x.AddSingleton<ITRexSpatialMemoryCache>(tRexSpatialMemoryCache.Object))

        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))

        .Complete();
    }

    public void Dispose()
    {
      DIBuilder.Continue().Eject();
    }
  }

  public class RequestorUtilitiesTests : IClassFixture<RequestorUtilitiesTestsLoggingFixture>
  {
    [Fact]
    public void Test_RequestorUtilities_Creation()
    {
      var ru = new RequestorUtilities();

      ru.Should().NotBe(null);
    }

    [Fact]
    public void Test_RequestorUtilities_CreateIntermediaries_SingleDefaultFilter_NoSurveyedSurfaces()
    {
      var ru = new RequestorUtilities();

      Mock<ISiteModel> MockSiteModel = new Mock<ISiteModel>();

      ICombinedFilter filter = new CombinedFilter();
      IFilterSet filters = new FilterSet(filter);

      var intermediaries = ru.ConstructRequestorIntermediaries(MockSiteModel.Object, filters, false, GridDataType.Height);

      intermediaries.Length.Should().Be(1);
      intermediaries[0].Filter.Should().Be(filter);
      intermediaries[0].FilteredSurveyedSurfaces.Should().BeNull();
      intermediaries[0].FilteredSurveyedSurfacesAsArray.Should().BeEmpty();
      intermediaries[0].CacheContext.Should().NotBeNull();
      intermediaries[0].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
    }

    [Fact]
    public void Test_RequestorUtilities_CreateIntermediaries_SingleDefaultFilter_WithSurveyedSurfaces()
    {
      var ru = new RequestorUtilities();

      Guid ssGuid = Guid.NewGuid();
      ISurveyedSurfaces surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();
      surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid, DesignDescriptor.Null(), DateTime.MinValue, BoundingWorldExtent3D.Null());

      Mock<ISiteModel> MockSiteModel = new Mock<ISiteModel>();
      MockSiteModel.Setup(x => x.SurveyedSurfacesLoaded).Returns(true);
      MockSiteModel.Setup(x => x.SurveyedSurfaces).Returns(surveyedSurfaces);

      ICombinedFilter filter = new CombinedFilter();
      IFilterSet filters = new FilterSet(filter);

      var intermediaries = ru.ConstructRequestorIntermediaries(MockSiteModel.Object, filters, true, GridDataType.Height);

      intermediaries.Length.Should().Be(1);
      intermediaries[0].Filter.Should().Be(filter);
      intermediaries[0].FilteredSurveyedSurfaces.Should().Equal(surveyedSurfaces);
      intermediaries[0].FilteredSurveyedSurfacesAsArray.Should().Equal(new [] { ssGuid });
      intermediaries[0].CacheContext.Should().NotBeNull();
      intermediaries[0].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
    }
  }
}
