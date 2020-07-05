using FluentAssertions;
using Xunit;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurface.GridFabric.Responses;
using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Responses;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.GridFabric.Affinity;

namespace VSS.TRex.Tests.SurveyedSurfaces.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(RemoveSurveyedSurfaceRequest))]
  public class RemoveSurveyedSurfaceRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationRouting()
    {
      IgniteMock.Mutable.AddApplicationGridRouting<AddSurveyedSurfaceComputeFunc, AddSurveyedSurfaceArgument, AddSurveyedSurfaceResponse>();
      IgniteMock.Mutable.AddApplicationGridRouting<RemoveSurveyedSurfaceComputeFunc, RemoveSurveyedSurfaceArgument, RemoveSurveyedSurfaceResponse>();
    }

    [Fact]
    public void Creation()
    {
      var req = new RemoveSurveyedSurfaceRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public async void Remove_FailWithNoProject()
    {
      AddApplicationRouting();

      var request = new RemoveSurveyedSurfaceRequest();
      var response = await request.ExecuteAsync(new RemoveSurveyedSurfaceArgument
      {
        ProjectID = Guid.NewGuid(),
        DesignID = Guid.NewGuid()
      });

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.NoSelectedSiteModel);
    }

    [Fact]
    public async void Remove_FailWithNoDesign()
    {
      AddApplicationRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new RemoveSurveyedSurfaceRequest();
      var response = await request.ExecuteAsync(new RemoveSurveyedSurfaceArgument
      {
        ProjectID = siteModel.ID,
        DesignID = Guid.NewGuid()
      });

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.DesignDoesNotExist);
    }

    [Fact]
    public async void Remove()
    {
      AddApplicationRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Add te design to be removed
      var designID = Guid.NewGuid();
      var existenceMap = new TRex.SubGridTrees.SubGridTreeSubGridExistenceBitMask();
      existenceMap[0, 0] = true;

      var request = new AddSurveyedSurfaceRequest();
      var addResponse = await request.ExecuteAsync(new AddSurveyedSurfaceArgument
      {
        ProjectID = siteModel.ID,
        DesignDescriptor = new DesignDescriptor(designID, "folder", "filename"),
        AsAtDate = DateTime.UtcNow,
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1),
        ExistenceMap = existenceMap
      });

      addResponse.Should().NotBeNull();
      addResponse.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);
      siteModel.SurveyedSurfaces.Count.Should().Be(1);

      var removeRequest = new RemoveSurveyedSurfaceRequest();
      var removeResponse = await removeRequest.ExecuteAsync(new RemoveSurveyedSurfaceArgument
      {
        ProjectID = siteModel.ID,
        DesignID = designID
      });

      removeResponse.Should().NotBeNull();
      removeResponse.DesignUid.Should().Be(designID);
      removeResponse.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);
      siteModel.SurveyedSurfaces.Count.Should().Be(0);

      var readExistenceMap = DIContext.Obtain<IExistenceMapServer>().GetExistenceMap(new NonSpatialAffinityKey(siteModel.ID,
  BaseExistenceMapRequest.CacheKeyString(TRex.ExistenceMaps.Interfaces.Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, designID)));
      readExistenceMap.Should().BeNull();
    }
  }
}

