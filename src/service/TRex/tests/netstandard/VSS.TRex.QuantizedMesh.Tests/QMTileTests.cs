using System;
using VSS.TRex.Filters;
using VSS.TRex.QuantizedMesh.Executors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using VSS.TRex.Types;
using VSS.TRex.Geometry;
using Moq;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.Tests;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.QuantizedMesh.Tests
{
  public class QMTileTests_NoIgniteDI : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public async Task Execute_FailWithNoSiteModel()
    {
      var filter = new FilterSet(new CombinedFilter());
      var request = new QMTileExecutor(new Guid(), filter, 0, 0, 19, 0, "1");
      var result = await request.ExecuteAsync();
      result.Should().BeFalse();
    }
  }

  public class QMTileTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {

    const int DECIMALS = 6;
    private ISiteModel siteModel;
    private FilterSet filter;

    private void SetupTest()
    {
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile-MDP.tag"),
      };

      siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var boundary = new List<Fence>() { new Fence() };
      boundary[0].Points.Add(new FencePoint(2700.20170260547, 1225.08445683629, 0.0));
      boundary[0].Points.Add(new FencePoint(2700.16517351542, 1224.38744027628, 0.0));
      boundary[0].Points.Add(new FencePoint(2700.10136538994, 1223.16990871245, 0.0));
      boundary[0].Points.Add(new FencePoint(2889.7599542129, 1178.36648123432, 0.0));

      // Mocked ConvertCoordinates expected result.
      var neeCoords = new XYZ[boundary[0].Points.Count];

      // mock tile boundary within model extents 
      //WS
      neeCoords[0].X = Math.Round(2847.26, DECIMALS);
      neeCoords[0].Y = Math.Round(1219.93, DECIMALS);
      //EN
      neeCoords[1].X = Math.Round(2879.11, DECIMALS);
      neeCoords[1].Y = Math.Round(1276.37, DECIMALS);
      //WN
      neeCoords[2].X = Math.Round(2847.26, DECIMALS);
      neeCoords[2].Y = Math.Round(1276.37, DECIMALS);
      // ES
      neeCoords[3].X = Math.Round(2879.11, DECIMALS);
      neeCoords[3].Y = Math.Round(1219.93, DECIMALS);

      var llhCoords = new XYZ[2500];
      for (int i = 0; i < 2500; i++)
      {
        llhCoords[i].X = Math.Round(2847.26 + i, DECIMALS);
        llhCoords[i].Y = Math.Round(1219.93 + i, DECIMALS);
      }

      var expectedCoordinateConversionResult = (RequestErrorStatus.OK, neeCoords);
      var expectedCoordinateConversionResult2 = (RequestErrorStatus.OK, llhCoords);

      // LL to NEE
      var convertCoordinatesMock = new Mock<IConvertCoordinates>();
      convertCoordinatesMock.Setup(x => x.LLHToNEE(It.IsAny<string>(), It.IsAny<XYZ[]>(), true)).ReturnsAsync(expectedCoordinateConversionResult);
      convertCoordinatesMock.Setup(x => x.NEEToLLH(It.IsAny<string>(), It.IsAny<XYZ[]>())).ReturnsAsync(expectedCoordinateConversionResult2);
      DIBuilder.Continue().Add(x => x.AddSingleton(convertCoordinatesMock.Object)).Complete();

      filter = new FilterSet(new CombinedFilter());
    }

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    [Fact]
    public void Creation()
    {
      var filter = new FilterSet(new CombinedFilter());
      var request = new QMTileExecutor(Guid.NewGuid(), filter, 0, 0, 0, 0, "1");
      request.Should().NotBeNull();
    }

    [Fact]
    public void Execute_EmptySiteModel_ReturnEmptyTile()
    {
  
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var filter = new FilterSet(new CombinedFilter());
      var request = new QMTileExecutor(siteModel.ID, filter, 0, 0, 19, 0, "1");
      request.ExecuteAsync();
      request.ResultStatus.Should().NotBe(RequestErrorStatus.Unknown);
      var QMTileResponse = request.QMTileResponse;
      QMTileResponse.data.Should().HaveCount(164); // Should return an empty tile
    }


    [Fact(Skip = "Missing test data. todo")]
    public void Execute_RootTile_Expected()
    {
      AddClusterComputeGridRouting();
      SetupTest();
      var request = new QMTileExecutor(siteModel.ID, filter, 0, 1, 0, 0, "1");
      request.ExecuteAsync();
      request.ResultStatus.Should().NotBe(RequestErrorStatus.Unknown);
      var QMTileResponse = request.QMTileResponse;
      QMTileResponse.data.Should().HaveCount(845); // Should return a root tile
    }


    [Fact (Skip="Missing test data. Todo")]
    public void Execute_NotEmptySiteModel_Success()
    {
      AddClusterComputeGridRouting();
      SetupTest();
      var request = new QMTileExecutor(siteModel.ID, filter, 47317, 12155, 17, 1, "1");
      request.ExecuteAsync();
      request.ResultStatus.Should().Be(RequestErrorStatus.OK);
      var QMTileResponse = request.QMTileResponse;
      QMTileResponse.data.Should().HaveCount(1345); // Should return a valid tile
    }

  }
}
