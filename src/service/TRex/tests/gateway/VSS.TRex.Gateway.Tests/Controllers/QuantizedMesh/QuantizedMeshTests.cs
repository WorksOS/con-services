using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Core2;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.QuantizedMesh
{

  public class QuantizedMeshTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting
      <TileRenderRequestComputeFunc, TileRenderRequestArgument, TileRenderResponse>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting
      <SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    private void AddRoutings()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();
    }


    [Fact]
    public void TileExecutor_EmptySiteModel()
    {
      AddRoutings();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new QMTileRequest
      (
        siteModel.ID,
        new Guid(),
        new FilterResult(),
        0,0,0
      );
            
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<QuantizedMeshTileExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());

      var result = executor.Process(request) as QMTileResult;

      result.Should().NotBeNull();
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.TileData.Should().NotBeNull();
    }
  }


}
