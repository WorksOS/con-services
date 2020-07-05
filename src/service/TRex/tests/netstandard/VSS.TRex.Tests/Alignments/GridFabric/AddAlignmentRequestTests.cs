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
  [UnitTestCoveredRequest(RequestType = typeof(AddAlignmentRequest))]
  public class AddAlignmentRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationRouting() => IgniteMock.Mutable.AddApplicationGridRouting<AddAlignmentComputeFunc, AddAlignmentArgument, AddAlignmentResponse>();

    [Fact]
    public void Creation()
    {
      var req = new AddAlignmentRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public async void Add_FailWithNoProject()
    {
      AddApplicationRouting();

      var request = new AddAlignmentRequest();
      var response = await request.ExecuteAsync(new AddAlignmentArgument
      {
        ProjectID = Guid.NewGuid(),
        DesignDescriptor = new DesignDescriptor(Guid.NewGuid(), "folder", "filename"),
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1)
      });

      response.Should().NotBeNull();
      response.AlignmentUid.Should().Be(Guid.Empty);
      response.RequestResult.Should().Be(DesignProfilerRequestResult.FailedToAddDesign);
    }

    [Fact]
    public async void Add()
    {
      AddApplicationRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Mutable);
      siteModel.Alignments.Count.Should().Be(0);

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.Alignments.Count.Should().Be(0);

      var alignmentID = Guid.NewGuid();
      var request = new AddAlignmentRequest();
      var response = await request.ExecuteAsync(new AddAlignmentArgument
      {
        ProjectID = siteModel.ID,
        DesignDescriptor = new DesignDescriptor(alignmentID, "folder", "filename"),
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1)
      });

      response.Should().NotBeNull();
      response.AlignmentUid.Should().Be(alignmentID);
      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Mutable);
      siteModel.Alignments.Count.Should().Be(1);
      siteModel.Alignments[0].DesignDescriptor.Should().BeEquivalentTo(new DesignDescriptor(alignmentID, "folder", "filename"));

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.Alignments.Count.Should().Be(1);
      siteModel.Alignments[0].DesignDescriptor.Should().BeEquivalentTo(new DesignDescriptor(alignmentID, "folder", "filename"));
    }
  }
}
