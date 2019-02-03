using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;

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

      TTemperatureDetails details = new TTemperatureDetails();

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
      var request = new TemperatureDetailsRequest(0, null, new double[0], null, null);

      var logger = new Mock<ILoggerFactory>();

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_TEMPERATURE")).Returns("true");
      
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"Temperature Details statistics have not been implemented in TRex yet. ProjectUid: {projectUid}"));

      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      trexCompactionDataProxy.Setup(x => x.SendTemperatureDetailsRequest(It.IsAny<TemperatureDetailRequest>(), It.IsAny<IDictionary<string, string>>()))
        .Throws(exception);

      var executor = RequestExecutorContainerFactory
        .Build<DetailedTemperatureExecutor>(logger.Object, configStore: mockConfigStore.Object,
          trexCompactionDataProxy: trexCompactionDataProxy.Object);

      var result = Assert.ThrowsException<ServiceException>(() => executor.Process(request));

      Assert.AreEqual(HttpStatusCode.InternalServerError, result.Code);
      Assert.AreEqual(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.AreEqual(exception.GetResult.Message, result.GetResult.Message);
    }
#if RAPTOR
    [TestMethod]
    public void TemperatureDetailsExecutorSuccess()
    {
      var request = new TemperatureDetailsRequest(0, null, new double[0], null, null);

      TTemperatureDetails details = new TTemperatureDetails { Percents = new[] { 5.0, 40.0, 13.0, 10.0, 22.0, 5.0, 6.0 } };

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
