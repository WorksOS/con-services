using System;
using FluentAssertions;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AddTTMDesignRequest))]
  public class AddTTMDesignRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationRouting() => IgniteMock.Mutable.AddApplicationGridRouting<AddTTMDesignComputeFunc, AddTTMDesignArgument, AddTTMDesignResponse>();

    [Fact]
    public void Creation()
    {
      var req = new AddTTMDesignRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public async void Add_FailWithNoProject()
    {
      AddApplicationRouting();

      var request = new AddTTMDesignRequest();
      var response = await request.ExecuteAsync(new AddTTMDesignArgument
      {
        ProjectID = Guid.NewGuid(),
        DesignDescriptor = new DesignDescriptor(Guid.NewGuid(), "folder", "filename"),
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1),
        ExistenceMap = new TRex.SubGridTrees.SubGridTreeSubGridExistenceBitMask()
      });

      response.Should().NotBeNull();
      response.DesignUid.Should().Be(Guid.Empty);
      response.RequestResult.Should().Be(DesignProfilerRequestResult.FailedToAddDesign);
    }

    [Fact]
    public async void Add()
    {
      AddApplicationRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Mutable);
      siteModel.Designs.Count.Should().Be(0);

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.Designs.Count.Should().Be(0);

      var designID = System.Guid.NewGuid();
      var existenceMap = new TRex.SubGridTrees.SubGridTreeSubGridExistenceBitMask();
      existenceMap[0, 0] = true;

      var request = new AddTTMDesignRequest();
      var response = await request.ExecuteAsync(new AddTTMDesignArgument
      {
        ProjectID = siteModel.ID,
        DesignDescriptor = new DesignDescriptor(designID, "folder", "filename"),
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1),
        ExistenceMap = existenceMap
      });

      response.Should().NotBeNull();
      response.DesignUid.Should().Be(designID);
      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Mutable);
      siteModel.Designs.Count.Should().Be(1);
      siteModel.Designs[0].DesignDescriptor.Should().BeEquivalentTo(new TRex.Designs.Models.DesignDescriptor(designID, "folder", "filename"));

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.Designs.Count.Should().Be(1);
      siteModel.Designs[0].DesignDescriptor.Should().BeEquivalentTo(new TRex.Designs.Models.DesignDescriptor(designID, "folder", "filename"));

      var readExistenceMap = DIContext.Obtain<IExistenceMapServer>().GetExistenceMap(new NonSpatialAffinityKey(siteModel.ID,
        BaseExistenceMapRequest.CacheKeyString(TRex.ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designID)));
      readExistenceMap.Should().NotBeNull();
      existenceMap.CountBits().Should().Be(readExistenceMap.CountBits());
      readExistenceMap[0, 0].Should().BeTrue();
    }
  }
}
