using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASNodeRPC;
using DesignProfiler.ComputeProfile.RPC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;

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
        //.AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ErrorCodesProvider>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public void CutFillExecutorNoResult()
    {
      /*
      var request = CutFillDetailsRequest.CreateCutFillDetailsRequest(0, null, null, null, null);

      TCutFillDetails details = new TCutFillDetails();

      var raptorClient = new Mock<IASNodeClient>();

      raptorClient
        .Setup(x => x.GetCutFillDetails(request.projectId.Value, It.IsAny<TASNodeRequestDescriptor>,
          It.IsAny<TCutFillSettings>, It.IsAny<TICFilterSettings>, It.IsAny<TICLiftBuildSettings>,
          out details()))
        .Returns(false);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionCutFillExecutor>(logger, raptorClient.Object);
      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
      */
    }

    [TestMethod]
    public void CutFillExecutorSuccess()
    {
      /*
      var request = CutFillDetailsRequest.CreateCutFillDetailsRequest(0, null, null, null, null);

      TCutFillDetails details = new TCutFillDetails{ Percents = new double[] {5.0, 20.0, 13.0, 10.0, 22.0, 12.0, 18.0}};

      var raptorClient = new Mock<IASNodeClient>();

      raptorClient
        .Setup(x => x.GetCutFillDetails(request.projectId.Value, It.IsAny<TASNodeRequestDescriptor>,
          It.IsAny<TCutFillSettings>, It.IsAny<TICFilterSettings>, It.IsAny<TICLiftBuildSettings>,
          out details()))
        .Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionCutFillExecutor>(logger, raptorClient.Object);
      var result = executor.Process(request) as CompactionCutFillDetailedResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual<double[]>(details.Percents, result.Percents, "Wrong percents");
      */
    }
  }
}
