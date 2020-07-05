using System;
using FluentAssertions;
using VSS.TRex.Alignments.GridFabric.Arguments;
using VSS.TRex.Alignments.GridFabric.ComputeFuncs;
using VSS.TRex.Alignments.GridFabric.Requests;
using VSS.TRex.Alignments.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Alignments.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(RemoveAlignmentRequest))]
  public class RemoveAlignmentRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationRouting()
    {
      IgniteMock.Mutable.AddApplicationGridRouting<AddAlignmentComputeFunc, AddAlignmentArgument, AddAlignmentResponse>();
      IgniteMock.Mutable.AddApplicationGridRouting<RemoveAlignmentComputeFunc, RemoveAlignmentArgument, RemoveAlignmentResponse>();
    }

    [Fact]
    public void Creation()
    {
      var req = new RemoveAlignmentRequest();
      req.Should().NotBeNull();
    }


    [Fact]
    public async void Remove_FailWithNoProject()
    {
      AddApplicationRouting();

      var request = new RemoveAlignmentRequest();
      var response = await request.ExecuteAsync(new RemoveAlignmentArgument
      {
        ProjectID = Guid.NewGuid(),
        AlignmentID = Guid.NewGuid()
      });

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.NoSelectedSiteModel);
    }

    [Fact]
    public async void Remove_FailWithNoAlignment()
    {
      AddApplicationRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new RemoveAlignmentRequest();
      var response = await request.ExecuteAsync(new RemoveAlignmentArgument
      {
        ProjectID = siteModel.ID,
        AlignmentID = Guid.NewGuid()
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
      var alignmentID = Guid.NewGuid();
      var addRequest = new AddAlignmentRequest();
      var addResponse = await addRequest.ExecuteAsync(new AddAlignmentArgument
      {
        ProjectID = siteModel.ID,
        DesignDescriptor = new DesignDescriptor(alignmentID, "folder", "filename"),
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1)
      });

      addResponse.Should().NotBeNull();
      addResponse.AlignmentUid.Should().Be(alignmentID);
      addResponse.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);
      siteModel.Alignments.Count.Should().Be(1);

      var removeRequest = new RemoveAlignmentRequest();
      var removeResponse = await removeRequest.ExecuteAsync(new RemoveAlignmentArgument
      {
        ProjectID = siteModel.ID,
        AlignmentID = alignmentID
      });

      removeResponse.Should().NotBeNull();
      removeResponse.AlignmentUid.Should().Be(alignmentID);
      removeResponse.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);
      siteModel.Alignments.Count.Should().Be(0);
    }
  }
}
