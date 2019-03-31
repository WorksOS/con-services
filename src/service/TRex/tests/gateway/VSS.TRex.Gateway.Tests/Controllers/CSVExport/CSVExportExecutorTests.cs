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
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.CSVExport
{
  public class CSVExportExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void CSVExportExecutor_SiteModelNotFound()
    {
      var projectUid = Guid.NewGuid(); 
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] {"first machineName"};
      var userPreferences = new UserPreferences();

      var request = new CompactionVetaExportRequest(
        projectUid, filter, fileName,
        coordType, outputType, userPreferences, machineNames);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(compactionCSVExportRequest));
      result.Code.Should().Be(HttpStatusCode.BadRequest);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be($"Site model {projectUid} is unavailable");
    }

    [Fact]
    public void CSVExportExecutor_NoCSIB()
    {
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] { "first machineName" };
      var userPreferences = new UserPreferences();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new CompactionVetaExportRequest(
        siteModel.ID, null, fileName,
        coordType, outputType, userPreferences, machineNames);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(compactionCSVExportRequest));
      result.Code.Should().Be(HttpStatusCode.InternalServerError);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be("Failed to complete TRex request: CSVExportExecutor with error: Unable to load CSIB for LatLong conversion");
    }

    [Fact]
    public void CSVExportExecutor_GotSiteAndFilter()
    {
      var fileName = "gotAFilename";
      var coordType = CoordType.Northeast;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] { "first machineName" };
      var userPreferences = new UserPreferences();
      var filter = Productivity3D.Filter.Abstractions.Models.Filter.CreateFilter(
        new DateTime(2018, 1, 10),
        new DateTime(2019, 2, 11), "", "",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null);
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new CompactionVetaExportRequest(
        siteModel.ID, filterResult, fileName,
        coordType, outputType, userPreferences, machineNames);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(compactionCSVExportRequest));
      result.Code.Should().Be(HttpStatusCode.InternalServerError);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be("Failed to complete TRex request: CSVExportExecutor with error: Failed to configure internal pipeline.");
    }
  }
}


