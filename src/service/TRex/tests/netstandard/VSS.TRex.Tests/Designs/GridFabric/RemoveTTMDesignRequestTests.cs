using FluentAssertions;
using Xunit;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using System;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(RemoveTTMDesignRequest))]
  public class RemoveTTMDesignRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationRouting()
    {
      IgniteMock.Mutable.AddApplicationGridRouting<RemoveTTMDesignComputeFunc, RemoveTTMDesignArgument, RemoveTTMDesignResponse>();
      IgniteMock.Mutable.AddApplicationGridRouting<AddTTMDesignComputeFunc, AddTTMDesignArgument, AddTTMDesignResponse>();
    }

    [Fact]
    public void Creation()
    {
      var req = new RemoveTTMDesignRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public async void Remove_FailWithNoProject()
    {
      AddApplicationRouting();

      var request = new RemoveTTMDesignRequest();
      var response = await request.ExecuteAsync(new RemoveTTMDesignArgument
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

      var request = new RemoveTTMDesignRequest();
      var response = await request.ExecuteAsync(new RemoveTTMDesignArgument
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

      var addRequest = new AddTTMDesignRequest();
      var addResponse = await addRequest.ExecuteAsync(new AddTTMDesignArgument
      {
        ProjectID = siteModel.ID,
        DesignDescriptor = new DesignDescriptor(designID, "folder", "filename"),
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1),
        ExistenceMap = existenceMap
      });

      addResponse.Should().NotBeNull();
      addResponse.DesignUid.Should().Be(designID);
      addResponse.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);
      siteModel.Designs.Count.Should().Be(1);

      var removeRequest = new RemoveTTMDesignRequest();
      var removeResponse = await removeRequest.ExecuteAsync(new RemoveTTMDesignArgument
      {
        ProjectID = siteModel.ID,
        DesignID = designID
      });

      removeResponse.Should().NotBeNull();
      removeResponse.DesignUid.Should().Be(designID);
      removeResponse.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);
      siteModel.Designs.Count.Should().Be(0);

      var readExistenceMap = DIContext.Obtain<IExistenceMapServer>().GetExistenceMap(new NonSpatialAffinityKey(siteModel.ID,
  BaseExistenceMapRequest.CacheKeyString(TRex.ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designID)));
      readExistenceMap.Should().BeNull();
    }
  }
}
