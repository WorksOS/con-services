using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling
{
  public class DesignProfilingRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      return siteModel;
    }

    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <CalculateDesignProfileComputeFunc, CalculateDesignProfileArgument, CalculateDesignProfileResponse>();

    [Fact]
    public void Test_DesignProfileRequest_Creation()
    {
      var request = new DesignProfileRequest();

      request.Should().NotBeNull();
    }

    private static XYZS[] DesignProfileResult(int index)
    {
      switch (index)
      {
        // Profile line between two adjacent triangles, one edge crossed.
        case 0: return new [] {
          new XYZS(247645.000, 193072.000, 31.500, 0, 0),
          new XYZS(247668.341, 193059.996, 31.500, 26.247, 0),
          new XYZS(247680.000, 193054.000, 30.168, 39.357, 0)
        };
      }

      return new XYZS[0];
    }

    [InlineData(247645, 193072, 247680, 193054, 3, 0)] // Profile line between two adjacent triangles, one edge crossed.
    [Theory]
    public void Test_DesignProfileRequest_OverTTM_NoFilter(double startX, double startY, double endX, double endY, int expectedPointCount, int resultIndex)
    {
      const double EPSILON = 0.001;

      AddDesignProfilerGridRouting();

      var siteModel = NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "bug36372.ttm");

      var request = new DesignProfileRequest();
      var response = request.Execute(new CalculateDesignProfileArgument
      {
        ProjectID = siteModel.ID,
        CellSize = SubGridTreeConsts.DefaultCellSize,
        ReferenceDesignUID = designUid,
        Filters = new FilterSet(new CombinedFilter()),
        ProfilePath = new [] { new XYZ(startX, startY), new XYZ(endX, endY) },
        TRexNodeID = "UnitTest_TRexNodeID"
      });

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Profile.Count.Should().Be(expectedPointCount);

      var profile = DesignProfileResult(resultIndex);
      response.Profile.Count.Should().Be(profile.Length);

      for (int i = 0; i < response.Profile.Count - 1; i++)
      {
        profile[i].X.Should().BeApproximately(response.Profile[i].X, EPSILON);
        profile[i].Y.Should().BeApproximately(response.Profile[i].Y, EPSILON);
        profile[i].Z.Should().BeApproximately(response.Profile[i].Z, EPSILON);
        profile[i].Station.Should().BeApproximately(response.Profile[i].Station, EPSILON);
      }
    }
  }
}
