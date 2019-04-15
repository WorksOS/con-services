using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.TRex.Cells;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridFabric
{
  public class SurfaceExportProxy : DITAGFileAndSubGridRequestsWithIgniteFixture, IDisposable
  {
    public SurfaceExportProxy()
    {
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();
    }
  }

  [UnitTestCoveredRequest(RequestType = typeof(TINSurfaceRequest))]
  public class TINSurfaceRequestTests : IClassFixture<SurfaceExportProxy>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<TINSurfaceRequestComputeFunc, TINSurfaceRequestArgument, TINSurfaceResult>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting
      <SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    private void AddGridRouting()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddClusterComputeGridRouting();
    }

    [Fact]
    public void Creation()
    {
      var request = new TINSurfaceRequest();
      request.Should().NotBeNull();
    }

    [Fact]
    public void Request_EmptyModel_ZeroTolerance()
    {
      AddGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new TINSurfaceRequest();

      var result = request.Execute(new TINSurfaceRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Tolerance = 0
      });

      result.Should().NotBeNull();
      result.data.Should().BeNull();
    }

    [Fact]
    public void Request_SingleTriangle_ZeroTolerance()
    {
      AddGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Create three cells in a triangle at (0, 0), (0, 1) & (1, 0)
      var cellPasses = new List<CellPass>
      {
        new CellPass
        {
          Time = DateTime.UtcNow,
          Height = 1.0f
        }
      };

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

      var result = request.Execute(new TINSurfaceRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Tolerance = 0
      });

      result.Should().NotBeNull();
      result.data.Should().NotBeNull();

      var model = new VSS.TRex.Designs.TTM.TrimbleTINModel();
      model.Read(new BinaryReader(new MemoryStream(result.data)));

      model.Vertices.Count.Should().Be(4);
      model.Triangles.Count.Should().Be(2);
    }
  }
}
