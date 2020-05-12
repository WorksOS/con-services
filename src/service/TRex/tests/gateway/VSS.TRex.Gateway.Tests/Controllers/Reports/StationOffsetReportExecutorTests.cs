using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Reports
{
  public class StationOffsetReportExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public async Task StationOffsetReportExecutor_SiteModelNotFound()
    {
      var projectUid = Guid.NewGuid(); 
      FilterResult filter = null;
      bool reportElevation = true;
      bool reportCmv = true;
      bool reportMdp = true;
      bool reportPassCount = true;
      bool reportTemperature = true;
      bool reportCutFill = false;
      Guid? cutFillDesignUid = null;
      double? cutFillDesignOffset = null;
      var alignmentDesignUid = Guid.NewGuid();
      double crossSectionInterval = 1.0;
      double startStation = 100;
      double endStation = 200;
      var offsets = new double[] {-1, 0, 1};

      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutFillDesignOffset, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets, null, null);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<StationOffsetReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain <IServiceExceptionHandler>());
      var result = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(request));
      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }

    [Fact]
    public async Task StationOffsetReportExecutor_GotSiteAndFilter()
    {
      bool reportElevation = true;
      bool reportCmv = true;
      bool reportMdp = true;
      bool reportPassCount = true;
      bool reportTemperature = true;
      bool reportCutFill = false;
      Guid? cutFillDesignUid = null;
      double? cutFillDesignOffset = null;
      var alignmentDesignUid = Guid.NewGuid();
      double crossSectionInterval = 1.0;
      double startStation = 100;
      double endStation = 200;
      double[] offsets = { -1, 0, 1 };

      var filter = new Productivity3D.Filter.Abstractions.Models.Filter(
        DateTime.SpecifyKind(new DateTime(2018, 1, 10), DateTimeKind.Utc),
        DateTime.SpecifyKind(new DateTime(2019, 2, 11), DateTimeKind.Utc), "", "",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null, null);
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        siteModel.ID, filterResult,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutFillDesignOffset, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets, null, null);
      request.Validate();

      // Mock the export request functionality to return an empty report reponse to stimulate the desired success
      var mockCompute = IgniteMock.Immutable.mockCompute;
      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<StationOffsetReportRequestComputeFunc_ApplicationService>(), It.IsAny<StationOffsetReportRequestArgument_ApplicationService>(), It.IsAny<CancellationToken>()))
        .Returns((StationOffsetReportRequestComputeFunc_ApplicationService func, StationOffsetReportRequestArgument_ApplicationService argument, CancellationToken token) => Task.FromResult(new StationOffsetReportRequestResponse_ApplicationService()));

      var executor = RequestExecutorContainer
        .Build<StationOffsetReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = await executor.ProcessAsync(request) as GriddedReportDataResult;
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.GriddedData.Should().NotBeNull();
      result.GriddedData.Should().NotBeEmpty();
    }
  }
}


