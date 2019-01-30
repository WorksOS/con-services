using System;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Reports.Gridded
{
  public class GriddedReportExecutorTests : IClassFixture<DITagFileFixture>
  {
    private static Guid NewSiteModelGuid = Guid.NewGuid();

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, false, false,
      null, 
      null, GridReportOption.Automatic,
      800000, 400000, 800001, 400001, 10)]
    public void GriddedTRexRequest_Successful(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, 
      double? gridInterval, GridReportOption gridReportOption, 
      double startNorthing, double startEasting, double endNorthing, double endEasting, double azimuth)
    {
      var request = CompactionReportGridTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);
      request.Validate();
    }

    [Theory]
    [InlineData(null, null,
      true, false, false, false, false, false,
      null,
      null, GridReportOption.Automatic,
      800000, 400000, 800001, 400001, 10,
      "ProjectUid must be provided")]
    public void GriddedTRexRequest_Unsuccesfull(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid,
      double? gridInterval, GridReportOption gridReportOption,
      double startNorthing, double startEasting, double endNorthing, double endEasting, double azimuth,
      string errorMessage)
    {
      var request = CompactionReportGridTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);
      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal(errorMessage, ex.GetResult.Message);
    }

    [Fact]
    public void GriddedReportExecutor_SiteModelNotFound()
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
      double? gridInterval = null;
      GridReportOption gridReportOption = GridReportOption.Automatic;
      double startNorthing = 800000;
      double startEasting = 400000;
      double endNorthing = 800010;
      double endEasting = 400010;
      double azimuth = 10;

      var request = CompactionReportGridTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<GriddedReportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          TRex.DI.DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain <IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(request));
      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }
   
  }
}


