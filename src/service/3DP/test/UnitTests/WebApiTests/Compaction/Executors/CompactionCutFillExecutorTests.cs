using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using ASNodeRPC;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using VLPDDecls;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionCutFillExecutorTests
  {
    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
#if RAPTOR
        .AddTransient<IErrorCodesProvider, RaptorResult>()
#endif
        ;

      serviceProvider = serviceCollection.BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
    }
#if RAPTOR
    [TestMethod]
    public void CutFillExecutor_Raptor_NoResult()
    {
      var request = new CutFillDetailsRequest(0, null, null, null, null, null);

      TCutFillDetails details = new TCutFillDetails();

      var raptorClient = new Mock<IASNodeClient>();
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_CUTFILL")).Returns(false);

      raptorClient
        .Setup(x => x.GetCutFillDetails(request.ProjectId.Value, It.IsAny<TASNodeRequestDescriptor>(),
          It.IsAny<TCutFillSettings>(), It.IsAny<TICFilterSettings>(), It.IsAny<TICLiftBuildSettings>(),
          out details))
        .Returns(TASNodeErrorStatus.asneUnknown);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionCutFillExecutor>(logger, raptorClient.Object, configStore: configStore.Object);
      Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
    }
#endif
    [TestMethod]
    public async Task CutFillExecutor_TRex_NoResult()
    {
      var projectUid = Guid.NewGuid();
      var request = new CutFillDetailsRequest(0, projectUid, null, null, null, null);

      var configStore = new Mock<IConfigurationStore>();
#if RAPTOR
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_CUTFILL")).Returns(true);
#endif

      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"Cut/Fill statistics have not been implemented in TRex yet. ProjectUid: {projectUid}"));

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataPostRequest<CompactionCutFillDetailedResult, TRexCutFillDetailsRequest>(It.IsAny<TRexCutFillDetailsRequest>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), false))
        .Throws(exception);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionCutFillExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object);

      var result = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));

      Assert.AreEqual(HttpStatusCode.InternalServerError, result.Code);
      Assert.AreEqual(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.AreEqual(exception.GetResult.Message, result.GetResult.Message);
    }
#if RAPTOR
    [TestMethod]
    public async Task CutFillExecutorSuccess()
    {
      var request = new CutFillDetailsRequest(0, null, null, null, null, null);

      TCutFillDetails details = new TCutFillDetails { Percents = new[] { 5.0, 20.0, 13.0, 10.0, 22.0, 12.0, 18.0 } };

      var raptorClient = new Mock<IASNodeClient>();
      var configStore = new Mock<IConfigurationStore>();

      raptorClient
        .Setup(x => x.GetCutFillDetails(request.ProjectId.Value, It.IsAny<TASNodeRequestDescriptor>(),
          It.IsAny<TCutFillSettings>(), It.IsAny<TICFilterSettings>(), It.IsAny<TICLiftBuildSettings>(),
          out details))
        .Returns(TASNodeErrorStatus.asneOK);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionCutFillExecutor>(logger, raptorClient.Object, configStore: configStore.Object);
      var result = await executor.ProcessAsync(request) as CompactionCutFillDetailedResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(details.Percents, result.Percents, "Wrong percents");
    }
#endif
  }
}
