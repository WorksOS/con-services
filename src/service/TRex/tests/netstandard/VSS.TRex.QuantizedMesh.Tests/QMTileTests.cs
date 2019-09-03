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
using VSS.TRex.QuantizedMesh.MeshUtils;
using VSS.TRex.QuantizedMesh.Models;
using VSS.TRex.QuantizedMesh.GridFabric;

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

    [Fact]
    public void BoundingSphere_CoordinateUtils_CalculateHeader()
    {
      var ecefPoints = new Vector3[4];
      ecefPoints[0] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(1.00001), Y = MapUtils.Deg2Rad(1.00001), Z = 0 });
      ecefPoints[1] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(1.00002), Y = MapUtils.Deg2Rad(1.00001), Z = 0 });
      ecefPoints[2] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(1.00001), Y = MapUtils.Deg2Rad(1.00002), Z = 10 });
      ecefPoints[3] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(1.00002), Y = MapUtils.Deg2Rad(1.00002), Z = 10 });
      BBSphere sphere = new BBSphere();
      sphere.FromPoints(ecefPoints);
      var rad = Math.Round(sphere.Radius, 2);
      rad.Should().Be(5.06);
      var geo = CoordinateUtils.ecef_to_geo(sphere.Center);
      var alt = Math.Round(geo.Z, 1);
      alt.Should().Be(5.0);
      var lon = Math.Round(MapUtils.Rad2Deg(geo.X), 6);
      var lat = Math.Round(MapUtils.Rad2Deg(geo.Y), 6);
      lon.Should().Be(1.000015);
      lat.Should().Be(1.000015);
    }

    [Fact]
    public void Cartesian3D_Vector_Maths_Tests()
    {
      Vector3 v1 = new Vector3(1, 1, 1);
      Vector3 v2 = new Vector3(1, 4, 1);
      Vector3 v3 = new Vector3(10, 0, 0);
      Vector3 v4 = new Vector3(0, 3, 0);
      Vector3 v5 = new Vector3(2, 5, 2);
      Vector3 normal = new Vector3(1, 0, 0);
      var sq = Cartesian3D.DistanceSquared(v1,v2);
      sq.Should().Be(9);
      var dst = Cartesian3D.Distance(v1, v2);
      dst.Should().Be(3);
      var nm = Cartesian3D.Normalize(v3);
      nm.Should().Be(normal);
      var mag = Cartesian3D.Magnitude(v3);
      mag.Should().Be(10);
      var sub = Cartesian3D.Subtract(v2, v1);
      sub.Should().Be(v4);
      var add = Cartesian3D.Add(v2, v1);
      add.Should().Be(v5);

    }

    [Fact]
    public void TileBuilder_BuildTile()
    {
      LLBoundingBox TileBoundaryLL = MapGeo.TileXYZToRectLL(0, 0, 20, out var yFlip);
      ElevationData elevData = new ElevationData(0, 5);
      elevData.MakeEmptyTile(TileBoundaryLL);
      QMTileBuilder tileBuilder = new QMTileBuilder()
      {
        TileData = elevData,
        GridSize = elevData.GridSize
      };

      var res = tileBuilder.BuildQuantizedMeshTile();
      res.Should().Be(true);
      tileBuilder.QuantizedMeshTile.Should().HaveCountGreaterOrEqualTo(162);
      tileBuilder.QuantizedMeshTile.Should().HaveCountLessOrEqualTo(164);
    }

    [Fact]
    public void MapGeo_NumberOfTiles()
    {
      var tiles1 = MapGeo.GetNumberOfXTilesAtLevel(1);
      tiles1.Should().Be(4);
      var tiles2 = MapGeo.GetNumberOfYTilesAtLevel(1);
      tiles2.Should().Be(2);
    }

    [Fact]
    public void MapUtils_MapCoordinate_Tests()
    {
      var flipedY = MapUtils.FlipY(78341,18);
      flipedY.Should().Be(183802);
      var ecef = MapUtils.LatLonToEcef(0,0,100);
      ecef.X.Should().Be(6378237); // earth raduis + 100m
      var ecef2 = MapUtils.LatLonToEcef(0,0,200);
      ecef2.X.Should().Be(6378337);// earth raduis + 200m
      var dist = Math.Round(MapUtils.GetDistance(1,1,2,1),2);
      dist.Should().Be(111.27);
      var mid = MapUtils.MidPointLL(1,1,2,1);
      MapPoint mp = new MapPoint(1.5, 1);
      mp.Should().Be(mp);
    }

    [Fact]
    public void MeshBuilder_QuantizeHeight_Tests()
    {
      var min = MeshBuilder.QuantizeHeight(0, 100, 0);
      min.Should().Be(0);
      var mid = MeshBuilder.QuantizeHeight(0, 100, 50);
      mid.Should().Be(16383);
      var max = MeshBuilder.QuantizeHeight(0, 100, 100);
      max.Should().Be(32767);
    }

    [Fact]
    public void VertextData_AddVertex_Test()
    {
      var vert = new VertexData(1,1);
      vert.AddVertex(0, 1, 1, 1);
      vert.height[0].Should().Be(1);
      vert.u[0].Should().Be(0); // zero based
      vert.v[0].Should().Be(0); // zero based
      vert.vertexCount.Should().Be(1);
    }

  }

  public class QMTileTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    const int DECIMALS = 6;
    private ISiteModel siteModel;
    private FilterSet filter;
    private int DisplayMode = 1;

    private void SetupTest()
    {
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile-QMesh.tag"),
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
      var request = new QMTileExecutor(Guid.NewGuid(), filter, 0, 0, 0, DisplayMode, "1");
      request.Should().NotBeNull();
    }

    [Fact]
    public void Execute_EmptySiteModel_ReturnEmptyTile()
    {
  
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var filter = new FilterSet(new CombinedFilter());
      var request = new QMTileExecutor(siteModel.ID, filter, 0, 0, 19, DisplayMode, "1");
      request.ExecuteAsync();
      request.ResultStatus.Should().NotBe(RequestErrorStatus.Unknown);
      var QMTileResponse = request.QMTileResponse;
      //QMTileResponse.data.Should().HaveCount(164); // Should return an empty tile
      // Todo why OS makes a difference for this test
      QMTileResponse.data.Should().HaveCountGreaterOrEqualTo(162);
      QMTileResponse.data.Should().HaveCountLessOrEqualTo(164);
    }

    [Fact]
    public void Execute_RootTile_Expected()
    {
      AddClusterComputeGridRouting();
      SetupTest();
      var request = new QMTileExecutor(siteModel.ID, filter, 0, 1, 0, DisplayMode, "1");
      request.ExecuteAsync();
      request.ResultStatus.Should().NotBe(RequestErrorStatus.Unknown);
      var QMTileResponse = request.QMTileResponse;
      QMTileResponse.data.Should().HaveCount(845); // Should return a root tile
    }

    [Fact]
    public void Execute_TooFarOut_EmptyTile_Expected()
    {
      AddClusterComputeGridRouting();
      SetupTest();
      var request = new QMTileExecutor(siteModel.ID, filter, 0, 1, 10, DisplayMode, "1");
      request.ExecuteAsync();
      request.ResultStatus.Should().NotBe(RequestErrorStatus.Unknown);
      var QMTileResponse = request.QMTileResponse;
      QMTileResponse.data.Should().HaveCountGreaterOrEqualTo(172);
      QMTileResponse.data.Should().HaveCountLessOrEqualTo(176);
    }

    [Fact]
    public void Execute_ValidProductionTile_Expected()
    {
      AddClusterComputeGridRouting();
      SetupTest();
      var request = new QMTileExecutor(siteModel.ID, filter, 47317, 12155, 17, DisplayMode, "1");
      request.ExecuteAsync();
      request.ResultStatus.Should().Be(RequestErrorStatus.OK);
      var QMTileResponse = request.QMTileResponse;
      QMTileResponse.data.Should().HaveCountGreaterOrEqualTo(3591);
    }

    [Fact]
    public void Execute_FailPipeLineSetup_EmptyTile_Expected()
    {
      AddClusterComputeGridRouting();
      SetupTest();
      // Missing coordinate system
      var request = new QMTileExecutor(siteModel.ID, filter, 47317, 12155, 17, 0, "1");
      request.ExecuteAsync();
      request.ResultStatus.Should().Be(RequestErrorStatus.OK); // Empty tile expected
      var QMTileResponse = request.QMTileResponse;
      QMTileResponse.data.Should().HaveCountGreaterOrEqualTo(172);
      QMTileResponse.data.Should().HaveCountLessOrEqualTo(176);
    }

  }
}
