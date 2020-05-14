using System;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(DesignFilterSubGridMaskRequest))]
  public class DesignFilterSubGridMaskRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddDesignProfilerGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <DesignFilterSubGridMaskComputeFunc, DesignSubGridFilterMaskArgument, DesignFilterSubGridMaskResponse>();

    [Fact]
    public void Creation()
    {
      var req = new DesignFilterSubGridMaskRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public async Task SimpleSurface_EmptySiteModel_NoDesign_AtOrigin_DefaultCellSize_FullExtent()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var req = new DesignFilterSubGridMaskRequest();

      // Ask for a design that does not exist
      var response = await req.ExecuteAsync(new DesignSubGridFilterMaskArgument(siteModel.ID, 0, 0, new DesignOffset(Guid.NewGuid(), -0.5), siteModel.CellSize));

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.DesignDoesNotExist);
      response.Bits.Should().BeNull();
    }

    [Fact]
    public async Task SimpleSurface_EmptySiteModel_AtOrigin_DefaultCellSize_FullExtent()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 100.0f);
      var referenceDesign = new DesignOffset(designUid, 0);
      var req = new DesignFilterSubGridMaskRequest();

      var response = await req.ExecuteAsync(new DesignSubGridFilterMaskArgument
        (siteModel.ID,
         SubGridTreeConsts.DefaultIndexOriginOffset, // Cell address of originX, 
         SubGridTreeConsts.DefaultIndexOriginOffset, // Cell address of originY
         referenceDesign, siteModel.CellSize));

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Bits.CountBits().Should().Be(903);
    }

    [Fact]
    public async Task SimpleSurface_EmptySiteModel_AwayFromDesignCoverage_DefaultCellSize_FullExtent()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 100.0f);
      var refeenceDesign = new DesignOffset(designUid, 0);
      var req = new DesignFilterSubGridMaskRequest();

      var response = await req.ExecuteAsync(new DesignSubGridFilterMaskArgument
      (siteModel.ID,
        SubGridTreeConsts.DefaultIndexOriginOffset + 10000, // Cell address of originX, about 3km from location of triangle at origin 
        SubGridTreeConsts.DefaultIndexOriginOffset + 10000, // Cell address of originY, about 3km from location of triangle at origin
        refeenceDesign, siteModel.CellSize));

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.NoElevationsInRequestedPatch);
      response.Bits.Should().BeNull();
    }
  }
}
