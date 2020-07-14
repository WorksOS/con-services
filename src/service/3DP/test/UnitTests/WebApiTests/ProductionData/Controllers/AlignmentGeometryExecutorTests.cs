using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;
using Xunit;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  public class AlignmentGeometryExecutorTests
  {
    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;
    private static IHeaderDictionary _customHeaders;

    public AlignmentGeometryExecutorTests()
    {
      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
        .BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new HeaderDictionary();
    }

    [Fact]
    public void Creation()
    {
      var exec = new AlignmentGeometryExecutor();
      exec.Should().NotBeNull();
    }

    [Fact]
    public void Success()
    {
      var projectUid = Guid.NewGuid();
      var designUid = Guid.NewGuid();

      var request = new AlignmentGeometryRequest(projectUid, false, 0.0, "", designUid);
      var expectedResult = new AlignmentGeometryResult
      (
        0,
        new AlignmentGeometry
        (
          designUid,
          "",
          new[] {new[] {new double[] {1, 2, 3}}},
          new[] {new AlignmentGeometryResultArc(0, 1, 2, 3, 4, 5, 6, 7, 8, true)},
          new[] {new AlignmentGeometryResultLabel(0, 1, 2, 3),}
        )
      );

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<AlignmentGeometryResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>(), It.IsAny<List<KeyValuePair<string, string>>>()))
        .ReturnsAsync(expectedResult);

      var executor = RequestExecutorContainerFactory
        .Build<AlignmentGeometryExecutor>(logger, trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var result = executor.ProcessAsync(request).Result as AlignmentGeometryResult;

      result.Should().NotBeNull();
      result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void Failure()
    {
      var projectUid = Guid.NewGuid();
      var designUid = Guid.NewGuid();

      var request = new AlignmentGeometryRequest(projectUid, false, 0.0, "", designUid);
      AlignmentGeometryResult expectedResult = null;

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<AlignmentGeometryResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>(), It.IsAny<List<KeyValuePair<string, string>>>()))
        .ReturnsAsync(expectedResult);

      var executor = RequestExecutorContainerFactory
        .Build<AlignmentGeometryExecutor>(logger, trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      Action act = () => _ = executor.ProcessAsync(request).Result as AlignmentGeometryResult;
      act.Should().Throw<ServiceException>().WithMessage($"Failed to get alignment center line geometry for alignment: {designUid}");
    }
  }
}
