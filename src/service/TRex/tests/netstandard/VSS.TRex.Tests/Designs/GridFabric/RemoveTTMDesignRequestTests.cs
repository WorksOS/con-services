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
    public async void RemoveDesign_FailWithNoProject()
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
    public async void RemoveDesign()
    {
      AddApplicationRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Add te design to be removed
      var designID = System.Guid.NewGuid();

      var addRequest = new AddTTMDesignRequest();
      var addResponse = await addRequest.ExecuteAsync(new AddTTMDesignArgument
      {
        ProjectID = siteModel.ID,
        DesignDescriptor = new DesignDescriptor(designID, "folder", "filename"),
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1),
        ExistenceMap = new TRex.SubGridTrees.SubGridTreeSubGridExistenceBitMask()
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
    }
  }
}
