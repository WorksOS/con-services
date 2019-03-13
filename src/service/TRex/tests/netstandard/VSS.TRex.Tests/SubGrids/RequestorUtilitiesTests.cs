using System;
using System.Collections.Generic;
using System.Linq;
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
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.SubGrids
{
  public class RequestorUtilitiesTestsLoggingFixture : IDisposable
  {
    public static ISurfaceElevationPatchRequest SurfaceElevationPatchRequest;
    public static ITRexSpatialMemoryCacheContext TRexSpatialMemoryCacheContext;

    public RequestorUtilitiesTestsLoggingFixture()
    {
      // Provide the surveyed surface request mock
      Mock<ISurfaceElevationPatchRequest> surfaceElevationPatchRequest = new Mock<ISurfaceElevationPatchRequest>();
      surfaceElevationPatchRequest.Setup(x => x.Execute(It.IsAny<ISurfaceElevationPatchArgument>())).Returns(new ClientHeightAndTimeLeafSubGrid());
      SurfaceElevationPatchRequest = surfaceElevationPatchRequest.Object;

      // Provide the mocks for spatial caching
      Mock<ITRexSpatialMemoryCacheContext> tRexSpatialMemoryCacheContext = new Mock<ITRexSpatialMemoryCacheContext>();
      TRexSpatialMemoryCacheContext = tRexSpatialMemoryCacheContext.Object;

      Mock<ITRexSpatialMemoryCache> tRexSpatialMemoryCache = new Mock<ITRexSpatialMemoryCache>();
      tRexSpatialMemoryCache.Setup(x => x.LocateOrCreateContext(It.IsAny<Guid>(), It.IsAny<string>())).Returns(TRexSpatialMemoryCacheContext);
      tRexSpatialMemoryCache.Setup(x => x.LocateOrCreateContext(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).Returns(TRexSpatialMemoryCacheContext);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))

        // Register the mock factory for surface elevation requests
        .Add(x => x.AddSingleton<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>((cache, context) => SurfaceElevationPatchRequest))

        .Add(x => x.AddSingleton<ITRexSpatialMemoryCache>(tRexSpatialMemoryCache.Object))

        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))

        .Add(x => x.AddSingleton<ISiteModels>(new TRex.SiteModels.SiteModels(() => null)))
        .Add(x => x.AddSingleton<Func<ISubGridRequestor>>(factory => () => new SubGridRequestor()))

        .Complete();
    }

    public void Dispose()
    {
      DIBuilder.Eject();
      SurfaceElevationPatchRequest = null;
      TRexSpatialMemoryCacheContext = null;
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

      var mockGrid = new Mock<IServerSubGridTree>();
      mockGrid.Setup(x => x.CellSize).Returns(SubGridTreeConsts.DefaultCellSize);
      var mockSiteModel = new Mock<ISiteModel>();
      mockSiteModel.Setup(x => x.Grid).Returns(mockGrid.Object);

      ICombinedFilter filter = new CombinedFilter();
      IFilterSet filters = new FilterSet(filter);

      var intermediaries = ru.ConstructRequestorIntermediaries(mockSiteModel.Object, filters, false, GridDataType.Height);

      intermediaries.Length.Should().Be(1);
      intermediaries[0].Filter.Should().Be(filter);
      intermediaries[0].FilteredSurveyedSurfaces.Should().BeNull();
      intermediaries[0].CacheContext.Should().Be(RequestorUtilitiesTestsLoggingFixture.TRexSpatialMemoryCacheContext);
      intermediaries[0].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
    }

    [Fact]
    public void Test_RequestorUtilities_CreateIntermediaries_SingleDefaultFilter_WithSurveyedSurfaces()
    {
      var ru = new RequestorUtilities();

      Guid ssGuid = Guid.NewGuid();
      ISurveyedSurfaces surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();
      surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid, DesignDescriptor.Null(), DateTime.MinValue, BoundingWorldExtent3D.Null());

      var mockGrid = new Mock<IServerSubGridTree>();
      mockGrid.Setup(x => x.CellSize).Returns(SubGridTreeConsts.DefaultCellSize);
      var mockSiteModel = new Mock<ISiteModel>();
      mockSiteModel.Setup(x => x.SurveyedSurfacesLoaded).Returns(true);
      mockSiteModel.Setup(x => x.SurveyedSurfaces).Returns(surveyedSurfaces);
      mockSiteModel.Setup(x => x.Grid).Returns(mockGrid.Object);

      ICombinedFilter filter = new CombinedFilter();
      IFilterSet filters = new FilterSet(filter);

      var intermediaries = ru.ConstructRequestorIntermediaries(mockSiteModel.Object, filters, true, GridDataType.Height);

      intermediaries.Length.Should().Be(1);
      intermediaries[0].Filter.Should().Be(filter);
      intermediaries[0].FilteredSurveyedSurfaces.Should().Equal(surveyedSurfaces);
      intermediaries[0].CacheContext.Should().Be(RequestorUtilitiesTestsLoggingFixture.TRexSpatialMemoryCacheContext);
      intermediaries[0].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public void Test_RequestorUtilities_CreateIntermediaries_MultipleDefaultFilters_NoSurveyedSurfaces(int filterCount)
    {
      var ru = new RequestorUtilities();

      var mockGrid = new Mock<IServerSubGridTree>();
      mockGrid.Setup(x => x.CellSize).Returns(SubGridTreeConsts.DefaultCellSize);
      var mockSiteModel = new Mock<ISiteModel>();
      mockSiteModel.Setup(x => x.Grid).Returns(mockGrid.Object);

      ICombinedFilter[] filters = Enumerable.Range(1, filterCount).Select(x => new CombinedFilter()).ToArray();
      IFilterSet filterSet = new FilterSet(filters);

      var intermediaries = ru.ConstructRequestorIntermediaries(mockSiteModel.Object, filterSet, false, GridDataType.Height);

      intermediaries.Length.Should().Be(filters.Length);

      for (int i = 0; i < intermediaries.Length; i++)
      {
        intermediaries[i].Filter.Should().Be(filters[i]);
        intermediaries[i].FilteredSurveyedSurfaces.Should().BeNull();
        intermediaries[i].CacheContext.Should().NotBeNull();
        intermediaries[i].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public void Test_RequestorUtilities_CreateIntermediaries_MultipleDefaultFilters_WithSurveyedSurfaces(int filterCount)
    {
      var ru = new RequestorUtilities();

      Guid ssGuid = Guid.NewGuid();
      ISurveyedSurfaces surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();
      surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid, DesignDescriptor.Null(), DateTime.MinValue, BoundingWorldExtent3D.Null());

      var mockGrid = new Mock<IServerSubGridTree>();
      mockGrid.Setup(x => x.CellSize).Returns(SubGridTreeConsts.DefaultCellSize);

      var mockSiteModel = new Mock<ISiteModel>();
      mockSiteModel.Setup(x => x.SurveyedSurfacesLoaded).Returns(true);
      mockSiteModel.Setup(x => x.SurveyedSurfaces).Returns(surveyedSurfaces);
      mockSiteModel.Setup(x => x.Grid).Returns(mockGrid.Object);

      ICombinedFilter[] filters = Enumerable.Range(1, filterCount).Select(x => new CombinedFilter()).ToArray();
      IFilterSet filterSet = new FilterSet(filters);

      var intermediaries = ru.ConstructRequestorIntermediaries(mockSiteModel.Object, filterSet, true, GridDataType.Height);

      intermediaries.Length.Should().Be(filters.Length);

      for (int i = 0; i < intermediaries.Length; i++)
      {
        intermediaries[i].Filter.Should().Be(filters[i]);
        intermediaries[i].FilteredSurveyedSurfaces.Should().Equal(surveyedSurfaces);
        intermediaries[i].CacheContext.Should().NotBeNull();
        intermediaries[i].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public void Test_RequestorUtilities_CreateIntermediaries_MultipleFilters_WithSingleExcludedSurveyedSurface(int filterCount)
    {
      var ru = new RequestorUtilities();

      Guid ssGuid = Guid.NewGuid();
      ISurveyedSurfaces surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();
      surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid, DesignDescriptor.Null(), DateTime.MinValue, BoundingWorldExtent3D.Null());

      var mockGrid = new Mock<IServerSubGridTree>();
      mockGrid.Setup(x => x.CellSize).Returns(SubGridTreeConsts.DefaultCellSize);

      var mockSiteModel = new Mock<ISiteModel>();
      mockSiteModel.Setup(x => x.SurveyedSurfacesLoaded).Returns(true);
      mockSiteModel.Setup(x => x.SurveyedSurfaces).Returns(surveyedSurfaces);
      mockSiteModel.Setup(x => x.Grid).Returns(mockGrid.Object);

      ICombinedFilter[] filters = Enumerable.Range(1, filterCount).Select(x =>
      {
        var filter = new CombinedFilter();
        filter.AttributeFilter.SurveyedSurfaceExclusionList = new[] {ssGuid};
        return filter;
      }).ToArray();
      IFilterSet filterSet = new FilterSet(filters);

      var intermediaries = ru.ConstructRequestorIntermediaries(mockSiteModel.Object, filterSet, true, GridDataType.Height);

      intermediaries.Length.Should().Be(filters.Length);

      for (int i = 0; i < intermediaries.Length; i++)
      {
        intermediaries[i].Filter.Should().Be(filters[i]);
        intermediaries[i].FilteredSurveyedSurfaces.Should().BeEmpty();
        intermediaries[i].CacheContext.Should().NotBeNull();
        intermediaries[i].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public void Test_RequestorUtilities_CreateIntermediaries_MultipleFilters_WithOneOfTwoSurveyedSurfacesExcluded(int filterCount)
    {
      var ru = new RequestorUtilities();

      ISurveyedSurfaces surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

      // Create two surveyed surfaces that bracket current time by one day either side and set the filter end time to be current time
      // which will cause only one surveyed surface to be filtered
      Guid ssGuid1 = Guid.NewGuid();
      var ss1 = surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid1, DesignDescriptor.Null(), DateTime.Now.AddDays(-1), BoundingWorldExtent3D.Null());

      Guid ssGuid2 = Guid.NewGuid();
      var ss2 = surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid2, DesignDescriptor.Null(), DateTime.Now.AddDays(+1), BoundingWorldExtent3D.Null());

      var mockGrid = new Mock<IServerSubGridTree>();
      mockGrid.Setup(x => x.CellSize).Returns(SubGridTreeConsts.DefaultCellSize);

      var mockSiteModel = new Mock<ISiteModel>();
      mockSiteModel.Setup(x => x.SurveyedSurfacesLoaded).Returns(true);
      mockSiteModel.Setup(x => x.SurveyedSurfaces).Returns(surveyedSurfaces);
      mockSiteModel.Setup(x => x.Grid).Returns(mockGrid.Object);

      ICombinedFilter[] filters = Enumerable.Range(1, filterCount).Select(x =>
      {
        var filter = new CombinedFilter();
        filter.AttributeFilter.SurveyedSurfaceExclusionList = new[] { ssGuid1 };
        return filter;
      }).ToArray();
      IFilterSet filterSet = new FilterSet(filters);

      var intermediaries = ru.ConstructRequestorIntermediaries(mockSiteModel.Object, filterSet, true, GridDataType.Height);

      intermediaries.Length.Should().Be(filters.Length);

      for (int i = 0; i < intermediaries.Length; i++)
      {
        intermediaries[i].Filter.Should().Be(filters[i]);
        intermediaries[i].FilteredSurveyedSurfaces.Should().Equal(new List<ISurveyedSurface>{ss2});
        intermediaries[i].CacheContext.Should().NotBeNull();
        intermediaries[i].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public void Test_RequestorUtilities_CreateIntermediaries_MultipleFilters_WithOneOfTwoSurveyedSurfacesFilteredByTime_NoSurveyedSurfaceExclusions(int filterCount)
    {
      var ru = new RequestorUtilities();

      ISurveyedSurfaces surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

      // Create two surveyed surfaces that bracket current time by one day either side and set the filter end time to be current time
      // which will cause only one surveyed surface to be filtered
      Guid ssGuid1 = Guid.NewGuid();
      var ss1 = surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid1, DesignDescriptor.Null(), DateTime.Now.AddDays(-1), BoundingWorldExtent3D.Null());

      Guid ssGuid2 = Guid.NewGuid();
      var ss2 = surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid2, DesignDescriptor.Null(), DateTime.Now.AddDays(+1), BoundingWorldExtent3D.Null());

      var mockGrid = new Mock<IServerSubGridTree>();
      mockGrid.Setup(x => x.CellSize).Returns(SubGridTreeConsts.DefaultCellSize);

      var mockSiteModel = new Mock<ISiteModel>();
      mockSiteModel.Setup(x => x.SurveyedSurfacesLoaded).Returns(true);
      mockSiteModel.Setup(x => x.SurveyedSurfaces).Returns(surveyedSurfaces);
      mockSiteModel.Setup(x => x.Grid).Returns(mockGrid.Object);

      ICombinedFilter[] filters = Enumerable.Range(1, filterCount).Select(x =>
      {
        var filter = new CombinedFilter
        {
          AttributeFilter =
          {
            HasTimeFilter = true,
            StartTime = DateTime.MinValue,
            EndTime = DateTime.Now
          }
        };
        return filter;
      }).ToArray();
      IFilterSet filterSet = new FilterSet(filters);

      var intermediaries = ru.ConstructRequestorIntermediaries(mockSiteModel.Object, filterSet, true, GridDataType.Height);

      intermediaries.Length.Should().Be(filters.Length);

      for (int i = 0; i < intermediaries.Length; i++)
      {
        intermediaries[i].Filter.Should().Be(filters[i]);
        intermediaries[i].FilteredSurveyedSurfaces.Should().Equal(new List<ISurveyedSurface> { ss1 });
        intermediaries[i].CacheContext.Should().NotBeNull();
        intermediaries[i].surfaceElevationPatchRequest.Should().Be(RequestorUtilitiesTestsLoggingFixture.SurfaceElevationPatchRequest);
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public void Test_RequestorUtilities_CreateRequestors_DefaultFilters(int filterCount)
    {
      var ru = new RequestorUtilities();

      ISurveyedSurfaces surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

      // Create two surveyed surfaces that bracket current time by one day either side and set the filter end time to be current time
      // which will cause only one surveyed surface to be filtered
      Guid ssGuid1 = Guid.NewGuid();
      var ss1 = surveyedSurfaces.AddSurveyedSurfaceDetails(ssGuid1, DesignDescriptor.Null(), DateTime.MinValue, BoundingWorldExtent3D.Null());

      var mockGrid = new Mock<IServerSubGridTree>();
      mockGrid.Setup(x => x.CellSize).Returns(SubGridTreeConsts.DefaultCellSize);

      var mockSiteModel = new Mock<ISiteModel>();
      mockSiteModel.Setup(x => x.SurveyedSurfacesLoaded).Returns(true);
      mockSiteModel.Setup(x => x.SurveyedSurfaces).Returns(surveyedSurfaces);
      mockSiteModel.Setup(x => x.Grid).Returns(mockGrid.Object);

      ICombinedFilter[] filters = Enumerable.Range(1, filterCount).Select(x => new CombinedFilter()).ToArray();
      IFilterSet filterSet = new FilterSet(filters);
      
      var intermediaries = ru.ConstructRequestorIntermediaries(mockSiteModel.Object, filterSet, true, GridDataType.Height);
      var requestors = ru.ConstructRequestors(mockSiteModel.Object, intermediaries, AreaControlSet.CreateAreaControlSet(), null);

      requestors.Length.Should().Be(filters.Length);

      for (int i = 0; i < requestors.Length; i++)
      {
        requestors[i].CellOverrideMask.Should().NotBe(null);
      }
    }
  }
}
