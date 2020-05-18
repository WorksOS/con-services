using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
#if RAPTOR
using ASNode.ElevationStatistics.RPC;
using ASNodeDecls;
using ASNodeRPC;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using BoundingExtents;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.Report.Executors
{
  [TestClass]
  public class ElevationStatisticsExecutorTests
  {
#if RAPTOR
    [TestMethod]
    public void ElevationStatisticsExecutor_Raptor_NoResult()
    {
      var request = new ElevationStatisticsRequest(0, null, null, null, -1, null);

      var raptorClient = new Mock<IASNodeClient>();
      var logger = new Mock<ILoggerFactory>();

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_ELEVATION")).Returns(false);

      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();

      var result = new TASNodeElevationStatisticsResult();

      raptorClient
        .Setup(x => x.GetElevationStatistics(request.ProjectId.Value, It.IsAny<TASNodeRequestDescriptor>(),
          It.IsAny<TICFilterSettings>(), It.IsAny<TICLiftBuildSettings>(), out result))
        .Returns(TASNodeErrorStatus.asneNoResultReturned);

      var executor = RequestExecutorContainerFactory
        .Build<ElevationStatisticsExecutor>(logger.Object, raptorClient.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);

      Assert.ThrowsExceptionAsync<AggregateException>(async () => await executor.ProcessAsync(request));
    }
#endif

    [TestMethod]
    public async Task ElevationStatisticsExecutor_TRex_NoResult()
    {
      var request = new ElevationStatisticsRequest(0, Guid.NewGuid(), null, null, -1, null);

      var logger = new Mock<ILoggerFactory>();

      var mockConfigStore = new Mock<IConfigurationStore>();
#if RAPTOR
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_ELEVATION")).Returns(true);
#endif

      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      var elevationStatisticsRequest = new ElevationDataRequest(request.ProjectUid.Value, request.Filter, null, null);

      trexCompactionDataProxy.Setup(x => x.SendDataPostRequest<ElevationStatisticsResult, ElevationDataRequest>(elevationStatisticsRequest, It.IsAny<string>(), It.IsAny<IHeaderDictionary>(), false))
        .Returns((Task<ElevationStatisticsResult>)null);

      var executor = RequestExecutorContainerFactory
        .Build<ElevationStatisticsExecutor>(logger.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);

      var result = await executor.ProcessAsync(request);

      Assert.IsNull(result, "Result should be null");
    }
#if RAPTOR
    [TestMethod]
    public async Task ElevationStatisticsExecutorSuccess()
    {
      var request = new ElevationStatisticsRequest(0, null, null, null, -1, null);

      var raptorClient = new Mock<IASNodeClient>();
      var logger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();
      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();

      var elevationStatisticsResult = new TASNodeElevationStatisticsResult(111.22, 333.44, 12345.79, new T3DBoundingWorldExtent(123.35, 456.65, 133.48, 466.77));

      raptorClient
        .Setup(x => x.GetElevationStatistics(request.ProjectId.Value, It.IsAny<TASNodeRequestDescriptor>(),
          It.IsAny<TICFilterSettings>(), It.IsAny<TICLiftBuildSettings>(), out elevationStatisticsResult))
        .Returns(TASNodeErrorStatus.asneOK);

      var executor = RequestExecutorContainerFactory
        .Build<ElevationStatisticsExecutor>(logger.Object, raptorClient.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);

      var result = await executor.ProcessAsync(request) as ElevationStatisticsResult;

      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(elevationStatisticsResult.CoverageArea, result.TotalCoverageArea, "Wrong CoverageArea");
      Assert.AreEqual(elevationStatisticsResult.MinElevation, result.MinElevation, "Wrong MinElevation");
      Assert.AreEqual(elevationStatisticsResult.MaxElevation, result.MaxElevation, "Wrong MaxElevation");
      Assert.AreEqual(elevationStatisticsResult.BoundingExtents.MinX, result.BoundingExtents.MinX, "Wrong BoundingExtents.MinX");
      Assert.AreEqual(elevationStatisticsResult.BoundingExtents.MinY, result.BoundingExtents.MinY, "Wrong BoundingExtents.MinY");
      Assert.AreEqual(elevationStatisticsResult.BoundingExtents.MinZ, result.BoundingExtents.MinZ, "Wrong BoundingExtents.MinZ");
      Assert.AreEqual(elevationStatisticsResult.BoundingExtents.MaxX, result.BoundingExtents.MaxX, "Wrong BoundingExtents.MaxX");
      Assert.AreEqual(elevationStatisticsResult.BoundingExtents.MaxY, result.BoundingExtents.MaxY, "Wrong BoundingExtents.MaxY");
      Assert.AreEqual(elevationStatisticsResult.BoundingExtents.MaxZ, result.BoundingExtents.MaxZ, "Wrong BoundingExtents.MaxZ");
    }
#endif
  }
}
