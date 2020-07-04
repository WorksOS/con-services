using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AddTTMDesignRequest))]
  public class AddTTMDesignRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationRouting() => IgniteMock.Immutable.AddApplicationGridRouting<AddTTMDesignComputeFunc, AddTTMDesignArgument, AddTTMDesignResponse>();

    [Fact]
    public void Creation()
    {
      var req = new AddTTMDesignRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public async void AddDesign()
    {
      AddApplicationRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Mutable);
      siteModel.Designs.Count.Should().Be(0);

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.Designs.Count.Should().Be(0);

      var designID = System.Guid.NewGuid();

      var request = new AddTTMDesignRequest();
      await request.ExecuteAsync(new AddTTMDesignArgument
      {
        ProjectID = siteModel.ID,
        DesignDescriptor = new TRex.Designs.Models.DesignDescriptor(designID, "folder", "filename"),
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1),
        ExistenceMap = new VSS.TRex.SubGridTrees.SubGridTreeSubGridExistenceBitMask()
      });

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Mutable);
      siteModel.Designs.Count.Should().Be(1);
      siteModel.Designs[0].DesignDescriptor.Should().BeEquivalentTo(new TRex.Designs.Models.DesignDescriptor(designID, "folder", "filename"));

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.Designs.Count.Should().Be(1);
      siteModel.Designs[0].DesignDescriptor.Should().BeEquivalentTo(new TRex.Designs.Models.DesignDescriptor(designID, "folder", "filename"));
    }
  }
}
