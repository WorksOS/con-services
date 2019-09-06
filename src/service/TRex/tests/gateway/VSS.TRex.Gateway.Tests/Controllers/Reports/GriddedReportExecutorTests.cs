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
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Reports
{
  public class GriddedReportExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public async Task GriddedReportExecutor_SiteModelNotFound()
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
      double? gridInterval = null;
      var gridReportOption = GridReportOption.Automatic;
      double startNorthing = 800000;
      double startEasting = 400000;
      double endNorthing = 800010;
      double endEasting = 400010;
      double azimuth = 4;

      var request = new CompactionReportGridTRexRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutFillDesignOffset,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth, null, null);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<GriddedReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain <IServiceExceptionHandler>());
      var result = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(request));
      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }

    [Fact]
    public async Task CSVExportExecutor_GotSiteAndFilter()
    {
      bool reportElevation = true;
      bool reportCmv = true;
      bool reportMdp = true;
      bool reportPassCount = true;
      bool reportTemperature = true;
      bool reportCutFill = false;
      Guid? cutFillDesignUid = null;
      double? cutFillDesignOffset = null;
      double? gridInterval = null;
      var gridReportOption = GridReportOption.Automatic;
      double startNorthing = 800000;
      double startEasting = 400000;
      double endNorthing = 800010;
      double endEasting = 400010;
      double azimuth = 4;

      var filter = new Productivity3D.Filter.Abstractions.Models.Filter(
        DateTime.SpecifyKind(new DateTime(2018, 1, 10), DateTimeKind.Utc),
        DateTime.SpecifyKind(new DateTime(2019, 2, 11), DateTimeKind.Utc), "", "",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null, null);
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new CompactionReportGridTRexRequest(
        siteModel.ID, filterResult,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutFillDesignOffset,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth, null, null);
      request.Validate();

      // Mock the CSV export request functionality to return a null CSV export reponse to stimulate the desired success
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();
      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<GriddedReportRequestComputeFunc>(), It.IsAny<GriddedReportRequestArgument>(), It.IsAny<CancellationToken>())).Returns((GriddedReportRequestComputeFunc func, GriddedReportRequestArgument argument, CancellationToken token) => Task.FromResult(new GriddedReportRequestResponse()));

      var executor = RequestExecutorContainer
        .Build<GriddedReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = await executor.ProcessAsync(request) as GriddedReportDataResult;
      result?.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result?.GriddedData.Should().NotBeNull();
      result?.GriddedData.Should().NotBeEmpty();
    }
  }
}


