using DesignProfiler.ComputeProfile.RPC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICProfileCell;
using System;
using System.IO;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionDesignProfileExecutorTests : ExecutorTestsBase
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
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ErrorCodesProvider>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public void Should_return_empty_result_When_no_result_returned_from_Raptor()
    {
      var raptorClient = new Mock<IASNodeClient>();

      raptorClient
        .Setup(x => x.GetDesignProfile(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignProfile_Args>()))
        .Returns((MemoryStream)null);

      var request = CompactionProfileDesignRequest.CreateCompactionProfileDesignRequest(
        1234, null,  null, -1, null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionDesignProfileExecutor>(logger, raptorClient.Object);
      var result = executor.Process(request) as CompactionProfileResult<CompactionProfileVertex>;
      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(0, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(0, result.results.Count, ResultsShouldBeEmpty);
    }

    [TestMethod]
    public void Should_return_correct_grid_distance_and_point_count()
    {
      var packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station = 0.000,
            DesignElev = 597.4387F
          },
          new TICProfileCell
          {
            Station = 0.80197204271533173,
            DesignElev = 597.4356F
          },
          new TICProfileCell
          {
            Station = 1.6069349835347948,
            DesignElev = 597.434265F
          }
        },
        GridDistanceBetweenProfilePoints = 1.3951246308791798E+306
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(3, result.results.Count, IncorrectNumberOfPoints);
    }

    private CompactionProfileResult<CompactionProfileVertex> MockGetProfile(TICProfileCellListPackager packager)
    {
      var raptorClient = new Mock<IASNodeClient>();
      using (var ms = new MemoryStream())
      {
        packager.WriteToStream(ms);
        ms.Position = 0;
        raptorClient
          .Setup(x => x.GetDesignProfile(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignProfile_Args>()))
          .Returns(ms);

        var request = CompactionProfileDesignRequest.CreateCompactionProfileDesignRequest(
          1234, null, null, -1, null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION);

        var executor = RequestExecutorContainerFactory
          .Build<CompactionDesignProfileExecutor>(logger, raptorClient.Object);
        var result = executor.Process(request) as CompactionProfileResult<CompactionProfileVertex>;
        return result;
      }
    }
  }
}