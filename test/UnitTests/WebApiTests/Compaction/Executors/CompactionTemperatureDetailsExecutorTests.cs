﻿using ASNodeRPC;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionTemperatureDetailsExecutorTests
  {
    [TestMethod]
    public void TemperatureDetailsExecutorNoResult()
    {
      /*
      var request = new TemperatureDetailsRequest(0, null, null, null);

      TTemperatureDetails details = new TTemperatureDetails();

      var raptorClient = new Mock<IASNodeClient>();
      var logger = new Mock<ILoggerFactory>();

      raptorClient
        .Setup(x => x.GetTemperatureDetails(request.ProjectId.Value, It.IsAny<TASNodeRequestDescriptor>(),
          It.IsAny<TTemperatureDetailSettings>(), It.IsAny<TICFilterSettings>(), It.IsAny<TICLiftBuildSettings>(),
          out details))
        .Returns(false);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionTemperatureDetailsExecutor>(logger.Object, raptorClient.Object);
      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
      */
      Assert.Inconclusive("Include this test when Raptor temperature details implemented");
    }

    [TestMethod]
    public void TemperatureDetailsExecutorSuccess()
    {
      /*
      var request = new TemperatureDetailsRequest(0, null, null, null);

      TTemperatureDetails details = new TTemperatureDetails { Percents = new[] { 5.0, 40.0, 23.0, 10.0, 22.0 } };

      var raptorClient = new Mock<IASNodeClient>();
      var logger = new Mock<ILoggerFactory>();

      raptorClient
        .Setup(x => x.GetTemperatureDetails(request.ProjectId.Value, It.IsAny<TASNodeRequestDescriptor>(),
          It.IsAny<TCutFillSettings>(), It.IsAny<TICFilterSettings>(), It.IsAny<TICLiftBuildSettings>(),
          out details))
        .Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionTemperatureDetailsExecutor>(logger.Object, raptorClient.Object);
      var result = executor.Process(request) as CompactionTemperatureDetailResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.IsNotNull(result.DetailsData, "Details should not be null");
      Assert.AreEqual(details.Percents, result.DetailsData.Percents, "Wrong percents");
      */
      Assert.Inconclusive("Include this test when Raptor temperature details implemented");
    }
  }
}
