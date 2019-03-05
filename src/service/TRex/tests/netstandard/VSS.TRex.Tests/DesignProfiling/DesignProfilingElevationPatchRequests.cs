using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling
{
  public class DesignProfilingElevationPatchRequests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private ISiteModel NewEmptyModel() => DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

    private void AddDesignProfilerGridRouting() => DITAGFileAndSubGridRequestsWithIgniteFixture.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    [Fact]
    public void Test_DesignElevationPatchRequest_Creation()
    {
      var request = new DesignElevationPatchRequest();
      request.Should().NotBeNull();
    }

    [Theory]
    [InlineData(247645, 193072, 31.50, 0, true)]
    [InlineData(247668.341, 193059.996, 31.500, 0, true)]
    [InlineData(247680.000, 193054.000, 30.168, 0, true)]
    [InlineData(247680.000 + 100, 193054.000, Consts.NullDouble, 0, false)] // Outside of surface so returns NullDouble
    [InlineData(247645, 193072, 31.50, 1.0, true)]
    [InlineData(247668.341, 193059.996, 31.500, -2.3, true)]
    [InlineData(247680.000, 193054.000, 30.168, 100.1, true)]
    [InlineData(247680.000 + 100, 193054.000, Consts.NullDouble, 100.1, false)]  // Outside of surface so returns NullDouble
    public void Test_DesignElevationPatchRequest_WithOffset(double spotX, double spotY, double expectedHeight, double offset, bool patchExists)
    {
      AddDesignProfilerGridRouting();

      var siteModel = NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Bug36372.ttm");

      // Get the cell location of the probe position. Note that the request will return the sub grid
      // that contains this cell, so the origin location of the sub grid may not be the same as the cell location
      // 
      siteModel.Grid.CalculateIndexOfCellContainingPosition(spotX, spotY, out uint cellX, out uint cellY);

      var request = new DesignElevationPatchRequest();
      var argument = new CalculateDesignElevationPatchArgument
      {
        ProjectID = siteModel.ID,
        ReferenceDesignUID = designUid,
        Offset = 0,
        CellSize = siteModel.CellSize,
        Filters = new FilterSet(new CombinedFilter()),
        OriginX = cellX,
        OriginY = cellY
      };

      var response = request.Execute(argument);

      response.Should().NotBeNull();
      response.CalcResult.Should().Be(patchExists ? DesignProfilerRequestResult.OK : DesignProfilerRequestResult.NoElevationsInRequestedPatch);

      if (patchExists)
      {
        response.Heights.CellSize.Should().Be(siteModel.CellSize);
        response.Heights.GridDataType.Should().Be(GridDataType.Height);
        response.Heights.Level.Should().Be(siteModel.Grid.NumLevels);
        response.Heights.OriginX.Should().Be((uint) (cellX & ~SubGridTreeConsts.SubGridLocalKeyMask));
        response.Heights.OriginY.Should().Be((uint) (cellY & ~SubGridTreeConsts.SubGridLocalKeyMask));
        response.Heights.Owner.Should().BeNull();
        response.Heights.Parent.Should().BeNull();
        response.Heights.Cells.Should().NotBeNull();
      }
      else
      {
        response.Heights.Should().BeNull();
      }

      if (patchExists)
      {
        // Check a request with an offset provides the expected answer
        argument.Offset = offset;

        var response2 = request.Execute(argument);

        response2.Should().NotBeNull();
        response2.CalcResult.Should().Be(DesignProfilerRequestResult.OK);
        response2.Heights.Should().NotBeNull();

        response2.Heights.ForEach((x, y) => response2.Heights.Cells[x, y].Should().BeApproximately((float)(response.Heights.Cells[x, y] + offset), TestHelper.ALLOWED_HEIGHT_TOLERANCE_AS_FLOAT));
      }
    }
  }
}
