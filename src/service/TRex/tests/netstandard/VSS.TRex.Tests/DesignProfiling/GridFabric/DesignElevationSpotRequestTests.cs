using System;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(DesignElevationSpotRequest))]
  public class DesignElevationSpotRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <CalculateDesignElevationSpotComputeFunc, CalculateDesignElevationSpotArgument, double>();

    [Fact]
    public void Test_DesignElevationSpotRequest_Creation()
    {
      var request = new DesignElevationSpotRequest();
      request.Should().NotBeNull();
    }

    [Theory]
    [InlineData(247645, 193072, 31.50, 0)]
    [InlineData(247668.341, 193059.996, 31.500, 0)]
    [InlineData(247680.000, 193054.000, 30.168, 0)]
    [InlineData(247680.000 + 100, 193054.000, Consts.NullDouble, 0)] // Outside of surface so returns NullDouble
    [InlineData(247645, 193072, 31.50, 1.0)]
    [InlineData(247668.341, 193059.996, 31.500, -2.3)]
    [InlineData(247680.000, 193054.000, 30.168, 100.1)]
    [InlineData(247680.000 + 100, 193054.000, Consts.NullDouble, 100.1)]  // Outside of surface so returns NullDouble
    public async Task Test_DesignElevationSpotRequest_EmptySiteModel_SpotLookup_WithOffset(double spotX, double spotY, double expectedHeight, double offset)
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Bug36372.ttm", false);
      var referenceDesign = new DesignOffset(designUid, offset);

      var request = new DesignElevationSpotRequest();
      var response = await request.ExecuteAsync(new CalculateDesignElevationSpotArgument
      {
        ProjectID = siteModel.ID,
        ReferenceDesign = referenceDesign,
        SpotX = spotX,
        SpotY = spotY,
      });

      response.Should().BeApproximately(expectedHeight + offset, 0.001);
    }
  }
}
