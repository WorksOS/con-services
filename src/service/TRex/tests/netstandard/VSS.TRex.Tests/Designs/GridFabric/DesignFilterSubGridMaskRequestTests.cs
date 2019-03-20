using System;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(DesignFilterSubGridMaskRequest))]
  public class DesignFilterSubGridMaskRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <DesignFilterSubGridMaskComputeFunc, DesignSubGridFilterMaskArgument, DesignFilterSubGridMaskResponse>();

    [Fact]
    public void Creation()
    {
      var req = new DesignFilterSubGridMaskRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public void SimpleSurface_EmptySiteModel_NoDesign_AtOrigin_DefaultCellSize_FullExtent()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var req = new DesignFilterSubGridMaskRequest();

      // Ask for a design that does not exist
      var response = req.Execute(new DesignSubGridFilterMaskArgument(siteModel.ID, 0, 0, Guid.NewGuid(), siteModel.CellSize));

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.DesignDoesNotExist);
      response.Bits.Should().BeNull();
    }

    [Fact]
    public void SimpleSurface_EmptySiteModel_AtOrigin_DefaultCellSize_FullExtent()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 100.0f);
      var req = new DesignFilterSubGridMaskRequest();

      var response = req.Execute(new DesignSubGridFilterMaskArgument
        (siteModel.ID,
         SubGridTreeConsts.DefaultIndexOriginOffset, // Cell address of originX, 
         SubGridTreeConsts.DefaultIndexOriginOffset, // Cell address of originY
         designUid, siteModel.CellSize));

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Bits.CountBits().Should().Be(SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
    }

    [Fact]
    public void SimpleSurface_EmptySiteModel_AwayFromDesignCoverage_DefaultCellSize_FullExtent()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 100.0f);
      var req = new DesignFilterSubGridMaskRequest();

      var response = req.Execute(new DesignSubGridFilterMaskArgument
      (siteModel.ID,
        SubGridTreeConsts.DefaultIndexOriginOffset + 10000, // Cell address of originX, about 3km from location of triangle at origin 
        SubGridTreeConsts.DefaultIndexOriginOffset + 10000, // Cell address of originY, about 3km from location of triangle at origin
        designUid, siteModel.CellSize));

      response.Should().NotBeNull();
      response.RequestResult.Should().Be(DesignProfilerRequestResult.NoElevationsInRequestedPatch);
      response.Bits.Should().BeNull();
    }
  }
}
