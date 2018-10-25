using ASNodeDecls;
using ASNodeRPC;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionTemperatureDetailsExecutorTests
  {
    [TestMethod]
    public void TemperatureDetailsExecutorNoResult()
    {
      var request = new TemperatureDetailsRequest(0, null, new double[0], null, null);

      TTemperatureDetails details = new TTemperatureDetails();

      var raptorClient = new Mock<IASNodeClient>();
      var logger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();
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
  }
}
