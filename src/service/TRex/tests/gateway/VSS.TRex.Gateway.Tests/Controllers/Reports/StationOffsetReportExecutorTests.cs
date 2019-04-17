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
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Reports
{
  public class StationOffsetReportExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void StationOffsetReportExecutor_SiteModelNotFound()
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
      Guid alignmentDesignUid = Guid.NewGuid();
      double crossSectionInterval = 1.0;
      double startStation = 100;
      double endStation = 200;
      double[] offsets = new double[] {-1, 0, 1};

      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<StationOffsetReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain <IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(request));
      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }

    [Fact]
    public void StationOffsetReportExecutor_GotSiteAndFilter()
    {
      bool reportElevation = true;
      bool reportCmv = true;
      bool reportMdp = true;
      bool reportPassCount = true;
      bool reportTemperature = true;
      bool reportCutFill = false;
      Guid? cutFillDesignUid = null;
      Guid alignmentDesignUid = Guid.NewGuid();
      double crossSectionInterval = 1.0;
      double startStation = 100;
      double endStation = 200;
      double[] offsets = new double[] { -1, 0, 1 };

      var filter = Productivity3D.Filter.Abstractions.Models.Filter.CreateFilter(
        DateTime.SpecifyKind(new DateTime(2018, 1, 10), DateTimeKind.Utc),
        DateTime.SpecifyKind(new DateTime(2019, 2, 11), DateTimeKind.Utc), "", "",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null);
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        siteModel.ID, filterResult,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<StationOffsetReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = executor.Process(request) as GriddedReportDataResult;
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.GriddedData.Should().NotBeNull();
      result.GriddedData.Should().NotBeEmpty();
    }
  }
}


