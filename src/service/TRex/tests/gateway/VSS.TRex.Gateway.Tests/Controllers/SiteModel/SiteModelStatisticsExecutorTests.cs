using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.SiteModel
{
  public class SiteModelStatisticsExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void SiteModelStatisticsExecutor_EmptySiteModel()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new ProjectStatisticsTRexRequest(siteModel.ID, null);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<SiteModelStatisticsExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = executor.Process(request) as ProjectStatisticsResult;
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.cellSize.Should().Be(SubGridTreeConsts.DefaultCellSize);
      result.startTime.Should().Be(Consts.MAX_DATETIME_AS_UTC);
      result.endTime.Should().Be(Consts.MIN_DATETIME_AS_UTC);
      result.extents.MinX.Should().Be(BoundingWorldExtent3D.Inverted().MinX);
      result.extents.MaxX.Should().Be(BoundingWorldExtent3D.Inverted().MaxX);
      result.extents.MinY.Should().Be(BoundingWorldExtent3D.Inverted().MinY);
      result.extents.MaxY.Should().Be(BoundingWorldExtent3D.Inverted().MaxY);
      result.extents.MinZ.Should().Be(BoundingWorldExtent3D.Inverted().MinZ);
      result.extents.MaxZ.Should().Be(BoundingWorldExtent3D.Inverted().MaxZ);
      result.indexOriginOffset.Should().Be((int) SubGridTreeConsts.DefaultIndexOriginOffset);

      var expectedJson = "{\"startTime\":\"9999-12-31T23:59:59.9999999\",\"endTime\":\"0001-01-01T00:00:00\",\"cellSize\":0.34,\"indexOriginOffset\":536870912,\"extents\":{\"maxX\":-1E+100,\"maxY\":-1E+100,\"maxZ\":-1E+100,\"minX\":1E+100,\"minY\":1E+100,\"minZ\":1E+100},\"Code\":0,\"Message\":\"success\"}";
      var json = JsonConvert.SerializeObject(result);
      json.Should().Be(expectedJson);
    }
  }
}
