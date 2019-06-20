using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using ASNodeRPC;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using VLPDDecls;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionTemperatureDetailsExecutorTests
  {
#if RAPTOR
    [TestMethod]
    public void TemperatureDetailsExecutor_Raptor_NoResult()
    {
      var request = new TemperatureDetailsRequest(0, null, new double[0], null, null);

      var details = new TTemperatureDetails();

      var raptorClient = new Mock<IASNodeClient>();
      var logger = new Mock<ILoggerFactory>();

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_TEMPERATURE")).Returns("false");

      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();

      raptorClient
        .Setup(x => x.GetTemperatureDetails(request.ProjectId.Value, It.IsAny<TASNodeRequestDescriptor>(),
          It.IsAny<TTemperatureDetailSettings>(), It.IsAny<TICFilterSettings>(), It.IsAny<TICLiftBuildSettings>(),
          out details))
        .Returns(TASNodeErrorStatus.asneNoResultReturned);

      var executor = RequestExecutorContainerFactory
        .Build<DetailedTemperatureExecutor>(logger.Object, raptorClient.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);
      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }
#endif
    [TestMethod]
    public void TemperatureDetailsExecutor_TRex_NoResult()
    {
      var projectUid = Guid.NewGuid();
      var request = new TemperatureDetailsRequest(0, projectUid, new double[0], null, null);

      var logger = new Mock<ILoggerFactory>();

      var mockConfigStore = new Mock<IConfigurationStore>();
#if RAPTOR
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_TEMPERATURE")).Returns("true");
#endif

      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      trexCompactionDataProxy.Setup(x => x.SendDataPostRequest<TemperatureDetailResult, TemperatureDetailsRequest>(request, It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), false))
        .Returns((Task<TemperatureDetailResult>)null);


      var executor = RequestExecutorContainerFactory
        .Build<DetailedTemperatureExecutor>(logger.Object, configStore: mockConfigStore.Object,
          trexCompactionDataProxy: trexCompactionDataProxy.Object);

      var result = executor.Process(request);

      Assert.AreEqual(0, result.Code);
      Assert.AreEqual(result.Message, "success");
      Assert.AreEqual(((CompactionTemperatureDetailResult)result).Percents, null);
      Assert.AreEqual(((CompactionTemperatureDetailResult)result).TemperatureTarget, null);
    }
#if RAPTOR
    [TestMethod]
    public void TemperatureDetailsExecutorSuccess()
    {
      var request = new TemperatureDetailsRequest(0, null, new double[0], null, null);

      var details = new TTemperatureDetails { Percents = new[] { 5.0, 40.0, 13.0, 10.0, 22.0, 5.0, 6.0 } };

      var raptorClient = new Mock<IASNodeClient>();
      var logger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();
      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();

      raptorClient
        .Setup(x => x.GetTemperatureDetails(request.ProjectId.Value, It.IsAny<TASNodeRequestDescriptor>(),
          It.IsAny<TTemperatureDetailSettings>(), It.IsAny<TICFilterSettings>(), It.IsAny<TICLiftBuildSettings>(),
          out details))
        .Returns(TASNodeErrorStatus.asneOK);

      var executor = RequestExecutorContainerFactory
        .Build<DetailedTemperatureExecutor>(logger.Object, raptorClient.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);
      var result = executor.Process(request) as CompactionTemperatureDetailResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(details.Percents, result.Percents, "Wrong percents");
    }
#endif
  }
}
