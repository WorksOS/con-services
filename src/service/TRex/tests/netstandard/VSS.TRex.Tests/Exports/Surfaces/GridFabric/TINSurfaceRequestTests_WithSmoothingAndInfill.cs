using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Cells;
using VSS.TRex.DataSmoothing;
using VSS.TRex.DI;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridFabric
{
  public class SurfaceExportFixture_WithSmoothAndInfill : SurfaceExportProxy
  {
    public SurfaceExportFixture_WithSmoothAndInfill()
    {
      SetupFixture();
    }

    private static IDataSmoother SurfaceExportSmootherFactoryMethod()
    {
      return null;
    }

    public new void SetupFixture()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<Func<IDataSmoother>>(provider => SurfaceExportSmootherFactoryMethod))
        .Complete();
    }
  }

  [UnitTestCoveredRequest(RequestType = typeof(TINSurfaceRequest))]
  public class TINSurfaceRequestTests_WithSmoothAndInfill : TINSurfaceRequestTestsBase, IClassFixture<SurfaceExportFixture_WithSmoothAndInfill>
  {
    [Fact]
    public void Creation()
    {
      var request = new TINSurfaceRequest();
      request.Should().NotBeNull();
    }

    [Fact]
    public async Task Request_EmptyModel_ZeroTolerance()
    {
      AddGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new TINSurfaceRequest();

      var result = await request.ExecuteAsync(new TINSurfaceRequestArgument {ProjectID = siteModel.ID, Filters = new FilterSet(new CombinedFilter()), Tolerance = 0});

      result.Should().NotBeNull();
      result.data.Should().BeNull();
    }

    [Fact]
    public async Task Request_SingleTriangle_ZeroTolerance()
    {
      AddGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Create three cells in a triangle at (0, 0), (0, 1) & (1, 0)
      var cellPasses = new List<CellPass> {new CellPass {Time = DateTime.UtcNow, Height = 1.0f}};

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);
      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset + 1, cellPasses);
      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset + 1, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);
      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset + 1, SubGridTreeConsts.DefaultIndexOriginOffset + 1, cellPasses);

      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      var request = new TINSurfaceRequest();

      var result = await request.ExecuteAsync(new TINSurfaceRequestArgument {ProjectID = siteModel.ID, Filters = new FilterSet(new CombinedFilter()), Tolerance = 0});

      result.Should().NotBeNull();
      result.data.Should().NotBeNull();

      var model = new VSS.TRex.Designs.TTM.TrimbleTINModel();
      model.Read(new BinaryReader(new MemoryStream(result.data)));

      model.Vertices.Count.Should().Be(4);
      model.Triangles.Count.Should().Be(2);
    }

    [Fact]
    public async Task Request_SingleTAGFile_Smooth()
    {
      AddGridRouting();

      var tagFiles = new[] {Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag")};
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var request = new TINSurfaceRequest();
      var result = await request.ExecuteAsync(new TINSurfaceRequestArgument {ProjectID = siteModel.ID, Filters = new FilterSet(new CombinedFilter()), Tolerance = 0});

      result.Should().NotBeNull();
      result.data.Should().NotBeNull();

      var model = new TRex.Designs.TTM.TrimbleTINModel();
      model.Read(new BinaryReader(new MemoryStream(result.data)));

      model.Vertices.Count.Should().Be(2768);
      model.Triangles.Count.Should().Be(5011);
    }
  }
}
