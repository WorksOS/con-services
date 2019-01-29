using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Reports.StationOffset
{
  public class StationOffsetReportExecutorTests : IClassFixture<DITagFileFixture>
  {
    private static Guid NewSiteModelGuid = Guid.NewGuid();

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, false, false,
      null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] {-1, 0, 1})]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, true, true, true, true, false,
      null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] {-1, 0, 1})]
    public void StationOffsetTRexRequest_Successful(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, Guid alignmentDesignUid,
      double crossSectionInterval, double startStation, double endStation, double[] offsets)
    {
      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets);
      request.Validate();
    }

    [Theory]
    [InlineData(null, null,
      true, false, false, false, false, false,
      null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] {-1, 0, 1},
      "ProjectUid must be provided")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      false, false, false, false, false, false,
      null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] {-1, 0, 1},
      "There are no selected fields to be reported on")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, true, false,
      null, null,
      1.0, 100, 200, new double[3] {-1, 0, 1},
      "Alignment file must be specified for station and offset report.")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, true, false,
      null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      400, 100, 200, new double[3] {-1, 0, 1},
      "Interval must be >= 0.1m and <= 100m. Actual value: 400")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, true, false,
      null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[0],
      "Offsets must be specified for station and offset report.")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, true, false,
      null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 300, 200, new double[3] {-1, 0, 1},
      "Invalid station range for station and offset report.")]
    public void StationOffsetTRexRequest_Unsuccesfull(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, Guid alignmentDesignUid,
      double crossSectionInterval, double startStation, double endStation, double[] offsets,
      string errorMessage)
    {
      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets);
      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal(errorMessage, ex.GetResult.Message);
    }

    [Fact]
    public void StationOffsetReportExecutor_SiteModelNotFound()
    {
      Guid projectUid = Guid.NewGuid(); // use NewSiteModelGuid to get mocked siteModel
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
          TRex.DI.DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain <IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(request));
      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }
   
  }
}


