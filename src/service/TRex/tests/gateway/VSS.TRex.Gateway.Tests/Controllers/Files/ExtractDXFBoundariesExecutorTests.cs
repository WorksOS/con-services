using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Files;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors.Files;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Tests.TestFixtures;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Files
{
  public class ExtractDXFBoundariesExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var executor = new ExtractDXFBoundariesExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
      executor.Should().NotBeNull();
    }

    [Fact]
    public async void SingleBoundary()
    {
      var request = new DXFBoundariesRequest("", ImportedFileType.SiteBoundary, Path.Combine("TestData", "Southern Motorway 55 point polygon.dxf"));
      var executor = new ExtractDXFBoundariesExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
      executor.Should().NotBeNull();

      var result = await executor.ProcessAsync<DXFBoundariesRequest>(request);
      result.Should().NotBeNull();
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.Message.Should().Be("Success");

      if (result is DXFBoundaryResult boundary)
      {
        boundary.Boundaries.Count.Should().Be(1);
        boundary.Boundaries[0].Fence.Count.Should().Be(55);
      }
      else
        false.Should().BeTrue(); // fail the test
    }
  }
}
