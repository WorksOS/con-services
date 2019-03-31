using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Reports
{
  public class GriddedReportExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void GriddedReportExecutor_SiteModelNotFound()
    {
      Guid projectUid = Guid.NewGuid(); 
      FilterResult filter = null;
      bool reportElevation = true;
      bool reportCmv = true;
      bool reportMdp = true;
      bool reportPassCount = true;
      bool reportTemperature = true;
      bool reportCutFill = false;
      Guid? cutFillDesignUid = null;
      double? gridInterval = null;
      GridReportOption gridReportOption = GridReportOption.Automatic;
      double startNorthing = 800000;
      double startEasting = 400000;
      double endNorthing = 800010;
      double endEasting = 400010;
      double azimuth = 4;

      var request = new CompactionReportGridTRexRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<GriddedReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain <IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(request));
      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }

    [Fact]
    public void CSVExportExecutor_GotSiteAndFilter()
    {
      bool reportElevation = true;
      bool reportCmv = true;
      bool reportMdp = true;
      bool reportPassCount = true;
      bool reportTemperature = true;
      bool reportCutFill = false;
      Guid? cutFillDesignUid = null;
      double? gridInterval = null;
      GridReportOption gridReportOption = GridReportOption.Automatic;
      double startNorthing = 800000;
      double startEasting = 400000;
      double endNorthing = 800010;
      double endEasting = 400010;
      double azimuth = 4;

      var filter = Productivity3D.Filter.Abstractions.Models.Filter.CreateFilter(
        new DateTime(2018, 1, 10),
        new DateTime(2019, 2, 11), "", "",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null);
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new CompactionReportGridTRexRequest(
        siteModel.ID, filterResult,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<GriddedReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = executor.Process(request) as GriddedReportDataResult;
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.GriddedData.Should().NotBeNull();
      result.GriddedData.Should().NotBeEmpty();
    }
  }
}


