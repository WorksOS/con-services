using System;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.GridFabric.Responses;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurfaces.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AddSurveyedSurfaceRequest))]
  public class AddSurveyedSurfaceRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationRouting() => IgniteMock.Mutable.AddApplicationGridRouting<AddSurveyedSurfaceComputeFunc, AddSurveyedSurfaceArgument, AddSurveyedSurfaceResponse>();

    [Fact]
    public void Creation()
    {
      var req = new AddSurveyedSurfaceRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public async void Add_FailWithNoProject()
    {
      AddApplicationRouting();

      var request = new AddSurveyedSurfaceRequest();
      var response = await request.ExecuteAsync(new AddSurveyedSurfaceArgument
      {
        ProjectID = Guid.NewGuid(),
        DesignDescriptor = new DesignDescriptor(Guid.NewGuid(), "folder", "filename"),
        AsAtDate = DateTime.UtcNow,
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
      siteModel.SurveyedSurfaces.Count.Should().Be(0);

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.SurveyedSurfaces.Count.Should().Be(0);

      var designID = Guid.NewGuid();
      var existenceMap = new TRex.SubGridTrees.SubGridTreeSubGridExistenceBitMask();
      existenceMap[0, 0] = true;
      var asAtDate = DateTime.UtcNow;
      var request = new AddSurveyedSurfaceRequest();
      var response = await request.ExecuteAsync(new AddSurveyedSurfaceArgument
      {
        ProjectID = siteModel.ID,
        DesignDescriptor = new DesignDescriptor(designID, "folder", "filename"),
        AsAtDate = asAtDate,
        Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 1, 1),
        ExistenceMap = existenceMap
      });

      response.Should().NotBeNull();
      response.DesignUid.Should().Be(designID);
      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);

      // Re-request the sitemodel to reflect the change
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID, false);

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Mutable);
      siteModel.SurveyedSurfaces.Count.Should().Be(1);
      siteModel.SurveyedSurfaces[0].DesignDescriptor.Should().BeEquivalentTo(new TRex.Designs.Models.DesignDescriptor(designID, "folder", "filename"));
      siteModel.SurveyedSurfaces[0].AsAtDate.Should().Be(asAtDate);

      siteModel.SetStorageRepresentationToSupply(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.SurveyedSurfaces.Count.Should().Be(1);
      siteModel.SurveyedSurfaces[0].DesignDescriptor.Should().BeEquivalentTo(new TRex.Designs.Models.DesignDescriptor(designID, "folder", "filename"));
      siteModel.SurveyedSurfaces[0].AsAtDate.Should().Be(asAtDate);

      var readExistenceMap = DIContext.Obtain<IExistenceMapServer>().GetExistenceMap(new NonSpatialAffinityKey(siteModel.ID,
                             BaseExistenceMapRequest.CacheKeyString(TRex.ExistenceMaps.Interfaces.Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, designID)));
      existenceMap.Should().NotBeNull();
      existenceMap.CountBits().Should().Be(readExistenceMap.CountBits());
      readExistenceMap[0, 0].Should().BeTrue();
    }
  }
}
